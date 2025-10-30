using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Chirp.Core;
using Chirp.Core.Domain_Model;

namespace Chirp.Infastructure;

public class CheepRepository :  ICheepRepository
{
    private ChirpDBContext _dbContext;

    public CheepRepository(ChirpDBContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<List<Author>> GetAuthor(string identifier)
    {
        if (identifier.Contains("@"))
        {
            return await GetAuthorByEmail(identifier);
        }
        return await GetAuthorByName(identifier);
    }

    public async Task<List<Author>> GetAuthorByName(string name)
    {
        var query = (from author in _dbContext.Authors
            where author.Name == name
            orderby author.Name
            select author);
        return await query.ToListAsync();
    }

    public async Task<List<Author>> GetAuthorByEmail(string email)
    {
        var query = (from author in _dbContext.Authors
            where author.Email == email
            orderby author.Name
            select author);
        return await query.ToListAsync();
    }

    public async Task CreateAuthor(string name, string email)
    {
        Author author = new Author() { Name = name, Email = email };
        await _dbContext.Authors.AddAsync(author);
        await _dbContext.SaveChangesAsync();
    }

    public async Task CreateCheep(Author author, string message, DateTime timestamp)
    {
        if (message.Length > Cheep.MAX_TEXT_LENGTH) {
            throw new ArgumentException("Message is too long. Maximum length is "
                                        + Cheep.MAX_TEXT_LENGTH);
        }
        Cheep cheep = new Cheep() {Author  = author, Text = message, TimeStamp = timestamp};
        await _dbContext.Cheeps.AddAsync(cheep);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<CheepDTO>> GetCheeps(int pageNr)
    {
        var query = (from cheep in _dbContext.Cheeps
                orderby cheep.TimeStamp descending
                select cheep)
            .Skip((pageNr - 1) * 32).Take(32).Select(cheep =>
                new CheepDTO(cheep.Author.Name, cheep.Text, cheep.TimeStamp.ToString(CultureInfo.CurrentCulture)));

        return await query.ToListAsync();
    }

    public async Task<List<CheepDTO>> GetCheepsFromAuthor(string author, int pageNr)
    {
        var query = (from cheep in _dbContext.Cheeps
                where cheep.Author.Name == author
                orderby cheep.TimeStamp descending
                select new CheepDTO(cheep.Author.Name, cheep.Text, cheep.TimeStamp.ToString(CultureInfo.CurrentCulture)))
            .Skip((pageNr - 1) * 32).Take(32);

        return await query.ToListAsync();
    }
}