using Chirp.Core;


public interface ICheepService
{
    public Task<List<CheepDTO>> GetCheeps(int pageNr);
    public Task<List<CheepDTO>> GetCheepsFromAuthor(string author, int pageNr);
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
    public async Task<List<CheepDTO>> GetCheepsFromAuthor(string author, int pageNr)
    {
        return await cheepRepository.GetCheepsFromAuthor(author, pageNr);
    }
}