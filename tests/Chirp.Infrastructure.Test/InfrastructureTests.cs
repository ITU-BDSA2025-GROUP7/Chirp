using Chirp.Core;
using Chirp.Core.Domain_Model;
using static Chirp.Core.ICheepRepository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Chirp.Infrastructure.Test;

public class InfrastructureTests {
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ICheepRepository _cheepRepo;
    private readonly IAuthorRepository _authorRepo;
    private readonly ChirpDBContext _context;

    public InfrastructureTests(ITestOutputHelper testOutputHelper) {
        _testOutputHelper = testOutputHelper;
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        DbContextOptions<ChirpDBContext> options = new DbContextOptionsBuilder<ChirpDBContext>()
                                                  .UseSqlite(connection)
                                                  .Options;
        _context = new ChirpDBContext(options);
        _context.Database.EnsureCreated();
        _cheepRepo = new CheepRepository(_context);
        _authorRepo = new AuthorRepository(_context);
        DbInitializer.SeedDatabase(_context);
        _context.SaveChanges();
    }

    /** Asserts that the GetOwnAndFollowedCheeps returns an empty list if an Author
     * has no cheeps of their own nor any people they follow. */
    [Fact]
    public async Task PrivateTimelineNoOwnCheepsNoFollowedCheeps() {
        await _authorRepo.CreateAuthor("Ms Mute and Deaf", "mad@test.dk");
        List<AuthorDTO> users = await _authorRepo.GetAuthor("mad@test.dk");
        AuthorDTO user = users.Single();

        List<AuthorDTO> followed = await _authorRepo.Following(user.UserName);
        Assert.Single(followed);

        List<CheepDTO> cheeps = await _cheepRepo.GetCheepsFromFollowed(user.UserName);
        Assert.Empty(cheeps);
    }

    /** Asserts that if GetOwnAndFollowedCheeps returns a list containing exactly the author's
     * own cheeps if they do not follow any authors.
     */
    [Fact]
    public async Task PrivateTimelineNoFollowedCheeps() {
        await _authorRepo.CreateAuthor("Ms Mute and Deaf", "mad@test.dk");
        List<AuthorDTO> users = await _authorRepo.GetAuthor("mad@test.dk");
        AuthorDTO user = users.Single();

        List<AuthorDTO> followed = await _authorRepo.Following(user.UserName);
        Assert.Single(followed);
        Author? a = GetAuthorFromDatabase(user);
        await _cheepRepo.CreateCheep(a!, "Test message", DateTime.Now);

        List<CheepDTO> cheeps = await _cheepRepo.GetCheepsFromFollowed(user.UserName);
        Assert.Single(cheeps);
    }

    /** Asserts that, if an author has no cheeps but follows one author,
     * the result of GetOwnAndFollowedCheeps is equal to a list of that followed author's
     * cheeps.
     */
    [Fact]
    public async Task PrivateTimelineNoOwnCheepsOneFollowedAuthor() {
        // Create a new user account, and ensure it now exists.
        await _authorRepo.CreateAuthor("Ms Mute", "mad@test.dk");
        AuthorDTO user = (await _authorRepo.GetAuthor("mad@test.dk")).Single();
        Assert.Empty(await _cheepRepo.GetCheepsFromUserName(user.UserName!, 1));

        // Select a different user account to follow, making sure it has cheeps.
        AuthorDTO toFollow =
            (await _authorRepo.GetAuthor("Jacqualine.Gilcoine@gmail.com")).Single();
        List<CheepDTO> cheepsFromFollowed =
            await _cheepRepo.GetCheepsFromUserName(toFollow.UserName!, 1);
        Assert.NotEmpty(cheepsFromFollowed);

        // Follow the secondary user account, and ensure this has occurred successfully.
        await _authorRepo.Follow(user, toFollow);
        List<AuthorDTO> following = await _authorRepo.Following(user.UserName);
        Assert.Equal(user, following.First());
        Assert.Equal(toFollow, following[1]);

        // Assert that the list of cheeps is exactly equal to the list of cheeps from the one
        // follower.
        List<CheepDTO> timelineCheeps = await _cheepRepo.GetCheepsFromFollowed(user.UserName, 1);
        Assert.Equal(cheepsFromFollowed, timelineCheeps);
    }

