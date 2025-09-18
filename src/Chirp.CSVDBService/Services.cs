using Chirp.CSVDB;
using Chirp.General;

public class Services
{
    public static void Main(string[] args)
    {
        new Services();
    }
    private Services()
    {
        // Setup database
        var db = CsvDataBase<Cheep>.Instance;
        db.SetPath("chirp_cli_db.csv"); // im not sure if this is the best place for the .csv file

        // setup app
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

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
}

