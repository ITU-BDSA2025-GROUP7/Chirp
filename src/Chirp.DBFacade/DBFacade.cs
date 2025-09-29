using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Chirp.CSVDB;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.FileProviders;
using SQL = SQLitePCL;
using static Chirp.DBFacade.SqlHelper;

namespace Chirp.DBFacade;

/** Facade to be used for interacting with the SQL database. */
public sealed class DBFacade<T> : IDisposable, IDataBaseRepository<T> { // where T : Cheep {
    private DBFacade() {
        SQL.raw.SetProvider(new SQL.SQLite3Provider_sqlite3());
        (_connection, bool hasToInitialise) = Connect();
        if (hasToInitialise)
            Init();
    }

    private bool HashUsernames { get; set; } =
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Test";

    public string DatabaseLocation { get; private set; } = DEFAULT_DATABASE_DIR 
                                                           + Path.PathSeparator 
                                                           + DEFAULT_DATABASE_FILENAME;

    private static readonly Type Type = typeof(T);
    private static readonly string Name = typeof(T).Name;
    private const string DEFAULT_DATABASE_DIR = "data";
    private const string DEFAULT_DATABASE_FILENAME = "Chirp.db";
    private const string DEFAULT_SCHEMA_PATH = "data/schema.sql";
    private const string DEFAULT_INITIAL_DATA_PATH = "data/dump.sql";

    private readonly SqliteConnection _connection;

    private static DBFacade<T>? _instance = null;

    // ReSharper disable once StaticMemberInGenericType // <- The lock has to be static
    private static readonly Lock Padlock = new();

    public static DBFacade<T> Instance {
        get {
            lock (Padlock) {
                return _instance ??= new DBFacade<T>();
            }
        }
    }

    /** <inheritdoc cref="IDataBaseRepository{T}.Read(int?)"/>
    <exception cref="ArgumentOutOfRangeException">If a negative value is provided.</exception> */
    public IEnumerable<T> Read(int? limit = null) {
        if (limit is 0) return [];
        if (limit is < 0) throw new ArgumentOutOfRangeException(nameof(limit));

        using SqliteCommand command = _connection.CreateCommand();
        SqliteParameter? p = null;
        if (limit is > 0) {
            p = NewParam(limit, nameof(limit));
            command.Parameters.Add(p);
        }

        command.CommandText = Queries.ReadQuery(p);
        command.Prepare();
        using SqliteDataReader reader = command.ExecuteReader();

        if (!reader.HasRows) return [];
        if (reader.FieldCount != Type.GetProperties().Length) { // Won't be able to convert
            throw new InvalidCastException("The number of columns returned (" + reader.FieldCount +
                                           ") does not match the number of properties in " +
                                           Name + " (" + Type.GetProperties().Length + ")");
        }

        ConstructorInfo constructor = GetConstructor();

        var values = new object?[reader.FieldCount];
        var data = new List<T>();
        while (reader.Read()) {
            reader.GetValues(values); // stores current row in 'values'
            object? record = constructor.Invoke(values); // call constructor with params from 'values'
            if (record == null) {
                throw new NullReferenceException(
                    "Record is null; something went wrong while invoking the constructor of " + Name);
            }

            data.Add((T)record);
        }

        return data;
    }

    public void Store(T record) {
        List<SqliteParameter> values = PrepareValues(record);
        if (HashUsernames)
            values[0].Value = HashUserName(values[0].Value!.ToString()!);
        bool exists = DoesUserExist(values[0]);
        if (!exists) {
            CreateUser(values[0]);
        }

        using SqliteCommand cmd = _connection.CreateCommand();
        cmd.Parameters.AddRange(values);
        string[] names = values.Select(p => p.ParameterName).ToArray();
        cmd.CommandText = Queries.PrepareInsertionCommand(names);
        cmd.Prepare();
        CarefulInsert(cmd);
    }

