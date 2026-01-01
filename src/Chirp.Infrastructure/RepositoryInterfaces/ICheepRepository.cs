using Chirp.Core.Domain_Model;

namespace Chirp.Infrastructure;
using Chirp.Core;

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

    /** Retrieves the given page of cheeps written by <c>username</c>. */
    public Task<List<CheepDTO>> GetAllCheepsFromUserName(string username);

    /** Retrieves the given page of cheeps written by all of <c>username</c>'s followers. */
    public Task<List<CheepDTO>> GetCheepsFromFollowed(string author, int pageNr = 1);

    /** Computes the number of cheeps written by <c>username</c>. */
    public Task<int> CheepCountFromUserName(string username);

    /** Computes the number of cheeps written by all <c>username</c>'s followed accounts. */
    public Task<int> CheepCountFromFollowed(string username);

    /** Creates a new cheep by <c>author</c> with the given <c>message</c> content. */
    public Task CreateCheep(Author author, string message, DateTime timestamp);

    /** The total number of cheeps in the database. Updated whenever a cheep is added or removed. */
    int TotalCheepCount { get; }

    public Task DeleteCheep(CheepDTO cheep);
}