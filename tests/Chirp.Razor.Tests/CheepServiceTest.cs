using Chirp.CSVDBService;
using Chirp.General;
using Xunit;
using Chirp.Razor;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Chirp.Razor;


public class CheepServiceTest : IClassFixture<WebApplicationFactory<Services>>
{
    
    private readonly CheepService _cheepService;
    /* needs refactoring
    public CheepServiceTest(WebApplicationFactory<Services> factory)
    {
        // does so no extra infomation is printed in the console
        Console.SetOut(new StringWriter());
        
        var client = factory.CreateClient();
        _cheepService = new CheepService(client);
    }
    
    
    
    //Test that there is only cheeps from the selected author when getcheepsfromauthor is called
    [Theory]
    [InlineData("ropf")]
    [InlineData("adho")]
    [InlineData("bikzi")]
    [InlineData("pines")]
    [InlineData("louis")]
    [InlineData("mette")]
    [InlineData("dfiuhweiufhwe")] //not an author 
    
    public async Task GetCheepsFromAuthor(string name)
    {
        //arrange
        List<CheepViewModel> cheeps = await _cheepService.GetCheepsFromAuthor(name,1);
        
        
        foreach (CheepViewModel cheep in cheeps)
        {
            Assert.Equal(name, cheep.Author);
            Assert.NotEqual("hjdfiluwriu", cheep.Author);
        }
            

    }
    */
    
}