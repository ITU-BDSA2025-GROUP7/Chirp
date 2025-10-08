using Chirp.General;
using Xunit;
using Chirp.Razor;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Chirp.Razor;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Chirp.Razor.Domain_Model;

public class CheepServiceTest {
    
    public ChirpDBContext Context;

    public CheepServiceTest()
    {
        Context = GetInMemoryDbContext();
    }
   
    public ChirpDBContext GetInMemoryDbContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ChirpDBContext>()
            .UseSqlite(connection)
            .Options;
        var context = new ChirpDBContext(options);
        context.Database.EnsureCreated();
        return context;
    }
    
    
    
    /*Test that there is only cheeps from the selected author when getcheepsfromauthor is called
    and that it doesnt crash if the author doesnt exists
*/
    [Theory]
    [InlineData("Roger Histand")]
    [InlineData("Luanna Muro")]
    [InlineData("Wendell Ballan")] 
    [InlineData("dfiuhweiufhwe")] //not an author 
    
    public async Task GetCheepsFromAuthor(string name)
    {
        //arrange
        var service = new CheepService(Context);

        var a1 = new Author()
            { AuthorId = 1, Name = "Roger Histand", Email = "Roger+Histand@hotmail.com", Cheeps = new List<Cheep>() };
        var a2 = new Author() { AuthorId = 2, Name = "Luanna Muro", Email = "Luanna-Muro@ku.dk", Cheeps = new List<Cheep>() };
            
        var a3 = new Author() { AuthorId = 3, Name = "Wendell Ballan", Email = "Wendell-Ballan@gmail.com", Cheeps = new List<Cheep>() };
        Context.Authors.Add(a1);
        Context.Authors.Add(a2);
        Context.Authors.Add(a3);
        
        var c14 = new Cheep() { CheepId = 14, AuthorId = a1.AuthorId, Author = a1, Text = "You are here for at all?", TimeStamp = DateTime.Parse("2023-08-01 13:13:18") };
        var c6 = new Cheep() { CheepId = 6, AuthorId = a3.AuthorId, Author = a3, Text = "At first he had only exchanged one trouble for another.", TimeStamp = DateTime.Parse("2023-08-01 13:14:13") };
        var c8 = new Cheep() { CheepId = 8, AuthorId = a2.AuthorId, Author = a2, Text = "It was but a very ancient cluster of blocks generally painted green, and for no other, he shielded me.", TimeStamp = DateTime.Parse("2023-08-01 13:14:01") };
        
        Context.Cheeps.Add(c14);
        Context.Cheeps.Add(c6);
        Context.Cheeps.Add(c8);
        
        Context.SaveChanges();
        // act 
        var cheeps = await service.GetCheepsFromAuthor(name,3);
        
        //assert
        foreach (var cheep in cheeps)
        {
            Assert.Equal(name, cheep.Author);
            Assert.NotEqual("hjdfiluwriu", cheep.Author);
        }
            

    }
    
    // private readonly WebApplicationFactory<Services> _factory;
    //
    // public ServicesTest(WebApplicationFactory<Services> factory) {
    //     _factory = factory.WithWebHostBuilder(builder => {
    //         builder.ConfigureServices(services => {
    //             Environment.SetEnvironmentVariable(DBEnv.envCHIRPDBPATH, Guid.NewGuid().ToString("N") + ".db");
    //             Environment.SetEnvironmentVariable(DBEnv.envDATA, "data/testDump.sql");
    //         });
    //     });
    //
    //     Console.SetOut(new StringWriter());
    // }
  
    
    // read tests 
    // private async Task<List<Cheep>> GetCheepsAsync()
    // {
    //     var client = _factory.CreateClient();
    //     var response = await client.GetAsync("/cheeps");
    //     response.EnsureSuccessStatusCode();
    //     var cheeps = await response.Content.ReadFromJsonAsync<List<Cheep>>();
    //     return cheeps ?? new List<Cheep>();
    // }

    
    
    /*Testing that the cheeps contain the expeected author, message and timestamp */
    [Theory]
    [InlineData(1, "ropf", "ropf@itu.dk" , "Hello, BDSA students!", "2025-08-30 13:14:37")]
    [InlineData(2, "adho", "adho@itu.dk", "Welcome to the course!", "2023-02-01 08:11:27")]
    public async Task ReadCheepsTest(int authorId, string author, string email,  string message, string timestamp)
    {
        //arrange
        var service = new CheepService(Context);
        var a10 = new Author() { AuthorId = authorId, Name = author, Email = email, Cheeps = new List<Cheep>() };
        Context.Authors.Add(a10);
        Context.Cheeps.Add(new Cheep() { CheepId = 1, AuthorId = a10.AuthorId, Author = a10, Text = message, TimeStamp = DateTime.Parse(timestamp )});
        Context.SaveChanges();
        // act 
        var cheep = await service.GetCheeps(1);
        
        //assert
        
            Assert.Equal(author, cheep[0].Author);
            Assert.Equal(message, cheep[0].Message);
            Assert.Equal(timestamp, cheep[0].TimeStamp);
          
        
            Assert.Single(cheep);
            
            
    }
    /*tests that we dont get a different result, when querying again after no changes have been made*/
    [Fact]
    public async Task SubsequentReadsReturnSameData()
    {
        //arrange
        var service = new CheepService(Context);
        var testauthor = new Author() { AuthorId = 2, Name = "Test guy", Email = "test@itu.dk", Cheeps = new List<Cheep>() };
        var testcheep = new Cheep() { CheepId = 1, AuthorId = testauthor.AuthorId, Author = testauthor, Text = "Testing subsequent reads", TimeStamp = DateTime.Parse("2025-07-10 21:21:13") };
        Context.Authors.Add(testauthor);
        Context.Cheeps.Add(testcheep);
        
        Context.SaveChanges();
        
        // act 
        var firstRead = await service.GetCheeps(10);
        var secondRead = await service.GetCheeps(10);
        
        // assert 
        
        Assert.Equal(firstRead, secondRead);
    }
    
    
    // Test of pagination
    [Fact]
    public async Task PaginationTest()
    {
        var service = new CheepService(Context);
        DbInitializer.SeedDatabase(Context);

        var cheeps1 = await service.GetCheeps(1);
        Assert.Equal(32, cheeps1.Count);
        
        // there should also be 32 cheeps on the second page
        var cheeps2 = await service.GetCheeps(2);
        Assert.Equal(32, cheeps2.Count);
        
        // unique pages 
        Assert.NotStrictEqual(cheeps1, cheeps2);
        
        // no cheeps on a page where there isnt enough
        var cheeps400 = await service.GetCheeps(400);
        Assert.Empty(cheeps400);
    }
    
    // test of timestamp sorting 
    [Fact]
    public async Task TimestampSortedTest()
    {
        var service = new CheepService(Context);
        DbInitializer.SeedDatabase(Context);

        var cheeps = await service.GetCheeps(1);
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
    //[InlineData(Int32.MinValue)]
    
   
    public async Task PaginationEdgeCaseTest(int pagenr)
    {
        var service = new CheepService(Context);
        DbInitializer.SeedDatabase(Context);
        
        // page 1
        var cheeps1 = await service.GetCheeps(1);
        var cheepsWeird = await service.GetCheeps(pagenr);
        
        
        Assert.Equal(32, cheepsWeird.Count);
        Assert.Equivalent(cheeps1, cheepsWeird);
        
    }
    
    

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    

    
    /*Tests if the server crashes when pushing something that's not a cheep*/
     
    // [Fact]
    // public async Task PushToServerFalty()
    // {
    //     // //Arrange
    //     // var service = new CheepService(Context);
    //     // var testauthor = new Author() { AuthorId = 2, Name = "Test guy", Email = "test@itu.dk", Cheeps = new List<Cheep>() };
    //     // var testcheep = new Cheep() { CheepId = 1, AuthorId = testauthor.AuthorId, Author = testauthor, Text = "Testing faulty stuff", TimeStamp = DateTime.Parse("2025-07-10 21:21:13") };
    //     // Context.Authors.Add(testauthor);
    //     // Context.Cheeps.Add(testcheep);
    //     //
    //     
    //     var client = _factory.CreateClient();
    //     
    //     var responseBefore = await client.GetAsync("/cheeps"); // Read what the database contains before
   	//     var resultBefore = await responseBefore.Content.ReadAsStringAsync();
    //     
    //     //  // act
    //     // var resultBefore = await service.GetCheeps(10);
    //     //
    //     // var notauthor = "Not an author";
    //     // var notcheep = "Not an cheep";
    //     //
    //     // Context.Authors.Add(notauthor);
    //     // Context.Cheeps.Add(notcheep);
    //     //
    //     // var resultAfter = await service.GetCheeps(10);
    //     //
    //      await client.PostAsJsonAsync("/cheep", "Hello i am not a cheep!");
    //     //  
    //     //  // Assert
    //     var responseAfter = await client.GetAsync("/cheeps"); // read from the database contains after
    //      var resultAfter = await responseAfter.Content.ReadAsStringAsync();
    //      
    //      Assert.Equal(resultBefore, resultAfter);
    //     }
    
    
    /** Asserts that valid (and essentially valid) entries are written and read
      as expected without throwing errors. This includes empty strings, messages
     that are not properly surrounded by quotation marks, and timestamps that
      are negative, as well as at the limit of what can be stored in a <see cref="long">long</see>.
     */
    
//     [Theory]
//     [InlineData("testauthor", "\"Test message, that's the way it is!!\"",
//         "\"Test message, that's the way it is!!\"", 1757601000L)]
//     [InlineData("testauthor", "Test message that's the way it is!!",
//         "Test message that's the way it is!!", 1757601000L)]
//     [InlineData("", "\"Test message, that's the way it is!!\"",
//         "\"Test message, that's the way it is!!\"", 1757601000L)]
//     [InlineData("testauthor", "", "", 1757601000L)]
//     [InlineData("testauthor", "\"\"", "\"\"", 1757601000L)]
//     [InlineData("testauthor", "\"\n\"", "\"\n\"", 1757601000L)]
//     [InlineData("testauthor", "\"Test message, that's the way it is!!\"",
//         "\"Test message, that's the way it is!!\"", long.MaxValue)]
//     public async Task write(string author, string writtenMessage, string readMessage, long timestamp) {
//         //Arrange
//         var client = _factory.CreateClient();
//         var message = new Cheep(author, writtenMessage, timestamp);
//
//         var responseBefore = await client.GetAsync("/cheeps"); // Read what the database contains before
//         var resultBefore = await responseBefore.Content.ReadAsStringAsync();
//
//         // act
//         await client.PostAsJsonAsync("/cheep", message);
//
//         // Assert
//         var responseAfter = await client.GetAsync("/cheeps"); // read what the database contains after
//         var resultAfter = await responseAfter.Content.ReadAsStringAsync();
//         var cheeps = await responseAfter.Content.ReadFromJsonAsync<List<Cheep>>();
//         Assert.NotNull(cheeps);
//         Assert.NotEqual(resultBefore, resultAfter);
//
//         var firstCheep = cheeps[0];
//         Assert.Equal(author, firstCheep.Author);
//         Assert.Equal(readMessage, firstCheep.Message);
//         Assert.Equal(timestamp, firstCheep.Timestamp);
//     }
//
//     [Theory]
//     [InlineData("testauthor", "\"Test message, that's the way it is!!\"",
//         "\"Test message, that's the way it is!!\"", -1757601000L)]
//     [InlineData("testauthor", "Test message that's the way it is!!",
//         "Test message that's the way it is!!", -1757601000L)]
//     [InlineData("", "\"Test message, that's the way it is!!\"",
//         "\"Test message, that's the way it is!!\"", -1757601000L)]
//     [InlineData("testauthor", "", "", -1757601000L)]
//     [InlineData("testauthor", "\"\"", "\"\"", -1757601000L)]
//     [InlineData("testauthor", "\"\n\"", "\"\n\"", -1757601000L)]
//     [InlineData("testauthor", "\"Test message, that's the way it is!!\"",
//         "\"Test message, that's the way it is!!\"", long.MinValue)]
//     public async Task WriteNegativeTimestamp(string author, string writtenMessage,
//             string readMessage, long timestamp) {
//         //Arrange
//         var client = _factory.CreateClient();
//         var message = new Cheep(author, writtenMessage, timestamp);
//         
//         var responseBefore = await client.GetAsync("/cheeps"); // Read what the database contains before
//         var resultBefore = await responseBefore.Content.ReadAsStringAsync();
//         
//         // act
//         await client.PostAsJsonAsync("/cheep", message);
//         
//         // Assert
//         var responseAfter = await client.GetAsync("/cheeps"); // read what the database contains after
//         var resultAfter = await responseAfter.Content.ReadAsStringAsync();
//         var cheeps = await responseAfter.Content.ReadFromJsonAsync<List<Cheep>>();
//         Assert.NotNull(cheeps);
//         Assert.NotEqual(resultBefore, resultAfter);
//
//         var lastCheep = cheeps[^1];
//         Assert.Equal(author, lastCheep.Author);
//         Assert.Equal(readMessage, lastCheep.Message);
//         Assert.Equal(timestamp, lastCheep.Timestamp);
//     }
//
//     /** Missing <c>"</c> before and after the message causes a TypeConverterException if the
//      * message includes a comma, as the text after the comma will be understood as a long.
//      * We test that the programme handles this smoothly without crashing. * /
//     [Theory]
//     [InlineData("testauthor", "Test message, that's the way it is!!", 1757601000L)]
//     [InlineData("testauthor", "Test message, that's the way it is!!", -1757601000L)]
//     [InlineData("testauthor", ",", 1757601000L)]
//     [InlineData("testauthor", ",", -1757601000L)]
//     
//     public async Task WriteTypeConverterException(string author, string writtenMessage, long timestamp)
//     {
//         //Arrange
//         var client = _factory.CreateClient();
//         var message = new Cheep(author, writtenMessage, timestamp);
//         
//         var responseBefore = await client.GetAsync("/cheeps"); 
//         var resultBefore = await responseBefore.Content.ReadAsStringAsync();
//         
//         // act
//         await client.PostAsJsonAsync("/cheep", message);
//         
//         // Assert
//         var responseAfter = await client.GetAsync("/cheeps"); 
//         var resultAfter = await responseAfter.Content.ReadAsStringAsync();
//
//         Assert.NotEqual(resultBefore, resultAfter);
//     }
//
//     [Theory]
//     [InlineData("testauthor", "\n", 1757601000L)]
//     [InlineData("testauthor", "\n", -1757601000L)]
//     public async Task WriteMissingFieldException(string author, string writtenMessage, long timestamp)
//     {
//         var client = _factory.CreateClient();
//         var message = new Cheep(author, writtenMessage, timestamp);
//
//         var responseBefore = await client.GetAsync("/cheeps");
//         var resultBefore = await responseBefore.Content.ReadAsStringAsync();
//
//         await client.PostAsJsonAsync("/cheep", message);
//
//         var responseAfter = await client.GetAsync("/cheeps");
//         var resultAfter = await responseAfter.Content.ReadAsStringAsync();
//
//         Assert.NotEqual(resultBefore, resultAfter);
//     }
//     
//     
//     [Theory]
//     [InlineData("testauthor", "\"Test message, that's the way it is!!", 1757601000L)]
//     [InlineData("testauthor", "Test message that's the way it is!!\"", 1757601000L)]
//     [InlineData("testauthor", "Test message that's\" the way it is!!", 1757601000L)]
//     [InlineData("testauthor", "\"\"\"", 1757601000L)]
//     [InlineData("testauthor", "\"", 1757601000L)]
//     public async Task WriteBadData(string author, string writtenMessage, long timestamp)
//     {
//         var client = _factory.CreateClient();
//         var message = new Cheep(author, writtenMessage, timestamp);
//
//         var responseBefore = await client.GetAsync("/cheeps");
//         var resultBefore = await responseBefore.Content.ReadAsStringAsync();
//
//         await client.PostAsJsonAsync("/cheep", message);
//
//         var responseAfter = await client.GetAsync("/cheeps");
//         var resultAfter = await responseAfter.Content.ReadAsStringAsync();
//
//         Assert.NotEqual(resultBefore, resultAfter); // nothing gets stored 
//     }
//
//     /*test reading of cheeps stored in the database, along with the index* /
//     [Fact]
//     public async Task ReadCheeps() {
//         var client = _factory.CreateClient();
//         
//         var message1 = new Cheep("ropf", "Hello, BDSA students!", 1690891760L);
//         await client.PostAsJsonAsync("/cheep", message1);
//         var message2 = new Cheep("adho", "Welcome to the course!", 1690978778L);
//         await client.PostAsJsonAsync("/cheep", message2);
//
//         var response = await client.GetAsync("/cheeps");
//         var cheeps = await response.Content.ReadFromJsonAsync<List<Cheep>>() ?? new List<Cheep>();
//
//         int index1 = cheeps.IndexOf(message1);
//         int index2 = cheeps.IndexOf(message2);
//
//         Assert.True(index1 != -1 && index2 != -1);
//         Assert.True(index2 < index1); // Message2 has higher timestamp, so should come first
//
//         Assert.Equal(message2, cheeps[index2]);
//         Assert.Equal(message1, cheeps[index1]);
//     }
//     
//     // test that we can read a page and get a result that is of length PAGE_SIZE
//     [Fact]
//     public async Task ReadPage()
//     {
//         // anrange
//         var client = _factory.CreateClient();
//         
//         // act
//         var response = await client.GetAsync("/cheepsPage?page=1");
//         
//         //assert
//         var page1 = await response.Content.ReadFromJsonAsync<List<Cheep>>() ?? new List<Cheep>();
//         Assert.Equal(Services.PAGE_SIZE, page1.Count);
//     }
//
//     // Tests that the api /cheepsPageWithAuthor finds results that are only of the chosen author
//     // Also tests that if a invalid input is given, that the output shpuld be empty
//     [Theory]
//     [InlineData("Jacqualine Gilcoine", false)]
//     [InlineData("Adrian", false)]
//     [InlineData("", true)]
//     [InlineData("\n", true)]
//     [InlineData("THIS USER IS NOT IN DATABASE 288282882827172672", true)]
//     public async Task ReadPageUser(string author, bool shouldBeEmpty)
//     {
//         // arrange 
//         var client = _factory.CreateClient();
//         
//         // act
//         var response = await client.GetAsync("/cheepsPageWithAuthor?page=1&author=" + author);
//         
//         // assert
//         var page = await response.Content.ReadFromJsonAsync<List<Cheep>>() ?? new List<Cheep>();
//         Assert.True(response.IsSuccessStatusCode);
//         foreach (var cheep in page)
//         {
//             Assert.Equal(author, cheep.Author);
//         }
//         Assert.Equal(shouldBeEmpty, page.Count == 0);
//     }
//     
//     // test that when changing the page we get differen answers
//     [Fact]
//     public async Task PagesAreUnique()
//     {
//         // arrange
//         var client = _factory.CreateClient();
//         
//         // act
//         var response1 = await client.GetAsync("/cheepsPage?page=1");
//         var response2 = await client.GetAsync("/cheepsPage?page=2");
//
//         // assert
//         var page1 =  await response1.Content.ReadFromJsonAsync<List<Cheep>>() ?? new List<Cheep>();
//         var page2 = await response2.Content.ReadFromJsonAsync<List<Cheep>>() ?? new List<Cheep>();
//         Assert.Equal(page1.Count, page2.Count);
//         for (var i = 0; i < page1.Count; i++)
//         {
//             bool equalAuthor = page1[i].Author == page2[i].Author;
//             bool equalMessage = page1[i].Message == page2[i].Message;
//             bool equalTimestamp = page1[i].Timestamp == page2[i].Timestamp;
//             Assert.False(equalAuthor &&  equalMessage && equalTimestamp); // they should not be equal
//         }
//     }
//     
//     //test that we can ask for a page witch does not exist
//     [Fact]
//     public async Task PagesDoesNotExist()
//     {
//         // arrange
//         var client = _factory.CreateClient();
//         int max = Int32.MaxValue / Services.PAGE_SIZE;
//         
//         // act
//         var response = await client.GetAsync("/cheepsPage?page=" + max);
//         
//         // assert
//         var page = await response.Content.ReadFromJsonAsync<List<Cheep>>() ?? new  List<Cheep>();
//         Assert.Empty(page);
//     }
//     
//     //test that reading a negetive page gives the same page as page 1
//     [Theory]
//     [InlineData(0)]
//     [InlineData(-1)]
//     [InlineData(-2)]
//     [InlineData(-3)]
//     public async Task ReadingNegativePages(int pageToRead)
//     {
//         // arrange
//         var client = _factory.CreateClient();
//         
//         // act
//         var response = await client.GetAsync("/cheepsPage?page=" + pageToRead);
//         
//         // assert
//         var page = await response.Content.ReadFromJsonAsync<List<Cheep>>() ?? new  List<Cheep>();
//         var expected = await client.GetAsync("/cheepsPage?page=1");
//         var ExpactedPage = await expected.Content.ReadFromJsonAsync<List<Cheep>>() ?? new  List<Cheep>();
//         
//         Assert.Equal(ExpactedPage.Count, page.Count);
//         for (int i = 0; i < page.Count; i++)
//         {
//             bool sameAuthor = page[i].Author.Equals(ExpactedPage[i].Author);
//             bool sameMessage = page[i].Message.Equals(ExpactedPage[i].Message);
//             bool sameTimestamp = page[i].Timestamp == ExpactedPage[i].Timestamp;
//             Assert.True(sameAuthor && sameMessage && sameTimestamp);
//         }
//     }
//     
//     // test that when changing the page we get different answers. Even when we ask by name
//     [Fact]
//     public async Task PagesAreUniqueButHasSameName()
//     {
//         // arrange
//         var client = _factory.CreateClient();
//         string author = "Jacqualine Gilcoine";
//         
//         // act
//         var response1 = await client.GetAsync("/cheepsPageWithAuthor?page=1&author=" + author);
//         var response2 = await client.GetAsync("/cheepsPageWithAuthor?page=2&author=" + author);
//
//         // assert
//         var page1 =  await response1.Content.ReadFromJsonAsync<List<Cheep>>() ?? new List<Cheep>();
//         var page2 = await response2.Content.ReadFromJsonAsync<List<Cheep>>() ?? new List<Cheep>();
//         Assert.True(page1.Count >= 3);
//         Assert.True(page2.Count >= 3);
//         for (var i = 0; i < 3; i++)
//         {
//             bool equalAuthor = page1[i].Author == page2[i].Author;
//             bool equalMessage = page1[i].Message == page2[i].Message;
//             bool equalTimestamp = page1[i].Timestamp == page2[i].Timestamp;
//             Assert.False(equalAuthor && equalMessage && equalTimestamp); // they should not be the same post
//         }
//
//         // All authors should be of the requested author
//         foreach (var cheep in page1) Assert.Equal(author, cheep.Author);
//         foreach (var cheep in page2) Assert.Equal(author, cheep.Author);
//         
//     }
//     
//     
}