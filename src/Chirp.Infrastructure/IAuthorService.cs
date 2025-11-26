using Chirp.Core.Domain_Model;

namespace Chirp.Infrastructure;

public interface IAuthorService {
    public Task<List<Author>> GetAuthorByUserName(string username);

    public Task<bool> IsFollowing(Author authorA, Author authorB);

    public Task Follow(Author follower, Author followed);

    public Task Follow(string authorA, string authorB);

    public Task Unfollow(string authorA, string authorB);
}