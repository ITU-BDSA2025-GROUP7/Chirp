using Chirp.Core;
using Chirp.Core.Domain_Model;

namespace Chirp.Infrastructure;

public class AuthorService : IAuthorService {
    private IAuthorRepository authorRepository;

    public AuthorService(IAuthorRepository authorRepository) {
        this.authorRepository = authorRepository;
    }

    public async Task<List<Author>> GetAuthorByUserName(string username) {
        return await authorRepository.GetAuthorByUserName(username);
    }

    public async Task Follow(Author follower, Author followed) {
        await authorRepository.Follow(follower, followed);
    }

    public async Task<bool> IsFollowing(Author authorA, Author authorB)
    {
        return await authorRepository.IsFollowing(authorA, authorB);
    }

    public async Task Follow(string authorA, string authorB)
    {
        await authorRepository.Follow(authorA, authorB);
    }

    public async Task Unfollow(string authorA, string authorB)
    {
        await authorRepository.Unfollow(authorA, authorB);
    }
}