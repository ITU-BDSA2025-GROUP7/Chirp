namespace Chirp.Core;

using Domain_Model;

public interface ICheepRepository {
    /// The number of cheeps to display on each page.
    const int CHEEPS_PER_PAGE = 32;

    /**
     * Gets all cheeps within the given page nr
     */
    public Task<List<CheepDTO>> GetCheeps(int pageNr);

    /**
     * Gets all cheeps within the given page nr that have the given author
     */
    public Task<List<CheepDTO>> GetCheepsFromUserName(string username, int pageNr);

    public Task<List<CheepDTO>> GetAllCheepsFromUserName(string username);

    public Task<List<CheepDTO>> GetOwnAndFollowedCheeps(Author author, int pageNr = 1);

    public Task CreateCheep(Author author, string message, DateTime timestamp);
    public Task DeleteCheep(CheepDTO cheep);
}