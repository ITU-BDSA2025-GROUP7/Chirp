using Chirp.Core;
using Chirp.Core.Domain_Model;

namespace Chirp.Infrastructure;

public interface ICheepService {
    public Task<List<CheepDTO>> GetCheeps(int pageNr);
    public Task<List<CheepDTO>> GetCheepsFromUserName(string username, int pageNr);
    public Task<List<CheepDTO>> GetOwnAndFollowedCheeps(Author author, int pageNr = 1);
    public Task CreateCheep(Author author, string message);
}