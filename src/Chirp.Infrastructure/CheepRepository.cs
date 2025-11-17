using Microsoft.EntityFrameworkCore;
using Chirp.Core;
using Chirp.Core.Domain_Model;

namespace Chirp.Infrastructure;

public class CheepRepository : ICheepRepository {
    private ChirpDBContext _dbContext;

    public CheepRepository(ChirpDBContext dbContext) {
        this._dbContext = dbContext;
    }

    public async Task<List<Author>> GetAuthor(string identifier) {
        if (identifier.Contains('@')) {
            return await GetAuthorByEmail(identifier);
        }

        return await GetAuthorByUserName(identifier);
    }

    public async Task<List<Author>> GetAuthorByUserName(string username) {
        IOrderedQueryable<Author> query = (from author in _dbContext.Authors
                                           where author.UserName == username
                                           orderby author.DisplayName
                                           select author);
        return await query.ToListAsync();
    }

    public async Task<List<Author>> GetAuthorByEmail(string email) {
        IOrderedQueryable<Author> query = (from author in _dbContext.Authors
                                           where author.Email == email
                                           orderby author.DisplayName
                                           select author);
        return await query.ToListAsync();
    }

    /** Returns the 32 cheeps on a given page number which either belong to the input author,
     * or to one of the authors followed by the input author.
     */
    public async Task<List<CheepDTO>> GetOwnAndFollowedCheeps(string username, int pageNr = 1) {
        // Queried separately to avoid performing outer joins, which can be... messy with LINQ.
        // We only actually take the first
        List<Cheep> followedCheeps = await QueryCheepsFromFollowedAuthors(username)
                                          .Take(32 * pageNr)
                                          .ToListAsync();
        List<Cheep> ownCheeps = await QueryCheepsFromAuthor(username)
                                     .Take(32 * pageNr)
                                     .ToListAsync();
        followedCheeps.AddRange(ownCheeps);
        followedCheeps.Sort();
        return followedCheeps[..32]
              .Select(cheep => new CheepDTO(
                          cheep.Author.DisplayName,
                          cheep.Text,
                          cheep.TimeStamp.ToString(),
                          cheep.Author.UserName))
              .ToList();
    }

    private IQueryable<Cheep> QueryCheepsFromFollowedAuthors(string username) {
        return (from cheep in _dbContext.Cheeps
                join followed in _dbContext.FollowRelations
                    on cheep.Author.UserName equals followed.Follower.UserName
                where cheep.Author.UserName == username
                orderby cheep.TimeStamp descending
                select cheep);
    }

    private IQueryable<Cheep> QueryCheepsFromAuthor(string username) {
        return (from cheep in _dbContext.Cheeps
                where cheep.Author.UserName == username
                orderby cheep.TimeStamp descending
                select cheep);
    }

    public async Task CreateAuthor(string name, string email) {
        var author = Author.Create(name, email);
        await _dbContext.Authors.AddAsync(author);
        await _dbContext.SaveChangesAsync();
    }

    public async Task CreateCheep(Author author, string message, DateTime timestamp) {
        if (message.Length > Cheep.MAX_TEXT_LENGTH) {
            throw new ArgumentException("Message is too long. Maximum length is "
                                      + Cheep.MAX_TEXT_LENGTH);
        }

        Cheep cheep = new Cheep() { Author = author, Text = message, TimeStamp = timestamp };
        await _dbContext.Cheeps.AddAsync(cheep);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<CheepDTO>> GetCheeps(int pageNr) {
        IQueryable<CheepDTO> query = (from cheep in _dbContext.Cheeps
                                      orderby cheep.TimeStamp descending
                                      select cheep)
                                    .Skip((pageNr - 1) * 32)
                                    .Take(32)
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
                                    .Skip((pageNr - 1) * 32)
                                    .Take(32)
                                    .Select(cheep =>
                                                new CheepDTO(
                                                    cheep.Author.DisplayName,
                                                    cheep.Text,
                                                    cheep.TimeStamp.ToString(),
                                                    cheep.Author.UserName));

        return await query.ToListAsync();
    }

    public async Task Follow(Author follower, Author followed) {
        if (await ValidifyFollowRelationAsync(follower, followed)) {
            return;
        }

        FollowRelation newFollowRelation = new FollowRelation()
            { Follower = follower, Followed = followed };
        await _dbContext.AddAsync(newFollowRelation);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Unfollow(Author followerToDelete, Author followedToDelete) {
        FollowRelation followRelationToDelete = (from followRelation in _dbContext.FollowRelations
                                                 where followRelation.Follower ==
                                                     followerToDelete && followRelation.Followed ==
                                                     followedToDelete
                                                 select followRelation).First();
        if (followedToDelete == null) {
            return;
        }

        _dbContext.FollowRelations.Remove(followRelationToDelete);
        await _dbContext.SaveChangesAsync();
    }

    /**
     * Returns true if breaks rules
     */
    private async Task<bool> ValidifyFollowRelationAsync(Author follower, Author followed) {
        return !_dbContext.Authors.Any(author => author == follower) ||
               !_dbContext.Authors.Any(author => author == followed) ||
               follower.Id == followed.Id ||
               (await Following(follower))
              .Contains(followed); //checks if follower already follows followed :3
    }

    /**
     * returns all FollowRelations where `author` is follower
     */
    public async Task<List<FollowRelation>> GetFollowRelations(Author author) {
        return await (from followRelation in _dbContext.FollowRelations
                      where followRelation.Follower == author
                      select followRelation).ToListAsync();
    }

    /**
     * this is borderline unreadable, but it just gets all Authors which `author` follows
     */
    public async Task<List<Author>> Following(Author author) {
        return await (from user in _dbContext.FollowRelations
                      where user.Follower.UserName == author.UserName
                      select user.Followed).ToListAsync();
    }
}