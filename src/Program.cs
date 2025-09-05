using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace Chirp {
    class Program
    {

        private static string path = "chirp_cli_db.csv";
        static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "read") Read();
            else if (args.Length == 2 && args[0] == "cheep") Write(args[1]);
        }
        private static void Read()
        {
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<Cheep>();
                foreach (var record in records)
                {
                    Console.WriteLine(record);
                }
            }
        }

        private static void Write(string message)
        {
            message = "\"" + message + "\""; 
            string author = Environment.UserName;
            DateTimeOffset timeOffset = DateTimeOffset.UtcNow;
            long unixTime = timeOffset.ToUnixTimeSeconds();
            
            Cheep cheep = new Cheep(author, message , unixTime);
            
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
                csv.WriteRecord(cheep);
                csv.NextRecord();
            }
        }
    }
    
}