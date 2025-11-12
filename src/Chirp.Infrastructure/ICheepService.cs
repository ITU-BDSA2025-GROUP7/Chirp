using Chirp.Core;

namespace Chirp.Infrastructure;

public interface ICheepService
{
    public Task<List<CheepDTO>> GetCheeps(int pageNr);
    public Task<List<CheepDTO>> GetCheepsFromUserName(string username, int pageNr);
}