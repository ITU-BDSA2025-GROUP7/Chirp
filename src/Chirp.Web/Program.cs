using Chirp.Core;
using Chirp.Core.Domain_Model;
using Chirp.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.Web.json")
    .AddJsonFile($"appsettings.Web.{environment}.json", optional: true)
    .Build();

// use in memory database for testing
if (environment.Equals("Development")) {
    Console.WriteLine("We are running in Development mode");
    var connection = new SqliteConnection("DataSource=:memory:");
    connection.Open();
    builder.Services.AddDbContext<ChirpDBContext>();
    builder.Services.AddDbContext<ChirpDBContext>(options => options.UseSqlite(connection));
} else {
    string? connectionString = config["ConnectionStrings:DefaultConnection"];
    builder.Services.AddDbContext<ChirpDBContext>(options => options.UseSqlite(connectionString));
}

builder.Services.AddScoped<ICheepRepository, CheepRepository>();
builder.Services.AddScoped<ICheepService, CheepService>();
builder.Services.AddDefaultIdentity<Author>(options => {
            options.SignIn.RequireConfirmedAccount = true;
        })
       .AddEntityFrameworkStores<ChirpDBContext>();
builder.Services.AddAuthentication(options => {
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    options.DefaultChallengeScheme = "GitHub";
})
.AddCookie(o => {
    o.LoginPath = "/Identity/Account/Login";
    o.LogoutPath = "/Identity/Account/Logout";
})
.AddGitHub(o => {
    o.ClientId = builder.Configuration["authenticationGitHubClientId"]
              ?? throw new InvalidOperationException();
    o.ClientSecret = builder.Configuration["authenticationGitHubClientSecret"]
                  ?? throw new InvalidOperationException();
    o.Scope.Add("user:email");
});
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (IServiceScope scope = app.Services.CreateScope()) {
    /* moved the seeding of the db initializer out of
    the chirpDBContext so it is possible to use a
    different test database*/
    var dbContext = scope.ServiceProvider.GetRequiredService<ChirpDBContext>();
    DbInitializer.SeedDatabase(dbContext);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();

public partial class Program { }
