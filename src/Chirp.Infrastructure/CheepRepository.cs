using Microsoft.EntityFrameworkCore;
using Chirp.Core;
using Chirp.Core.Domain_Model;

namespace Chirp.Infrastructure;

public class CheepRepository : ICheepRepository {
    private ChirpDBContext _dbContext;

    public int TotalCheepCount { get; private set; }

    public CheepRepository(ChirpDBContext dbContext) {
        this._dbContext = dbContext;
        TotalCheepCount = _dbContext.Cheeps.Count();
    }

    public async Task<int> CheepCountFromUserName(string username) {
        return await _dbContext.Cheeps.CountAsync(cheep => cheep.Author.UserName == username);
    }

    public async Task<List<CheepDTO>> GetCheepsFromFollowed(string username, int pageNr = 1) {
        return await QueryCheepsFromFollowedAuthors(username)
                    .Pick(pageNr)
                    .ToListAsync();
    }

    public async Task<int> CheepCountFromFollowed(string username) {
        return await QueryCheepsFromFollowedAuthors(username).CountAsync();
    }

    private IQueryable<CheepDTO> QueryCheepsFromFollowedAuthors(string username) {
        return (from cheep in _dbContext.Cheeps
                join follow in _dbContext.FollowRelations
                    on cheep.Author.UserName equals follow.Followed.UserName
                where follow.Follower.UserName == username
                orderby cheep.TimeStamp descending
                select new CheepDTO(
                    cheep.Author.DisplayName,
                    cheep.Text,
                    cheep.TimeStamp.ToString(),
                    cheep.Author.UserName));
    }

    public async Task CreateCheep(Author author, string? message, DateTime timestamp) {
        message ??= string.Empty; // Message *can* be null, since it is coming from an HTML form.
        if (message.Length > Cheep.MAX_TEXT_LENGTH) {
            throw new ArgumentException("Message is too long. Maximum length is "
                                      + Cheep.MAX_TEXT_LENGTH);
        }

        Cheep cheep = new Cheep() { Author = author, Text = message, TimeStamp = timestamp };
        await _dbContext.Cheeps.AddAsync(cheep);
        int changes = await _dbContext.SaveChangesAsync();
        TotalCheepCount += changes;
    }

    public async Task<List<CheepDTO>> GetCheeps(int pageNr) {
        IQueryable<CheepDTO> query = (from cheep in _dbContext.Cheeps
                                      orderby cheep.TimeStamp descending
                                      select cheep)
                                    .Pick(pageNr)
                                    .Select(cheep =>
                                                new CheepDTO(
                                                    cheep.Author.DisplayName,
                                                    cheep.Text,
                                                    cheep.TimeStamp.ToString(),
                                                    cheep.Author.UserName));

        return await query.ToListAsync();
    }

    public async Task<List<CheepDTO>> GetCheepsFromUserName(string username, int pageNr) {
        IQueryable<CheepDTO> query = (from cheep in _dbContext.Cheeps
                                      where cheep.Author.UserName == username
                                      orderby cheep.TimeStamp descending
                                      select cheep)
                                    .Pick(pageNr)
                                    .Select(cheep =>
                                                new CheepDTO(
                                                    cheep.Author.DisplayName,
                                                    cheep.Text,
                                                    cheep.TimeStamp.ToString(),
                                                    cheep.Author.UserName));

        return await query.ToListAsync();
    }

    public async Task<List<CheepDTO>> GetAllCheepsFromUserName(string username) {
        return await (from cheeps in _dbContext.Cheeps
                      where cheeps.Author.UserName == username
                      orderby cheeps.TimeStamp descending
                      select new CheepDTO(
                          cheeps.Author.DisplayName,
                          cheeps.Text,
                          cheeps.TimeStamp.ToString(),
                          cheeps.Author.UserName
                      )
            )
           .ToListAsync();
    }
}