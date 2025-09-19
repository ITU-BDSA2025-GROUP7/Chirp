using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Chirp.CSVDB;

public class CsvDataBase<T> : IDataBaseRepository<T>
{
    private string path = "";
    private static CsvDataBase<T> instance = null;
    private static readonly object padlock = new object();

    private CsvDataBase()
    {
    }

    public static CsvDataBase<T> Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new CsvDataBase<T>();
                }
                return instance;
            }
        }
    }

    /** Returns up to and including <c>limit</c> records from the database.<br/>
     * If <c>limit</c> is null, returns all records.<br/>
     * If a corrupted/unreadable record is encountered, everything
     * up until that point is returned, excluding the unreadable data. */
    public IEnumerable<T> Read(int? limit)
    {
        if (limit != null && limit <= 0)
            return new List<T>();
        var safeLimit = int.MaxValue; // C# will run out of memory before the list gets this big.
        if (limit != null)
            safeLimit = (int)limit;

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        using var records = csv.GetRecords<T>().GetEnumerator();
        var output = new List<T>();
        try
        {
            // We do this semi-manually so that we can smoothly recover from exceptions.
            for (var i = 0; i < safeLimit && records.MoveNext(); i++)
            {
                output.Add(records.Current);
            }
        }
        catch (Exception)
        {
            // Tests have proved BadDataException, TypeConverterException, and MissingFieldException here
            // An exception during iteration ends the process (even if caught inside the loop).
            // We ignore the exception (for now?), and return the records we retrieved before it crashed.
        }
        return output;
    }

    public void SetPath(string path)
    {
        this.path = path;
    }

    public string GetPath()
    {
        return path;
    }
    public static void Reset() {
        
        instance = null;
    }
    
    

    /** Adds a new record to the database.<br/>
     * No sanity checks are applied; this method assumes that the record is safe
     * to be read back by <see cref="Read(int?)"/> later on. */
    public void Store(T record)
    {
        var shouldWriteHeader = !File.Exists(path); // if file does not exist, we write the header
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            // Don't write the header again.
            HasHeaderRecord = false,
            NewLine = Environment.NewLine, 
            ShouldQuote = args => false
        };
        
        
        using var stream = File.Open(path, FileMode.Append, FileAccess.Write,FileShare.ReadWrite);
        using var writer = new StreamWriter(stream);
        using var csv = new CsvWriter(writer, config);
        if (shouldWriteHeader)
        {
            csv.WriteHeader<T>();
            csv.NextRecord();
        }
        csv.WriteRecord(record);
        csv.NextRecord();
    }
}