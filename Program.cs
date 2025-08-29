using System.IO;

namespace Chirp {
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "read")
            {
                string[] chirps = File.ReadAllLines("chirp_cli_db.csv");
                for (int i = 1; i < chirps.Length; i++)
                {
                    int indexOfFirstComma = chirps[i].IndexOf(",");
                    int indexOfLastComma = chirps[i].LastIndexOf(",");
                    string author = chirps[i].Substring(0, indexOfFirstComma);
                    string message = chirps[i].Substring(indexOfFirstComma + 2, indexOfLastComma - author.Length - 3);
                    string timestamp = chirps[i].Substring(indexOfLastComma + 1);
    
                    DateTime time = new DateTime(1970, 1, 1);
                    time = time.AddSeconds(int.Parse(timestamp));
                    time = time.ToLocalTime();
                
                    Console.WriteLine(author + " @ " + time + ": " + message);
                }
            }
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
    }
}