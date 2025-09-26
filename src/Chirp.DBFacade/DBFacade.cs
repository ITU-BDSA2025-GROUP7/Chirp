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
    
    private readonly Type _type = typeof(T);
    private readonly string _name = typeof(T).Name;
    private const string DEFAULT_DATABASE_FILENAME = "Chirp.db";
    private const string SCHEMA_PATH = "data/schema.sql";
    private const string INITIAL_DATA_PATH = "data/dump.sql";
    private readonly string _dbLocation = 
        Environment.GetEnvironmentVariable("CHIRPDBPATH") ?? "";
    
    private const string READ_QUERY = 
        "SELECT username, text, pub_date FROM message JOIN user ON author_id=user_id ORDER BY pub_date desc";
    
    private static DBFacade<T>? _instance = null; //  ? makes it nullable
    private static readonly Lock _padlock = new();
    
    public static DBFacade<T> Instance
    {
        get
        {
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
    private static string DetermineQueryString(int? limit = null) {
        if (limit.HasValue) {
            return string.Concat(READ_QUERY, " LIMIT ", limit.ToString());
        }
        return READ_QUERY;
    }

    /** <inheritdoc cref="IDataBaseRepository{T}.Read(int?)"/>
    <exception cref="ArgumentOutOfRangeException">If a negative value is provided.</exception> */
    public IEnumerable<T> Read(int? limit = null) {
        if (limit is 0) return [];
        if (limit is < 0) throw new ArgumentOutOfRangeException(nameof(limit));
        
        using SqliteConnection connection = Connect();
        connection.Open();
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = DetermineQueryString(limit);
        using SqliteDataReader reader = command.ExecuteReader();
        
        if (!reader.HasRows) return [];
        if (reader.IsClosed) throw new SqliteException("The database reader is closed", 1);
        if (reader.FieldCount != _type.GetProperties().Length) { // Won't be able to convert
            throw new InvalidCastException("The number of columns returned (" + reader.FieldCount + 
                                           ") does not match the number of properties in " +
                                           _name + " (" + _type.GetProperties().Length + ")");
        }
        
        // Get the types of T's properties, and then the matching constructor method.
        Type[] types = _type.GetProperties().Select(p => p.PropertyType).ToArray();
        ConstructorInfo? constructor = _type.GetConstructor(types);
        if (constructor == null) {
            string typeString = "[" +
                                string.Join(", ", types.Select(p => p.Name)) +
                                "]";
            throw new MissingMethodException("Could not find a constructor for " + _name
            + " which matches the property types " + typeString);
        }

        var values = new object?[reader.FieldCount]; // Array in which to store a row from the database.
        var data = new List<T>();
        while (reader.Read()) {
            reader.GetValues(values); // stores current row in 'values'
            object? record = constructor.Invoke(values); // call constructor with params from 'values'
            if (record == null) {
                throw new NullReferenceException(
                    "Record is null; something went wrong while invoking the constructor of " + _name);
            }
            data.Add((T)record);
        }
        
        reader.Close();
        connection.Close();
        return data;
    }

    public void Store(T record) {
        throw new NotImplementedException();
    }
}