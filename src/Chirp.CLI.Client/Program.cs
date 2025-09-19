using System.Net.Http.Headers;
using System.Net.Http.Json;
using Chirp.General;
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

        /// Boxes the integer for automatic JSON serialisation.
        private struct Limit (int val) {
            public int limit = val;
        }

        /// Send HTTP request to server to ask for a list of <see cref="Cheep"/>s.<br/>
        /// If <c>limit</c> is null, sends a Get request. Otherwise,
        /// sends a Post request along with a JSON-serialised <see cref="Limit"/>.
        private static async Task<IEnumerable<Cheep>> RequestCheepsFromServer(int? limit = null)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.Clear();
            client.DefaultRequestHeaders.UserAgent.Add(new  ProductInfoHeaderValue("Chirp", "0.5.0"));
            client.BaseAddress = new Uri(baseURL);

            if (limit.HasValue)
            {
                if (limit <= 0) return [];
                 HttpResponseMessage response =
                     await client.PostAsJsonAsync<Limit>("cheeps", new Limit(limit.Value));
                 var task = await response.Content.ReadFromJsonAsync<IEnumerable<Cheep>>();
                 return task ?? []; // If null, returns empty array
            }
            return await client.GetFromJsonAsync<IEnumerable<Cheep>>("cheeps") ?? [];
        }

        /// Starts the background process of getting a list of <see cref="Cheep"/>s from the
        /// server by calling <see cref="RequestCheepsFromServer"/>,
        /// and prints out the result.<br/>
        /// Returns 0 if successful, and 1 otherwise.
        private static int ReadFromServer(string? amount = null)
        {
            Task<IEnumerable<Cheep>> cheepTask;
            if (string.IsNullOrEmpty(amount))
            {
                 cheepTask = RequestCheepsFromServer();
            } else if (int.TryParse(amount, out int intVal)
                       && intVal > 0)
            {
                cheepTask = RequestCheepsFromServer(intVal);
            }
            else
            {
                return 1;
            }
            IEnumerable<Cheep> cheeps = cheepTask.Result;
            UserInterface.PrintCheeps(cheeps);
            return 0;
        }

        private static void Read(CsvDataBase<Cheep> dataBase, int? limit = null)
        {
            var records = dataBase.Read(limit);
            UserInterface.PrintCheeps(records);
        }

        private static void Write(string message, CsvDataBase<Cheep> database)
        {
            database.Store(Cheep.Assemble(message));
        }

        /// Send post-request to server to store a new <see cref="Cheep"/>.
        private static async Task<string> MessageServer(Cheep cheep)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(baseURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.Clear();
            client.DefaultRequestHeaders.UserAgent.Add(new  ProductInfoHeaderValue("Chirp", "0.5.0"));

            using HttpResponseMessage response = await client.PostAsJsonAsync("cheep", cheep);
            response.EnsureSuccessStatusCode();

            string acknowledge = await response.Content.ReadAsStringAsync();
            Console.WriteLine(acknowledge);
            return acknowledge;
        }

        /// Starts the background process of sending a <see cref="Cheep"/> to the server
        /// by calling <see cref="MessageServer"/>, then prints the response to
        /// the standard output. <br/>
        /// Returns 0 if the HTTP request was successful, and 1 otherwise.
        private static int SendCheepToServer(string message)
        {
            Task<string> task = MessageServer(Cheep.Assemble(message));
            if (task.IsCompletedSuccessfully)
            {
                Console.WriteLine(task.Result);
                return 0;
            }
            Console.WriteLine(task.Exception);
            return 1;
        }

        private static int ShowHelp(string help) {
            Console.WriteLine(help); return 0;
        }

        private static int OnError(string error) {
            Console.Error.WriteLine(error); return 1;
        }

        /// Returns true if the input dictionary has exactly one value which is true.
        private static bool ValidateExactlyOneCommand(IDictionary<string, ArgValue> arguments) {
            var seenTrue = false;
            foreach (KeyValuePair<string, ArgValue> pair in arguments) {
                if (pair.Value.IsTrue && seenTrue) return false;
                if (pair.Value.IsTrue && !seenTrue) seenTrue = true;
            }
            return seenTrue;
        }

        public static int Run(IDictionary<string, ArgValue> arguments, CsvDataBase<Cheep> dataBase) {
            if (!ValidateExactlyOneCommand(arguments)) return 1;

            if (arguments["post"].IsTrue && !arguments["<message>"].IsNone)
            {
                return SendCheepToServer(arguments["<message>"].ToString());
            }
            if (arguments["get"].IsTrue)
            {
                return ReadFromServer(arguments["<amount>"].ToString());
            }
            if (arguments["read"].IsTrue)
            {
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
            }
            else if (arguments["cheep"].IsTrue && !arguments["<message>"].IsNone)
            {
                string message = arguments["<message>"].ToString();
                Write(message, dataBase);
                return 0;
            }
            return 1;
        }
    }
}