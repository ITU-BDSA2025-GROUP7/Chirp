using Chirp.Core;
using Chirp.Core.Domain_Model;

namespace Chirp.Infrastructure;

public interface IAuthorService {
    /**
     * Retries all users that has the given username
     */
    public Task<List<AuthorDTO>> GetAuthorByUserName(string username);

    /**
     * Returns true if authorA follows authorB
     */
    public Task<bool> IsFollowing(string authorA, string authorB);

    /**
     * Makes authorA follow AuthorB.
     * overload for <see cref="Follow(AuthorDTO follower, AuthorDTO followed)"/>
     */
    public Task Follow(string authorA, string authorB);

    /**
     * Makes follower follow followd.
     */
    public Task Follow(AuthorDTO follower, AuthorDTO followed);

    /**
     * makes follow unfollow followed
     */
    public Task Unfollow(string follower, string followed);

    /**
     * gets all followrelation wher follower is the follower
     */
    public Task<List<FollowRelation>> GetFollowRelations(string follower);

    /** Search the database for any authors whose username or display name contain the
     * given input <c>query</c>. Not case-sensitive. */
    public Task<List<AuthorDTO>> Search(string query);
}