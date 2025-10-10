namespace Chirp.Razor;

public interface ICheepRepository
{
    /**
     * recognizes the string as a name or email and calls the relevant GetAuthor method
     */
    public Author GetAuthor(string identifier);
    
    private Author GetAuthorByName(string name);

    private Author GetAuthorByEmail(string email);
    
    /**
     * Gets all cheeps within the given page nr
     */
    public Task<List<CheepDTO>> GetCheeps(int pageNr);
    
    /**
     * Gets all cheeps within the given page nr that have the given author
     */
    public Task<List<CheepDTO>> GetCheepsFromAuthor(string author, int pageNr);
    
    public void SendCheep(CheepDTO cheep);
    
    public void CreateCheep(string author, string message, DateTime timestamp);
}