using Chirp.Core;
using Chirp.Core.Domain_Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Chirp.Infrastructure.Test;

public class InfrastructureTests {
    private readonly ICheepRepository _repo;

    public InfrastructureTests() {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        DbContextOptions<ChirpDBContext> options = new DbContextOptionsBuilder<ChirpDBContext>()
                                                  .UseSqlite(connection)
                                                  .Options;
        var context = new ChirpDBContext(options);
        context.Database.EnsureCreated();
        _repo = new CheepRepository(context);
        DbInitializer.SeedDatabase(context);
        context.SaveChanges();
    }

    /** Asserts that the GetOwnAndFollowedCheeps returns an empty list if an Author
     * has no cheeps of their own nor any people they follow. */
    [Fact]
    public async Task PrivateTimelineNoOwnCheepsNoFollowedCheeps() {
        await _repo.CreateAuthor("Ms Mute and Deaf", "mad@test.dk");
        List<Author> users = await _repo.GetAuthor("mad@test.dk");
        Author user = users.Single();

        List<Author> followed = await _repo.Following(user);
        Assert.Empty(followed);

        string? username = user.UserName;
        Assert.NotNull(username);

        List<CheepDTO> cheeps = await _repo.GetOwnAndFollowedCheeps(username);
        Assert.Empty(cheeps);
    }

    /** Asserts that if GetOwnAndFollowedCheeps returns a list containing exactly the author's
     * own cheeps if they do not follow any authors.
     */
    [Fact]
    public async Task PrivateTimelineNoFollowedCheeps() {
        await _repo.CreateAuthor("Ms Mute and Deaf", "mad@test.dk");
        List<Author> users = await _repo.GetAuthor("mad@test.dk");
        Author user = users.Single();

        List<Author> followed = await _repo.Following(user);
        Assert.Empty(followed);

        await _repo.CreateCheep(user, "Test message", DateTime.Now);

        string? username = user.UserName;
        Assert.NotNull(username);

        List<CheepDTO> cheeps = await _repo.GetOwnAndFollowedCheeps(username);
        Assert.Single(cheeps);
    }

    /** Asserts that, if an author has no cheeps but follows one author,
     * the result of GetOwnAndFollowedCheeps is equal to a list of that followed author's
     * cheeps.
     */
    [Fact]
    public async Task PrivateTimelineNoOwnCheepsOneFollowedAuthor() {
        // Create a new user account, and ensure it now exists.
        await _repo.CreateAuthor("Ms Mute", "mad@test.dk");
        List<Author> users = await _repo.GetAuthor("mad@test.dk");
        Author user = users.Single();
        Assert.NotNull(user.UserName); // Remove subsequent nullability warnings

        // Ensure newly-created authors have no cheeps or followers
        Assert.Empty(await _repo.GetCheepsFromUserName(user.UserName, 1));
        Assert.Empty(await _repo.Following(user));

        // Select a different user account to follow, making sure it has cheeps.
        List<Author> toBeFollowed = await _repo.GetAuthor("Jacqualine.Gilcoine@gmail.com");
        Author toFollow = toBeFollowed.Single();
        Assert.NotNull(toFollow.UserName);
        List<CheepDTO> cheepsFromFollowed = await _repo.GetCheepsFromUserName(toFollow.UserName, 1);
        Assert.NotEmpty(cheepsFromFollowed);

        // Follow the secondary user account, and ensure this has occurred successfully.
        await _repo.Follow(user, toFollow);
        List<Author> following = await _repo.Following(user);
        Assert.Equal(toFollow, following.Single());

        // Assert that the list of cheeps is exactly equal to the list of cheeps from the one follower.
        List<CheepDTO> cheeps = await _repo.GetOwnAndFollowedCheeps(user.UserName);
        Assert.Equal(cheepsFromFollowed, cheeps);
    }

