using Microsoft.EntityFrameworkCore;
using Chirp.Core;
using Chirp.Core.Domain_Model;
using Microsoft.AspNetCore.Components;
using Microsoft.Net.Http.Headers;
using System.Dynamic;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Chirp.Infrastructure;

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

    public async Task<List<CheepDTO>> GetCheepsFromUserName(string username, int pageNr)
    {
        var query = (from cheep in _dbContext.Cheeps
             where cheep.Author.UserName == username
             orderby cheep.TimeStamp descending
             select cheep)
           .Skip((pageNr - 1) * 32).Take(32).Select(cheep =>
               new CheepDTO(cheep.Author.DisplayName,
                            cheep.Text,
                            cheep.TimeStamp.ToString(),
                            cheep.Author.UserName));

        return await query.ToListAsync();
    }

    public async Task Follow(Author follower, Author followed)
    {
        Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! WE ARE HERE");
        
        if (await ValidifyfollowRelationAsync(follower, followed))
        {
            return;
        }
        
        Console.WriteLine("!!!!!!!!!!!! WE ARE HERE 2");
        FollowRelation newFollowRelation = new FollowRelation() { Follower = follower, Followed = followed };
        await _dbContext.AddAsync(newFollowRelation);
        _dbContext.SaveChanges();
    }

    public async Task Follow(String follower, String followed)
    {
       Author followerAuthor = (await GetAuthorByUserName(follower)).First();
       Author followedAuthor = (await GetAuthorByUserName(followed)).First();
       Follow(followerAuthor, followedAuthor);
    }

    public async Task Unfollow(Author followerToDelete, Author followedToDelete)
    {
        FollowRelation followRelationToDelete = (from followRelation in _dbContext.FollowRelations
        where followRelation.Follower == followerToDelete && followRelation.Followed == followedToDelete
        select followRelation).First();
        if (followedToDelete == null)
        {
            return;
        }
        _dbContext.FollowRelations.Remove(followRelationToDelete);
        _dbContext.SaveChanges();
    }
    /**
     * Returns true if breaks rules
     */
    private async Task<bool> ValidifyfollowRelationAsync(Author follower, Author followed)
    {
        if (!_dbContext.Authors.Any(author => author == follower)||
            !_dbContext.Authors.Any(author => author == followed)||
            follower.Id == followed.Id|| 
            (await Following(follower)).Contains(followed)) //checks if follower already follows followed :3
        { 
            return true;
        }
        return false;   
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
    public async Task<List<Author>> Following(Author author)
    {
        return(from user in _dbContext.FollowRelations
                where user.Follower.UserName == author.UserName
                select user.Followed).ToList();
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