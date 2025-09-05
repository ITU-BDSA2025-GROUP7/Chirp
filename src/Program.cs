using System.Globalization;
using System.IO;
using CsvHelper;

namespace Chirp {
    class Program
    {

        private static string path = "chirp_cli_db.csv";
        static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "read") Read();
            
            else if (args.Length == 2 && args[0] == "cheep")
            {
                string author = Environment.UserName;
                DateTimeOffset timeOffset = DateTimeOffset.UtcNow;
                long unixTime = timeOffset.ToUnixTimeSeconds();
                Console.WriteLine(unixTime);
                StreamWriter writer = File.AppendText("chirp_cli_db.csv");
                writer.WriteLine(author + ",\"" + args[1] + "\"," + unixTime);
                writer.Flush();
                writer.Close();
            };
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
        
        
    }
    
}