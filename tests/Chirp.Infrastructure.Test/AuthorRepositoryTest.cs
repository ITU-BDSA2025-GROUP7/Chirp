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
        List<Author> authors = await _authorRepository.GetAuthor("BartonCooper");
        Author barton = authors.Single();
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
        Assert.Equal(2, (await _authorRepository.GetFollowRelations(barton)).Count);
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
        Assert.Equal(2, myList.Count);
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
        Assert.Single(await _authorRepository.GetFollowRelations(barton));
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
        string username = name.Replace(" ", "");
        const string email1 = "TheCakeMaster@copper.com";
        await _authorRepository.CreateAuthor(name, email1);
        List<Author> authors2 = await _authorRepository.GetAuthor("BartonCooper");
        Author barton = authors2.Single();
        Author? Wendell = null;
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
        string username = name.Replace(" ", "");
        const string email1 = "TheCakeMaster@copper.com";
        await _authorRepository.CreateAuthor(name, email1);
        List<Author> authors2 = await _authorRepository.GetAuthor("BartonCooper");
        Author barton = authors2.Single();
        Author myAuthor = Author.Create("Bartoon2", "batman@gmail.com");
        //act
        _ = _authorRepository.Follow(barton, myAuthor);
        //assert
        Assert.Single(await _authorRepository.GetFollowRelations(barton));
    }
}