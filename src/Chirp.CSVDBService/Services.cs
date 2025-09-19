using Chirp.CSVDB;
using Chirp.General;

namespace Chirp.CSVDBService;

public class Services
{
    // required to start the server
    public static void Main(string[] args)
    {
        new Services();
    }

    private WebApplication app;
    public Services()
    {
        // Setup database
        var db = CsvDataBase<Cheep>.Instance;
        if (File.Exists("chirp_cli_db.csv")) db.SetPath("chirp_cli_db.csv"); 
        else if  (File.Exists("chirp_cli_db_Testing.csv")) db.SetPath("chirp_cli_db_Testing.csv");

        // setup app
        var builder = WebApplication.CreateBuilder();
        app = builder.Build();
        app.MapGet("/cheeps", () => db.Read(null));
        app.MapPost("/cheep", (Cheep cheep) =>
        {
            db.Store(cheep);
            return new { Message = "Cheep stored." };
        });
        //app.MapPost("/cheep", (Cheep cheep) => db.Store(cheep));
        app.Run("http://localhost:5012");
    }
}

