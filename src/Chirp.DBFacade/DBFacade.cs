using System.Reflection;
using Chirp.CSVDB;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.FileProviders;
using SQL = SQLitePCL;

namespace Chirp.DBFacade;

/** Facade to be used for interacting with the SQL database. */
public class DBFacade<T> : IDataBaseRepository<T> {
    private DBFacade() {
        SQL.raw.SetProvider(new SQL.SQLite3Provider_sqlite3());
    }

    private static readonly Type Type = typeof(T);
    private static readonly string Name = typeof(T).Name;
    private const string DEFAULT_DATABASE_FILENAME = "Chirp.db";
    private const string SCHEMA_PATH = "data/schema.sql";
    private const string INITIAL_DATA_PATH = "data/dump.sql";

    private readonly string _dbLocation =
        Environment.GetEnvironmentVariable("CHIRPDBPATH") ?? "";

    /** Return [username, text, pub_date] */
    private const string ReadQuery =
        "SELECT username, text, pub_date FROM message JOIN user ON author_id=user_id ORDER BY pub_date desc";

    /** Returns the user_id, and count thereof, which matches whatever @username is replaced with.
     * In other words, even if no exceptions were to be thrown from this, we could still check the second field
     * to know whether the person exists. */
    private static string Check_Username_Query(string value) =>
        $"SELECT DISTINCT (user_id, COUNT(user_id)) FROM user JOIN message ON author_id=user_id WHERE username={value}";

    /** Insert the new record into the message table, supplying the 'author_id' field from the
     user table by selecting this as one of the values. If something goes wrong, the operation is
     aborted and any changes in progress are automatically rolled back. */
    private static string PrepareInsertionCommand(string[] values) =>
            "INSERT OR ROLLBACK INTO message (author_id, text, pub_date) " +
            "VALUES (" +
            "(SELECT DISTINCT user_id " +
            "FROM user JOIN message ON author_id=user_id " +
            $"WHERE username={values[0]}), {values[1]}, {values[2]})";

    private static DBFacade<T>? _instance = null; //  ? makes it nullable
    private static readonly Lock _padlock = new();

    public static DBFacade<T> Instance {
        get {
            lock (_padlock) {
                _instance ??= new DBFacade<T>();
                return _instance;
            }
        }
    }

    /** Execute the given embedded file's SQL command, e.g. inserting schema or initial data. */
    private static void ExecuteCommandInFile(SqliteConnection connection, string filename) {
        var embeddedProvider = new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly());
        using Stream? reader = embeddedProvider.GetFileInfo(filename).CreateReadStream();
        using var sr = new StreamReader(reader);
        string query = sr.ReadToEnd();

        connection.Open();
        using SqliteTransaction transaction = connection.BeginTransaction();
        SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = query;
        command.ExecuteNonQuery();
        transaction.Commit();
        connection.Close();
    }

    /** Connects to the database. If the file cannot be found, create a new one
     * in the user's temporary directory and initialise it. */
    private SqliteConnection Connect() {
        if (File.Exists(_dbLocation)) {
            return new SqliteConnection($"Data Source={_dbLocation}");
        }

        string tempDir = Path.GetTempPath();
        string dbFile = Path.Join(tempDir, DEFAULT_DATABASE_FILENAME);
        var connection = new SqliteConnection($"Data Source={dbFile}");
        ExecuteCommandInFile(connection, SCHEMA_PATH);
        ExecuteCommandInFile(connection, INITIAL_DATA_PATH);
        return connection;
    }

    /// Returns the correct query based on the input <tt>limit</tt>.
    /// Expects <tt>limit</tt> to be positive.
    private static string PrepareReadQuery(int? limit = null) {
        if (limit.HasValue)
            return string.Concat(ReadQuery, " LIMIT ", limit.ToString());
        return ReadQuery;
    }

    /** <inheritdoc cref="IDataBaseRepository{T}.Read(int?)"/>
    <exception cref="ArgumentOutOfRangeException">If a negative value is provided.</exception> */
    public IEnumerable<T> Read(int? limit = null) {
        if (limit is 0) return [];
        if (limit is < 0) throw new ArgumentOutOfRangeException(nameof(limit));

        using SqliteConnection connection = Connect();
        connection.Open();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = PrepareReadQuery(limit);
        using SqliteDataReader reader = command.ExecuteReader();

        if (!reader.HasRows) return [];
        if (reader.IsClosed) throw new SqliteException("The database reader is closed", 1);
        if (reader.FieldCount != Type.GetProperties().Length) { // Won't be able to convert
            throw new InvalidCastException("The number of columns returned (" + reader.FieldCount +
                                           ") does not match the number of properties in " +
                                           Name + " (" + Type.GetProperties().Length + ")");
        }

        // Get the types of T's properties, and then the matching constructor method.
        Type[] types = Type.GetProperties().Select(p => p.PropertyType).ToArray();
        ConstructorInfo? constructor = Type.GetConstructor(types);
        if (constructor == null) {
            throw new MissingMethodException("Could not find a constructor for " + Name
                + " which matches its property types.");
        }

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

        reader.Close();
        connection.Close();
        return data;
    }

    /** SQL escapes ' by putting another one before. */
    private static string EscapeQuotes(string value) => 
        value.Replace("'", "\'\'");
    
    /** We add single-quotes around the string after making sure all apostrophes are
     * properly escaped. */
    private static string SanitiseString(string value) => 
        '\'' + EscapeQuotes(value) + '\'';
    
    /** Make sure everything is non-null and can be formatted into a string.
     */
    private static string[] PrepareValues(T record) {
        PropertyInfo[] properties = Type.GetProperties();
        Type stringType = typeof(string);
        var values = new string[properties.Length];
        for (var i = 0; i < properties.Length; i++) {
            PropertyInfo p = properties[i];
            object v = p.GetValue(record) ?? 
                       throw new NullReferenceException("Value of " + p.Name + " is null.");
            string s = v.ToString() ??
                       throw new NullReferenceException("Value of " + p.Name + 
                                                        " could not be converted to a string.");
            if (p.PropertyType == stringType) {
                values[i] = SanitiseString(s);
            } else {
                values[i] = EscapeQuotes(s);
            }
        }

        return values;
    }

    public void Store(T record) {
        using SqliteConnection connection = Connect();
        connection.Open();
        using SqliteCommand command = connection.CreateCommand();
        string[] values = PrepareValues(record);
        command.CommandText = PrepareInsertionCommand(values);
        command.Transaction = connection.BeginTransaction();
        int updatedRows;
        try {
            updatedRows = command.ExecuteNonQuery();
        } catch {
            command.Transaction.Rollback();
            connection.Close();
            Console.Error.WriteLine("A(n) SQLite error occured during execution of the command." +
                                    "The transaction has been rolled back.");
            throw;
        }
        switch (updatedRows) {
            case 0:
                command.Transaction.Rollback();
                connection.Close();
                throw new Exception("Could not insert record.");
            case > 1:
                command.Transaction.Rollback();
                connection.Close();
                throw new Exception("More than one row was updated during execution of the command.");
            // The only case where the transaction was successful is when precisely one row was added.
            case 1:
                command.Transaction.Commit();
                connection.Close();
                break;
        }
    }
}