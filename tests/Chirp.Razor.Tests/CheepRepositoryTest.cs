
using Chirp.Razor.Domain_Model;
using Xunit;
namespace Chirp.Razor;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

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

    


    /*Test that there is only cheeps from the selected author when getcheepsfromauthor is called
    and that it doesn't crash if the author doesn't exist
*/
    [Theory]
    [InlineData("Roger Histand")]
    [InlineData("Luanna Muro")]
    [InlineData("Wendell Ballan")]
    [InlineData("dfiuhweiufhwe")] //not an author 
    public async Task GetCheepsFromAuthor(string name)
    {
        //arrange

        // act 
        var cheeps = await _cheepRepository.GetCheepsFromAuthor(name, 1);

        //assert
        foreach (var cheep in cheeps)
        {
            Assert.Equal(name, cheep.Author);
            Assert.NotEqual("hjdfiluwriu", cheep.Author);
        }


    }
    
    
    

    /*Testing that the cheeps contain the expeected author, message and timestamp */
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
        //arrange

        // act 
        var cheep = await _cheepRepository.GetCheeps(1);

        //assert
        Assert.Equal(author, cheep[index].Author);
        Assert.Equal(message, cheep[index].Message);
        Assert.Equal(timestamp, cheep[index].TimeStamp);





    }

    /*tests that we don't get a different result, when querying again after no changes have been made*/
    [Fact]
    public async Task SubsequentReadsReturnSameData()
    {
        //arrange


        // act 
        var firstRead = await _cheepRepository.GetCheeps(10);
        var secondRead = await _cheepRepository.GetCheeps(10);

        // assert 

        Assert.Equivalent(firstRead, secondRead);
    }


    // Test of pagination
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

    // test of timestamp sorting 
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

    // tests that it just returns the first page in case it get negative or weird numbers
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
    public async Task CreateCheepTest()
    {
        List<Author> authors = await _cheepRepository.GetAuthor("Wendell Ballan");
        string message = "I like turtles";
        DateTime date = DateTime.Parse("2023-08-02 13:13:45");
        await _cheepRepository.CreateCheep(authors.First(), message, date);
        var query = (from cheep in _cheepRepository.GetDbContext().Cheeps
            where cheep.Text == message
            select cheep);
        Cheep createdcheep = query.First();
        Assert.Equal(createdcheep.Text, message);
    }

    [Fact]
    public async Task CreateAuthorTest()
    {
        string name, email;
        name = "Barton Cooper";
        email = "cooper@copper.com";
        await _cheepRepository.CreateAuthor(name, email);
        var query = (from author in _cheepRepository.GetDbContext().Authors
                     where author.Name == name
                     select author);
        Author actualAuthor = await query.FirstAsync();
        Assert.Equal(email, actualAuthor.Email);
    }

    [Fact]
    public async Task reusingEmailTest()
    {
        string name1, name2, email;
        name1 = "Barton Cooper";
        name2 = "Bar2n Cooper";
        email = "cooper@copper.com";
        await _cheepRepository.CreateAuthor(name1, email);
        await Assert.ThrowsAsync<DbUpdateException>(() => _cheepRepository.CreateAuthor(name2, email));
    }

    [Fact]
    public async Task sameNameTest()
    {
        string name, email1, email2;
        name = "Barton Cooper";
        email1 = "TheCakeMaster@copper.com";
        email2 = "muffinEnjoyer@copper.com";
        await _cheepRepository.CreateAuthor(name, email1);
        await _cheepRepository.CreateAuthor(name, email2);
        List<Author> bartons = await _cheepRepository.GetAuthor("Barton Cooper");
        Assert.NotEqual(bartons.First(), bartons.Last());
    }
    [Fact]
    public async Task noKnownAuthorTest()
    {
        List<Author> authorsFound = await _cheepRepository.GetAuthor("ThisNameorEmailDoesNotExist");
        Assert.Empty(authorsFound);
    }
    
    [Fact]
    public async Task CheepOwnershipTest() 
    {
        List<Author> users = await _cheepRepository.GetAuthor("Wendell Ballan");
        string message = "I really like turtles";
        DateTime date = DateTime.Parse("2023-08-02 14:13:45");
        await _cheepRepository.CreateCheep(users.First(), message, date);
        var query = (from author in _cheepRepository.GetDbContext().Authors
            where author.Name == "Wendell Ballan"
            select author.Cheeps);
        string actualmessage = query.First().Last().Text;
        Assert.Equal(message, actualmessage);
    }

    [Theory]
    [InlineData("Wendell Ballan", 3)]
    [InlineData("Roger Histand", 1)]
    [InlineData("Luanna Muro", 2)]
    [InlineData("Roger+Histand@hotmail.com", 1)]
    [InlineData("Luanna-Muro@ku.dk", 2)]
    [InlineData("Quintin+Sitts@itu.dk", 5)]
    public async Task GetAuthorTest(string identifier, int authorId)
    {
        List<Author> authors =  await _cheepRepository.GetAuthor(identifier);
        Assert.Equal(authors.First().AuthorId, authorId);
    }
}