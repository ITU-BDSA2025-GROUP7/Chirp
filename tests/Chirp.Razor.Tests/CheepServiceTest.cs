using Chirp.General;
using Xunit;
using Chirp.Razor;

namespace Chirp.Razor;


public class CheepServiceTest
{
    
    /*Test that there is only cheeps from the selected author when getcheepsfromauthor is called*/
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
        var service = new CheepService(); 
        List<CheepViewModel> cheeps = await service.GetCheepsFromAuthor(name);
        
        
        foreach (CheepViewModel cheep in cheeps)
        {
            Assert.Equal(name, cheep.Author);
            Assert.NotEqual("hjdfiluwriu", cheep.Author);
        }
            

    }
}