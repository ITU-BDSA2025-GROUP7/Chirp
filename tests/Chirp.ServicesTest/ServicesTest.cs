using System.Net.Http.Json;
using Xunit;
using Chirp.CSVDBService;
using Chirp.General;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Chirp.ServicesTest;
public class ServicesTest : IClassFixture<WebApplicationFactory<Services>>
{
    private readonly WebApplicationFactory<Services> _factory;

    public ServicesTest(WebApplicationFactory<Services> factory)
    {
		Console.SetOut(new StringWriter());
        _factory = factory;
    }
    
    /* Tests that it's possible to retreve data using the '/cheeps' api*/
    [Fact]
    public async Task ReadServer()
    {
        //Arrange
        var client = _factory.CreateClient();
        
        //Act
        var response = await client.GetAsync("/cheeps");
        var result = await response.Content.ReadAsStringAsync();
        
        //assert
        String expected =
            "[{\"author\":\"ropf\",\"message\":\"Hello, BDSA students!\",\"timestamp\":1690891760},{\"author\":\"adho\",\"message\":\"Welcome to the course!\",\"timestamp\":1690978778}]";
        Assert.Equal(expected, result);
    }

    /*Tests if it's possible to push a cheep to the server */
    [Fact]
    public async Task PushToServer()
    {
        //Arrange
        var client = _factory.CreateClient();
        var message = new Cheep("Nikki", "What a chirpy world!", 1757672108);
        
        var responseBefore = await client.GetAsync("/cheeps"); // Read what the database contains before
        var resultBefore = await responseBefore.Content.ReadAsStringAsync();
        
        // act
        await client.PostAsJsonAsync("/cheep", message);
        
        // Assert
        var responseAfter = await client.GetAsync("/cheeps"); // read from the database contains after
        var resultAfter = await responseAfter.Content.ReadAsStringAsync();
        
        String expected =
                "[{\"author\":\"ropf\",\"message\":\"Hello, BDSA students!\",\"timestamp\":1690891760},{\"author\":\"adho\",\"message\":\"Welcome to the course!\",\"timestamp\":1690978778},{\"author\":\"Nikki\",\"message\":\"What a chirpy world!\",\"timestamp\":1757672108}]"
            ;
        Assert.NotEqual(resultBefore, resultAfter);
        Assert.Equal(expected, resultAfter);
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
        
        Assert.Equal(resultBefore, resultAfter);
    }
    
    
}