    /** Asserts that, if an author has no cheeps but follows several authors,
     * the result of GetOwnAndFollowedCheeps is equal to a sorted list of those followed authors'
     * cheeps.
     */
    [Fact]
    public async Task PrivateTimelineNoOwnCheepsMultipleFollowedAuthors() {
        // Create a new user account, and ensure it now exists.
        await _repo.CreateAuthor("Ms Mute", "mad@test.dk");
        List<Author> users = await _repo.GetAuthor("mad@test.dk");
        Author user = users.Single();
        Assert.NotNull(user.UserName); // Remove subsequent nullability warnings

        // Ensure newly-created authors have no cheeps or followers
        Assert.Empty(await _repo.GetCheepsFromUserName(user.UserName, 1));
        Assert.Empty(await _repo.Following(user));

        List<string> authors = [
            "Jacqualine.Gilcoine@gmail.com",
            "Roger+Histand@hotmail.com",
            "Luanna-Muro@ku.dk",
        ];
        List<CheepDTO> cheepsFromFollowed = [];
        foreach (string email in authors) {
            List<Author> toBeFollowed = await _repo.GetAuthor(email);
            Author author = toBeFollowed.Single();
            await _repo.Follow(user, author);
            List<CheepDTO> cheepsFromNewlyFollowedPerson =
                await _repo.GetAllCheepsFromUserName(author.UserName!);
            cheepsFromFollowed.AddRange(cheepsFromNewlyFollowedPerson);
        }

        cheepsFromFollowed.Sort((a, b) => DateTime.Compare(DateTime.Parse(a.TimeStamp),
                                                           DateTime.Parse(b.TimeStamp)));

        // Assert that the list of cheeps is exactly equal to the list of cheeps from the followers.
        for (int i = 0; i < cheepsFromFollowed.Count; i += ICheepRepository.CHEEPS_PER_PAGE) {
            List<CheepDTO> cheeps = await _repo.GetOwnAndFollowedCheeps(user.UserName);
            Assert.Equal(cheepsFromFollowed, cheeps);
        }
    }

    [Fact]
    public async Task NewAuthorFollowsNoPeople() {
        await _repo.CreateAuthor("Ms Deaf", "mad@test.dk");
        List<Author> users = await _repo.GetAuthor("mad@test.dk");
        Author user = users.Single();
        Assert.Empty(await _repo.Following(user));
    }

    [Fact]
    public async Task NewAuthorHasNoCheeps() {
        await _repo.CreateAuthor("Ms Mute", "mad@test.dk");
        List<Author> users = await _repo.GetAuthor("mad@test.dk");
        Author user = users.Single();
        Assert.Empty(await _repo.GetCheepsFromUserName(user.UserName!, 1));
    }

    [Fact]
    public async Task NewAuthorHasNonNullUserName() {
        await _repo.CreateAuthor("Ms HasAName", "mad@test.dk");
        List<Author> users = await _repo.GetAuthor("mad@test.dk");
        Author user = users.Single();
        Assert.NotNull(user.UserName);
        Assert.NotEmpty(user.UserName); // Assert that it is not blank
    }

    [Fact]
    public async Task FollowMultiplePeople() {
        // Create a new user account, and ensure it now exists.
        await _repo.CreateAuthor("Ms Mute", "mad@test.dk");
        List<Author> users = await _repo.GetAuthor("mad@test.dk");
        Author user = users.Single();

        List<string> authors = [
            "Jacqualine.Gilcoine@gmail.com",
            "Roger+Histand@hotmail.com",
            "Luanna-Muro@ku.dk",
        ];
        List<Author> authorsToFollow = [];
        // Add several authors, making sure they each have cheeps.
        foreach (string email in authors) {
            List<Author> toBeFollowed = await _repo.GetAuthor(email);
            authorsToFollow.Add(toBeFollowed.Single());

            string username = authorsToFollow.Last().UserName!;
            List<CheepDTO> cheepsFromFollowed = await _repo.GetCheepsFromUserName(username, 1);
            Assert.NotEmpty(cheepsFromFollowed);

            await _repo.Follow(user, authorsToFollow.Last());
        }

        List<Author> following = await _repo.Following(user);
        Assert.Equal(authors.Count, following.Count);
        foreach (Author author in authorsToFollow) {
            Assert.Contains(author, following);
        }
    }
}