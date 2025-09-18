using Xunit;
using Chirp.CSVDBService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Chirp.ServicesTest;
public class ServicesTest : IClassFixture<WebApplicationFactory<Services>>
{
    private readonly WebApplicationFactory<Services> _factory;

    public ServicesTest(WebApplicationFactory<Services> factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public void GetDB()
    {
        var client = _factory.CreateClient();
        
        
        var response = client.GetAsync("/cheeps").Result;
        
        var result = response.Content.ReadAsStringAsync().Result;
        
        Assert.Equal("[{\"author\":\"ropf\",\"message\":\"Hello, BDSA students!\",\"timestamp\":1690891760},{\"author\":\"adho\",\"message\":\"Welcome to the course!\",\"timestamp\":1690978778}]", result);
    }
}

