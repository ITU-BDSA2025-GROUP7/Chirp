namespace Chirp.Core;

using Domain_Model;

public interface IAuthorRepository
{
    
    public Task CreateAuthor(string name, string email);
    
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

    public Task Follow(Author follower, Author followed);
    /**
     * Deletes a follow relation
     */
    public Task Unfollow(Author follower, Author followed);

    public Task<List<FollowRelation>> GetFollowRelations(Author author);
    public Task<List<Author>> Following(Author author);
}