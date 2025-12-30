using System.Text;
using Chirp.Core;
using static Chirp.Infrastructure.ICheepRepository;
using Chirp.Core.Domain_Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Chirp.Infrastructure.Test;

public class CheepRepositoryTest {
    private ChirpDBContext _context;
    private SqliteConnection _connection;
    private ICheepRepository _cheepRepository;
    private IAuthorRepository _authorRepository;

    public CheepRepositoryTest() {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        DbContextOptions<ChirpDBContext> options = new DbContextOptionsBuilder<ChirpDBContext>()
                                                  .UseSqlite(_connection)
                                                  .Options;
        _context = new ChirpDBContext(options);
        _context.Database.EnsureCreated();

        _cheepRepository = new CheepRepository(_context);
        _authorRepository = new AuthorRepository(_context);

        DbInitializer.SeedDatabase(_context);

        _context.SaveChanges();
    }


    /** Test that there is only cheeps from the selected author when getcheepsfromauthor is called
    and that it doesn't crash if the author doesn't exist
    */
    [Theory]
    [InlineData("RogerHistand")]
    [InlineData("LuannaMuro")]
    [InlineData("WendellBallan")]
    public async Task GetCheepsFromAuthor(string name) {
        //arrange

        // act
        var cheeps = await _cheepRepository.GetCheepsFromUserName(name, 1);

        //assert
        Assert.NotEmpty(cheeps);
        foreach (var cheep in cheeps) {
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
    [InlineData(0, "Jacqualine Gilcoine", "Starbuck now is what we hear the worst.",
                "2023-08-01 13:17:39")]
    [InlineData(1, "Helge", "Hello, BDSA students!", "2023-08-01 13:17:37")]
    [InlineData(2, "Jacqualine Gilcoine",
                "The train pulled up at his bereavement; but his eyes riveted upon that heart for ever; who ever conquered it?",
                "2023-08-01 13:17:36")]
    [InlineData(3, "Jacqualine Gilcoine",
                "I wonder if he''d give a very shiny top hat and my outstretched hand and countless subtleties, to which it contains.",
                "2023-08-01 13:17:34")]
    [InlineData(4, "Mellie Yost", "But what was behind the barricade.", "2023-08-01 13:17:33")]
    [InlineData(5, "Quintin Sitts",
                "It''s bad enough to appal the stoutest man who was my benefactor, and all for our investigation.",
                "2023-08-01 13:17:32")]
    [InlineData(6, "Jacqualine Gilcoine",
                "Seems to me of Darmonodes'' elephant that so caused him to the kitchen door.",
                "2023-08-01 13:17:29")]
    public async Task ReadCheepsTest(int index, string author, string message, string timestamp) {
        var cheep = await _cheepRepository.GetCheeps(1);
        Assert.Equal(author, cheep[index].AuthorDisplayName);
        Assert.Equal(message, cheep[index].Message);
        Assert.Equal(timestamp, cheep[index].TimeStamp);
    }

    /** tests that we don't get a different result, when querying again after no changes have been made*/
    [Fact]
    public async Task SubsequentReadsReturnSameData() {
        var firstRead = await _cheepRepository.GetCheeps(10);
        var secondRead = await _cheepRepository.GetCheeps(10);
        Assert.Equivalent(firstRead, secondRead);
    }

    /// Test of pagination
    [Fact]
    public async Task PaginationTest() {
        var cheeps1 = await _cheepRepository.GetCheeps(1);
        Assert.Equal(CHEEPS_PER_PAGE, cheeps1.Count);

        // there should also be 32 cheeps on the second page
        var cheeps2 = await _cheepRepository.GetCheeps(2);
        Assert.Equal(CHEEPS_PER_PAGE, cheeps2.Count);

        // unique pages
        Assert.NotStrictEqual(cheeps1, cheeps2);

        // no cheeps on a page where there isn't enough
        var cheeps400 = await _cheepRepository.GetCheeps(400);
        Assert.Empty(cheeps400);
    }

    /// test of timestamp sorting
    [Fact]
    public async Task TimestampSortedTest() {
        // var service = new CheepService(_context);
        // DbInitializer.SeedDatabase(_context);

        var cheeps = await _cheepRepository.GetCheeps(1);
        Assert.Equal(CHEEPS_PER_PAGE, cheeps.Count);
        string lastTimeStamp = "2050-07-10 21:21:13";

        foreach (var cheep in cheeps) {
            // descending timestamps
            Assert.True(DateTime.Parse(lastTimeStamp) >= DateTime.Parse(cheep.TimeStamp));
            lastTimeStamp = cheep.TimeStamp;
        }
    }

    /// tests that it just returns the first page in case it get negative or weird numbers
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task PaginationEdgeCaseTest(int pagenr) {
        // page 1
        var cheeps1 = await _cheepRepository.GetCheeps(1);
        var cheepsWeird = await _cheepRepository.GetCheeps(pagenr);

        Assert.Equal(CHEEPS_PER_PAGE, cheepsWeird.Count);
        Assert.Equivalent(cheeps1, cheepsWeird);
    }


    /**
     * It's possible to retrieve a users cheeps based on the author.cheep
     */
    [Fact]
    public async Task CheepOwnershipTest() {
        AuthorDTO authorDTO = (await _authorRepository.GetAuthor("WendellBallan")).Single();
        Author? user = GetAuthorFromDatabase(authorDTO);
        string message = "I really like turtles";
        DateTime date = DateTime.Parse("2023-08-02 14:13:45");
        await _cheepRepository.CreateCheep(user!, message, date);
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
    public async Task CreateCheepTest(string message) {
        IQueryable<Cheep> queryBefore = (from cheep in _context.Cheeps
                                         where cheep.Text == message
                                         select cheep);
        Assert.Empty(queryBefore);

        AuthorDTO authorDTO = (await _authorRepository.GetAuthor("WendellBallan")).Single();
        Author? user = GetAuthorFromDatabase(authorDTO);
        DateTime date = DateTime.Parse("2023-08-02 13:13:45");
        await _cheepRepository.CreateCheep(user!, message, date);
        IQueryable<Cheep> query = (from cheep in _context.Cheeps
                                   where cheep.Text == message
                                   select cheep);
        Cheep createdcheep = query.Single();
        Assert.Equal(createdcheep.Text, message);
    }

    /** Tests that writing a test that is beyond the limit in length fails,
     * throwing an exception.
     */
    [Fact]
    public async Task CreateTooLongCheepTest() {
        AuthorDTO authorDTO = (await _authorRepository.GetAuthor("WendellBallan")).Single();
        Author? user = GetAuthorFromDatabase(authorDTO);
        StringBuilder sb = new StringBuilder(160);
        while (sb.Length <= Cheep.MAX_TEXT_LENGTH) {
            sb.Append("Cheep text");
        }

        string message = sb.ToString();

        DateTime date = DateTime.Parse("2023-08-02 13:13:45");
        await Assert.ThrowsAsync<ArgumentException>(() => _cheepRepository.CreateCheep(
                                                        user!, message, date));
    }

    /**
     * Tests the outcome of writing a cheep whose message is exactly the maximum
     * allowed length.
     */
    [Fact]
    public async Task CreateCheepAtExactlyLimit() {
        AuthorDTO authorDTO = (await _authorRepository.GetAuthor("WendellBallan")).Single();
        Author? user = GetAuthorFromDatabase(authorDTO);
        StringBuilder sb = new StringBuilder(160);
        while (sb.Length < Cheep.MAX_TEXT_LENGTH) {
            sb.Append('a');
        }

        string message = sb.ToString();
        Assert.Equal(Cheep.MAX_TEXT_LENGTH, message.Length);

        IQueryable<Cheep> queryBefore = (from cheep in _context.Cheeps
                                         where cheep.Text == message
                                         select cheep);
        Assert.Empty(queryBefore);

        DateTime date = DateTime.Parse("2023-08-02 13:13:45");
        await _cheepRepository.CreateCheep(user!, message, date);
        IQueryable<Cheep> query = (from cheep in _context.Cheeps
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
    public async Task CreateTooLongSqlInjectionCheepTest() {
        AuthorDTO authorDTO = (await _authorRepository.GetAuthor("WendellBallan")).Single();
        Author? user = GetAuthorFromDatabase(authorDTO);
        StringBuilder sb = new StringBuilder(160);
        while (sb.Length <= Cheep.MAX_TEXT_LENGTH) {
            sb.Append("msg', '2023-08-02 13:13:45'); DROP TABLE Cheeps;");
        }

        string message = sb.ToString();

        DateTime date = DateTime.Parse("2023-08-02 13:13:45");
        await Assert.ThrowsAsync<ArgumentException>(() => _cheepRepository.CreateCheep(
                                                        user!, message, date));

        IQueryable<Cheep> queryBefore = (from cheep in _context.Cheeps
                                         where cheep.Text == message
                                         select cheep);
        Assert.Empty(queryBefore);
    }

    /**
     * Tests that creating cheeps are safe from SQL attacks
     */
    [Fact]
    public async Task SqlInjection() {
        // arrange
        List<AuthorDTO> authors = await _authorRepository.GetAuthor("WendellBallan");
        Assert.NotEmpty(authors);
        string dangerusMessage = "Danger!'); drop table Cheeps; --";
        DateTime date = DateTime.Parse("2023-08-02 13:13:45");
        Author? user = GetAuthorFromDatabase(authors.Single());

        // act
        await  _cheepRepository.CreateCheep(user!, dangerusMessage, date);

        // assert
        Assert.NotEmpty(await _cheepRepository.GetAllCheepsFromUserName(authors.First().UserName!));
    }

    /**
     * Deleting a cheep
     */
    [Fact]
    public async Task DeletingCheepTest() {
        string message = "This cheep should be deleted";
        IQueryable<Cheep> queryFirst = (from cheep in _context.Cheeps
                                         where cheep.Text == message
                                         select cheep);
        Assert.Empty(queryFirst);
        // write new cheep
        var author = new Author { DisplayName = "greatName", Email = "yolo@itu.dk", UserName = "username" };

        int cheepCountBeforeCreating = _cheepRepository.TotalCheepCount;
        await _cheepRepository.CreateCheep(author, message, DateTime.Parse("2025-11-28 22:25:45"));
        int cheepCountAfterCreating = _cheepRepository.TotalCheepCount;

        Assert.Equal(cheepCountBeforeCreating + 1, cheepCountAfterCreating);

        // expect it to exist
        IQueryable<Cheep> queryAfterAdding = (from cheep in _context.Cheeps
                                         where cheep.Text == message
                                         select cheep);
        Assert.Equal(message, queryAfterAdding.First().Text);
        Assert.Equal(author, queryAfterAdding.First().Author);
        Assert.Single(queryAfterAdding);

        // delete cheep
        CheepDTO cheepToDelete = new CheepDTO("", message, "2025-11-28 22:25:45", "username");
        await _cheepRepository.DeleteCheep(cheepToDelete);
        Assert.Equal(cheepCountBeforeCreating, _cheepRepository.TotalCheepCount);

        // expect it not to exist
        IQueryable<Cheep> queryAfterDelete = (from cheep in _context.Cheeps
                            where cheep.Text == message
                            select cheep);

        Assert.Empty(queryAfterDelete);
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