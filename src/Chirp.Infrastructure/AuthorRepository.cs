namespace Chirp.Infrastructure;

using Chirp.Core;
using Chirp.Core.Domain_Model;
using Microsoft.EntityFrameworkCore;

public class AuthorRepository : IAuthorRepository {
    private ChirpDBContext _dbContext;

    public AuthorRepository(ChirpDBContext dbContext) {
        this._dbContext = dbContext;
    }

    public async Task CreateAuthor(string name, string email) {
        var author = Author.Create(name, email);
        await _dbContext.Authors.AddAsync(author);
        await _dbContext.SaveChangesAsync();
        if (author.UserName != null) {
            await Follow(author.UserName, author.UserName);
        }
    }

    public async Task<List<AuthorDTO>> GetAuthor(string identifier) {
        if (identifier.Contains('@')) {
            return await GetAuthorByEmail(identifier);
        }

        return await GetAuthorByUserName(identifier);
    }

    public async Task<List<AuthorDTO>> GetAuthorByUserName(string username) {
        var query = (from author in _dbContext.Authors
                     where author.UserName == username
                     orderby author.DisplayName
                     select new AuthorDTO(author.DisplayName, author.UserName));
        return await query.ToListAsync();
    }

    public async Task<List<AuthorDTO>> GetAuthorByEmail(string email) {
        var query = (from author in _dbContext.Authors
                     where author.Email == email
                     orderby author.DisplayName
                     select new AuthorDTO(author.DisplayName, author.UserName));
        return await query.ToListAsync();
    }

    private Author? FindAuthor(AuthorDTO authorDTO) {
        return (from author in _dbContext.Authors
                where author.UserName == authorDTO.UserName
                orderby author.DisplayName
                select author).FirstOrDefault();
    }

    public async Task Follow(AuthorDTO follower, AuthorDTO followed) {
        if (await IsFollowRelationInvalid(follower, followed)) {
            return;
        }

        Author? followerAuthor = FindAuthor(follower);
        Author? followedAuthor = FindAuthor(followed);
        if (followerAuthor == null || followedAuthor == null) {
            return;
        }

        var newFollowRelation = new FollowRelation
            { Follower = followerAuthor, Followed = followedAuthor };
        await _dbContext.AddAsync(newFollowRelation);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Unfollow(AuthorDTO followerToDelete, AuthorDTO followedToDelete) {
        if (Equals(followerToDelete, followedToDelete)) {
            return;
        }

        FollowRelation followRelationToDelete = (from followRelation in _dbContext.FollowRelations
                                                 where followRelation.Follower.UserName ==
                                                       followerToDelete.UserName
                                                    && followRelation.Followed.UserName ==
                                                       followedToDelete.UserName
                                                 select followRelation).First();
        _dbContext.FollowRelations.Remove(followRelationToDelete);
        await _dbContext.SaveChangesAsync();
    }

    /**
     * Returns true if breaks rules
     */
    private async Task<bool> IsFollowRelationInvalid(AuthorDTO follower, AuthorDTO followed) {
        return !_dbContext.Authors.Any(author => author.UserName == follower.UserName) ||
               !_dbContext.Authors.Any(author => author.UserName == followed.UserName) ||
               (await Following(follower.UserName))
              .Contains(followed); //checks if follower already follows followed :3
    }

    /**
     * returns all FollowRelations where `follower` is follower
     */
    public async Task<List<FollowRelation>> GetFollowRelations(string follower) {
        return await _dbContext.FollowRelations
                               .Include(fr => fr.Follower)
                               .Include(fr => fr.Followed)
                               .Where(fr => fr.Follower.UserName == follower)
                               .ToListAsync();
    }

    /**
     * this is borderline unreadable, but it just gets all Authors which `follower` follows
     */
    public async Task<List<AuthorDTO>> Following(string follower) {
        return await (from user in _dbContext.FollowRelations
                      where user.Follower.UserName == follower
                      select new AuthorDTO(user.Followed.DisplayName, user.Followed.UserName))
           .ToListAsync();
    }


    public async Task Follow(string follower, string followed) {
        AuthorDTO followerAuthor = (await GetAuthorByUserName(follower)).First();
        AuthorDTO followedAuthor = (await GetAuthorByUserName(followed)).First();
        await Follow(followerAuthor, followedAuthor);
    }

    public async Task Unfollow(string follower, string followed) {
        AuthorDTO followerAuthor = (await GetAuthorByUserName(follower)).First();
        AuthorDTO followedAuthor = (await GetAuthorByUserName(followed)).First();
        await Unfollow(followerAuthor, followedAuthor);
    }

    /**
     * Returns true if `follower` is following `followed`, false otherwise.
     */
    public async Task<bool> IsFollowing(string follower, string followed) {
        var matches = await (from followRelation in _dbContext.FollowRelations
                             where followRelation.Follower.UserName == follower &&
                                   followRelation.Followed.UserName == followed
                             select followRelation).ToListAsync();
        return matches.Count > 0;
    }
}