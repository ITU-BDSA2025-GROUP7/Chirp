using System.Text;
using System.Threading.Tasks.Dataflow;
using Chirp.Core;
using Chirp.Core.Domain_Model;
using Chirp.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Chirp.Web.Test;

public class CheepRepositoryTest
{
    private ChirpDBContext _context;
    private SqliteConnection _connection;
    private ICheepRepository _cheepRepository;

    public CheepRepositoryTest()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<ChirpDBContext>()
            .UseSqlite(_connection)
            .Options;
        _context = new ChirpDBContext(options);
        _context.Database.EnsureCreated();

        _cheepRepository = new CheepRepository(_context);

        DbInitializer.SeedDatabase(_context);

        _context.SaveChanges();
    }

    [Theory]
    [InlineData("Helge", "ropf@itu.dk")]
    [InlineData("Adrian", "adho@itu.dk")]
    public async Task RequiredAuthorsExist(string name, string email) {
        List<Author> authors = await _cheepRepository.GetAuthorByUserName(name);
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

    [Fact]
    public async Task CheepsDeletedWithAuthor() {
        var author = new Author
            { DisplayName = "DisappearingSoon", Email = "test@itu.dk", UserName = "test@itu.dk" };
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

    /** Test that there is only cheeps from the selected author when getcheepsfromauthor is called
    and that it doesn't crash if the author doesn't exist
    */
    [Theory]
    [InlineData("RogerHistand")]
    [InlineData("LuannaMuro")]
    [InlineData("WendellBallan")]
    public async Task GetCheepsFromAuthor(string name)
    {
        //arrange

        // act
        var cheeps = await _cheepRepository.GetCheepsFromUserName(name, 1);

        //assert
        Assert.NotEmpty(cheeps);
        foreach (var cheep in cheeps)
        {
            Assert.Equal(name, cheep.AuthorUserName);
        }
    }

    [Theory]
    [InlineData("dfiuhweiufhwe")]
    [InlineData("")]
    public async Task GetCheepsFromNonexistentAuthor(string username) {
        List<CheepDTO> cheeps = await _cheepRepository.GetCheepsFromUserName(username, 1);
        Assert.Empty(cheeps);
    }




    /** Testing that the cheeps contain the expeected author, message and timestamp */
    [Theory]
    [InlineData(0, "Jacqualine Gilcoine", "Starbuck now is what we hear the worst.", "2023-08-01 13:17:39")]
    [InlineData(1, "Jacqualine Gilcoine",
        "The train pulled up at his bereavement; but his eyes riveted upon that heart for ever; who ever conquered it?",
        "2023-08-01 13:17:36")]
    [InlineData(2, "Jacqualine Gilcoine",
        "I wonder if he''d give a very shiny top hat and my outstretched hand and countless subtleties, to which it contains.",
        "2023-08-01 13:17:34")]
    [InlineData(3, "Mellie Yost", "But what was behind the barricade.", "2023-08-01 13:17:33")]
    [InlineData(4, "Quintin Sitts",
        "It''s bad enough to appal the stoutest man who was my benefactor, and all for our investigation.",
        "2023-08-01 13:17:32")]
    [InlineData(5, "Jacqualine Gilcoine",
        "Seems to me of Darmonodes'' elephant that so caused him to the kitchen door.", "2023-08-01 13:17:29")]
    public async Task ReadCheepsTest(int index, string author, string message, string timestamp)
    {
        var cheep = await _cheepRepository.GetCheeps(1);
        Assert.Equal(author, cheep[index].AuthorDisplayName);
        Assert.Equal(message, cheep[index].Message);
        Assert.Equal(timestamp, cheep[index].TimeStamp);
    }

    /** tests that we don't get a different result, when querying again after no changes have been made*/
    [Fact]
    public async Task SubsequentReadsReturnSameData()
    {
        var firstRead = await _cheepRepository.GetCheeps(10);
        var secondRead = await _cheepRepository.GetCheeps(10);
        Assert.Equivalent(firstRead, secondRead);
    }

    /// Test of pagination
    [Fact]
    public async Task PaginationTest()
    {
        var cheeps1 = await _cheepRepository.GetCheeps(1);
        Assert.Equal(32, cheeps1.Count);

        // there should also be 32 cheeps on the second page
        var cheeps2 = await _cheepRepository.GetCheeps(2);
        Assert.Equal(32, cheeps2.Count);

        // unique pages
        Assert.NotStrictEqual(cheeps1, cheeps2);

        // no cheeps on a page where there isn't enough
        var cheeps400 = await _cheepRepository.GetCheeps(400);
        Assert.Empty(cheeps400);
    }

    /// test of timestamp sorting
    [Fact]
    public async Task TimestampSortedTest()
    {
        // var service = new CheepService(_context);
        // DbInitializer.SeedDatabase(_context);

        var cheeps = await _cheepRepository.GetCheeps(1);
        Assert.Equal(32, cheeps.Count);
        string lastTimeStamp = "2050-07-10 21:21:13";

        foreach (var cheep in cheeps)
        {
            // descending timestamps
            Assert.True(DateTime.Parse(lastTimeStamp) >= DateTime.Parse(cheep.TimeStamp));
            lastTimeStamp = cheep.TimeStamp;
        }
    }

    /// tests that it just returns the first page in case it get negative or weird numbers
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task PaginationEdgeCaseTest(int pagenr)
    {

        // page 1
        var cheeps1 = await _cheepRepository.GetCheeps(1);
        var cheepsWeird = await _cheepRepository.GetCheeps(pagenr);

        Assert.Equal(32, cheepsWeird.Count);
        Assert.Equivalent(cheeps1, cheepsWeird);
    }

    [Fact]
    public async Task CreateAuthorTest()
    {
        string name, email;
        name = "Barton Cooper";
        email = "cooper@copper.com";
        await _cheepRepository.CreateAuthor(name, email);
        var query = (from author in _context.Authors
                     where author.DisplayName == name
                     select author);
        Author actualAuthor = await query.FirstAsync();
        Assert.Equal(email, actualAuthor.Email);
    }

    [Fact]
    public async Task AuthorReusingEmailTest()
    {
        string name1, name2, email;
        name1 = "Barton Cooper";
        name2 = "Bar2n Cooper";
        email = "cooper@copper.com";
        await _cheepRepository.CreateAuthor(name1, email);
        await Assert.ThrowsAsync<DbUpdateException>(() => _cheepRepository.CreateAuthor(name2, email));
    }

    [Fact]
    public async Task AuthorSameNameTest()
    {
        const string name = "Barton Cooper";
        string username = name.Replace(" ", "");
        const string email1 = "TheCakeMaster@copper.com";
        const string email2 = "muffinEnjoyer@copper.com";
        await _cheepRepository.CreateAuthor(name, email1);
        await Assert.ThrowsAsync<DbUpdateException>(() => _cheepRepository.CreateAuthor(name, email2));
        List<Author> bartons = await _cheepRepository.GetAuthor(username);
        Assert.Equal(name, bartons.Single().DisplayName);
        Assert.Equal(username, bartons.Single().UserName);
        Assert.Equal(email1, bartons.Single().Email);
    }

    [Fact]
    public async Task NoKnownAuthorTest()
    {
        List<Author> authorsFound = await _cheepRepository.GetAuthor("ThisNameorEmailDoesNotExist");
        Assert.Empty(authorsFound);
    }

    [Fact]
    public async Task AuthorBlankName()
    {
        string name, email;
        name = "";
        email = "cooper@copper.com";
        await _cheepRepository.CreateAuthor(name, email);
        var query = (from author in _context.Authors
                     where author.DisplayName == ""
                     select author);
        Author actualAuthor = query.Single();
        Assert.NotNull(actualAuthor);
    }

    [Fact]
    public async Task CheepOwnershipTest()
    {
        List<Author> users = await _cheepRepository.GetAuthor("WendellBallan");
        string message = "I really like turtles";
        DateTime date = DateTime.Parse("2023-08-02 14:13:45");
        await _cheepRepository.CreateCheep(users.Single(), message, date);
        var query = (from author in _context.Authors
                     where author.DisplayName == "Wendell Ballan"
                     select author.Cheeps);
        Assert.NotEmpty(query);
        // The first author returned by the query's last cheep.
        string actualmessage = query.First().Last().Text;
        Assert.Equal(message, actualmessage);
    }

    /** Tests the outcome of creating a cheep whose message should be acceptable,
     * whether it is empty, is a normal string, or includes an attempt at SQL injection.
     */
    [Theory]
    [InlineData("")]
    [InlineData("I like turtles")]
    [InlineData("msg', '2023-08-02 13:13:45'); DROP TABLE Cheeps;")]
    public async Task CreateCheepTest(string message)
    {
        var queryBefore = (from cheep in _context.Cheeps
            where cheep.Text == message
            select cheep);
        Assert.Empty(queryBefore);

        List<Author> authors = await _cheepRepository.GetAuthor("WendellBallan");
        Assert.NotEmpty(authors);
        DateTime date = DateTime.Parse("2023-08-02 13:13:45");
        await _cheepRepository.CreateCheep(authors.First(), message, date);
        var query = (from cheep in _context.Cheeps
            where cheep.Text == message
            select cheep);
        Cheep createdcheep = query.Single();
        Assert.Equal(createdcheep.Text, message);
    }

    /** Tests that writing a test that is beyond the limit in length fails,
     * throwing an exception.
     */
    [Fact]
    public async Task CreateTooLongCheepTest()
    {
        List<Author> authors = await _cheepRepository.GetAuthor("WendellBallan");
        Assert.NotEmpty(authors);
        StringBuilder sb = new StringBuilder(160);
        while (sb.Length <= Cheep.MAX_TEXT_LENGTH) {
            sb.Append("Cheep text");
        }
        string message = sb.ToString();

        DateTime date = DateTime.Parse("2023-08-02 13:13:45");
        await Assert.ThrowsAsync<ArgumentException>(() => _cheepRepository.CreateCheep(authors.First(), message, date));
    }

    /**
     * Tests the outcome of writing a cheep whose message is exactly the maximum
     * allowed length.
     */
    [Fact]
    public async Task CreateCheepAtExactlyLimit()
    {
        List<Author> authors = await _cheepRepository.GetAuthor("WendellBallan");
        Assert.NotEmpty(authors);
        StringBuilder sb = new StringBuilder(160);
        while (sb.Length < Cheep.MAX_TEXT_LENGTH) {
            sb.Append('a');
        }
        string message = sb.ToString();
        Assert.Equal(Cheep.MAX_TEXT_LENGTH, message.Length);

        var queryBefore = (from cheep in _context.Cheeps
            where cheep.Text == message
            select cheep);
        Assert.Empty(queryBefore);

        DateTime date = DateTime.Parse("2023-08-02 13:13:45");
        await _cheepRepository.CreateCheep(authors.Single(), message, date);
        var query = (from cheep in _context.Cheeps
            where cheep.Text == message
            select cheep);
        Cheep createdcheep = query.First();
        Assert.Equal(createdcheep.Text, message);
    }

    /**
     * Tests the outcome of creating a cheep whose message length is greater
     * than the allowed limit, and which contains an attempt at SQL injecting.
     */
    [Fact]
    public async Task CreateTooLongSqlInjectionCheepTest()
    {
        List<Author> authors = await _cheepRepository.GetAuthor("WendellBallan");
        Assert.NotEmpty(authors);
        StringBuilder sb = new StringBuilder(160);
        while (sb.Length <= Cheep.MAX_TEXT_LENGTH)
        {
            sb.Append("msg', '2023-08-02 13:13:45'); DROP TABLE Cheeps;");
        }
        string message = sb.ToString();

        DateTime date = DateTime.Parse("2023-08-02 13:13:45");
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _cheepRepository.CreateCheep(authors.Single(), message, date));

        var queryBefore = (from cheep in _context.Cheeps
            where cheep.Text == message
            select cheep);
        Assert.Empty(queryBefore);
    }

    [Fact]
    public async Task AttemptToFollowSelf()
    {
        //arrange
        const string name = "Barton Cooper";
        string username = name.Replace(" ", "");
        const string email1 = "TheCakeMaster@copper.com";
        await _cheepRepository.CreateAuthor(name, email1);
        List<Author> authors= await _cheepRepository.GetAuthor("BartonCooper");
        Author barton = authors.Single();
        //act
        _ = _cheepRepository.Follow(barton, barton);
        //assert
        Assert.Empty(await _cheepRepository.GetFollowRelations(barton));
    }

    [Fact]
    public async Task AttemptToFollowSomeone()
    {
        //arrange
        const string name = "Barton Cooper";
        string username = name.Replace(" ", "");
        const string email1 = "TheCakeMaster@copper.com";
        await _cheepRepository.CreateAuthor(name, email1);
        List<Author> authors1= await _cheepRepository.GetAuthor("WendellBallan");
        Author Wendell = authors1.Single();
        List<Author> authors2= await _cheepRepository.GetAuthor("BartonCooper");
        Author barton = authors2.Single();
        //act
        _ = _cheepRepository.Follow(barton, Wendell);
        //assert
        Assert.NotEmpty(await _cheepRepository.GetFollowRelations(barton));
    }

    [Fact]
    public async Task AttemptToFollowSomeoneAlreadyFollowed()
    {
        //arrange
        const string name = "Barton Cooper";
        string username = name.Replace(" ", "");
        const string email1 = "TheCakeMaster@copper.com";
        await _cheepRepository.CreateAuthor(name, email1);
        List<Author> authors1= await _cheepRepository.GetAuthor("WendellBallan");
        Author Wendell = authors1.Single();
        List<Author> authors2= await _cheepRepository.GetAuthor("BartonCooper");
        Author barton = authors2.Single();
        //act
        _ = _cheepRepository.Follow(barton, Wendell);
        _ = _cheepRepository.Follow(barton, Wendell);
        List<Author> myList= await _cheepRepository.Following(barton);
        //assert
        Assert.Single(myList);
    }

    [Fact]
    public async Task AttemptToUnfollowSomeoneFollowed()
    {
        //arrange
        const string name = "Barton Cooper";
        string username = name.Replace(" ", "");
        const string email1 = "TheCakeMaster@copper.com";
        await _cheepRepository.CreateAuthor(name, email1);
        List<Author> authors1= await _cheepRepository.GetAuthor("WendellBallan");
        Author Wendell = authors1.Single();
        List<Author> authors2= await _cheepRepository.GetAuthor("BartonCooper");
        Author barton = authors2.Single();
        //act
        _ = _cheepRepository.Follow(barton, Wendell);
        _ = _cheepRepository.Unfollow(barton, Wendell);
        //assert
        Assert.Empty(await _cheepRepository.GetFollowRelations(barton));
    }

    [Fact]
    public async Task AttemptToUnfollowSomeoneNotFollowed()
    {
        //arrange
        const string name = "Barton Cooper";
        string username = name.Replace(" ", "");
        const string email1 = "TheCakeMaster@copper.com";
        await _cheepRepository.CreateAuthor(name, email1);
        List<Author> authors1= await _cheepRepository.GetAuthor("WendellBallan");
        Author Wendell = authors1.Single();
        List<Author> authors2= await _cheepRepository.GetAuthor("BartonCooper");
        Author barton = authors2.Single();
        //act
        _ = _cheepRepository.Unfollow(barton, Wendell);
        //assert
        Assert.Empty(await _cheepRepository.GetFollowRelations(barton));
    }

    [Fact]
    public async Task FollowNull()
    {
        //arrange
        const string name = "Barton Cooper";
        string username = name.Replace(" ", "");
        const string email1 = "TheCakeMaster@copper.com";
        await _cheepRepository.CreateAuthor(name, email1);
        List<Author> authors2= await _cheepRepository.GetAuthor("BartonCooper");
        Author barton = authors2.Single();
        Author? Wendell = null;
        //act
        _ = _cheepRepository.Follow(barton, Wendell);
        //assert
        Assert.Empty(await _cheepRepository.GetFollowRelations(barton));
    }
    [Fact]
    public async Task FollowAuthorNotInDbContext()
    {
        //arrange
        const string name = "Barton Cooper";
        string username = name.Replace(" ", "");
        const string email1 = "TheCakeMaster@copper.com";
        await _cheepRepository.CreateAuthor(name, email1);
        List<Author> authors2= await _cheepRepository.GetAuthor("BartonCooper");
        Author barton = authors2.Single();
        Author myAuthor = Author.Create("Bartoon2", "batman@gmail.com");
        //act
        _ = _cheepRepository.Follow(barton, myAuthor);
        //assert
        Assert.Empty(await _cheepRepository.GetFollowRelations(barton));
    }

    /**
     * Test whether Isfollowing behaves as intended 
     */
    [Fact]
    public async Task FollowTest()
    {
        //arrange
        const string nameA = "Barton Cooper";
        string usernameA = nameA.Replace(" ", "");
        const string emailA = "TheCakeMaster@copper.com";
        await _cheepRepository.CreateAuthor(nameA, emailA);
        Author authorA= (await _cheepRepository.GetAuthor(usernameA)).First();
        const string nameB = "Abba Booper";
        string usernameB = nameB.Replace(" ", "");
        const string emailB = "Abba@Booper.com";
        await _cheepRepository.CreateAuthor(nameB, emailB);
        Author authorB = (await _cheepRepository.GetAuthor(usernameB)).First();
        
        // act
        var AFollowBBefore = await _cheepRepository.IsFollowing(authorA,authorB);
        var BFollowABefore = await _cheepRepository.IsFollowing(authorB,authorA);
        await _cheepRepository.Follow(authorA, authorB); 
        var AFollowBAfter = await _cheepRepository.IsFollowing(authorA,authorB);
        var BFollowAAfter = await _cheepRepository.IsFollowing(authorB,authorA);
        
        // Assert
        Assert.False(AFollowBBefore);
        Assert.False(BFollowABefore);
        Assert.True(AFollowBAfter);
        Assert.False(BFollowAAfter);
    }
}