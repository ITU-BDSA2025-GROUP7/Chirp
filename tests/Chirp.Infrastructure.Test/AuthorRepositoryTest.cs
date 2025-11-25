using Chirp.Core;
using Chirp.Core.Domain_Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Chirp.Infrastructure.Test;

public class AuthorRepositoryTest {
    private readonly ChirpDBContext _context;
    private SqliteConnection _connection;
    private ICheepRepository _cheepRepository;
    private IAuthorRepository _authorRepository;

    public AuthorRepositoryTest() {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<ChirpDBContext>()
            .UseSqlite(_connection)
            .Options;
        _context = new ChirpDBContext(options);
        _context.Database.EnsureCreated();

        _cheepRepository = new CheepRepository(_context);
        _authorRepository = new AuthorRepository(_context);

        DbInitializer.SeedDatabase(_context);

        _context.SaveChanges();
    }
    [Fact]
    public async Task attemptToFollowSelf() {
        //arrange
        const string name = "Barton Cooper";
        string username = name.Replace(" ", "");
        const string email1 = "TheCakeMaster@copper.com";
        await _authorRepository.CreateAuthor(name, email1);
        List<Author> authors = await _authorRepository.GetAuthor("BartonCooper");
        Author barton = authors.Single();
        //act
        _ = _authorRepository.Follow(barton, barton);
        //assert
        Assert.Empty(await _authorRepository.GetFollowRelations(barton));
    }

    [Fact]
    public async Task attemptToFollowSomeone() {
        //arrange
        const string name = "Barton Cooper";
        string username = name.Replace(" ", "");
        const string email1 = "TheCakeMaster@copper.com";
        await _authorRepository.CreateAuthor(name, email1);
        List<Author> authors1 = await _authorRepository.GetAuthor("WendellBallan");
        Author Wendell = authors1.Single();
        List<Author> authors2 = await _authorRepository.GetAuthor("BartonCooper");
        Author barton = authors2.Single();
        //act
        _ = _authorRepository.Follow(barton, Wendell);
        //assert
        Assert.NotEmpty(await _authorRepository.GetFollowRelations(barton));
    }

    [Fact]
    public async Task attemptToFollowSomeoneAlreadyFollowed() {
        //arrange
        const string name = "Barton Cooper";
        string username = name.Replace(" ", "");
        const string email1 = "TheCakeMaster@copper.com";
        await _authorRepository.CreateAuthor(name, email1);
        List<Author> authors1 = await _authorRepository.GetAuthor("WendellBallan");
        Author Wendell = authors1.Single();
        List<Author> authors2 = await _authorRepository.GetAuthor("BartonCooper");
        Author barton = authors2.Single();
        //act
        _ = _authorRepository.Follow(barton, Wendell);
        _ = _authorRepository.Follow(barton, Wendell);
        List<Author> myList = await _authorRepository.Following(barton);
        //assert
        Assert.Single(myList);
    }

    [Fact]
    public async Task attemptToUnfollowSomeoneFollowed() {
        //arrange
        const string name = "Barton Cooper";
        string username = name.Replace(" ", "");
        const string email1 = "TheCakeMaster@copper.com";
        await _authorRepository.CreateAuthor(name, email1);
        List<Author> authors1 = await _authorRepository.GetAuthor("WendellBallan");
        Author Wendell = authors1.Single();
        List<Author> authors2 = await _authorRepository.GetAuthor("BartonCooper");
        Author barton = authors2.Single();
        //act
        _ = _authorRepository.Follow(barton, Wendell);
        _ = _authorRepository.Unfollow(barton, Wendell);
        //assert
        Assert.Empty(await _authorRepository.GetFollowRelations(barton));
    }

    [Fact]
    public async Task attemptToUnfollowSomeoneNotFollowed() {
        //arrange
        const string name = "Barton Cooper";
        string username = name.Replace(" ", "");
        const string email1 = "TheCakeMaster@copper.com";
        await _authorRepository.CreateAuthor(name, email1);
        List<Author> authors1 = await _authorRepository.GetAuthor("WendellBallan");
        Author Wendell = authors1.Single();
        List<Author> authors2 = await _authorRepository.GetAuthor("BartonCooper");
        Author barton = authors2.Single();
        //act
        _ = _authorRepository.Unfollow(barton, Wendell);
        //assert
        Assert.Empty(await _authorRepository.GetFollowRelations(barton));
    }

