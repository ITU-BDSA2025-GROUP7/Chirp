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

    /** Verifies that you cannot unfollow yourself. */
    [Fact]
    public async Task UnfollowSelf() {
        Author author = (await _authorRepository.GetAuthor("Helge")).Single();
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
     * Tests that the author gotten by Author GetAuthorByUserName exists and has valid feilds.
     */
    [Theory]
    [InlineData("Helge", "ropf@itu.dk")]
    [InlineData("Adrian", "adho@itu.dk")]
    public async Task RequiredAuthorsExist(string name, string email) {
        List<Author> authors = await _authorRepository.GetAuthorByUserName(name);
        Assert.NotNull(authors);
        Assert.Single(authors);
        Author author = authors.Single();
        Assert.Equal(name, author.DisplayName);
        Assert.Equal(email, author.Email);
        Assert.Equal(name, author.UserName);
        Assert.Equal(author.Email?.ToUpper(), author.NormalizedEmail);
        Assert.Equal(author.UserName?.ToUpper(), author.NormalizedUserName);
        Assert.True(author.EmailConfirmed);
    }

    /**
     * Tests that deleting an author also deletes their cheeps
     */
    [Fact]
    public async Task CheepsDeletedWithAuthor() {
        var author = new Author { DisplayName = "DisappearingSoon", Email = "test@itu.dk", UserName = "test@itu.dk" };
        var cheep = new Cheep {
            CheepId = 90000,
            Author = author,
            Text = "This is a cheep",
            TimeStamp = DateTime.Now
        };
        author.Cheeps.Add(cheep);

        Assert.DoesNotContain(author, _context.Authors);
        Assert.DoesNotContain(cheep, _context.Cheeps);

        _context.Authors.Add(author);
        await _context.SaveChangesAsync();

        Assert.Contains(author, _context.Authors);
        Assert.Contains(cheep, _context.Cheeps);

        _context.Authors.Remove(author);
        await _context.SaveChangesAsync();

        Assert.DoesNotContain(author, _context.Authors);
        Assert.DoesNotContain(cheep, _context.Cheeps);
    }

    /**
    * Tests creating an author
    */
    [Fact]
    public async Task CreateAuthorTest() {
        string name, email;
        name = "Barton Cooper";
        email = "cooper@copper.com";
        await _authorRepository.CreateAuthor(name, email);
        var query = (from author in _context.Authors
                     where author.DisplayName == name
                     select author);
        Author actualAuthor = await query.FirstAsync();
        Assert.Equal(email, actualAuthor.Email);
    }

    /**
     * Cannot create 2 authors with same email
     */
    [Fact]
    public async Task AuthorReusingEmailTest() {
        string name1, name2, email;
        name1 = "Barton Cooper";
        name2 = "Bar2n Cooper";
        email = "cooper@copper.com";
        await _authorRepository.CreateAuthor(name1, email);
        await Assert.ThrowsAsync<DbUpdateException>(() => _authorRepository.CreateAuthor(name2, email));
    }

    /**
     * Cannot have multiple users with same username
     */
    [Fact]
    public async Task AuthorSameNameTest() {
        const string name = "Barton Cooper";
        string username = name.Replace(" ", "");
        const string email1 = "TheCakeMaster@copper.com";
        const string email2 = "muffinEnjoyer@copper.com";
        await _authorRepository.CreateAuthor(name, email1);
        await Assert.ThrowsAsync<DbUpdateException>(() => _authorRepository.CreateAuthor(name, email2));
        List<Author> bartons = await _authorRepository.GetAuthor(username);
        Assert.Equal(name, bartons.Single().DisplayName);
        Assert.Equal(username, bartons.Single().UserName);
        Assert.Equal(email1, bartons.Single().Email);
    }

    /**
     * Tests that retriving a author that does not exist, gives an empty list
     */
    [Fact]
    public async Task NoKnownAuthorTest() {
        List<Author> authorsFound = await _authorRepository.GetAuthor("ThisNameorEmailDoesNotExist");
        Assert.Empty(authorsFound);
    }

    /**
    * the name "" is valid
    */
    [Fact]
    public async Task AuthorBlankName() {
        string name, email;
        name = "";
        email = "cooper@copper.com";
        await _authorRepository.CreateAuthor(name, email);
        var query = (from author in _context.Authors
                     where author.DisplayName == ""
                     select author);
        Author actualAuthor = query.Single();
        Assert.NotNull(actualAuthor);
    }

}