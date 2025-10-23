using Chirp.Core;
using Chirp.Infastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.Web.json")
    .AddJsonFile($"appsettings.Web.{environment}.json", optional:true)
    .Build();

string? connectionString = config["ConnectionStrings:DefaultConnection"];
builder.Services.AddDbContext<ChirpDBContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<ICheepRepository, CheepRepository>();
builder.Services.AddScoped<ICheepService, CheepService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
using (var scope = app.Services.CreateScope())
{  /* moved the seeding of the db initializer out of 
	the chirpDBContext so it is possible to use a 
	different test database*/
    var dbContext = scope.ServiceProvider.GetRequiredService<ChirpDBContext>();
    DbInitializer.SeedDatabase(dbContext);
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.Run();