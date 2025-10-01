using System.Web;
using Chirp.DBFacade;
using Chirp.General;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;

namespace Chirp.CSVDBService;

public class Services : IDisposable, IAsyncDisposable {
    
    public static int PAGE_SIZE = 32;
    
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
    private DBFacade<Cheep> db;
    
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
        db = DBFacade<Cheep>.Instance;

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
        app.MapGet("/cheepsPage", (HttpRequest request) =>
        {
            // parse the page variable from the HttpRequest
            StringValues pageQuery = request.Query["page"];
            int pageNr;
            int.TryParse(pageQuery, out pageNr);
            if  (pageNr == 0) pageNr = 1; // if parsing failed, set page number to 1 as requested by session_05 1.b)
            
            StringValues author = request.Query["author"];
            IEnumerable<Cheep> result;
            if (author.ToString().Equals("")) result = db.ReadPage(pageNr);
            else result = db.ReadPage(pageNr, author);
            return result;
            
        });
        
        
        app.Run(port);
    }

    public void Dispose() {
        db.Dispose();
        ((IDisposable)app).Dispose();
    }

    public async ValueTask DisposeAsync() {
        db.Dispose();
        await app.DisposeAsync();
    }
}

