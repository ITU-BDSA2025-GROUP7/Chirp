using Chirp.Core;
using Chirp.Core.Domain_Model;
using Chirp.Infastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.Web.json")
    .AddJsonFile($"appsettings.Web.{environment}.json", optional: true)
    .Build();

// use in memory database for testing
if (environment.Equals("Test"))
{
    Console.WriteLine("We are running in Test mode");
    var connection = new SqliteConnection("DataSource=:memory:");
    connection.Open();
    builder.Services.AddDbContext<ChirpDBContext>();
    builder.Services.AddDbContext<ChirpDBContext>(options => options.UseSqlite(connection));
}
else
{
    string? connectionString = config["ConnectionStrings:DefaultConnection"];
    builder.Services.AddDbContext<ChirpDBContext>(options => options.UseSqlite(connectionString));
}

builder.Services.AddScoped<ICheepRepository, CheepRepository>();
builder.Services.AddScoped<ICheepService, CheepService>();
builder.Services.AddDefaultIdentity<Author>(options =>
    options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ChirpDBContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    /* moved the seeding of the db initializer out of
        the chirpDBContext so it is possible to use a
        different test database*/
    var dbContext = scope.ServiceProvider.GetRequiredService<ChirpDBContext>();
    DbInitializer.SeedDatabase(dbContext);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();

public partial class Program { }