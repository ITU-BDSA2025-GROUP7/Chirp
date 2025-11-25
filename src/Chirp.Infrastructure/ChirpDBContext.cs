using Chirp.Core.Domain_Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure;

public class ChirpDBContext : IdentityDbContext<Author> {
    public DbSet<Author> Authors { get; set; }
    public DbSet<Cheep> Cheeps { get; set; }
    public DbSet<FollowRelation> FollowRelations { get; set; }

    public ChirpDBContext(DbContextOptions<ChirpDBContext> options) : base(options) {
        this.Database.EnsureCreated();
        //DbInitializer.SeedDatabase(this);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Author>()
        .HasMany(a => a.followerRelations)
        .WithOne(fr => fr.Follower)
        .OnDelete(DeleteBehavior.ClientCascade);

    modelBuilder.Entity<Author>()
        .HasMany(a => a.followedRelations)
        .WithOne(fd => fd.Followed)
        .OnDelete(DeleteBehavior.ClientCascade);;
    }
}