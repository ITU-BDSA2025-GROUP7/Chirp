namespace Chirp.Core;

using Domain_Model;

public interface IAuthorRepository {
    public Task CreateAuthor(string name, string email);

    /**
     * recognizes the string as a name or email and calls the relevant GetAuthor method
     */
    public Task<List<AuthorDTO>> GetAuthor(string identifier);

    /**
     * Function used in GetAuthor if its argument is recognized as an Email.
     */
    public Task<List<AuthorDTO>> GetAuthorByEmail(string email);

    /**
     * Function used in GetAuthor if its argument is recognized as a name.
     */
    public Task<List<AuthorDTO>> GetAuthorByUserName(string username);

    public Task Follow(AuthorDTO follower, AuthorDTO followed);

    /**
     * Deletes a follow relation
     */
    public Task Unfollow(AuthorDTO follower, AuthorDTO followed);

    public Task<List<FollowRelation>> GetFollowRelations(AuthorDTO author);

    public Task<List<AuthorDTO>> Following(AuthorDTO author);

    /**
     * Creates a follow relation, and adds a reference of the followed to follower
     */
    public Task Follow(string follower, string followed);

    /**
     * Creates a follow relation, and adds a reference of the followed to follower
     */
    public Task Unfollow(string follower, string followed);

    /**
     * Returns true if authorA is following authorB, false otherwise.
     */
    public Task<bool> IsFollowing(string follower, string followed);
}