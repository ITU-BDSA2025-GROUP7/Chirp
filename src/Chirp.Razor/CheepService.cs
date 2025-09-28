using System.Net.Http.Headers;
using System.Net.Http.Json;
using Chirp.General;


public record CheepViewModel(string Author, string Message, string Timestamp);

public interface ICheepService
{
    public Task<List<CheepViewModel>> GetCheeps(int pageNr);
    public Task<List<CheepViewModel>> GetCheepsFromAuthor(string author, int pageNr);
}

public class CheepService : ICheepService
{
    private static string? baseURL;
    private static string? URLwithPort;
    private readonly HttpClient _httpClient;

    CheepService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    

    private static readonly List<CheepViewModel>? _cheeps = new();

    public async Task<List<CheepViewModel>> GetCheeps(int pageNr)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.BaseAddress = new Uri(GetUrlWithPort());

        var cheeps = await client.GetFromJsonAsync<List<Cheep>>("/cheepsWithPage" + "?page=" + pageNr) ??
                     new List<Cheep>();
        ;
        return cheeps.Select(c => new CheepViewModel(
            c.Author,
            c.Message,
            UnixTimeStampToDateTimeString(c.Timestamp)
        )).ToList();
    }

    public async Task<List<CheepViewModel>> GetCheepsFromAuthor(string author, int pageNr)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.BaseAddress = new Uri(GetUrlWithPort());
        
        var cheeps = await client.GetFromJsonAsync<List<Cheep>>("/cheepsWithPageFromUser" +"?auther=" + author + "&page="+pageNr) ?? new List<Cheep>();;
        return cheeps.Select(c => new CheepViewModel(
            c.Author,
            c.Message,
            UnixTimeStampToDateTimeString(c.Timestamp)
        )).ToList();
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


    private static string UnixTimeStampToDateTimeString(long unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime.ToString("MM/dd/yy H:mm:ss");
    }

    public static string GetUrlWithPort()
    {
        if (URLwithPort != null)
        {
            return URLwithPort;
        }

        // Decide URL depending on environment 
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Razor.json")
            .AddJsonFile($"appsettings.Razor.{environment}.json", optional: true)
            .Build();

        baseURL = config["AppSettings:BaseURL"] ?? throw new InvalidOperationException("Confing BaseURL is missing.");
        if (environment == "Test") URLwithPort = baseURL + config["AppSettings:DefaultPort"];
        else URLwithPort = baseURL;
        Console.WriteLine("Listening on: " + URLwithPort);

        return URLwithPort;
    }
}