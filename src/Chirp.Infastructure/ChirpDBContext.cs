using Chirp.Core.Domain_Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infastructure;

public class ChirpDBContext : IdentityDbContext<Author> {
    public DbSet<Author> Authors { get; set; }
    public DbSet<Cheep> Cheeps { get; set; }

    public ChirpDBContext(DbContextOptions<ChirpDBContext> options) : base(options) {
        this.Database.EnsureCreated();
        //DbInitializer.SeedDatabase(this);
    }
}