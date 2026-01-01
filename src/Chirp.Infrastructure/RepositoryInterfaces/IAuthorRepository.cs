using Chirp.Core.Domain_Model;

namespace Chirp.Infrastructure;

using Chirp.Core;

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

    public Task<List<FollowRelation>> GetFollowRelations(string follower);

    public Task<List<AuthorDTO>> Following(string follower);

    /**
     * Creates a follow relation, and adds a reference of the followed to follower
     */
    public Task Follow(string follower, string followed);

    /**
     * Removes a follow relation if `follower` and `followed` are not identical.
     */
    public Task Unfollow(string follower, string followed);

    /**
     * Returns true if `follower` is following `followed`, false otherwise.
     */
    public Task<bool> IsFollowing(string follower, string followed);

    /** Search the database for any authors whose username or display name contain the
     * given input <c>query</c>. Not case-sensitive. */
    public Task<List<AuthorDTO>> Search(string query);
}