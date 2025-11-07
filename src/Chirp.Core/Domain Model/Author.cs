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
}