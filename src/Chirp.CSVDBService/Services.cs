using System.Collections.ObjectModel;
using Chirp.CSVDB;
using Chirp.General;

namespace Chirp.CSVDBService;

public class Services
{
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

        app.MapGet("/cheeps", () =>
        {
            return db.Read();
        });

        app.MapPost("/cheep", (String message) =>
        {
            Console.WriteLine("i have recived a message: ", message);
        });

        app.Run();
    }

    public bool test()
    {
        return true;
    }
}

