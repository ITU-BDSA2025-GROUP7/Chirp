using System.ComponentModel.DataAnnotations;

namespace Chirp.Razor.Domain_Model;

public class Cheep {
    [Key]
    public int CheepId { get; set; }
    public string Text { get; set; }
    public DateTime Timestamp { get; set; }
    public Author Author { get; set; }
}