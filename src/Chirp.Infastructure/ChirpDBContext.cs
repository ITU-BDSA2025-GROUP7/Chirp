using Microsoft.EntityFrameworkCore;
using Chirp.Core.Domain_Model;

namespace Chirp.Infastructure;

public class ChirpDBContext:  DbContext
{
    public DbSet<Author> Authors { get; set; }
    public DbSet<Cheep> Cheeps { get; set; }

    public ChirpDBContext(DbContextOptions<ChirpDBContext> options) : base(options)
    {
        //DbInitializer.SeedDatabase(this);
    }
}