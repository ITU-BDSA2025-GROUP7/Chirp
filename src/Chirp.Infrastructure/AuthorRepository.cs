namespace Chirp.Infrastructure;

using Chirp.Core;
using Chirp.Core.Domain_Model;
using Microsoft.EntityFrameworkCore;

public class AuthorRepository : IAuthorRepository {
    private ChirpDBContext _dbContext;

    public AuthorRepository(ChirpDBContext dbContext) {
        this._dbContext = dbContext;
    }

    public async Task CreateAuthor(string name, string email)
    {
        var author = Author.Create(name, email);
        await _dbContext.Authors.AddAsync(author);
        await _dbContext.SaveChangesAsync();
        await Follow(author, author);
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

    public async Task Follow(Author follower, Author followed) {
        if (await IsFollowRelationInvalid(follower, followed)) {
            return;
        }

        FollowRelation newFollowRelation = new FollowRelation()
            { Follower = follower, Followed = followed };
        await _dbContext.AddAsync(newFollowRelation);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Unfollow(Author followerToDelete, Author followedToDelete)
    {
        FollowRelation followRelationToDelete = (from followRelation in _dbContext.FollowRelations
                                                 where followRelation.Follower ==
                                                     followerToDelete && followRelation.Followed ==
                                                     followedToDelete
                                                 select followRelation).First();
        if (followerToDelete == followedToDelete) {
            return;
        }

        _dbContext.FollowRelations.Remove(followRelationToDelete);
        await _dbContext.SaveChangesAsync();
    }

    /**
     * Returns true if breaks rules
     */
    private async Task<bool> IsFollowRelationInvalid(Author follower, Author followed) {
        return !_dbContext.Authors.Any(author => author == follower) ||
               !_dbContext.Authors.Any(author => author == followed) ||
               (await Following(follower))
              .Contains(followed); //checks if follower already follows followed :3
    }

    /**
     * returns all FollowRelations where `author` is follower
     */
    public async Task<List<FollowRelation>> GetFollowRelations(Author author)
    {
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


    public async Task Follow(string follower, string followed)
    {
        Author followerAuthor = (await GetAuthorByUserName(follower)).First();
        Author followedAuthor = (await GetAuthorByUserName(followed)).First();
        await Follow(followerAuthor, followedAuthor);
    }

    public async Task Unfollow(string follower, string followed)
    {
        Author followerAuthor = (await GetAuthorByUserName(follower)).First();
        Author followedAuthor = (await GetAuthorByUserName(followed)).First();
        await Unfollow(followerAuthor,followedAuthor);
    }

    /**
     * Returns true if authorA is following authorB, false otherwise.
     */
    public async Task<bool> IsFollowing(Author authorA, Author authorB)
    {
         var matches= await (from followRelation in _dbContext.FollowRelations
            where followRelation.Follower == authorA && followRelation.Followed == authorB
            select followRelation).ToListAsync();
         return matches.Count > 0;
    }
}