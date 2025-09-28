using Chirp.CSVDBService;
using Chirp.General;
using Xunit;
using Chirp.Razor;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Chirp.Razor;


public class CheepServiceTest : IClassFixture<WebApplicationFactory<Services>>
{
    
    private readonly WebApplicationFactory<Services> _factory;
    private string _tempPath;

    public CheepServiceTest(WebApplicationFactory<Services> factory)
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
        List<CheepViewModel> cheeps = await service.GetCheepsFromAuthor(name,1);
        
        
        foreach (CheepViewModel cheep in cheeps)
        {
            Assert.Equal(name, cheep.Author);
            Assert.NotEqual("hjdfiluwriu", cheep.Author);
        }
            

    }
}