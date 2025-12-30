using Chirp.Core;
using Chirp.Core.Domain_Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using static Chirp.Infrastructure.ICheepRepository;

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
        List<AuthorDTO> authors = await _authorRepository.GetAuthor("BartonCooper");
        AuthorDTO barton = authors.Single();
        List<FollowRelation> beforeFollowing =
            await _authorRepository.GetFollowRelations(barton.UserName);
        //act
        await _authorRepository.Follow(barton, barton);
        //assert
        List<FollowRelation> afterFollowing =
            await _authorRepository.GetFollowRelations(barton.UserName);
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
        Assert.Equal(2, (await _authorRepository.GetFollowRelations(barton.UserName)).Count);
    }

    [Fact]
    public async Task AttemptToUnfollowSelf() {
        Author user = _context.Authors.First();
        int followersBefore = (await _authorRepository.Following(user.UserName!)).Count;
        Assert.True(await _authorRepository.IsFollowing(user.UserName!, user.UserName!));
        await _authorRepository.Unfollow(user.UserName!, user.UserName!);
        int followersAfter = (await _authorRepository.Following(user.UserName!)).Count;
        Assert.True(await _authorRepository.IsFollowing(user.UserName!, user.UserName!));
        Assert.Equal(followersBefore, followersAfter);
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
        List<AuthorDTO> myList = await _authorRepository.Following(barton.UserName);
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
        Assert.Single(await _authorRepository.GetFollowRelations(barton.UserName));
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
        Assert.Single(await _authorRepository.GetFollowRelations(barton.UserName));
        //act
        _ = _authorRepository.Unfollow(barton, Wendell);
        //assert
        Assert.Single(await _authorRepository.GetFollowRelations(barton.UserName));
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
        Assert.Single(await _authorRepository.GetFollowRelations(barton.UserName));
        //act
#pragma warning disable CS8604 // Possible null reference argument.
        _ = _authorRepository.Follow(barton, Wendell);
        //assert
        Assert.Null(Wendell);
        Assert.Single(await _authorRepository.GetFollowRelations(barton.UserName));
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
        Assert.Single(await _authorRepository.GetFollowRelations(barton.UserName));
    }

    /** Verifies that you cannot unfollow yourself. */
    [Fact]
    public async Task UnfollowSelf() {
        AuthorDTO author = (await _authorRepository.GetAuthor("Helge")).Single();
        var followCountBefore = (await _authorRepository.GetFollowRelations(author.UserName)).Count;

        await _authorRepository.Unfollow(author, author);

        Assert.Equal((await _authorRepository.GetFollowRelations(author.UserName)).Count,
                     followCountBefore);
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

    [Theory]
    [InlineData("Helge", "ropf@itu.dk")]
    [InlineData("Adrian", "adho@itu.dk")]
    public async Task RequiredAuthorsExist(string name, string email) {
        // Test they exist as Authors in the underlying database.
        Author? author = (from user in _context.Authors
                          where user.UserName == name
                          orderby user.DisplayName
                          select user).ToList()
                                      .Single();

        Assert.Equal(name, author.DisplayName);
        Assert.Equal(email, author.Email);
        Assert.Equal(name, author.UserName);
        Assert.Equal(author.Email?.ToUpper(), author.NormalizedEmail);
        Assert.Equal(author.UserName?.ToUpper(), author.NormalizedUserName);
        Assert.True(author.EmailConfirmed);

        // Test they exist and are found as AuthorDTOs.
        List<AuthorDTO> authors = await _authorRepository.GetAuthorByUserName(name);
        AuthorDTO authorDTO = authors.Single();
        Assert.Equal(name, authorDTO.DisplayName);
        Assert.Equal(name, authorDTO.UserName);
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
        await Assert.ThrowsAsync<DbUpdateException>(() => _authorRepository.CreateAuthor(
                                                        name2, email));
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
        await Assert.ThrowsAsync<DbUpdateException>(() => _authorRepository.CreateAuthor(
                                                        name, email2));
        List<AuthorDTO> bartons = await _authorRepository.GetAuthor(username);
        Assert.Equal(name, bartons.Single().DisplayName);
        Assert.Equal(username, bartons.Single().UserName);
    }

    /**
     * Tests that retriving a author that does not exist, gives an empty list
     */
    [Fact]
    public async Task NoKnownAuthorTest() {
        List<AuthorDTO> authorsFound =
            await _authorRepository.GetAuthor("ThisNameorEmailDoesNotExist");
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
        List<AuthorDTO> users = await _authorRepository.GetAuthor("mad@test.dk");
        AuthorDTO user = users.Single();

        List<AuthorDTO> followed = await _authorRepository.Following(user.UserName);
        Assert.Single(followed);

        List<CheepDTO> cheeps = await _cheepRepository.GetCheepsFromFollowed(user.UserName);
        Assert.Empty(cheeps);
    }

    /** Asserts that if GetOwnAndFollowedCheeps returns a list containing exactly the author's
     * own cheeps if they do not follow any authors.
     */
    [Fact]
    public async Task PrivateTimelineNoFollowedCheeps() {
        await _authorRepository.CreateAuthor("Ms Mute and Deaf", "mad@test.dk");
        List<AuthorDTO> users = await _authorRepository.GetAuthor("mad@test.dk");
        AuthorDTO user = users.Single();

        List<AuthorDTO> followed = await _authorRepository.Following(user.UserName);
        Assert.Single(followed);
        Author? a = GetAuthorFromDatabase(user);
        await _cheepRepository.CreateCheep(a!, "Test message", DateTime.Now);

        List<CheepDTO> cheeps = await _cheepRepository.GetCheepsFromFollowed(user.UserName);
        Assert.Single(cheeps);
    }

    /**
     * New users follow themselves
     */
    [Fact]
    public async Task NewAuthorFollowsOnlySelf() {
        await _authorRepository.CreateAuthor("Ms Deaf", "mad@test.dk");
        List<AuthorDTO> users = await _authorRepository.GetAuthor("mad@test.dk");
        AuthorDTO user = users.Single();
        Assert.Equal(user, (await _authorRepository.Following(user.UserName)).Single());
    }

    /** Asserts that, if an author has no cheeps but follows one author,
      * the result of GetOwnAndFollowedCheeps is equal to a list of that followed author's
      * cheeps.
      */
    [Fact]
    public async Task PrivateTimelineNoOwnCheepsOneFollowedAuthor() {
        // Create a new user account, and ensure it now exists.
        await _authorRepository.CreateAuthor("Ms Mute", "mad@test.dk");
        AuthorDTO user = (await _authorRepository.GetAuthor("mad@test.dk")).Single();
        Assert.Empty(await _cheepRepository.GetCheepsFromUserName(user.UserName!, 1));

        // Select a different user account to follow, making sure it has cheeps.
        AuthorDTO toFollow =
            (await _authorRepository.GetAuthor("Jacqualine.Gilcoine@gmail.com")).Single();
        List<CheepDTO> cheepsFromFollowed =
            await _cheepRepository.GetCheepsFromUserName(toFollow.UserName!, 1);
        Assert.NotEmpty(cheepsFromFollowed);

        // Follow the secondary user account, and ensure this has occurred successfully.
        await _authorRepository.Follow(user, toFollow);
        List<AuthorDTO> following = await _authorRepository.Following(user.UserName);
        Assert.Equal(user, following.First());
        Assert.Equal(toFollow, following[1]);

        // Assert that the list of cheeps is exactly equal to the list of cheeps from the one
        // follower.
        List<CheepDTO> timelineCheeps = await _cheepRepository.GetCheepsFromFollowed(user.UserName, 1);
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
        AuthorDTO user = (await _authorRepository.GetAuthor("mad@test.dk")).Single();

        // Follow these three authors in the seeded database
        List<string> emails = [
            "Jacqualine.Gilcoine@gmail.com",
            "Roger+Histand@hotmail.com",
            "Luanna-Muro@ku.dk",
        ];
        List<CheepDTO> cheepsFromFollowed = [];
        foreach (string email in emails) {
            AuthorDTO author = (await _authorRepository.GetAuthor(email)).Single();
            await _authorRepository.Follow(user, author);
            cheepsFromFollowed.AddRange(
                await _cheepRepository.GetAllCheepsFromUserName(author.UserName));
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
            List<CheepDTO> cheeps = await _cheepRepository.GetCheepsFromFollowed(user.UserName, i + 1);
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
        List<AuthorDTO> users = await _authorRepository.GetAuthor("mad@test.dk");
        AuthorDTO user = users.Single();
        Assert.Empty(await _cheepRepository.GetCheepsFromUserName(user.UserName, 1));
    }

    /**
     * Author has valid Username even if they only specified
     */
    [Fact]
    public async Task NewAuthorHasNonNullUserName() {
        await _authorRepository.CreateAuthor("Ms HasAName", "mad@test.dk");
        List<AuthorDTO> users = await _authorRepository.GetAuthor("mad@test.dk");
        AuthorDTO user = users.Single();
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
        List<AuthorDTO> users = await _authorRepository.GetAuthor("mad@test.dk");
        AuthorDTO user = users.Single();

        List<string> authors = [
            "Jacqualine.Gilcoine@gmail.com",
            "Roger+Histand@hotmail.com",
            "Luanna-Muro@ku.dk",
        ];
        List<AuthorDTO> authorsToFollow = [];
        // Add several authors, making sure they each have cheeps.
        foreach (string email in authors) {
            List<AuthorDTO> toBeFollowed = await _authorRepository.GetAuthor(email);
            authorsToFollow.Add(toBeFollowed.Single());

            string username = authorsToFollow.Last().UserName;
            List<CheepDTO> cheepsFromFollowed = await _cheepRepository.GetCheepsFromUserName(username, 1);
            Assert.NotEmpty(cheepsFromFollowed);

            await _authorRepository.Follow(user, authorsToFollow.Last());
        }

        List<AuthorDTO> following = await _authorRepository.Following(user.UserName);
        Assert.Equal(authors.Count + 1, following.Count);
        foreach (AuthorDTO author in authorsToFollow) {
            Assert.Contains(author, following);
        }
    }


    /**
     * Tests if email validation can correctly determen weather a given string is a valid email
     */
    [Theory]
    [InlineData("IAmFulty@email", false)]
    [InlineData("IAmFulty@Email.com", false)]
    [InlineData("@email.com", false)]
    [InlineData("IAmFulty@.com", false)]
    [InlineData("I am not a email", false)]
    [InlineData("IAmEmail@email.com", true)]
    [InlineData("IAmEmail@email.dk", true)]
    [InlineData("IAmEmail.true@email.com", true)]
    [InlineData("IAmEmailtru2@email.com", true)]
    [InlineData("IAmvalid@email.frtyujhvbnklok", true)]
    public void EmailValidationTest(string email, bool shouldBeValid) {
        // act
        bool result = AuthorRepository.IsValidEmail(email);

        // assert
        Assert.Equal(shouldBeValid, result);
    }

    /** Tests that we can create an AuthorDTO based on an Author and have the correct */
    [Fact]
    private void CreateAuthorDTOFromAuthor() {
        var a = Author.Create("Display name", "em@ail.com");
        var derived = new AuthorDTO(a);
        Assert.Equal(a.DisplayName, derived.DisplayName);
        Assert.Equal(a.UserName, derived.UserName);
    }
    /** Tests that AuthorDTOs are compared by value rather than reference,
     * since AuthorDTOs are structs derived from Author.
     */
    [Fact]
    private void AuthorDTOEqualityByValue() {
        var a = new AuthorDTO("Display name", "User name");
        var b = new AuthorDTO("Display name", "User name");
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    /** Test that AuthorDTO's default comparison is by display name, not
     * username, */
    [Fact]
    private void AuthorDTOOrderedByDisplayName() {
        var first = new AuthorDTO("A", "B");
        var second = new AuthorDTO("B", "A");
        var sameAsFirst = new AuthorDTO("A", "C");
        Assert.Equal(-1, first.CompareTo(second));
        Assert.Equal(1, second.CompareTo(first));
        Assert.Equal(0, first.CompareTo(sameAsFirst));
    }

    /** Get an Author from the database. AuthorRepository returns an
     * AuthorDTO, so we do it manually like this for the tests instead. */
    private Author? GetAuthorFromDatabase(AuthorDTO authorDTO) {
        return (from author in _context.Authors
                where author.UserName == authorDTO.UserName
                orderby author.DisplayName
                select author).Single();
    }
}