using Chirp.Core.Domain_Model;

namespace Chirp.Infrastructure;

using Chirp.Core;

public interface IAuthorRepository {

    /**
     * Creates an author with name and email, generates a random pasword
     * Also saves the author to the database.
     * This method is only used for testing.
     */
    public Task CreateAuthor(string name, string email);

    /**
     * Uses the identifier to retrive the user.
     * If the identifier is an email, then it find the user using
     * <see cref="GetAuthorByEmail"/>. If not then it tries to find the user using <see cref="GetAuthorByUserName"/>.
     */
    public Task<List<AuthorDTO>> GetAuthor(string identifier);

    /**
     * Returns all Authors that has the given email.
     */
    public Task<List<AuthorDTO>> GetAuthorByEmail(string email);

    /**
     * Returns all Authors that have the given UserName.
     */
    public Task<List<AuthorDTO>> GetAuthorByUserName(string username);

    /**
     * Makes follower(AuthorA) follow followed(AuthorB)
     */
    public Task Follow(AuthorDTO follower, AuthorDTO followed);

    /**
     * Makes follower unfollow the followed user by deleting the follow relation
     */
    public Task Unfollow(AuthorDTO follower, AuthorDTO followed);

    /**
     * returns all FollowRelations where `follower` is follower
     */
    public Task<List<FollowRelation>> GetFollowRelations(string follower);

    /**
     *  Eeturns all Authors which `follower` follows
     */
    public Task<List<AuthorDTO>> Following(string follower);

    /**
     * Makes follower(AuthorA) follow followed(AuthorB)
     * overload function for <see cref=" Follow(AuthorDTO follower, AuthorDTO followed)"/>
     */
    public Task Follow(string follower, string followed);

    /**
     * Makes follower unfollow the followed user by deleting the follow relation.
     * overload function for <see cref="Unfollow(AuthorDTO follower, AuthorDTO followed)"/>
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