    [Fact]
    public async Task followNull() {
        //arrange
        const string name = "Barton Cooper";
        string username = name.Replace(" ", "");
        const string email1 = "TheCakeMaster@copper.com";
        await _authorRepository.CreateAuthor(name, email1);
        List<Author> authors2 = await _authorRepository.GetAuthor("BartonCooper");
        Author barton = authors2.Single();
        Author? Wendell = null;
        //act
#pragma warning disable CS8604 // Possible null reference argument.
        _ = _authorRepository.Follow(barton, Wendell);
        //assert
        Assert.Null(Wendell);
        Assert.Empty(await _authorRepository.GetFollowRelations(barton));
    }
    [Fact]
    public async Task followAuthorNotInDBContext() {
        //arrange
        const string name = "Barton Cooper";
        string username = name.Replace(" ", "");
        const string email1 = "TheCakeMaster@copper.com";
        await _authorRepository.CreateAuthor(name, email1);
        List<Author> authors2 = await _authorRepository.GetAuthor("BartonCooper");
        Author barton = authors2.Single();
        Author myAuthor = Author.Create("Bartoon2", "batman@gmail.com");
        //act
        _ = _authorRepository.Follow(barton, myAuthor);
        //assert
        Assert.Empty(await _authorRepository.GetFollowRelations(barton));
    }

    /**
     * Test whether Isfollowing behaves as intended when someone follows
     */
    [Fact]
    public async Task FollowTest() {
        //arrange
        const string nameA = "Barton Cooper";
        string usernameA = nameA.Replace(" ", "");
        const string emailA = "TheCakeMaster@copper.com";
        await _authorRepository.CreateAuthor(nameA, emailA);
        Author authorA = (await _authorRepository.GetAuthor(usernameA)).First();
        const string nameB = "Abba Booper";
        string usernameB = nameB.Replace(" ", "");
        const string emailB = "Abba@Booper.com";
        await _authorRepository.CreateAuthor(nameB, emailB);
        Author authorB = (await _authorRepository.GetAuthor(usernameB)).First();

        // act
        var AFollowBBefore = await _authorRepository.IsFollowing(authorA, authorB);
        var BFollowABefore = await _authorRepository.IsFollowing(authorB, authorA);
        await _authorRepository.Follow(authorA, authorB);
        var AFollowBAfter = await _authorRepository.IsFollowing(authorA, authorB);
        var BFollowAAfter = await _authorRepository.IsFollowing(authorB, authorA);

        // Assert
        Assert.False(AFollowBBefore);
        Assert.False(BFollowABefore);
        Assert.True(AFollowBAfter);
        Assert.False(BFollowAAfter);
    }

    /**
     * Test whether Isfollowing behaves as intended when someone unfollows
     */
    [Fact]
    public async Task UnfollowTest() {
        //arrange
        const string nameA = "Barton Cooper";
        string usernameA = nameA.Replace(" ", "");
        const string emailA = "TheCakeMaster@copper.com";
        await _authorRepository.CreateAuthor(nameA, emailA);
        Author authorA = (await _authorRepository.GetAuthor(usernameA)).First();
        const string nameB = "Abba Booper";
        string usernameB = nameB.Replace(" ", "");
        const string emailB = "Abba@Booper.com";
        await _authorRepository.CreateAuthor(nameB, emailB);
        Author authorB = (await _authorRepository.GetAuthor(usernameB)).First();
        await _authorRepository.Follow(authorA, authorB);

        // act
        var AFollowBBefore = await _authorRepository.IsFollowing(authorA, authorB);
        await _authorRepository.Unfollow(authorA, authorB);
        var AFollowBAfter = await _authorRepository.IsFollowing(authorA, authorB);

        // Assert
        Assert.True(AFollowBBefore);
        Assert.False(AFollowBAfter);
    }

