using Chirp.Core;

namespace Chirp.Infrastructure;

public class CheepService : ICheepService
{
    ICheepRepository cheepRepository;

    // default constructor
    public CheepService(ICheepRepository cheepRepository)
    {
        this.cheepRepository = cheepRepository;
    }

    /**
     * Calls on the Services to get all cheeps within the given page nr
     */
    public async Task<List<CheepDTO>> GetCheeps(int pageNr)
    {
       return await cheepRepository.GetCheeps(pageNr);
    }

    /**
     * Calls on the Services to get all cheeps within the given page nr that have the given author
     */
    public async Task<List<CheepDTO>> GetCheepsFromUserName(string username, int pageNr)
    {
        return await cheepRepository.GetCheepsFromUserName(username, pageNr);
    }
}