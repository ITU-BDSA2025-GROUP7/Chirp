namespace DefaultNamespace;

public class CheepServiceTest
{
    
    /*Test that there is only cheeps from the selected author when getcheepsfromauthor is called*/
    [InlineData("ropf")]
    [InlineData("adho")]
    [InlineData("bikzi")]
    [InlineData("pines")]
    [InlineData("louis")]
    [InlineData("mette")]
    [InlineData("dfiuhweiufhwe")] //not an author 
    
    public void GetCheepsFromAuthor(string name)
    {
        //arrange
        var cheeps = GetCheepsFromAuthor(name);
        
        
        for (cheep in cheeps)
        {
            assert.Equal(name, cheep.Author);
            assert.notEqual("hjdfiluwriu", cheep.Author);
        }
            

    }
}