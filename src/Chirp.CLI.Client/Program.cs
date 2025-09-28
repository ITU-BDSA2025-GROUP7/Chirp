using System.Net.Http.Headers;
using System.Net.Http.Json;
using Chirp.General;
using Chirp.CSVDB;
using DocoptNet;
using Microsoft.Extensions.Configuration;

namespace Chirp.CLI.Client {
   public static class Program
    {
        private const string Help = @"Chirp
Usage:
    -- read [<amount>]
    -- cheep <message>

Options:
    -h, --help  show this screen.
";

        private static string? baseURL;
        private static string? URLwithPort;
        
        public static int Main(string[] args)
        {
            var parser = Docopt.CreateParser(Help);
            return parser.Parse(args) switch
            {
                IArgumentsResult<IDictionary<string, ArgValue>>
                {
                    Arguments: var arguments
                } => Run(arguments),
                IHelpResult => ShowHelp(Help),
                IInputErrorResult { Usage: var usage } => OnError(usage),
                var result => throw new System.Runtime.CompilerServices.SwitchExpressionException(result)
            };
        }

        /// Send HTTP request to server to ask for a list of <see cref="Cheep"/>s.<br/>
        /// If <c>limit</c> is null, sends a Get request. Otherwise,
        /// sends a Post request along with a JSON-serialised <see cref="Limit"/>.
        private static async Task<IEnumerable<Cheep>> RequestCheepsFromServer(int? limit = null)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri(GetUrlWithPort());

            if (limit == null)
                return await client.GetFromJsonAsync<IEnumerable<Cheep>>("/cheeps") ?? [];
            if (limit.Value <= 0) return [];
            HttpResponseMessage response =
                await client.PostAsJsonAsync<Limit>("/cheeps", new Limit(limit.Value));
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<IEnumerable<Cheep>>().Result ?? [];
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

        /// Send post-request to server to store a new <see cref="Cheep"/>.
        private static async Task<string> MessageServer(Cheep cheep)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri(GetUrlWithPort());

            HttpResponseMessage response = await client.PostAsJsonAsync<Cheep>("/cheep", cheep);

            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        /// Starts the background process of sending a <see cref="Cheep"/> to the server
        /// by calling <see cref="MessageServer"/>, then prints the response to
        /// the standard output. <br/>
        /// Returns 0 if the HTTP request was successful, and 1 otherwise.
        private static int SendCheepToServer(string message)
        {
            // We HAVE TO get the Result property of the returned Task in order
            // for the Task inside the function call to complete.
            string response = MessageServer(Cheep.Assemble(message)).Result;
            Console.WriteLine(response);
            return 0;
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

        public static int Run(IDictionary<string, ArgValue> arguments) {
            if (!ValidateExactlyOneCommand(arguments)) return 1;

            if (arguments["cheep"].IsTrue && !arguments["<message>"].IsNone)
            {
                return SendCheepToServer(arguments["<message>"].ToString());
            }
            if (arguments["read"].IsTrue)
            {
                return ReadFromServer(arguments["<amount>"].ToString());
            }
            return 1;
        }
        
        /// Sets the port which the program will interact with. Used for testing.
        public static void SetPort(int port) {
            GetUrlWithPort(); // make sure the 
            URLwithPort = baseURL + port.ToString();
        }
        
        /**
         * Returns the URL of the website.
         * The URL might change depending on if the server is tarted with the environment variable ASPNETCORE_ENVIRONMENT set to test
         */
        public static string GetUrlWithPort()
        {
            if (URLwithPort != null)
            {
                return URLwithPort;
            }
            
            // Decide URL depending on environment 
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Client.json")
                .AddJsonFile($"appsettings.Client.{environment}.json", optional:true)
                .Build();
            
            baseURL = config["AppSettings:BaseURL"] ?? throw new InvalidOperationException("Confing BaseURL is missing.");
            if (environment == "Test") URLwithPort = baseURL + config["AppSettings:DefaultPort"];
            else URLwithPort = baseURL;
            
            return URLwithPort;
        }
    }
}