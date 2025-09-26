using Chirp.DBFacade;
using Chirp.General;

namespace Chirp.CSVDBService;

public class Services {
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
        var db = DBFacade<Cheep>.Instance;

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
        app.Run(port);
    }
}

