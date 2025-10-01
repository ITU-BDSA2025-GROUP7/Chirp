using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Chirp.CSVDB;
using Chirp.General;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.FileProviders;
using SQL = SQLitePCL;
using static Chirp.DBFacade.SqlHelper;

// ReSharper disable StaticMemberInGenericType // <-- _instance and Padlock have to be static.

namespace Chirp.DBFacade;

/** Facade to be used for interacting with the SQL database. */
public sealed class DBFacade<T> : IDisposable, IDataBaseRepository<T> where T : Cheep {
    private DBFacade() {
        SQL.raw.SetProvider(new SQL.SQLite3Provider_sqlite3());
        (_connection, bool hasToInitialise) = Connect();
        if (hasToInitialise)
            Init();
    }

    private readonly bool _shouldHashUsernames =
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Test";

    private string DatabaseLocation { get; set; } = DEFAULT_DATABASE_DIR
                                                    + Path.PathSeparator
                                                    + DEFAULT_DATABASE_FILENAME;

    private const string DEFAULT_DATABASE_DIR = "data";
    private const string DEFAULT_DATABASE_FILENAME = "Chirp.db";
    private const string DEFAULT_SCHEMA_PATH = "data/schema.sql";
    private const string DEFAULT_INITIAL_DATA_PATH = "data/dump.sql";

    private readonly SqliteConnection _connection;

    private static DBFacade<Cheep>? _instance = null;

    private static readonly Lock Padlock = new();

    public static DBFacade<Cheep> Instance {
        get {
            lock (Padlock) {
                return _instance ??= new DBFacade<Cheep>();
            }
        }
    }

    /** <inheritdoc cref="IDataBaseRepository{T}.Read(int?)"/> */
    public IEnumerable<T> Read(int? limit = null) {
        if (limit is <= 0) {
            yield break;
        }

        using SqliteCommand command = _connection.CreateCommand();
        SqliteParameter? p = null;
        if (limit is > 0) {
            p = NewParam(limit, nameof(limit));
            command.Parameters.Add(p);
        }

        command.CommandText = Queries.ReadQuery(p);
        command.Prepare();
        using SqliteDataReader reader = command.ExecuteReader();

        if (!reader.HasRows) yield break;
        if (reader.FieldCount != typeof(T).GetProperties().Length) { // Won't be able to convert
            throw new InvalidCastException("The number of columns returned (" + reader.FieldCount +
                                           ") does not match the number of properties in " +
                                           typeof(T).Name + " (" + typeof(T).GetProperties().Length + ")");
        }

        ConstructorInfo constructor = GetConstructor();
        var values = new object?[reader.FieldCount];
        while (reader.Read()) {
            reader.GetValues(values); // stores current row in 'values'
            object record = constructor.Invoke(values); // call constructor with params from 'values'
            yield return (T)record;
        }
    }

    /**
     Returns a page og cheeps. Each page is of size 32.
     - The page argument describes what page nr to return 
     - If the userName argument is set, then ownly entries from that user will be retuned
     */
    public IEnumerable<T> ReadPage(int page = 1, string? userName = null)
    {
        if (page < 1) page = 1;
        int? startingEntry = (page-1) * 32; // 32 is based on Service.PAGE_SIZE
        
        using SqliteCommand command = _connection.CreateCommand();
        
        SqliteParameter startingEntryPer = NewParam(startingEntry, nameof(startingEntry)); 
        command.Parameters.Add(startingEntryPer);
        
        if (userName == null)
        {
            command.CommandText = Queries.ReadPageQuery(startingEntryPer);
            command.Prepare();
            using SqliteDataReader reader = command.ExecuteReader();
            
            foreach (var item in GetFromReader(reader))
            {
                yield return item;
            }
        }
        else
        {
            SqliteParameter namePar = NewParam(userName, nameof(userName)); 
            command.Parameters.Add(namePar);
            
            command.CommandText = Queries.ReadPageQueryByName(namePar,startingEntryPer);
            command.Prepare();
            using SqliteDataReader reader = command.ExecuteReader();
            
            foreach (var item in GetFromReader(reader))
            {
                yield return item;
            }
        }
    }

    /** Helper method
     *  takes a reader and read all of it's results
     */
    private IEnumerable<T> GetFromReader(SqliteDataReader reader)
    {
        if (!reader.HasRows)
        {
            Console.WriteLine("Here1");
            yield break;
        }
        if (reader.FieldCount != typeof(T).GetProperties().Length) { // Won't be able to convert
            Console.WriteLine("Here2");
            throw new InvalidCastException("The number of columns returned (" + reader.FieldCount +
                                           ") does not match the number of properties in " +
                                           typeof(T).Name + " (" + typeof(T).GetProperties().Length + ")");
        }

        ConstructorInfo constructor = GetConstructor();
        var values = new object?[reader.FieldCount];
        while (reader.Read()) {
            reader.GetValues(values); // stores current row in 'values'
            object record = constructor.Invoke(values); // call constructor with params from 'values'
            yield return (T)record;
        }
    }

