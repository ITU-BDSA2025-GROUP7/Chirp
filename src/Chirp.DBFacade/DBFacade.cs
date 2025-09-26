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
    
    private const string READ_ALL_QUERY = 
        "SELECT (author_id, pub_data, message) FROM messages ORDER BY pub_data desc";
    private const string READ_SOME_QUERY = 
        "SELECT (author_id, pub_data, message) FROM messages ORDER BY pub_data desc LIMIT @limit";
    
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
    

    /** Execute the given embedded file's SQL command, e.g. inserting schema or initial data. */
    private static SqliteConnection ExecuteCommandInFile(SqliteConnection connection, string filename) {
        var embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
        using Stream? reader = embeddedProvider.GetFileInfo(filename).CreateReadStream();
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
        ExecuteCommandInFile(connection, SCHEMA_PATH);
        ExecuteCommandInFile(connection, INITIAL_DATA_PATH);
        return connection;
    }
    
    public IEnumerable<T> Read(int? limit = null) {
        using SqliteConnection connection = Connect();
        connection.Open();
        SqliteCommand command = connection.CreateCommand();
        
        if (limit.HasValue) {
            command.CommandText = READ_SOME_QUERY;
        } else {
            command.CommandText = READ_ALL_QUERY;
        }
        command.Transaction = connection.BeginTransaction();
        using SqliteDataReader reader = command.ExecuteReader();
        var data = new List<T>(limit ?? 10);
        while (reader.Read()) {
            data.AddRange(reader.Cast<T>());
        }
        command.Transaction.Commit();
        reader.Close();
        connection.Close();
        return data;
    }

    public void Store(T record) {
        throw new NotImplementedException();
    }
}