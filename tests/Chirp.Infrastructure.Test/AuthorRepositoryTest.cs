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
        const string email1 = "TheCakeMaster@copper.com";
        await _authorRepository.CreateAuthor(name, email1);
        List<AuthorDTO> authors = await _authorRepository.GetAuthor("BartonCooper");
        AuthorDTO barton = authors.Single();
        List<FollowRelation> beforeFollowing = await _authorRepository.GetFollowRelations(barton);
        //act
        await _authorRepository.Follow(barton, barton);
        //assert
        List<FollowRelation> afterFollowing = await _authorRepository.GetFollowRelations(barton);
        Assert.Single(afterFollowing);
        Assert.Equal(beforeFollowing, afterFollowing);
    }

    [Fact]
    public async Task attemptToFollowSomeone() {
        //arrange
        const string name = "Barton Cooper";
        const string email1 = "TheCakeMaster@copper.com";
        await _authorRepository.CreateAuthor(name, email1);
        List<AuthorDTO> authors1 = await _authorRepository.GetAuthor("WendellBallan");
        AuthorDTO Wendell = authors1.Single();
        List<AuthorDTO> authors2 = await _authorRepository.GetAuthor("BartonCooper");
        AuthorDTO barton = authors2.Single();
        //act
        _ = _authorRepository.Follow(barton, Wendell);
        //assert
        Assert.Equal(2, (await _authorRepository.GetFollowRelations(barton)).Count);
    }

    [Fact]
    public async Task attemptToFollowSomeoneAlreadyFollowed() {
        //arrange
        const string name = "Barton Cooper";
        const string email1 = "TheCakeMaster@copper.com";
        await _authorRepository.CreateAuthor(name, email1);
        List<AuthorDTO> authors1 = await _authorRepository.GetAuthor("WendellBallan");
        AuthorDTO Wendell = authors1.Single();
        List<AuthorDTO> authors2 = await _authorRepository.GetAuthor("BartonCooper");
        AuthorDTO barton = authors2.Single();
        //act
        _ = _authorRepository.Follow(barton, Wendell);
        _ = _authorRepository.Follow(barton, Wendell);
        List<AuthorDTO> myList = await _authorRepository.Following(barton);
        //assert
        Assert.Equal(2, myList.Count);
    }

    [Fact]
    public async Task attemptToUnfollowSomeoneFollowed() {
        //arrange
        const string name = "Barton Cooper";
        const string email1 = "TheCakeMaster@copper.com";
        await _authorRepository.CreateAuthor(name, email1);
        List<AuthorDTO> authors1 = await _authorRepository.GetAuthor("WendellBallan");
        AuthorDTO Wendell = authors1.Single();
        List<AuthorDTO> authors2 = await _authorRepository.GetAuthor("BartonCooper");
        AuthorDTO barton = authors2.Single();
        //act
        _ = _authorRepository.Follow(barton, Wendell);
        _ = _authorRepository.Unfollow(barton, Wendell);
        //assert
        Assert.Single(await _authorRepository.GetFollowRelations(barton));
    }

    [Fact]
    public async Task attemptToUnfollowSomeoneNotFollowed() {
        //arrange
        const string name = "Barton Cooper";
        const string email1 = "TheCakeMaster@copper.com";
        await _authorRepository.CreateAuthor(name, email1);
        List<AuthorDTO> authors1 = await _authorRepository.GetAuthor("WendellBallan");
        AuthorDTO Wendell = authors1.Single();
        List<AuthorDTO> authors2 = await _authorRepository.GetAuthor("BartonCooper");
        AuthorDTO barton = authors2.Single();
        Assert.Single(await _authorRepository.GetFollowRelations(barton));
        //act
        _ = _authorRepository.Unfollow(barton, Wendell);
        //assert
        Assert.Single(await _authorRepository.GetFollowRelations(barton));
    }

    [Fact]
    public async Task followNull() {
        //arrange
        const string name = "Barton Cooper";
        const string email1 = "TheCakeMaster@copper.com";
        await _authorRepository.CreateAuthor(name, email1);
        List<AuthorDTO> authors2 = await _authorRepository.GetAuthor("BartonCooper");
        AuthorDTO barton = authors2.Single();
        AuthorDTO? Wendell = null;
        Assert.Single(await _authorRepository.GetFollowRelations(barton));
        //act
#pragma warning disable CS8604 // Possible null reference argument.
        _ = _authorRepository.Follow(barton, Wendell);
        //assert
        Assert.Null(Wendell);
        Assert.Single(await _authorRepository.GetFollowRelations(barton));
    }

    [Fact]
    public async Task followAuthorNotInDBContext() {
        //arrange
        const string name = "Barton Cooper";
        const string email1 = "TheCakeMaster@copper.com";
        await _authorRepository.CreateAuthor(name, email1);
        List<AuthorDTO> authors2 = await _authorRepository.GetAuthor("BartonCooper");
        AuthorDTO barton = authors2.Single();
        AuthorDTO myAuthor = new AuthorDTO(Author.Create("Bartoon2", "batman@gmail.com"));
        //act
        _ = _authorRepository.Follow(barton, myAuthor);
        //assert
        Assert.Single(await _authorRepository.GetFollowRelations(barton));
    }

    /** Verifies that you cannot unfollow yourself. */
    [Fact]
    public async Task UnfollowSelf() {
        AuthorDTO author = (await _authorRepository.GetAuthor("Helge")).Single();
        var followCountBefore = (await _authorRepository.GetFollowRelations(author)).Count;

        await _authorRepository.Unfollow(author, author);

        Assert.Equal((await _authorRepository.GetFollowRelations(author)).Count, followCountBefore);
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
        AuthorDTO authorA = (await _authorRepository.GetAuthor(usernameA)).First();
        const string nameB = "Abba Booper";
        string usernameB = nameB.Replace(" ", "");
        const string emailB = "Abba@Booper.com";
        await _authorRepository.CreateAuthor(nameB, emailB);
        AuthorDTO authorB = (await _authorRepository.GetAuthor(usernameB)).First();

        // act
        var AFollowBBefore =
            await _authorRepository.IsFollowing(authorA.UserName, authorB.UserName);
        var BFollowABefore =
            await _authorRepository.IsFollowing(authorB.UserName, authorA.UserName);
        await _authorRepository.Follow(authorA, authorB);
        var AFollowBAfter = await _authorRepository.IsFollowing(authorA.UserName, authorB.UserName);
        var BFollowAAfter = await _authorRepository.IsFollowing(authorB.UserName, authorA.UserName);

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
        AuthorDTO authorA = (await _authorRepository.GetAuthor(usernameA)).First();
        const string nameB = "Abba Booper";
        string usernameB = nameB.Replace(" ", "");
        const string emailB = "Abba@Booper.com";
        await _authorRepository.CreateAuthor(nameB, emailB);
        AuthorDTO authorB = (await _authorRepository.GetAuthor(usernameB)).First();
        await _authorRepository.Follow(authorA, authorB);

        // act
        var AFollowBBefore =
            await _authorRepository.IsFollowing(authorA.UserName, authorB.UserName);
        await _authorRepository.Unfollow(authorA, authorB);
        var AFollowBAfter = await _authorRepository.IsFollowing(authorA.UserName, authorB.UserName);

        // Assert
        Assert.True(AFollowBBefore);
        Assert.False(AFollowBAfter);
    }
}