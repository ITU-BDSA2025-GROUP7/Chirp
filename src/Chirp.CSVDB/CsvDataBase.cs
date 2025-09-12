using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Chirp.CSVDB;

public sealed class CsvDataBase<T> : IDataBaseRepository<T>
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
    public IEnumerable<T> Read(int? limit = null)
    {
        
        using (var reader = new StreamReader(path))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<T>();
            return records.ToList<T>();
        }
    }

    public void SetPath(string path)
    {
        this.path = path;
    }

    public string GetPath()
    {
        return path;
    }

    public void Store(T record)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            // Don't write the header again.
            HasHeaderRecord = false,
            NewLine = Environment.NewLine,
            ShouldQuote = args => false
                
        };

        using var stream = File.Open(path, FileMode.Append);
        using var writer = new StreamWriter(stream);
        using (var csv = new CsvWriter(writer, config))
        {
            csv.WriteRecord(record);
            csv.NextRecord();
        }
    }
}