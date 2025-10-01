using System.Net.Http.Json;
using Xunit;
using Chirp.CSVDBService;
using Chirp.General;
using Microsoft.AspNetCore.Mvc.Testing;
using Chirp.DBFacade;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Chirp.ServicesTest;

public class ServicesTest : IClassFixture<WebApplicationFactory<Services>>, IDisposable
{
    private readonly WebApplicationFactory<Services> _factory;

    public ServicesTest(WebApplicationFactory<Services> factory) {
        _factory = factory.WithWebHostBuilder(builder => {
            builder.ConfigureServices(services => {
                Environment.SetEnvironmentVariable(DBEnv.envCHIRPDBPATH, Guid.NewGuid().ToString("N") + ".db");
                Environment.SetEnvironmentVariable(DBEnv.envDATA, "data/testDump.sql");
            });
        });

        Console.SetOut(new StringWriter());
    }
    
    
    // read tests 
    private async Task<List<Cheep>> GetCheepsAsync()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/cheeps");
        response.EnsureSuccessStatusCode();
        var cheeps = await response.Content.ReadFromJsonAsync<List<Cheep>>();
        return cheeps ?? new List<Cheep>();
    }

    [Theory]
    [InlineData("ropf", "Hello, BDSA students!", 1690891760L)]
    [InlineData("adho", "Welcome to the course!", 1690978778L)]
    public async Task ReadCheepsTest(string author, string message, long timestamp)
    {
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/cheep", new Cheep(author, message, timestamp));
        // Act
        var cheeps = await GetCheepsAsync();
        // Assert
        Assert.Contains(cheeps, c => c.Author == author && c.Message == message && c.Timestamp == timestamp);
    }
    
    [Fact]
    public async Task SubsequentReadsReturnSameData()
    {
        var client = _factory.CreateClient();
        var cheep = new Cheep("test", "subsequent read", 12345);
        await client.PostAsJsonAsync("/cheep", cheep);

        var firstRead = await GetCheepsAsync();
        var secondRead = await GetCheepsAsync();

        Assert.Equal(firstRead.Count, secondRead.Count);
        Assert.Equal(firstRead[0], secondRead[0]);
    }
    
    
    
    /*Tests if the server crashes when pushing something that's not a cheep*/
    [Fact]
    public async Task PushToServerFalty()
    {
        //Arrange
        var client = _factory.CreateClient();
        
        var responseBefore = await client.GetAsync("/cheeps"); // Read what the database contains before
   	    var resultBefore = await responseBefore.Content.ReadAsStringAsync();
        
         // act
         await client.PostAsJsonAsync("/cheep", "Hello i am not a cheep!");
         
         // Assert
        var responseAfter = await client.GetAsync("/cheeps"); // read from the database contains after
         var resultAfter = await responseAfter.Content.ReadAsStringAsync();
         
         Assert.Equal(resultBefore, resultAfter); }
    
    
    /** Asserts that valid (and essentially valid) entries are written and read
     * as expected without throwing errors. This includes empty strings, messages
     * that are not properly surrounded by quotation marks, and timestamps that
     * are negative, as well as at the limit of what can be stored in a <see cref="long">long</see>.
     */
    [Theory]
    [InlineData("testauthor", "\"Test message, that's the way it is!!\"",
        "\"Test message, that's the way it is!!\"", 1757601000L)]
    [InlineData("testauthor", "Test message that's the way it is!!",
        "Test message that's the way it is!!", 1757601000L)]
    [InlineData("", "\"Test message, that's the way it is!!\"",
        "\"Test message, that's the way it is!!\"", 1757601000L)]
    [InlineData("testauthor", "", "", 1757601000L)]
    [InlineData("testauthor", "\"\"", "\"\"", 1757601000L)]
    [InlineData("testauthor", "\"\n\"", "\"\n\"", 1757601000L)]
    [InlineData("testauthor", "\"Test message, that's the way it is!!\"",
        "\"Test message, that's the way it is!!\"", long.MaxValue)]
    public async Task write(string author, string writtenMessage, string readMessage, long timestamp) {
        //Arrange
        var client = _factory.CreateClient();
        var message = new Cheep(author, writtenMessage, timestamp);

        var responseBefore = await client.GetAsync("/cheeps"); // Read what the database contains before
        var resultBefore = await responseBefore.Content.ReadAsStringAsync();

        // act
        await client.PostAsJsonAsync("/cheep", message);

        // Assert
        var responseAfter = await client.GetAsync("/cheeps"); // read what the database contains after
        var resultAfter = await responseAfter.Content.ReadAsStringAsync();
        var cheeps = await responseAfter.Content.ReadFromJsonAsync<List<Cheep>>();
        Assert.NotNull(cheeps);
        Assert.NotEqual(resultBefore, resultAfter);

        var firstCheep = cheeps[0];
        Assert.Equal(author, firstCheep.Author);
        Assert.Equal(readMessage, firstCheep.Message);
        Assert.Equal(timestamp, firstCheep.Timestamp);
    }

    [Theory]
    [InlineData("testauthor", "\"Test message, that's the way it is!!\"",
        "\"Test message, that's the way it is!!\"", -1757601000L)]
    [InlineData("testauthor", "Test message that's the way it is!!",
        "Test message that's the way it is!!", -1757601000L)]
    [InlineData("", "\"Test message, that's the way it is!!\"",
        "\"Test message, that's the way it is!!\"", -1757601000L)]
    [InlineData("testauthor", "", "", -1757601000L)]
    [InlineData("testauthor", "\"\"", "\"\"", -1757601000L)]
    [InlineData("testauthor", "\"\n\"", "\"\n\"", -1757601000L)]
    [InlineData("testauthor", "\"Test message, that's the way it is!!\"",
        "\"Test message, that's the way it is!!\"", long.MinValue)]
    public async Task WriteNegativeTimestamp(string author, string writtenMessage,
            string readMessage, long timestamp) {
        //Arrange
        var client = _factory.CreateClient();
        var message = new Cheep(author, writtenMessage, timestamp);
        
        var responseBefore = await client.GetAsync("/cheeps"); // Read what the database contains before
        var resultBefore = await responseBefore.Content.ReadAsStringAsync();
        
        // act
        await client.PostAsJsonAsync("/cheep", message);
        
        // Assert
        var responseAfter = await client.GetAsync("/cheeps"); // read what the database contains after
        var resultAfter = await responseAfter.Content.ReadAsStringAsync();
        var cheeps = await responseAfter.Content.ReadFromJsonAsync<List<Cheep>>();
        Assert.NotNull(cheeps);
        Assert.NotEqual(resultBefore, resultAfter);

        var lastCheep = cheeps[^1];
        Assert.Equal(author, lastCheep.Author);
        Assert.Equal(readMessage, lastCheep.Message);
        Assert.Equal(timestamp, lastCheep.Timestamp);
    }

    /** Missing <c>"</c> before and after the message causes a TypeConverterException if the
     * message includes a comma, as the text after the comma will be understood as a long.
     * We test that the programme handles this smoothly without crashing. */
    [Theory]
    [InlineData("testauthor", "Test message, that's the way it is!!", 1757601000L)]
    [InlineData("testauthor", "Test message, that's the way it is!!", -1757601000L)]
    [InlineData("testauthor", ",", 1757601000L)]
    [InlineData("testauthor", ",", -1757601000L)]
    
    public async Task WriteTypeConverterException(string author, string writtenMessage, long timestamp)
    {
        //Arrange
        var client = _factory.CreateClient();
        var message = new Cheep(author, writtenMessage, timestamp);
        
        var responseBefore = await client.GetAsync("/cheeps"); 
        var resultBefore = await responseBefore.Content.ReadAsStringAsync();
        
        // act
        await client.PostAsJsonAsync("/cheep", message);
        
        // Assert
        var responseAfter = await client.GetAsync("/cheeps"); 
        var resultAfter = await responseAfter.Content.ReadAsStringAsync();

        Assert.NotEqual(resultBefore, resultAfter);
    }

    [Theory]
    [InlineData("testauthor", "\n", 1757601000L)]
    [InlineData("testauthor", "\n", -1757601000L)]
    public async Task WriteMissingFieldException(string author, string writtenMessage, long timestamp)
    {
        var client = _factory.CreateClient();
        var message = new Cheep(author, writtenMessage, timestamp);

        var responseBefore = await client.GetAsync("/cheeps");
        var resultBefore = await responseBefore.Content.ReadAsStringAsync();

        await client.PostAsJsonAsync("/cheep", message);

        var responseAfter = await client.GetAsync("/cheeps");
        var resultAfter = await responseAfter.Content.ReadAsStringAsync();

        Assert.NotEqual(resultBefore, resultAfter);
    }
    
    
    [Theory]
    [InlineData("testauthor", "\"Test message, that's the way it is!!", 1757601000L)]
    [InlineData("testauthor", "Test message that's the way it is!!\"", 1757601000L)]
    [InlineData("testauthor", "Test message that's\" the way it is!!", 1757601000L)]
    [InlineData("testauthor", "\"\"\"", 1757601000L)]
    [InlineData("testauthor", "\"", 1757601000L)]
    public async Task WriteBadData(string author, string writtenMessage, long timestamp)
    {
        var client = _factory.CreateClient();
        var message = new Cheep(author, writtenMessage, timestamp);

        var responseBefore = await client.GetAsync("/cheeps");
        var resultBefore = await responseBefore.Content.ReadAsStringAsync();

        await client.PostAsJsonAsync("/cheep", message);

        var responseAfter = await client.GetAsync("/cheeps");
        var resultAfter = await responseAfter.Content.ReadAsStringAsync();

        Assert.NotEqual(resultBefore, resultAfter); // nothing gets stored 
    }

    /*test reading of cheeps stored in the database, along with the index*/
    [Fact]
    public async Task ReadCheeps() {
        var client = _factory.CreateClient();
        
        var message1 = new Cheep("ropf", "Hello, BDSA students!", 1690891760L);
        await client.PostAsJsonAsync("/cheep", message1);
        var message2 = new Cheep("adho", "Welcome to the course!", 1690978778L);
        await client.PostAsJsonAsync("/cheep", message2);

        var response = await client.GetAsync("/cheeps");
        var cheeps = await response.Content.ReadFromJsonAsync<List<Cheep>>() ?? new List<Cheep>();

        int index1 = cheeps.IndexOf(message1);
        int index2 = cheeps.IndexOf(message2);

        Assert.True(index1 != -1 && index2 != -1);
        Assert.True(index2 < index1); // Message2 has higher timestamp, so should come first

        Assert.Equal(message2, cheeps[index2]);
        Assert.Equal(message1, cheeps[index1]);
    }
    
    // test that we can read a page and get a result that is of length PAGE_SIZE
    [Fact]
    public async Task ReadPage()
    {
        // anrange
        var client = _factory.CreateClient();
        
        // act
        var response = await client.GetAsync("/cheepsPage?page=1");
        
        //assert
        var page1 = await response.Content.ReadFromJsonAsync<List<Cheep>>() ?? new List<Cheep>();
        Assert.Equal(Services.PAGE_SIZE, page1.Count);
    }

    [Theory]
    [InlineData("Jacqualine Gilcoine", false)]
    [InlineData("Adrian", false)]
    [InlineData("", true)]
    [InlineData("\n", true)]
    [InlineData("NOT IN YOU DATABSE288282882827172672", true)]
    public async Task ReadPageUser(string author, bool shouldBeEmpty)
    {
        // arrange 
        var client = _factory.CreateClient();
        
        // act
        var response = await client.GetAsync("/cheepsPageWithAuthor?page=1&author=" + author);
        
        var page = await response.Content.ReadFromJsonAsync<List<Cheep>>() ?? new List<Cheep>();
        Assert.True(response.IsSuccessStatusCode);
        foreach (var cheep in page)
        {
            Assert.Equal(author, cheep.Author);
        }
        Assert.Equal(shouldBeEmpty, page.Count == 0);
    }
    
    // test that when changing the page we get differen answers
    [Fact]
    public async Task PagesAreUnique()
    {
        // arrange
        var client = _factory.CreateClient();
        
        // act
        var response1 = await client.GetAsync("/cheepsPage?page=1");
        var response2 = await client.GetAsync("/cheepsPage?page=2");

        // assert
        var page1 =  await response1.Content.ReadFromJsonAsync<List<Cheep>>() ?? new List<Cheep>();
        var page2 = await response2.Content.ReadFromJsonAsync<List<Cheep>>() ?? new List<Cheep>();
        Assert.Equal(page1.Count, page2.Count);
        for (var i = 0; i < page1.Count; i++)
        {
            bool equalAuthor = page1[i].Author == page2[i].Author;
            bool equalMessage = page1[i].Message == page2[i].Message;
            bool equalTimestamp = page1[i].Timestamp == page2[i].Timestamp;
            Assert.False(equalAuthor &&  equalMessage && equalTimestamp); // they should not be equal
        }
    }
    
    //test that we can ask for a page witch does not exist
    [Fact]
    public async Task PagesDoesNotExist()
    {
        // arrange
        var client = _factory.CreateClient();
        int max = Int32.MaxValue / 32;
        
        // act
        var response = await client.GetAsync("/cheepsPage?page=" + max);
        
        // assert
        var page = await response.Content.ReadFromJsonAsync<List<Cheep>>() ?? new  List<Cheep>();
        Assert.Empty(page);
    }
    
    // test that when changing the page we get differen answers. Even when we ask by name
    [Fact]
    public async Task PagesAreUniqueButHasSameName()
    {
        // arrange
        var client = _factory.CreateClient();
        string author = "Jacqualine Gilcoine";
        
        // act
        var response1 = await client.GetAsync("/cheepsPageWithAuthor?page=1&author=" + author);
        var response2 = await client.GetAsync("/cheepsPageWithAuthor?page=2&author=" + author);

        // assert
        var page1 =  await response1.Content.ReadFromJsonAsync<List<Cheep>>() ?? new List<Cheep>();
        var page2 = await response2.Content.ReadFromJsonAsync<List<Cheep>>() ?? new List<Cheep>();
        Assert.True(page1.Count >= 3);
        Assert.True(page2.Count >= 3);
        for (var i = 0; i < 3; i++)
        {
            bool equalAuthor = page1[i].Author == page2[i].Author;
            bool equalMessage = page1[i].Message == page2[i].Message;
            bool equalTimestamp = page1[i].Timestamp == page2[i].Timestamp;
            Assert.False(equalAuthor && equalMessage && equalTimestamp); // they should not be the same post
        }

        // All authors should be of the requested author
        foreach (var cheep in page1) Assert.Equal(author, cheep.Author);
        foreach (var cheep in page2) Assert.Equal(author, cheep.Author);
        
    }
    
    public void Dispose() {
        DBFacade<Cheep>.Reset();
    }
}