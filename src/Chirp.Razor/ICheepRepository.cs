namespace Chirp.Razor;

using System.Diagnostics.CodeAnalysis;
using Domain_Model;

public interface ICheepRepository
{
    /**
     * recognizes the string as a name or email and calls the relevant GetAuthor method
     */
    public Task<Author> GetAuthor(string identifier);
    
    /**
     * Gets all cheeps within the given page nr
     */
    public Task<List<CheepDTO>> GetCheeps(int pageNr);
    
    /**
     * Gets all cheeps within the given page nr that have the given author
     */
    public Task<List<CheepDTO>> GetCheepsFromAuthor(string author, int pageNr);
    
    public void SendCheep(CheepDTO cheep);
    
    public Task CreateAuthor(string name, string email);
    
    public Task CreateCheep(Author author, string message, DateTime timestamp);

    public ChirpDBContext GetDbContext();
}