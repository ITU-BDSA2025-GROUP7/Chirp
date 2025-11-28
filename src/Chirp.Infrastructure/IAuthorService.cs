using Chirp.Core;
using Chirp.Core.Domain_Model;

namespace Chirp.Infrastructure;

public interface IAuthorService {
    public Task<List<AuthorDTO>> GetAuthorByUserName(string username);

    public Task<bool> IsFollowing(string authorA, string authorB);

    public Task Follow(string authorA, string authorB);

    public Task Unfollow(string follower, string followed);

    public Task<List<FollowRelation>> GetFollowRelations(string follower);
}