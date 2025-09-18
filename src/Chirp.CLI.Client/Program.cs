using System.Net.Http.Headers;
using System.Net.Http.Json;
using Chirp.CSVDB;
using DocoptNet;

namespace Chirp.CLI.Client {
   public static class Program
    {
        private const string Help = @"Chirp
Usage:
    -- read
    -- cheep <message>

Options:
    -h, --help  show this screen.
";

        private static string path = "chirp_cli_db.csv";

        public static int Main(string[] args)
        {
            var dataBase =  CsvDataBase<Cheep>.Instance;
            dataBase.SetPath(path);
            var parser = Docopt.CreateParser(Help);

            return parser.Parse(args) switch
            {
                IArgumentsResult<IDictionary<string, ArgValue>>
                {
                    Arguments: var arguments
                } => Run(arguments, dataBase),
                IHelpResult => ShowHelp(Help),
                IInputErrorResult { Usage: var usage } => OnError(usage),
                var result => throw new System.Runtime.CompilerServices.SwitchExpressionException(result)
            };
        }

        private static async Task<IEnumerable<Cheep>> ReadRequestServer()
        {
            const string baseURL = "http://localhost:5012";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri(baseURL);

            return await client.GetFromJsonAsync<IEnumerable<Cheep>>("cheeps") ?? [];
        }

        private static void Read(CsvDataBase<Cheep> dataBase)
        {
            var records = dataBase.Read();
            UserInterface.PrintCheeps(records);
        }
        private static void Write(string message, CsvDataBase<Cheep> dataBase)
        {
            message = "\"" + message + "\""; 
            string author = Environment.UserName;
            DateTimeOffset timeOffset = DateTimeOffset.UtcNow;
            long unixTime = timeOffset.ToUnixTimeSeconds();
            
            Cheep cheep = new Cheep(author, message , unixTime);
            
            dataBase.Store(cheep);
        }

        static int ShowHelp(string help) {Console.WriteLine(help); return 0;}

        static int OnError(string error) {Console.Error.WriteLine(error);return 1;}

        public static int Run(IDictionary<string, ArgValue> arguments, CsvDataBase<Cheep> dataBase)
        {
            if (arguments["read"].IsTrue)
            {
                if (arguments["cheep"].IsTrue) return 1; // cant cheep and read at the same time
                Task<IEnumerable<Cheep>> cheepTask = ReadRequestServer();
                IEnumerable<Cheep> cheeps = cheepTask.Result;
                UserInterface.PrintCheeps(cheeps);
                //Read(dataBase);
                return 0;
            }
            if (arguments["cheep"].IsTrue)
            {
                if (arguments["<message>"].IsNone) return 1;
                string message = arguments["<message>"].ToString();
                
                Write(message,  dataBase);
                return 0;
            }
            return 1;
        }
    }
}