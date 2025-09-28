using System.Data;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Chirp.CSVDB;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.FileProviders;
using SQL = SQLitePCL;

namespace Chirp.DBFacade;

/** Facade to be used for interacting with the SQL database. */
public sealed class DBFacade<T> : IDisposable, IDataBaseRepository<T> { // where T : Cheep {
    private DBFacade() {
        SQL.raw.SetProvider(new SQL.SQLite3Provider_sqlite3());
        _connection = Connect();
        _connection.Open();
        ExecuteCommandInFile(SCHEMA_PATH);
        ExecuteCommandInFile(INITIAL_DATA_PATH);
    }

    public bool HashUsernames { get; set; } = 
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Test";

    public string DatabaseLocation { get; private set; } = DEFAULT_DATABASE_FILENAME;
    
    
    private static readonly Type Type = typeof(T);
    private static readonly string Name = typeof(T).Name;
    private const string DEFAULT_DATABASE_FILENAME = "Chirp.db";
    private const string SCHEMA_PATH = "data/schema.sql";
    private const string INITIAL_DATA_PATH = "data/dump.sql";

    private readonly SqliteConnection _connection;
    
    private static DBFacade<T>? _instance = null; //  ? makes it nullable
    // ReSharper disable once StaticMemberInGenericType // <- The lock has to be static
    private static readonly Lock Padlock = new();

    public static DBFacade<T> Instance {
        get {
            lock (Padlock) {
                return _instance ??= new DBFacade<T>();
            }
        }
    }
    
    /** Return [username, text, pub_date] */
    private const string ReadQuery =
        "SELECT username, text, pub_date FROM message JOIN user ON author_id=user_id ORDER BY pub_date desc";