    /** <inheritdoc cref="IDataBaseRepository{T}.Store(T)"/> */
    public void Store(T record) {
        string username = record.Author;
        if (_shouldHashUsernames) {
            username = HashUserName(record.Author);
        }

        SqliteParameter author = NewParam(username, nameof(record.Author));
        bool exists = DoesUserExist(author);
        if (!exists) {
            int changes = CreateUser(author);
            if (changes == 0) {
                throw new Exception("The user + " + author.Value +
                                    " did not exist, but also could not be created.");
            }
        }

        using SqliteCommand cmd = _connection.CreateCommand();
        List<SqliteParameter> parameters = [
            author,
            NewParam(record.Message, nameof(record.Message)),
            NewParam(record.Timestamp, nameof(record.Timestamp))
        ];
        cmd.Parameters.AddRange(parameters);
        string[] names = parameters.Select(p => p.ParameterName).ToArray();
        cmd.CommandText = Queries.PrepareInsertionCommand(names);
        cmd.Prepare();
        CarefulInsert(cmd);
    }

    /** Connects to the database.<br/>
     * Attempts to find the path specified by environment variable
     * <see cref="DBEnv.envCHIRPDBPATH"/> if this is set.<br/>
     * If it isn't set, the default will be <see cref="DEFAULT_DATABASE_FILENAME"/>
     * located in <see cref="DEFAULT_DATABASE_DIR"/>.<br/>
     * If the file cannot be found/opened, one with the given or default name is
     * opened (created and initialised if need be) in the user's temporary
     * directory.<br/>
     * <returns>A tuple containing:<br/>
     * 1. the SqliteConnection,<br/>
     * 2. and whether the file is new, and therefore needs to be initialised.</returns>
     * <exception cref="SqliteException">If the database could be neither opened nor
     * created.</exception> */
    private (SqliteConnection, bool) Connect() {
        DatabaseLocation = Environment.GetEnvironmentVariable(DBEnv.envCHIRPDBPATH) ??
                           Path.Join(DEFAULT_DATABASE_DIR, DEFAULT_DATABASE_FILENAME);
        var str = $"Data Source={DatabaseLocation}; Mode=ReadWrite";
        var conn = new SqliteConnection(str);
        try {
            conn.Open();
            Console.WriteLine("Opened database:\n  " + conn.DataSource);
            return (conn, false);
        } catch {
            Console.WriteLine("Unable to open database:\n  " + conn.DataSource);
        }

        string tempDir = Path.GetTempPath();
        string? tempName = Environment.GetEnvironmentVariable(DBEnv.envCHIRPDBPATH);
        if (tempName == null) {
            tempName = DEFAULT_DATABASE_FILENAME;
        } else {
            tempName = Path.GetFileName(tempName);
        }

        DatabaseLocation = Path.Join(tempDir, tempName);
        conn = new SqliteConnection($"Data Source={DatabaseLocation}");
        try {
            conn.Open();
            Console.WriteLine("Opened database:\n  " + conn.DataSource);
            return (conn, true);
        } catch (SqliteException ex) {
            if (ex.SqliteErrorCode == 14) {
                Console.Error.WriteLine("Failed to open database:\n  " + conn.DataSource);
            }

            throw;
        }
    }

    /** Initialises the current database. This will execute the
     * schema and data-dump files, which can be overriden by
     * setting environment variables whose names are located at
     * <see cref="DBEnv.envSCHEMA"/> and <see cref="DBEnv.envDATA"/>.
     * <example>dotnet run ... -e CHIRPDB_SCHEMA=data/empty.sql</example>
     * <exception cref="FileNotFoundException">If a file wasn't found.</exception> */
    private void Init() {
        IDictionary env = Environment.GetEnvironmentVariables();
        object? variable = env[DBEnv.envSCHEMA];
        string schema = variable?.ToString() ?? DEFAULT_SCHEMA_PATH;
        ExecuteEmbeddedFile(schema);

        variable = env[DBEnv.envDATA];
        string dump = variable?.ToString() ?? DEFAULT_INITIAL_DATA_PATH;
        ExecuteEmbeddedFile(dump);
    }

    /** Execute the given embedded file's SQL command, e.g. inserting schema or initial data.
     * <exception cref="FileNotFoundException">If the file wasn't found.</exception>
     */
    private void ExecuteEmbeddedFile(string filename) {
        var embeddedProvider = new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly());
        IFileInfo f = embeddedProvider.GetFileInfo(filename);
        if (!f.Exists)
            throw new FileNotFoundException(f.Name);
        using Stream? reader = f.CreateReadStream();
        using var sr = new StreamReader(reader);
        string query = sr.ReadToEnd();

