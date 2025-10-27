using Chirp.Core;
using Microsoft.EntityFrameworkCore;
using Chirp.Core.Domain_Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Chirp.Infastructure;

public class ChirpDBContext:  IdentityDbContext<ApplicationUser>
{
    public DbSet<Author> Authors { get; set; }
    public DbSet<Cheep> Cheeps { get; set; }
    
    
    
    public ChirpDBContext(DbContextOptions<ChirpDBContext> options) : base(options)
    {
        //DbInitializer.SeedDatabase(this);
    }
}