using Chirp.Core;
using Chirp.Core.Domain_Model;

namespace Chirp.Infrastructure;

public interface ICheepService
{
    public Task<List<CheepDTO>> GetCheeps(int pageNr);
    public Task<List<CheepDTO>> GetCheepsFromUserName(string username, int pageNr);
    public Task<List<Author>> GetAuthorByUserName(string username);
    public Task CreateCheep(Author author, string message);
    public Task<bool> IsFollowing(Author authorA, Author authorB);
    
    public Task Follow(Author authorA, Author authorB);
}