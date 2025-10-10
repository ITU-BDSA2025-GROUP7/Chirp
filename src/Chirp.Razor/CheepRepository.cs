using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor;

public class CheepRepository :  ICheepRepository
{
    private ChirpDBContext dbContext;
    
    public CheepRepository(ChirpDBContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public string GetAuthor(string identifier)
    {
        
    }
    private Author GetAuthorByName(string name)
    {
        var query = (from author in dbContext.Authors
                orderby author.Name
                where author.Name == name)
                    .Select(author => author);

        return query.First();
    }

    private string GetAuthorByEmail(string email)
    {
        
    }
    
    public async Task<List<CheepDTO>> GetCheeps(int pageNr)
    {
        var query = (from cheep in dbContext.Cheeps
                orderby cheep.TimeStamp descending
                select cheep)
            .Skip((pageNr - 1) * 32).Take(32).Select(cheep => 
                new CheepDTO(cheep.Author.Name, cheep.Text, cheep.TimeStamp.ToString()));

        return await query.ToListAsync();
    }

    
    public async Task<List<CheepDTO>> GetCheepsFromAuthor(string author, int pageNr)
    {
        var query = (from cheep in dbContext.Cheeps
                where cheep.Author.Name == author
                orderby cheep.TimeStamp descending
                select new CheepDTO(cheep.Author.Name, cheep.Text, cheep.TimeStamp.ToString()))
            .Skip((pageNr - 1) * 32).Take(32);

        return await query.ToListAsync();
    }

    public void SendCheep(CheepDTO cheep)
    {
        throw new NotImplementedException();
    }
}