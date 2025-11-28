using Chirp.Core;

namespace Chirp.Infrastructure;

public interface IAuthorService {
    public Task<List<AuthorDTO>> GetAuthorByUserName(string username);

    public Task<bool> IsFollowing(string authorA, string authorB);

    public Task Follow(string authorA, string authorB);

    public Task Unfollow(string authorA, string authorB);
}