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

    public async Task<List<Author>> GetAuthor(string identifier) {
        if (identifier.Contains('@')) {
            return await GetAuthorByEmail(identifier);
        }
        return await GetAuthorByUserName(identifier);
    }

    public async Task<List<Author>> GetAuthorByUserName(string username)
    {
        var query = (from author in _dbContext.Authors
            where author.UserName == username
            orderby author.DisplayName
            select author);
        return await query.ToListAsync();
    }

    public async Task<List<Author>> GetAuthorByEmail(string email)
    {
        var query = (from author in _dbContext.Authors
            where author.Email == email
            orderby author.DisplayName
            select author);
        return await query.ToListAsync();
    }

    public async Task CreateAuthor(string name, string email)
    {
        var author = Author.Create(name, email);
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
                new CheepDTO(cheep.Author.DisplayName,
                             cheep.Text,
                             cheep.TimeStamp.ToString(),
                             cheep.Author.UserName));

        return await query.ToListAsync();
    }

    public async Task<List<CheepDTO>> GetCheepsFromUserName(string author, int pageNr)
    {
        var query = (from cheep in _dbContext.Cheeps
             where cheep.Author.UserName == author
             orderby cheep.TimeStamp descending
             select cheep)
           .Skip((pageNr - 1) * 32).Take(32).Select(cheep =>
               new CheepDTO(cheep.Author.DisplayName,
                            cheep.Text,
                            cheep.TimeStamp.ToString(),
                            cheep.Author.UserName));

        return await query.ToListAsync();
    }
}