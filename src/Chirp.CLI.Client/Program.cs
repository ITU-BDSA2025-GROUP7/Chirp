using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Chirp.CSVDB;
using DocoptNet;

namespace Chirp.CLI.Client {
   public static class Program
    {
        private const string Help = @"Chirp
Usage:
    -- read [<amount>]
    -- cheep <message>
    -- get [<amount>]
    -- post <message>

Options:
    -h, --help  show this screen.
";
        const string baseURL = "http://localhost:5012";
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

        private static async Task<IEnumerable<Cheep>> ReadRequestServer(string? limit = "")
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri(baseURL);

            return await client.GetFromJsonAsync<IEnumerable<Cheep>>("cheeps") ?? [];
        }

        private static void Read(CsvDataBase<Cheep> dataBase, int? limit = null)
        {
            var records = dataBase.Read(limit);
            UserInterface.PrintCheeps(records);
        }

        private static Cheep AssembleCheep(string message)
        {
            message = "\"" + message + "\"";
            string author = Environment.UserName;
            DateTimeOffset timeOffset = DateTimeOffset.UtcNow;
            long unixTime = timeOffset.ToUnixTimeSeconds();

            return new Cheep(author, message , unixTime);
        }

        private static void Write(string message, CsvDataBase<Cheep> database)
        {
            database.Store(AssembleCheep(message));
        }

        private static async void MessageServer(Cheep cheep)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(baseURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using HttpResponseMessage response = await client.PostAsJsonAsync("cheep", cheep);

            response.EnsureSuccessStatusCode();

            string acknowledge = await response.Content.ReadAsStringAsync();
            Console.WriteLine(acknowledge);
        }

        private static void Post(string message)
        {
            MessageServer(AssembleCheep(message));
        }

        static int ShowHelp(string help) {Console.WriteLine(help); return 0;}

        static int OnError(string error) {Console.Error.WriteLine(error);return 1;}

        public static int Run(IDictionary<string, ArgValue> arguments, CsvDataBase<Cheep> dataBase)
        {
            if (arguments["post"].IsTrue)
            {
                if (arguments["<message>"].IsNone)
                {
                    Post(arguments["<message>"].ToString());
                    return 0;
                }

                return 1;
            }

            if (arguments["get"].IsTrue)
            {
                Task<IEnumerable<Cheep>> cheepTask;
                if (arguments["<amount>"].IsNone)
                {
                     cheepTask = ReadRequestServer();
                }
                else
                {
                    cheepTask = ReadRequestServer(arguments["<amount>"].ToString());
                }
                IEnumerable<Cheep> cheeps = cheepTask.Result;
                UserInterface.PrintCheeps(cheeps);
            }
            if (arguments["read"].IsTrue)
            {
                if (arguments["cheep"].IsTrue) return 1; // cant cheep and read at the same time
                if (arguments["<amount>"].IsNone)
                {
                    Read(dataBase);
                    return 0;
                }
                bool isInt = int.TryParse(arguments["<amount>"].ToString(), out int intVal);
                if (isInt && intVal >0)
                {
                    Read(dataBase, intVal);
                    return 0;
                }
                return 1;
            }
            if (arguments["cheep"].IsTrue)
            {
                if (arguments["<message>"].IsNone) return 1;
                string message = arguments["<message>"].ToString();
                Write(message, dataBase);
                return 0;
            }
            return 1;
        }
    }
}