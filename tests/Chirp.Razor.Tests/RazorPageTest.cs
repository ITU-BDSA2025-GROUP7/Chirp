
using Chirp.General;
using Xunit;
using Chirp.Razor;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Chirp.Razor.Domain_Model;
namespace Chirp.Razor;
public class RazorPageTest : IClassFixture<WebApplicationFactory<Program>>
{
    // private readonly CheepService _cheepService;
    private HttpClient client;
    private SqliteConnection connection;
    public RazorPageTest(WebApplicationFactory<Program> factory)
        {
            // does so no extra infomation is printed in the console
            Console.SetOut(new StringWriter());

            client = factory.CreateClient();
            // _cheepService = new CheepService(client);
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<ChirpDBContext>()
                .UseSqlite(connection)
                .Options;
            var context = new ChirpDBContext(options);
            context.Database.EnsureCreated();
        }
    
    [Fact]
    public async Task Get_HomePage_Returns_Success()
    {
        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();

     
        Assert.Contains("Chirp", html);
    }
    
    // Tests that the api /cheepsPageWithAuthor finds results that are only of the chosen author
     // Also tests that if a invalid input is given, that the output shpuld be empty
     // [Theory]
     // [InlineData("Jacqualine Gilcoine", false)]
     // [InlineData("Adrian", false)]
     // [InlineData("", true)]
     // [InlineData("\n", true)]
     // [InlineData("THIS USER IS NOT IN DATABASE 288282882827172672", true)]
     // public async Task ReadPageUser(string author, bool shouldBeEmpty)
     // {
     //     // arrange 
     //     var client = _factory.CreateClient();
     //     
     //     // act
     //     var response = await client.GetAsync("/cheepsPageWithAuthor?page=1&author=" + author);
     //     
     //     // assert
     //     var page = await response.Content.ReadFromJsonAsync<List<Cheep>>() ?? new List<Cheep>();
     //     Assert.True(response.IsSuccessStatusCode);
     //     foreach (var cheep in page)
     //     {
     //         Assert.Equal(author, cheep.Author);
     //     }
     //     Assert.Equal(shouldBeEmpty, page.Count == 0);
     // }
     //
     // // test that when changing the page we get differen answers
     // [Fact]
     // public async Task PagesAreUnique()
     // {
     //     // arrange
     //     var client = _factory.CreateClient();
     //     
     //     // act
     //     var response1 = await client.GetAsync("/cheepsPage?page=1");
     //     var response2 = await client.GetAsync("/cheepsPage?page=2");
     //
     //     // assert
     //     var page1 =  await response1.Content.ReadFromJsonAsync<List<Cheep>>() ?? new List<Cheep>();
     //     var page2 = await response2.Content.ReadFromJsonAsync<List<Cheep>>() ?? new List<Cheep>();
     //     Assert.Equal(page1.Count, page2.Count);
     //     for (var i = 0; i < page1.Count; i++)
     //     {
     //         bool equalAuthor = page1[i].Author == page2[i].Author;
     //         bool equalMessage = page1[i].Message == page2[i].Message;
     //         bool equalTimestamp = page1[i].Timestamp == page2[i].Timestamp;
     //         Assert.False(equalAuthor &&  equalMessage && equalTimestamp); // they should not be equal
     //     }
     // }
}