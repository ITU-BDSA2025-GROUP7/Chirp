using Chirp.Core;
using Chirp.Core.Domain_Model;

namespace Chirp.Infrastructure;

public interface IAuthorService {
    public Task<List<AuthorDTO>> GetAuthorByUserName(string username);

    public Task<bool> IsFollowing(string authorA, string authorB);

    public Task Follow(AuthorDTO follower, AuthorDTO followed);

    public Task Unfollow(string follower, string followed);

    public Task<List<FollowRelation>> GetFollowRelations(string follower);

    /** Search the database for any authors whose username or display name contain the
     * given input <c>query</c>. Not case-sensitive. */
    public Task<List<AuthorDTO>> Search(string query);
}