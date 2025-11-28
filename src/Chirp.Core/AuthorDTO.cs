using Chirp.Core.Domain_Model;

namespace Chirp.Core;

public class AuthorDTO : IComparable<AuthorDTO> {
    public string DisplayName { get; }
    public string UserName { get; }

    protected bool Equals(AuthorDTO other) {
        return DisplayName == other.DisplayName && UserName == other.UserName;
    }

    public override bool Equals(object? obj) {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((AuthorDTO)obj);
    }

    public override int GetHashCode() {
        return HashCode.Combine(DisplayName, UserName);
    }

    public AuthorDTO(string displayName, string? authorUserName) {
        DisplayName = displayName;
        UserName = authorUserName ?? displayName;
    }

    public AuthorDTO(Author source) {
        DisplayName = source.DisplayName;
        UserName = source.UserName ?? DisplayName;
    }

    public int CompareTo(AuthorDTO? other) {
        if (other == null) return 1;
        return string.CompareOrdinal(DisplayName, other.DisplayName);
    }
}