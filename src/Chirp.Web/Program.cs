using Chirp.Core;
using Chirp.Core.Domain_Model;
using Chirp.Infastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                  ?? throw new InvalidOperationException();
var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Web.json")
            .AddJsonFile($"appsettings.Web.{environment}.json", optional: true)
            .Build();

string? connectionString = config["ConnectionStrings:DefaultConnection"];
builder.Services.AddDbContext<ChirpDBContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<ICheepRepository, CheepRepository>();
builder.Services.AddScoped<ICheepService, CheepService>();
//builder.Services.AddDefaultIdentity<Author>(options =>
//                                                options.SignIn.RequireConfirmedAccount = true)
//       .AddEntityFrameworkStores<ChirpDBContext>();
builder.Services.AddAuthentication(options => {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            options.DefaultChallengeScheme = "GitHub";
        })
       .AddCookie(o => {
            o.LoginPath = "/signin";
            o.LogoutPath = "/signout";
        })
       .AddGitHub(o => {
            o.ClientId = builder.Configuration["Authentication:GitHub:ClientId"]
                      ?? throw new InvalidOperationException();
            o.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"]
                          ?? throw new InvalidOperationException();
            o.CallbackPath = "/signin-github";
            o.Scope.Add("user:email");
        })
       .AddIdentityCookies(o => { });
builder.Services.AddIdentityCore<Author>(o => {
            o.Stores.MaxLengthForKeys = 128;
            o.SignIn.RequireConfirmedAccount = true;
        })
       .AddDefaultUI()
       .AddDefaultTokenProviders()
       .AddEntityFrameworkStores<ChirpDBContext>();
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope()) { /* moved the seeding of the db initializer out of
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