    /** Asserts that, if an author has no cheeps but follows several authors,
     * the result of GetOwnAndFollowedCheeps is equal to a sorted list of those followed authors'
     * cheeps.
     */
    [Fact]
    public async Task PrivateTimelineNoOwnCheepsMultipleFollowedAuthors() {
        // Create a new user account, and ensure it now exists.
        await _authorRepo.CreateAuthor("Ms Mute", "mad@test.dk");
        AuthorDTO user = (await _authorRepo.GetAuthor("mad@test.dk")).Single();

        // Follow these three authors in the seeded database
        List<string> emails = [
            "Jacqualine.Gilcoine@gmail.com",
            "Roger+Histand@hotmail.com",
            "Luanna-Muro@ku.dk",
        ];
        List<CheepDTO> cheepsFromFollowed = [];
        foreach (string email in emails) {
            AuthorDTO author = (await _authorRepo.GetAuthor(email)).Single();
            await _authorRepo.Follow(user, author);
            cheepsFromFollowed.AddRange(
                await _cheepRepo.GetAllCheepsFromUserName(author.UserName));
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
            List<CheepDTO> cheeps = await _cheepRepo.GetCheepsFromFollowed(user.UserName, i + 1);
            timelineCheepCount += cheeps.Count;
            int lowerBound = i * CHEEPS_PER_PAGE;
            int upperBound = lowerBound + cheeps.Count;
            Assert.Equal(cheepsFromFollowed[lowerBound..upperBound], cheeps);
        }

        // Mostly to feel a little more confident that above loop works correctly.
        Assert.Equal(cheepsFromFollowed.Count, timelineCheepCount);
    }

    [Fact]
    public async Task NewAuthorFollowsOnlySelf() {
        await _authorRepo.CreateAuthor("Ms Deaf", "mad@test.dk");
        List<AuthorDTO> users = await _authorRepo.GetAuthor("mad@test.dk");
        AuthorDTO user = users.Single();
        Assert.Equal(user, (await _authorRepo.Following(user.UserName)).Single());
    }

    [Fact]
    public async Task NewAuthorHasNoCheeps() {
        await _authorRepo.CreateAuthor("Ms Mute", "mad@test.dk");
        List<AuthorDTO> users = await _authorRepo.GetAuthor("mad@test.dk");
        AuthorDTO user = users.Single();
        Assert.Empty(await _cheepRepo.GetCheepsFromUserName(user.UserName, 1));
    }

    [Fact]
    public async Task NewAuthorHasNonNullUserName() {
        await _authorRepo.CreateAuthor("Ms HasAName", "mad@test.dk");
        List<AuthorDTO> users = await _authorRepo.GetAuthor("mad@test.dk");
        AuthorDTO user = users.Single();
        Assert.NotNull(user.UserName);
        Assert.NotEmpty(user.UserName); // Assert that it is not blank
    }

    [Fact]
    public async Task FollowMultiplePeople() {
        // Create a new user account, and ensure it now exists.
        await _authorRepo.CreateAuthor("Ms Mute", "mad@test.dk");
        List<AuthorDTO> users = await _authorRepo.GetAuthor("mad@test.dk");
        AuthorDTO user = users.Single();

        List<string> authors = [
            "Jacqualine.Gilcoine@gmail.com",
            "Roger+Histand@hotmail.com",
            "Luanna-Muro@ku.dk",
        ];
        List<AuthorDTO> authorsToFollow = [];
        // Add several authors, making sure they each have cheeps.
        foreach (string email in authors) {
            List<AuthorDTO> toBeFollowed = await _authorRepo.GetAuthor(email);
            authorsToFollow.Add(toBeFollowed.Single());

            string username = authorsToFollow.Last().UserName;
            List<CheepDTO> cheepsFromFollowed = await _cheepRepo.GetCheepsFromUserName(username, 1);
            Assert.NotEmpty(cheepsFromFollowed);

            await _authorRepo.Follow(user, authorsToFollow.Last());
        }

        List<AuthorDTO> following = await _authorRepo.Following(user.UserName);
        Assert.Equal(authors.Count + 1, following.Count);
        foreach (AuthorDTO author in authorsToFollow) {
            Assert.Contains(author, following);
        }
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