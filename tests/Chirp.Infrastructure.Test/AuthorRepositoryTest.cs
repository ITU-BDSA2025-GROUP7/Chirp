using Chirp.Core;
using Chirp.Core.Domain_Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using static Chirp.Core.ICheepRepository;

namespace Chirp.Infrastructure.Test;

public class AuthorRepositoryTest {
    private readonly ChirpDBContext _context;
    private SqliteConnection _connection;
    private ICheepRepository _cheepRepository;
    private IAuthorRepository _authorRepository;
    private readonly ITestOutputHelper _testOutputHelper;

    public AuthorRepositoryTest(ITestOutputHelper testOutputHelper) {
        _testOutputHelper = testOutputHelper;

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

    /** Asserts that the GetOwnAndFollowedCheeps returns an empty list if an Author
     * has no cheeps of their own nor any people they follow. */
    [Fact]
    public async Task PrivateTimelineNoOwnCheepsNoFollowedCheeps() {
        await _authorRepository.CreateAuthor("Ms Mute and Deaf", "mad@test.dk");
        List<Author> users = await _authorRepository.GetAuthor("mad@test.dk");
        Author user = users.Single();

        List<Author> followed = await _authorRepository.Following(user);
        Assert.Single(followed);

        List<CheepDTO> cheeps = await _cheepRepository.GetOwnAndFollowedCheeps(user);
        Assert.Empty(cheeps);
    }

    /** Asserts that if GetOwnAndFollowedCheeps returns a list containing exactly the author's
    * own cheeps if they do not follow any authors.
    */
    [Fact]
    public async Task PrivateTimelineNoFollowedCheeps() {
        await _authorRepository.CreateAuthor("Ms Mute and Deaf", "mad@test.dk");
        List<Author> users = await _authorRepository.GetAuthor("mad@test.dk");
        Author user = users.Single();

        List<Author> followed = await _authorRepository.Following(user);
        Assert.Single(followed);

        await _cheepRepository.CreateCheep(user, "Test message", DateTime.Now);

        List<CheepDTO> cheeps = await _cheepRepository.GetOwnAndFollowedCheeps(user);
        Assert.Single(cheeps);
    }

    /**
     * New users follow themselves
     */
    [Fact]
    public async Task NewAuthorFollowsOnlySelf() {
        await _authorRepository.CreateAuthor("Ms Deaf", "mad@test.dk");
        List<Author> users = await _authorRepository.GetAuthor("mad@test.dk");
        Author user = users.Single();
        Assert.Equal(user, (await _authorRepository.Following(user)).Single());
    }

    /** Asserts that, if an author has no cheeps but follows one author,
     * the result of GetOwnAndFollowedCheeps is equal to a list of that followed author's
     * cheeps.
     */
    [Fact]
    public async Task PrivateTimelineNoOwnCheepsOneFollowedAuthor() {
        // Create a new user account, and ensure it now exists.
        await _authorRepository.CreateAuthor("Ms Mute", "mad@test.dk");
        Author user = (await _authorRepository.GetAuthor("mad@test.dk")).Single();
        Assert.Empty(await _cheepRepository.GetCheepsFromUserName(user.UserName!, 1));

        // Select a different user account to follow, making sure it has cheeps.
        Author toFollow = (await _authorRepository.GetAuthor("Jacqualine.Gilcoine@gmail.com")).Single();
        List<CheepDTO> cheepsFromFollowed =
            await _cheepRepository.GetCheepsFromUserName(toFollow.UserName!, 1);
        Assert.NotEmpty(cheepsFromFollowed);

        // Follow the secondary user account, and ensure this has occurred successfully.
        await _authorRepository.Follow(user, toFollow);
        List<Author> following = await _authorRepository.Following(user);
        Assert.Equal(user, following.First());
        Assert.Equal(toFollow, following[1]);

        // Assert that the list of cheeps is exactly equal to the list of cheeps from the one
        // follower.
        List<CheepDTO> timelineCheeps = await _cheepRepository.GetOwnAndFollowedCheeps(user, 1);
        Assert.Equal(cheepsFromFollowed, timelineCheeps);
    }

    /** Asserts that, if an author has no cheeps but follows several authors,
     * the result of GetOwnAndFollowedCheeps is equal to a sorted list of those followed authors'
     * cheeps.
     */
    [Fact]
    public async Task PrivateTimelineNoOwnCheepsMultipleFollowedAuthors() {
        // Create a new user account, and ensure it now exists.
        await _authorRepository.CreateAuthor("Ms Mute", "mad@test.dk");
        Author user = (await _authorRepository.GetAuthor("mad@test.dk")).Single();

        // Follow these three authors in the seeded database
        List<string> emails = [
            "Jacqualine.Gilcoine@gmail.com",
            "Roger+Histand@hotmail.com",
            "Luanna-Muro@ku.dk",
        ];
        List<CheepDTO> cheepsFromFollowed = [];
        foreach (string email in emails) {
            Author author = (await _authorRepository.GetAuthor(email)).Single();
            await _authorRepository.Follow(user, author);
            cheepsFromFollowed.AddRange(
                await _cheepRepository.GetAllCheepsFromUserName(author.UserName!));
        }

        // Sort the combined list of cheeps from followers so that they are mixed together and
        // ordered by timestamp (CheepDTO implements IComparable<CheepDTO>).
        cheepsFromFollowed.Sort();

        // Assert that the list of cheeps is exactly equal to the list of cheeps from the followers.
        // Does so by comparing 32-cheep subsections of the former to pages retrieved of the latter.
        // Make sure there's a point to the loop.
        Assert.True(cheepsFromFollowed.Count > CHEEPS_PER_PAGE);
        _testOutputHelper.WriteLine(cheepsFromFollowed.Count.ToString());
        int timelineCheepCount = 0;

        int totalPages = cheepsFromFollowed.Count / CHEEPS_PER_PAGE;
        if (cheepsFromFollowed.Count % CHEEPS_PER_PAGE != 0) {
            totalPages++;
        }

        for (int i = 0; i < totalPages; i++) {
            List<CheepDTO> cheeps = await _cheepRepository.GetOwnAndFollowedCheeps(user, i + 1);
            timelineCheepCount += cheeps.Count;
            int lowerBound = i * CHEEPS_PER_PAGE;
            int upperBound = lowerBound + cheeps.Count;
            Assert.Equal(cheepsFromFollowed[lowerBound..upperBound], cheeps);
        }

        // Mostly to feel a little more confident that above loop works correctly.
        Assert.Equal(cheepsFromFollowed.Count, timelineCheepCount);
    }

    /**
     * Tests that a new author has no cheeps created by them
     */
    [Fact]
    public async Task NewAuthorHasNoCheeps() {
        await _authorRepository.CreateAuthor("Ms Mute", "mad@test.dk");
        List<Author> users = await _authorRepository.GetAuthor("mad@test.dk");
        Author user = users.Single();
        Assert.Empty(await _cheepRepository.GetCheepsFromUserName(user.UserName!, 1));
    }

    /**
     * Author has valid Username even if they only specified
     */
    [Fact]
    public async Task NewAuthorHasNonNullUserName() {
        await _authorRepository.CreateAuthor("Ms HasAName", "mad@test.dk");
        List<Author> users = await _authorRepository.GetAuthor("mad@test.dk");
        Author user = users.Single();
        Assert.NotNull(user.UserName);
        Assert.NotEmpty(user.UserName); // Assert that it is not blank
    }

    /**
     * Tests that an author can follow multiple authors
     */
    [Fact]
    public async Task FollowMultiplePeople() {
        // Create a new user account, and ensure it now exists.
        await _authorRepository.CreateAuthor("Ms Mute", "mad@test.dk");
        List<Author> users = await _authorRepository.GetAuthor("mad@test.dk");
        Author user = users.Single();

        List<string> authors = [
            "Jacqualine.Gilcoine@gmail.com",
            "Roger+Histand@hotmail.com",
            "Luanna-Muro@ku.dk",
        ];
        List<Author> authorsToFollow = [];
        // Add several authors, making sure they each have cheeps.
        foreach (string email in authors) {
            List<Author> toBeFollowed = await _authorRepository.GetAuthor(email);
            authorsToFollow.Add(toBeFollowed.Single());

            string username = authorsToFollow.Last().UserName!;
            List<CheepDTO> cheepsFromFollowed = await _cheepRepository.GetCheepsFromUserName(username, 1);
            Assert.NotEmpty(cheepsFromFollowed);

            await _authorRepository.Follow(user, authorsToFollow.Last());
        }

        List<Author> following = await _authorRepository.Following(user);
        Assert.Equal(authors.Count + 1, following.Count);
        foreach (Author author in authorsToFollow) {
            Assert.Contains(author, following);
        }
    }

}