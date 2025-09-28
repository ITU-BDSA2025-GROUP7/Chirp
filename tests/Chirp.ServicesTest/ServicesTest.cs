using System.Net.Http.Json;
using Xunit;
using Chirp.CSVDBService;
using Chirp.General;
using Microsoft.AspNetCore.Mvc.Testing;
[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Chirp.ServicesTest;

public class ServicesTest : IClassFixture<WebApplicationFactory<Services>>, IDisposable
{
    private readonly WebApplicationFactory<Services> _factory;
    private string _tempPath;

    public ServicesTest(WebApplicationFactory<Services> factory)
    {
        _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        File.WriteAllText(_tempPath, "author,message,timestamp\n");

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                Chirp.CSVDB.CsvDataBase<Cheep>.Reset();
                Chirp.CSVDB.CsvDataBase<Cheep>.Instance.SetPath(_tempPath);
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
        "Test message, that's the way it is!!", 1757601000L)]
    [InlineData("testauthor", "Test message that's the way it is!!",
        "Test message that's the way it is!!", 1757601000L)]
    [InlineData("", "\"Test message, that's the way it is!!\"",
        "Test message, that's the way it is!!", 1757601000L)]
    [InlineData("testauthor", "", "", 1757601000L)]
    [InlineData("testauthor", "\"\"", "", 1757601000L)]
    [InlineData("testauthor", "\"\n\"", "\n", 1757601000L)]
    [InlineData("testauthor", "\"Test message, that's the way it is!!\"",
        "Test message, that's the way it is!!", -1757601000L)]
    [InlineData("testauthor", "Test message that's the way it is!!",
        "Test message that's the way it is!!", -1757601000L)]
    [InlineData("", "\"Test message, that's the way it is!!\"",
        "Test message, that's the way it is!!", -1757601000L)]
    [InlineData("testauthor", "", "", -1757601000L)]
    [InlineData("testauthor", "\"\"", "", -1757601000L)]
    [InlineData("testauthor", "\"\n\"", "\n", -1757601000L)]
    [InlineData("testauthor", "\"Test message, that's the way it is!!\"",
        "Test message, that's the way it is!!", long.MaxValue)]
    [InlineData("testauthor", "\"Test message, that's the way it is!!\"",
        "Test message, that's the way it is!!", long.MinValue)]
    public async Task write(string author, string writtenMessage, string readMessage, long timestamp)
    {
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
        
        var lastCheep = cheeps[cheeps.Count - 1];
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
        
        Assert.Equal(resultBefore, resultAfter);
        
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

        Assert.Equal(resultBefore, resultAfter); 
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

        Assert.Equal(resultBefore, resultAfter); // nothing gets stored 
    }
    /*test reading of cheeps stored in the database, along with the index*/
   [Fact]
    public async Task ReadCheeps()
    {
        var client = _factory.CreateClient();
        
        
        var message1 = new Cheep("ropf", "Hello, BDSA students!", 1690891760L);
        await client.PostAsJsonAsync("/cheep", message1);
        var message2 = new Cheep("adho", "Welcome to the course!", 1690978778L);
        await client.PostAsJsonAsync("/cheep", message2);
        
        var response = await client.GetAsync("/cheeps");
        var cheeps = await response.Content.ReadFromJsonAsync<List<Cheep>>() ?? new List<Cheep>();
        
        var firstCheep = cheeps[0] ;
        var secondCheep = cheeps[1] ;
        
        
        Assert.Equal("ropf", firstCheep.Author);
        Assert.Equal("Hello, BDSA students!", firstCheep.Message);
        Assert.Equal(1690891760L, firstCheep.Timestamp);
        
        Assert.Equal("adho", secondCheep.Author);
        Assert.Equal("Welcome to the course!", secondCheep.Message);
        Assert.Equal(1690978778L, secondCheep.Timestamp);
    }

    public async Task readPage()
    {
        var client _factory.CreateClient();
    }
    
    public void Dispose()
    {
        Chirp.CSVDB.CsvDataBase<Cheep>.Reset();
        
        if (File.Exists(_tempPath))
            File.Delete(_tempPath);
        
    }
    
}

