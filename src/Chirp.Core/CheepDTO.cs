namespace Chirp.Core;

public class CheepDTO
{
    public string Author {get; set;}
    public string Message {get; set;}
    public string TimeStamp {get; set;}

    public CheepDTO(string author, string message, string timeStamp)
    {
        Author = author;
        Message = message;
        TimeStamp = timeStamp;
    }
}