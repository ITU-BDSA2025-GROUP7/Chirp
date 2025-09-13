using System.Globalization;

namespace Chirp.CLI.Client;

public class CheepTest
{
    [Fact]
    public void cheepInstantiation()
    {
        //arrange
        string Author = "Chirp";
        string Message = "Hello World, i am alive!!!!";
        long Timestamp = 1690891760;
        //act 
        Cheep cheep = new Cheep(Author, Message, Timestamp);
        
        //assert
        Assert.Equal(cheep.Author, Author);
        Assert.Equal(cheep.Message,  Message);
        Assert.Equal(cheep.Timestamp,  Timestamp);
        
    }
    
    [Theory]
    [InlineData(1690891760, "01-08-2023 14:09:20")]
    [InlineData(-1690891760, "02-06-1916 13:50:40")]
    public void cheepToString(int timestamp, string expectedTimeString)
    {
        //arrange
        string Author = "Chirp";
        string Message = "Hello World, i am alive!!!!";
        long Timestamp = timestamp; // this tecnically also tests time conversion
        // Calculates the expeted time since it will depend on what time zone the computer running the test will be in
        string ExpectedTime = DateTime.Parse(expectedTimeString + " +02:00").ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture); 
        
        string expected ="Chirp @ " + ExpectedTime + ": Hello World, i am alive!!!!";
        
        //act
        Cheep cheep = new Cheep(Author, Message, Timestamp);
        string result = cheep.ToString();
        
        //assert
        Assert.Equal(expected, result);
    }
}