    private static string PrepareInsertionCommand(string[] p) {
        return "INSERT OR ROLLBACK INTO message (author_id, text, pub_date) " +
            $"VALUES ((SELECT user_id FROM user WHERE username=@{p[0]} LIMIT 1), @{p[1]}, @{p[2]})";
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
            hash[i] &= 0b0111_1111;  // Make it ASCII.
            hash[i] %= 52;           // Map it to values 0-51 (26 letters * 2)
            hash[i] += (byte)'A';    // Make it minimum A (which is #65 in ASCII).
            if (hash[i] > (byte)'Z') hash[i] += 'a'-'Z'; // Skip the characters in-between cases
        }
        return string.Concat(input[0], Encoding.ASCII.GetString(hash)[..7]);
    }

    /** Execute the given embedded file's SQL command, e.g. inserting schema or initial data. */
    private void ExecuteCommandInFile(string filename) {
        var embeddedProvider = new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly());
        using Stream? reader = embeddedProvider.GetFileInfo(filename).CreateReadStream();
        using var sr = new StreamReader(reader);
        string query = sr.ReadToEnd();

        SqliteCommand command = _connection.CreateCommand();
        command.CommandText = query;
        CarefulInsert(command);
    }

    /** Connects to the database. If the file cannot be found, create a new one
     * in the user's temporary directory and initialise it. */
    private SqliteConnection Connect() {
        string dbLocation = Environment.GetEnvironmentVariable("CHIRPDBPATH") ?? "";
        if (File.Exists(dbLocation)) {
            return new SqliteConnection($"Data Source={dbLocation}");
        }

        string tempDir = Path.GetTempPath();
        DatabaseLocation = Path.Join(tempDir, DEFAULT_DATABASE_FILENAME);
        return new SqliteConnection($"Data Source={DatabaseLocation}");
    }

    /** <inheritdoc cref="IDataBaseRepository{T}.Read(int?)"/>
    <exception cref="ArgumentOutOfRangeException">If a negative value is provided.</exception> */
    public IEnumerable<T> Read(int? limit = null) {
        if (limit is 0) return [];
        if (limit is < 0) throw new ArgumentOutOfRangeException(nameof(limit));
        
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = ReadQuery;
        if (limit.HasValue) {
            command.Parameters.Add(Parameter((int)limit, nameof(limit)));
            command.CommandText += $" LIMIT @{nameof(limit)}";
        }
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
                values.Add(Parameter((string)value, p.Name));
            } else if (p.PropertyType == typeof(long)) {
                values.Add(Parameter((long)value, p.Name));
            } else
                throw new ArgumentOutOfRangeException(value.GetType().Name + " not supported.");
        }
        return values;
    }

    private bool DoesUserExist(SqliteParameter user) {
        using SqliteCommand command = _connection.CreateCommand();
        command.Parameters.Add(user);
        command.CommandText =
            $"SELECT COUNT(user_id) FROM user WHERE username=@{user.ParameterName}";
        command.Prepare();
        object? scalar = command.ExecuteScalar();
        var str = scalar?.ToString();
        if (str == null) return false;
        long count = long.Parse(str);
        return count > 0;
    }

    /** Insert a new user into the database with the given username. */
    private void CreateUser(SqliteParameter username) {
        using SqliteCommand cmd = _connection.CreateCommand();
        cmd.Parameters.Add(username);
        SqliteParameter p = Parameter($"{username.Value}@itu.dk", "email");
        cmd.Parameters.Add(p);
        p = Parameter(Guid.NewGuid().ToString("N"), "pwHash");
        cmd.Parameters.Add(p);
        cmd.CommandText =
            "INSERT OR ROLLBACK INTO user (username, email, pw_hash) VALUES (" +
            $"@{cmd.Parameters[0].ParameterName},@{cmd.Parameters[1].ParameterName}," +
            $"@{cmd.Parameters[2].ParameterName})";
        cmd.Prepare();
        CarefulInsert(cmd);
    }

    private static SqliteParameter Parameter(string value, string? name = null) {
        return new SqliteParameter {
            ParameterName = name ?? Guid.NewGuid().ToString("N"), // generate name if left out
            Value = value,
            DbType = DbType.String,
            SqliteType = SqliteType.Text
        };
    }

    private static SqliteParameter Parameter(long value, string? name = null) {
        return new SqliteParameter {
            ParameterName = name ?? Guid.NewGuid().ToString("N"),
            Value = value,
            DbType = DbType.Int64,
            SqliteType = SqliteType.Integer
        };
    }

    private static SqliteParameter Parameter(int value, string? name = null) {
        return new SqliteParameter {
            ParameterName = name ?? Guid.NewGuid().ToString("N"),
            Value = value,
            DbType = DbType.Int32,
            SqliteType = SqliteType.Integer
        };
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
    private void ReadUsers() {
        SqliteCommand cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM user";
        using SqliteDataReader reader = cmd.ExecuteReader();
        while (reader.Read()) {
            for (int i = 0; i < reader.FieldCount; i++) {
                Console.Write(reader.GetString(i) + "  ");
            }
            Console.WriteLine();
        }
    }
    
    /** Helpful for testing. */
    private long GetRecordCount() {
        SqliteCommand cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM message";
        object? val = cmd.ExecuteScalar();
        if (val != null) {
            return (long)val;
        }

        return -1;
    }

    public void Store(T record) {
        List<SqliteParameter> values = PrepareValues(record);
        values[0].Value = HashUserName(values[0].Value!.ToString()!);
        if (!DoesUserExist(values[0])) {
            CreateUser(values[0]);
        }
        using SqliteCommand cmd = _connection.CreateCommand();
        cmd.Parameters.AddRange(values);
        string[] names = values.Select(p => p.ParameterName).ToArray();
        cmd.CommandText = PrepareInsertionCommand(names);
        cmd.Prepare();
        CarefulInsert(cmd);
    }

    public void Dispose() {
        _connection.Close();
        _connection.Dispose();
    }

    public void Reset() {
        _instance = null;
    }
}