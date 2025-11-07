using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Core.Domain_Model;

[Index(nameof(Email), IsUnique = true)]
public class Author : IdentityUser {
    [MaxLength(256)]
    [PersonalData]
    public string DisplayName { get; set; } = "";

    [PersonalData]
    public List<Cheep> Cheeps { get; set; } = [];

    /** Create an Author which would be a valid user in the database,
     * generating a username based on the given <c>displayName</c>.<br/>
     * The optional parameter <c>passwordHash</c> needs to be included to
     * be able to actually log in as the person in practice.
     */
    public static Author Create(string displayName, string email,
                  bool emailConfirmed = true,
                  string? passwordHash = null,
                  string? concurrencyStamp = null,
                  string? securityStamp = null)
    {
        string username = displayName.Replace(" ", "");
        return new Author {
            DisplayName = displayName,
            UserName = username,
            NormalizedUserName = username.ToUpper(),
            Email = email,
            NormalizedEmail = email.ToUpper(),
            EmailConfirmed = emailConfirmed,
            PasswordHash = passwordHash,
            ConcurrencyStamp = concurrencyStamp,
            SecurityStamp = securityStamp,
        };
    }
}