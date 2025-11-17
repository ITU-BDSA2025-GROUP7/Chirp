namespace Chirp.Core;

using Domain_Model;

public interface ICheepRepository
{
    /**
     * recognizes the string as a name or email and calls the relevant GetAuthor method
     */
    public Task<List<Author>> GetAuthor(string identifier);

    /**
     * Function used in GetAuthor if its argument is recognized as an Email.
     */
    public Task<List<Author>> GetAuthorByEmail(string email);

    /**
     * Function used in GetAuthor if its argument is recognized as a name.
     */
    public Task<List<Author>> GetAuthorByUserName(string username);
    /**
     * Gets all cheeps within the given page nr
     */
    public Task<List<CheepDTO>> GetCheeps(int pageNr);

    /**
     * Gets all cheeps within the given page nr that have the given author
     */
    public Task<List<CheepDTO>> GetCheepsFromUserName(string username, int pageNr);

    public Task CreateAuthor(string name, string email);

    public Task CreateCheep(Author author, string message, DateTime timestamp);
    /**
     * Creates a follow relation, and adds a reference of the followed to follower
     */
    public Task Follow(Author follower, Author followed);
    
    /**
     * Creates a follow relation, and adds a reference of the followed to follower
     */
    public Task Follow(string follower, string followed);
    /**
     * Deletes a follow relation
     */
    public Task Unfollow(Author follower, Author followed);

    /**
     * returns all FollowRelations where `author` is follower
     */
    public Task<List<FollowRelation>> GetFollowRelations(Author author);
    
    /**
     * Gets all Authors which `author` follows
     */
    public Task<List<Author>> Following(Author author);

    /**
     * Returns true if authorA is following authorB, false otherwise.
     */
    public Task<bool> IsFollowing(Author authorA, Author authorB);

}