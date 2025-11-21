namespace Chirp.Core;

using Domain_Model;

public interface ICheepRepository {
    /**
     * Gets all cheeps within the given page nr
     */
    public Task<List<CheepDTO>> GetCheeps(int pageNr);

    /**
     * Gets all cheeps within the given page nr that have the given author
     */
    public Task<List<CheepDTO>> GetCheepsFromUserName(string username, int pageNr);


    public Task CreateCheep(Author author, string message, DateTime timestamp);
}