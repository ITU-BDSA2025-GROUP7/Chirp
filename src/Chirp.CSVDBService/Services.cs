using Chirp.CSVDB;

// var db = Chirp.CSVDB.CsvDataBase<>

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();



app.MapGet("/cheeps", () =>
{
    return "hello cheeps";

});

app.Run();
