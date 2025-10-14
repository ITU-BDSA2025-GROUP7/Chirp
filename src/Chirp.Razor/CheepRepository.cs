using Chirp.Razor.Domain_Model;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor;

public class CheepRepository :  ICheepRepository
{
    private ChirpDBContext dbContext;

    public CheepRepository(ChirpDBContext dbContext)
    {
        this.dbContext = dbContext;
    }
    private Author GetAuthorByName(string name)
    {
        var query = (from author in dbContext.Authors
            where author.Name == name
            orderby author.Name
            select author);
        return query.First();
    }

    private Author GetAuthorByEmail(string email)
    {
        var query = (from author in dbContext.Authors
            where author.Email == email
            orderby author.Name
            select author);
        return query.First();
    }

    public void CreateAuthor(string name, string email)
    {
        Author author = new Author() {Name  = name, Email = email};
        dbContext.Authors.Add(author);
        dbContext.SaveChanges();
    }
    public void CreateCheep(Author author, string message, DateTime timestamp)
    {
        Cheep cheep = new Cheep() {Author  = author, Text = message, TimeStamp = timestamp};
        dbContext.Cheeps.Add(cheep);
        dbContext.SaveChanges();
    }

    public Author GetAuthor(string identifier)
    {
        if (identifier.Contains("@"))
        {
            return GetAuthorByEmail(identifier);
        }
        return GetAuthorByName(identifier);
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

    public ChirpDBContext GetDbContext()
    {
        return dbContext;
    }
    
}