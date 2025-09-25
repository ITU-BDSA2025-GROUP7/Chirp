using System.Reflection;
using Chirp.CSVDB;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.FileProviders;

namespace Chirp.DBFacade;

/** Facade to be used for interacting with the SQL database. */
public class DBFacade<T> : IDataBaseRepository<T> {
    private DBFacade() {}
    
    private const string DEFAULT_DATABASE_FILENAME = "Chirp.db";
    private const string SCHEMA_PATH = "./data/schema.sql";
    private const string INITIAL_DATA_PATH = "./data/dump.sql";
    private readonly string _dbLocation = 
        Environment.GetEnvironmentVariable("CHIRPDBPATH") ?? "";
    
    private static DBFacade<T>? _instance = null; //  ? makes it nullable
    private static readonly Lock _padlock = new Lock();
    
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
    

    /** Load the example data into the database. */
    private static SqliteConnection LoadData(SqliteConnection connection) {
        var embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
        using Stream? reader = embeddedProvider.GetFileInfo(INITIAL_DATA_PATH).CreateReadStream();
        using var sr = new StreamReader(reader);
        string query = sr.ReadToEnd();
        
        SqliteTransaction transaction = connection.BeginTransaction();
        SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = query;
        command.ExecuteNonQuery();
        transaction.Commit();
        
        return connection;
    }
    
    /** Load the database schema into the database. */
    private static SqliteConnection LoadSchema(SqliteConnection connection) {
        var embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
        using Stream? reader = embeddedProvider.GetFileInfo(SCHEMA_PATH).CreateReadStream();
        using var sr = new StreamReader(reader);
        string query = sr.ReadToEnd();
        
        SqliteTransaction transaction = connection.BeginTransaction();
        SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = query;
        command.ExecuteNonQuery();
        transaction.Commit();
        
        return connection;
    }
    
    /** Connect to the database. If the file cannot be found, create a new one
     * in the user's temporary directory and initialise it. */
    private SqliteConnection Connect() {
        if (File.Exists(_dbLocation)) {
            return new SqliteConnection($"Data Source={_dbLocation}");
        }

        string tempDir = Path.GetTempPath();
        string dbFile = Path.Join(tempDir, DEFAULT_DATABASE_FILENAME);
        var connection = new SqliteConnection($"Data Source={dbFile}");
        connection.Open();
        LoadSchema(connection);
        LoadData(connection);
        return connection;
    }
    
    /** Read up to <c>limit</c> records from the database. */
    public IEnumerable<T> Read(int? limit = null) {
        using SqliteConnection connection = Connect();
        connection.Open();
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT (author_id, pub_data, message) FROM messages ORDER BY pub_data desc LIMIT @limit";
        using SqliteDataReader reader = command.ExecuteReader();
        var data = new List<T>(limit ?? 10);
        while (reader.Read()) {
            data.AddRange(reader.Cast<T>());
        }
        return data;
    }

    public void Store(T record) {
        throw new NotImplementedException();
    }
}