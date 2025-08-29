using System.IO;

namespace Chirp {
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "read")
            {
                string[] chirps = File.ReadAllLines("chirp_cli_db.csv");
                for (int i = 1; i < chirps.Length; i++)
                {
                    int indexOfFirstComma = chirps[i].IndexOf(",");
                    int indexOfLastComma = chirps[i].LastIndexOf(",");
                    string author = chirps[i].Substring(0, indexOfFirstComma);
                    string message = chirps[i].Substring(indexOfFirstComma + 2, indexOfLastComma - 7);
                    string timestamp = chirps[i].Substring(indexOfLastComma + 1);
    
                    DateTime time = new DateTime(1970, 1, 1);
                    time = time.AddSeconds(int.Parse(timestamp));
                
                    Console.WriteLine(author + " @ " + time + ": " + message);
                }
            }
        }
    }
}