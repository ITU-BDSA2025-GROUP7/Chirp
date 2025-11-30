using Chirp.Core;
using Chirp.Core.Domain_Model;

namespace Chirp.Infrastructure;

public class AuthorService : IAuthorService {
    private IAuthorRepository authorRepository;

    public AuthorService(IAuthorRepository authorRepository) {
        this.authorRepository = authorRepository;
    }

    public async Task<List<AuthorDTO>> GetAuthorByUserName(string username) {
        return await authorRepository.GetAuthorByUserName(username);
    }

    public async Task<bool> IsFollowing(string authorA, string authorB) {
        return await authorRepository.IsFollowing(authorA, authorB);
    }

    public async Task Follow(string authorA, string authorB) {
        await authorRepository.Follow(authorA, authorB);
    }

    public async Task Unfollow(string follower, string followed) {
        await authorRepository.Unfollow(follower, followed);
    }

    public async Task<List<FollowRelation>> GetFollowRelations(string follower) {
        return await authorRepository.GetFollowRelations(follower);
    }
}