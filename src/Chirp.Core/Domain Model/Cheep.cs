using System.ComponentModel.DataAnnotations;

namespace Chirp.Core.Domain_Model;

public class Cheep : IComparable<Cheep> {
    public const int MAX_TEXT_LENGTH = 160;

    [Key]
    public int CheepId { get; set; }

    [MaxLength(MAX_TEXT_LENGTH)]
    public string Text { get; set; } = "";

    public DateTime TimeStamp { get; set; }
    public required Author Author { get; set; }

    public int CompareTo(Cheep? other) {
        if (other == null) return 1;
        return TimeStamp.CompareTo(other.TimeStamp);
    }
}