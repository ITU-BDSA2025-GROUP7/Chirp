using Chirp.Core.Domain_Model;

namespace Chirp.Core;

public class AuthorDTO : IComparable<AuthorDTO> {
    public string DisplayName { get; }
    public string UserName { get; }

    /**
    * A boolean check for whether a given AuthorDTO's DisplayName and UserName is the same as this
     * AuthorDTO's DisplayName and UserName.
    */
    protected bool Equals(AuthorDTO other) {
        return DisplayName == other.DisplayName && UserName == other.UserName;
    }

    /**
     * Checks whether a given object is an AuthorDTO by checking whether it is the same type as this
     * AuthorDTO, then returns result of the protected Equals function.
     */
    public override bool Equals(object? obj) {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((AuthorDTO)obj);
    }

    /**
     * Returns the combined HashCode of the AuthorDTOs, DisplayName and UserName fields.
     */
    public override int GetHashCode() {
        return HashCode.Combine(DisplayName, UserName);
    }

    public AuthorDTO(string displayName, string? authorUserName) {
        DisplayName = displayName;
        UserName = authorUserName ?? displayName;
    }

    public AuthorDTO(Author source)
        : this(source.DisplayName, source.UserName ?? source.DisplayName) { }

    /**
     * Uses ordinal-points to return whether another AuthorDTO has the exact same DisplayName as this
     * AuthorDTO.
     */
    public int CompareTo(AuthorDTO? other) {
        if (other == null) return 1;
        return string.CompareOrdinal(DisplayName, other.DisplayName);
    }
}