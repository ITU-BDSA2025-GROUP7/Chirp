using Chirp.Core;
using Chirp.Core.Domain_Model;

public interface ICheepService
{
    public Task<List<CheepDTO>> GetCheeps(int pageNr);
    public Task<List<CheepDTO>> GetCheepsFromUserName(string username, int pageNr);
    public Task<List<Author>> GetAuthorByUserName(string username);
    public Task CreateCheep(Author author, string message);
}

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

    public async Task<List<Author>> GetAuthorByUserName(string username)
    {
        return await cheepRepository.GetAuthorByUserName(username);
    }
    public async Task CreateCheep(Author author, string message)
    {
        await cheepRepository.CreateCheep(author, message, DateTime.Now);
    }
}