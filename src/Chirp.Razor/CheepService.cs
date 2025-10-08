using System.Net.Http.Headers;
using Chirp.DBFacade;
using Chirp.Razor;
using Chirp.Razor.Domain_Model;
using Microsoft.EntityFrameworkCore;
using Cheep = Chirp.General.Cheep;


public interface ICheepService
{
    public Task<List<CheepDTO>> GetCheeps(int pageNr);
    public Task<List<CheepDTO>> GetCheepsFromAuthor(string author, int pageNr);
}

public class CheepService : ICheepService
{
    private static string? baseURL;
    private static string? URLwithPort;
    private DBFacade<Cheep> db;
    private ChirpDBContext dbContext;


    // default constructor
    public CheepService(ChirpDBContext dbContext)
    {
        db = DBFacade<Cheep>.Instance;
        this.dbContext = dbContext;
		
    }
    

    /**
     * Calls on the Services to get all cheeps within the given page nr
     */
    public async Task<List<CheepDTO>> GetCheeps(int pageNr)
    {
        
        var query = (from cheep in dbContext.Cheeps
            orderby cheep.TimeStamp descending
            select cheep)
            .Skip((pageNr - 1) * 32).Take(32).Select(cheep => 
                new CheepDTO(cheep.Author.Name, cheep.Text, cheep.TimeStamp.ToString()));

        return await query.ToListAsync();
    }

    /**
     * Calls on the Services to get all cheeps within the given page nr that have the given author
     */
    public async Task<List<CheepDTO>> GetCheepsFromAuthor(string author, int pageNr)
    {
        var query = (from cheep in dbContext.Cheeps
                where cheep.Author.Name == author
                orderby cheep.TimeStamp descending
                select new CheepDTO(cheep.Author.Name, cheep.Text, cheep.TimeStamp.ToString()))
            .Skip((pageNr - 1) * 32).Take(32);

        return await query.ToListAsync();
    }



    
    private static string UnixTimeStampToDateTimeString(long unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime.ToString("MM/dd/yy H:mm:ss");
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