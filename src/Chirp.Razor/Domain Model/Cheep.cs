using System.ComponentModel.DataAnnotations;

namespace Chirp.Razor.Domain_Model;

public class Cheep {
    public const int MAX_TEXT_LENGTH = 160;

    [Key]
    public int CheepId { get; set; }

    [MaxLength(160)]
    public string Text { get; set; } = "";
    public DateTime TimeStamp { get; set; }
    public required Author Author { get; set; }

    public int AuthorId { get; set; }

}