using Chirp.Core;
using Chirp.Core.Domain_Model;
using Chirp.Infrastructure;

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
}