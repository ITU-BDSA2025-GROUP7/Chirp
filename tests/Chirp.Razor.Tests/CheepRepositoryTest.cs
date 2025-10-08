
using Xunit;
namespace Chirp.Razor;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;


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
    and that it doesn't crash if the author doesn't exists
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




}