    /**
     * Test that FollowRelations are automatically deleted with Authors
     */
    [Fact]
    public async Task FollowRelationDeletedWithAuthors() {
        var follower = new Author {DisplayName = "Barton Cooper", Email = "TheCakeMaster@copper.com", UserName = "TheCakeMaster@copper.com"};
        var followed = new Author { DisplayName = "DisappearingSoon", Email = "test@itu.dk", UserName = "test@itu.dk" };
        
        var myFollowRelation = new FollowRelation {
            FollowRelationId = 100000,
            Follower = follower,
            Followed = followed
        };
        follower.followerRelations.Add(myFollowRelation);
        followed.followedRelations.Add(myFollowRelation);

        Assert.DoesNotContain(follower, _context.Authors);
        Assert.DoesNotContain(followed, _context.Authors);
        Assert.DoesNotContain(myFollowRelation, _context.FollowRelations);

        _context.Authors.Add(follower);
        _context.Authors.Add(followed);
        await _context.SaveChangesAsync();

        Assert.Contains(follower, _context.Authors);
        Assert.Contains(followed, _context.Authors);
        Assert.Contains(myFollowRelation, _context.FollowRelations);

        _context.Authors.Remove(follower);
        _context.Authors.Remove(followed);
        await _context.SaveChangesAsync();

        Assert.DoesNotContain(follower, _context.Authors);
        Assert.DoesNotContain(followed, _context.Authors);
        Assert.DoesNotContain(myFollowRelation, _context.FollowRelations);
    }

    /**
     * Test that FollowRelations are automatically deleted with the Follower Author in the FollowRelation
     */
    [Fact]
    public async Task FollowRelationDeletedWithFollower() {
        var follower = new Author {DisplayName = "Barton Cooper", Email = "TheCakeMaster@copper.com", UserName = "Kent From barbie"};
        var followed = new Author { DisplayName = "DisappearingSoon", Email = "test@itu.dk", UserName = "test@itu.dk" };
        
        var myFollowRelation = new FollowRelation {
            FollowRelationId = 100000,
            Follower = follower,
            Followed = followed
        };
        follower.followerRelations.Add(myFollowRelation);
        followed.followedRelations.Add(myFollowRelation);

        Assert.DoesNotContain(follower, _context.Authors);
        Assert.DoesNotContain(followed, _context.Authors);
        Assert.DoesNotContain(myFollowRelation, _context.FollowRelations);
        
        _context.Authors.Add(followed);
        _context.Authors.Remove(follower);
        await _context.SaveChangesAsync();

        Assert.DoesNotContain(follower, _context.Authors);
        Assert.Contains(followed, _context.Authors);
        Assert.DoesNotContain(myFollowRelation, _context.FollowRelations);

        _context.Authors.Remove(followed);
        await _context.SaveChangesAsync();

        Assert.DoesNotContain(follower, _context.Authors);
        Assert.DoesNotContain(followed, _context.Authors);
        Assert.DoesNotContain(myFollowRelation, _context.FollowRelations);
    }

    /**
     * Test that FollowRelations are automatically deleted with the followed Author in the FollowRelation
     */
    [Fact]
    public async Task FollowRelationDeletedWithFollowed() {
        var follower = new Author {DisplayName = "Barton Cooper", Email = "TheCakeMaster@copper.com", UserName = "Kent From barbie"};
        var followed = new Author { DisplayName = "DisappearingSoon", Email = "test@itu.dk", UserName = "test@itu.dk" };
        
        var myFollowRelation = new FollowRelation {
            FollowRelationId = 100000,
            Follower = follower,
            Followed = followed
        };
        follower.followerRelations.Add(myFollowRelation);
        followed.followedRelations.Add(myFollowRelation);

        Assert.DoesNotContain(follower, _context.Authors);
        Assert.DoesNotContain(followed, _context.Authors);
        Assert.DoesNotContain(myFollowRelation, _context.FollowRelations);

        _context.Authors.Add(follower);
        _context.Authors.Remove(followed);
        await _context.SaveChangesAsync();

        Assert.DoesNotContain(followed, _context.Authors);
        Assert.Contains(follower, _context.Authors);
        Assert.DoesNotContain(myFollowRelation, _context.FollowRelations);

        _context.Authors.Remove(follower);
        await _context.SaveChangesAsync();

        Assert.DoesNotContain(follower, _context.Authors);
        Assert.DoesNotContain(followed, _context.Authors);
        Assert.DoesNotContain(myFollowRelation, _context.FollowRelations);
    }
}