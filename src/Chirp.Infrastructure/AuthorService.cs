using Chirp.Core;

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

    public async Task Unfollow(string authorA, string authorB) {
        await authorRepository.Unfollow(authorA, authorB);
    }
}