        SqliteCommand command = _connection.CreateCommand();
        command.CommandText = query;
        CarefulInsert(command);
    }

    /** Returns the constructor for T.
     * Modular like this so that any changes to Cheep later mean that changes
     * only need happen to the SQL query that reads from the database. */
    private static ConstructorInfo GetConstructor() {
        Type[] types = typeof(T).GetProperties().Select(p => p.PropertyType).ToArray();
        ConstructorInfo? constructor = typeof(T).GetConstructor(types);
        if (constructor == null) {
            throw new MissingMethodException("Could not find a constructor for "
                                             + typeof(T).Name
                                             + " which matches its property types.");
        }

        return constructor;
    }

    /** Returns whether the given user exists in the database. */
    private bool DoesUserExist(SqliteParameter user) {
        using SqliteCommand command = _connection.CreateCommand();
        command.Parameters.Add(user);
        command.CommandText = Queries.CountMatchingUsernames(user);
        command.Prepare();
        object? scalar = command.ExecuteScalar();
        var str = scalar?.ToString();
        if (str == null) return false;
        long count = long.Parse(str);
        return count > 0;
    }

    /** Inserts a new user into the database with the given username. */
    private int CreateUser(SqliteParameter username) {
        using SqliteCommand cmd = _connection.CreateCommand();
        cmd.Parameters.Add(username);
        cmd.Add($"{username.Value}@itu.dk", "email");
        cmd.Add(Guid.NewGuid().ToString("N"), "pwHash");
        cmd.CommandText = Queries.InsertUser(cmd.Parameters);
        cmd.Prepare();
        return CarefulInsert(cmd);
    }

    /** Performs a transaction which can update the database. Returns number of
     * rows that were updated.
     * If any error happened during the transaction, then the changes are rolled
     * back and an exception thrown.
     */
    private int CarefulInsert(SqliteCommand command) {
        using SqliteTransaction transaction = _connection.BeginTransaction();
        command.Transaction = transaction;
        int rowsUpdated;
        try {
            rowsUpdated = command.ExecuteNonQuery();
        } catch {
            command.Transaction.Rollback();
            Console.Error.WriteLine("A(n) SQLite error occured during execution of the command.");
            throw; // Throw the exception we caught here.
        }

        transaction.Commit();
        return rowsUpdated;
    }

    /** Helpful for testing.
     * Returns a list of all users. */
    public void ReadUsers() {
        using SqliteCommand cmd = _connection.CreateCommand();
        cmd.CommandText = Queries.SelectAllUsers;
        using SqliteDataReader reader = cmd.ExecuteReader();
        while (reader.Read()) {
            for (int i = 0; i < reader.FieldCount; i++) {
                Console.Write(reader.GetString(i) + "  ");
            }

            Console.WriteLine();
        }
    }

    /** Executes a scalar command based on the input text. */
    private long Count(string command) {
        using SqliteCommand cmd = _connection.CreateCommand();
        cmd.CommandText = command;
        object? val = cmd.ExecuteScalar();
        if (val != null) {
            return (long)val;
        }

        return -1;
    }

    /** Helpful for testing.
     * Returns the number of users in the database. */
    public long GetUserCount() {
        return Count(Queries.CountUsers);
    }

    /** Helpful for testing.
     * Returns the number of messages in the database. */
    public long GetRecordCount() {
        return Count(Queries.CountMessages);
    }

    /** Deterministically generates 8-character string based on input, with the first
     * character of the resulting name always being the same as the input. */
    public static string HashUserName(string input) {
        if (input.Length == 0) {
            throw new ArgumentException("Invalid username base: " + input);
        }

        var username = new List<byte>(32);
        while (username.Count < 32) { // The hashing algorithm wants reasonably long input
            username.AddRange(Encoding.UTF8.GetBytes(input));
        }

        byte[] hash = SHA256.HashData(username.ToArray());
        for (var i = 0; i < 7; i++) {
            hash[i] &= 0b0111_1111; // Make it ASCII.
            hash[i] %= 52; // Map it to values 0-51 (26 letters * 2)
            hash[i] += (byte)'A'; // Make it minimum A (which is #65 in ASCII).
            if (hash[i] > (byte)'Z') hash[i] += 'a' - 'Z'; // Skip the characters in-between cases
        }

        return string.Concat(input[0], Encoding.ASCII.GetString(hash)[..7]);
    }

    /** <inheritdoc cref="IDisposable.Dispose"/><br/>
     * Disposes of the database connection. */
    public void Dispose() {
        _connection.Close();
        _connection.Dispose();
    }

    /** Calls <see cref="Dispose"/> and then sets the instance to null. */
    public static void Reset() {
        Instance.Dispose();
        _instance = null;
    }
}