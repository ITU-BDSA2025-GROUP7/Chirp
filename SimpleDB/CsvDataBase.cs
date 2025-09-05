using System.Globalization;
using Chirp;
using CsvHelper;

namespace SimpleDB;

public sealed class CsvDataBase<T> : IDataBaseRepository<T>
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
        
    }
}