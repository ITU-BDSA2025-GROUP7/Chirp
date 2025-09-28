using Chirp.CSVDB;
using Chirp.General;
using Microsoft.Extensions.Primitives;

namespace Chirp.CSVDBService;

public class Services
{
    private int PAGE_SIZE = 3;
    
    // required to start the server
    public static void Main(string[] args)
    {
        if (args.Length == 1)
        {
            new Services("http://localhost:" + args[0]);
        }
        else
        {
            new Services();
        }
    }

    private WebApplication app;
    
    public Services(string? port = null)
    {
        if (port != null && !port.StartsWith("http://localhost:")) {
            port = "http://localhost:" + port;
        }
        
        // setup configs 
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Services.json")
            .AddJsonFile($"appsettings.Services.{environment}.json", optional:true)
            .Build();
        
        // Setup database
        var db = CsvDataBase<Cheep>.Instance;
        db.SetPath(config["AppSettings:DatabaseLocation"]);

        // setup app
        var builder = WebApplication.CreateBuilder();
        app = builder.Build();
        app.MapGet("/cheeps", () => db.Read(null));
        app.MapPost("/cheeps", (Limit limit) => db.Read(limit.Val));
        app.MapPost("/cheep", (Cheep cheep) =>
        {
            db.Store(cheep);
            return "Cheep stored.";
        });
        
        //Temporary Api's for development that should no longer be here after issue #44 is fixed
        app.MapGet("/cheepsWithPage", (HttpRequest request) =>
        {
            StringValues pageQuery = request.Query["page"];
            int pageNr;
            int.TryParse(pageQuery, out pageNr);
            if  (pageNr == 0) pageNr = 1; // if parsing failed, set page number to 1 as requested by session_05 1.b)

            // temperary solution until SQL is added
            var cheeps = db.Read(null);
            int startIndex = (pageNr -1) * PAGE_SIZE;
            var enumerable = cheeps.ToList();
            var returnCheeps = enumerable.Skip(startIndex).Take(PAGE_SIZE);
            
            Console.WriteLine("Gving page nr:" + pageNr + " A total of " + returnCheeps.Count() + " was returned.");
            
            return returnCheeps;
        });
        app.MapGet("/cheepsWithPageFromUser", (HttpRequest request) =>
        {
            StringValues pageQuery = request.Query["page"];
            int pageNr;
            int.TryParse(pageQuery, out pageNr);
            if  (pageNr == 0) pageNr = 1; // if parsing failed, set page number to 1 as requested by session_05 1.b)
            
            StringValues auther = request.Query["auther"];

            // temperary solution until SQL is added
            var cheeps = db.Read(null);
            var cheepsWithNameRestriction = cheeps.Where(x => x.Author == auther);
            int startIndex = (pageNr -1) * PAGE_SIZE;
            var returnCheeps = cheepsWithNameRestriction.Skip(startIndex).Take(PAGE_SIZE);
            
            Console.WriteLine("Gving page nr:" + pageNr + " With user: " + auther + " A total of " + returnCheeps.Count() + " was returned.");
            
            return returnCheeps;
        });
        
        
        app.Run(port);
    }
}