    /** Connects to the database. If the file cannot be found or opened, create
     * a new one in the user's temporary directory.<br/>
     * Returns a tuple containing the SqliteConnection and whether the file is
     * new, and therefore needs to be initialised. */
    private (SqliteConnection, bool) Connect() {
        DatabaseLocation = Environment.GetEnvironmentVariable(DBEnv.envCHIRPDBPATH) ?? 
                           Path.Join(DEFAULT_DATABASE_DIR, DEFAULT_DATABASE_FILENAME);
        var str = $"Data Source={DatabaseLocation}; Mode=ReadWrite";
        var conn = new SqliteConnection(str);
        Console.WriteLine("Creating SQL connection with connection string:\n  " + conn.ConnectionString);
        try {
            conn.Open();
            Console.WriteLine("Connection opened.");
            return (conn, false);
        } catch {
            Console.WriteLine("Unable to open existing database in\n  " + conn.DataSource);
        }
        
        string tempDir = Path.GetTempPath();
        string tempName = Environment.GetEnvironmentVariable(DBEnv.envCHIRPDBPATH) 
                          ?? DEFAULT_DATABASE_FILENAME;
        DatabaseLocation = Path.Join(tempDir, tempName);
        Console.WriteLine("Creating new database in temporary directory:\n  " + DatabaseLocation);
        conn = new SqliteConnection($"Data Source={DatabaseLocation}");
        try {
            conn.Open();
            Console.WriteLine("Connection opened.");
            return (conn, true);
        } catch (SqliteException ex) {
            if (ex.SqliteErrorCode == 14) {
                Console.Error.WriteLine("Failed to open database in temporary directory:\n  " + conn.DataSource);
            }

            throw;
        }
    }

    private void Init() {
        IDictionary env = Environment.GetEnvironmentVariables();
        object? variable = env["CHIRPDB_SCHEMA"];
        string schema = variable?.ToString() ?? DEFAULT_SCHEMA_PATH;
        ExecuteEmbeddedFile(schema);
        
        variable = env["CHIRPDB_DATA"];
        string dump = variable?.ToString() ?? DEFAULT_INITIAL_DATA_PATH;
        ExecuteEmbeddedFile(dump);
    }

    /** Execute the given embedded file's SQL command, e.g. inserting schema or initial data. */
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

    /** Returns the constructor for T. */
    private static ConstructorInfo GetConstructor() {
        Type[] types = Type.GetProperties().Select(p => p.PropertyType).ToArray();
        ConstructorInfo? constructor = Type.GetConstructor(types);
        if (constructor == null) {
            throw new MissingMethodException("Could not find a constructor for " + Name
                + " which matches its property types.");
        }

        return constructor;
    }

    /** Automatically create SQL parameters based on the record's properties. */
    private static List<SqliteParameter> PrepareValues(T record) {
        PropertyInfo[] properties = Type.GetProperties();
        var values = new List<SqliteParameter>();
        foreach (PropertyInfo p in properties) {
            if (p.GetValue(record) is null) {
                throw new NullReferenceException("Value of " + p.Name + " is null.");
            }

            object value = p.GetValue(record)!;
            if (p.PropertyType == typeof(string)) {
                values.Add(NewParam((string)value, p.Name));
            } else if (p.PropertyType == typeof(long)) {
                values.Add(NewParam((long)value, p.Name));
            } else
                throw new ArgumentOutOfRangeException(value.GetType().Name + " not supported.");
        }

        return values;
    }

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

    /** Insert a new user into the database with the given username. */
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

    /** Helpful for testing. */
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

    private long Count(string command) {
        using SqliteCommand cmd = _connection.CreateCommand();
        cmd.CommandText = command;
        object? val = cmd.ExecuteScalar();
        if (val != null) {
            return (long)val;
        }

        return -1;
    }

    /** Helpful for testing. */
    public long GetUserCount() {
        return Count(Queries.CountUsers);
    }

    /** Helpful for testing. */
    public long GetRecordCount() {
        return Count(Queries.CountMessages);
    }

    public void Dispose() {
        _connection.Close();
        _connection.Dispose();
    }

    public static void Reset() {
        Instance.Dispose();
        _instance = null;
    }
}