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

    modelBuilder.Entity<FollowRelation>()
        .HasOne(fr => fr.Follower)
        .WithMany(u => u.followerRelations)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<FollowRelation>()
        .HasOne(fr => fr.Followed)
        .WithMany(u => u.followedRelations)
        .OnDelete(DeleteBehavior.Cascade);;
    }
}