using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Chirp.CSVDB;

public class CsvDataBase<T> : IDataBaseRepository<T>
{
    private string path;

    public CsvDataBase(string path)
    {
        this.path = path;
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