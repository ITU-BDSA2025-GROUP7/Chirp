namespace Chirp.Razor;

public interface ICheepRepository
{
    /**
     * Gets all cheeps within the given page nr
     */
    public Task<List<CheepDTO>> GetCheeps(int pageNr);
    
    /**
     * Gets all cheeps within the given page nr that have the given author
     */
    public Task<List<CheepDTO>> GetCheepsFromAuthor(string author, int pageNr);
    
    public void SendCheep(CheepDTO cheep);
}