using Chirp.Core;
using Chirp.Core.Domain_Model;

namespace Chirp.Infrastructure;

public interface ICheepService {
    /** Retrieves the given page of cheeps written by all authors. */
    public Task<List<CheepDTO>> GetCheeps(int pageNr);

    /** Retrieves the given page of cheeps written by <c>username</c>. */
    public Task<List<CheepDTO>> GetCheepsFromUserName(string username, int pageNr);

    /** Computes the number of cheeps written by <c>username</c>. */
    public Task<int> CheepCountFromUserName(string username);

    /** Retrieves the given page of cheeps written by all of <c>username</c>'s followers. */
    public Task<List<CheepDTO>> GetCheepsFromFollowed(string username, int pageNr = 1);

    /** Computes the number of cheeps written by all <c>username</c>'s followed accounts. */
    public Task<int> CheepCountFromFollowed(string username);

    /** Creates a new cheep by <c>author</c> with the given <c>message</c> content. */
    public Task CreateCheep(Author author, string message);

    /** The total number of cheeps in the database. */
    int TotalCheepCount { get; }

    /**
     * deletes the given cheep from the database
     */
    public Task DeleteCheep(CheepDTO cheep);
}