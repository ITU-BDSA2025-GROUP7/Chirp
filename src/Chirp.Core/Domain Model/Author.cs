using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Core.Domain_Model;

[Index(nameof(Email), IsUnique = true)]
public class Author : IdentityUser, IEquatable<Author> {
    [MaxLength(256)]
    [PersonalData]
    public string Name { get; set; } = "";

    [PersonalData]
    public List<Cheep> Cheeps { get; set; } = [];

    [PersonalData] public List<IdentityUserLogin<Author>> ExternalLogins { get; set; } = [];

    public bool Equals(Author? other) {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(Name, other.Name, StringComparison.InvariantCulture) && Cheeps.Equals(other.Cheeps);
    }

    public override bool Equals(object? obj) {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Author)obj);
    }

    public override int GetHashCode() {
        var hashCode = new HashCode();
        hashCode.Add(Name, StringComparer.InvariantCulture);
        hashCode.Add(Cheeps);
        return hashCode.ToHashCode();
    }
}