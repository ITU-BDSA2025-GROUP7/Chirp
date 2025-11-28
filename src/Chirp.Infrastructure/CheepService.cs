using Chirp.Core;
using Chirp.Core.Domain_Model;

namespace Chirp.Infrastructure;

public class CheepService : ICheepService {
    ICheepRepository cheepRepository;

    // default constructor
    public CheepService(ICheepRepository cheepRepository) {
        this.cheepRepository = cheepRepository;
    }

    /**
     * Calls on the Services to get all cheeps within the given page nr
     */
    public async Task<List<CheepDTO>> GetCheeps(int pageNr) {
        return await cheepRepository.GetCheeps(pageNr);
    }

    /**
     * Calls on the Services to get all cheeps within the given page nr that have the given author
     */
    public async Task<List<CheepDTO>> GetCheepsFromUserName(string username, int pageNr) {
        return await cheepRepository.GetCheepsFromUserName(username, pageNr);
    }

    public async Task<List<CheepDTO>> GetOwnAndFollowedCheeps(Author author, int pageNr = 1) {
        return await cheepRepository.GetOwnAndFollowedCheeps(author, pageNr);
    }

    public async Task CreateCheep(Author author, string message) {
        DateTime date = DateTime.Now;
        await cheepRepository.CreateCheep(author, message,
                                          new DateTime(date.Year, date.Month, date.Day, date.Hour,
                                                       date.Minute, date.Second));
    }

    public async Task DeleteCheep(CheepDTO cheep) {
        await cheepRepository.DeleteCheep(cheep);

    }
}