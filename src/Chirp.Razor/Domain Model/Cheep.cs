using System.ComponentModel.DataAnnotations;

namespace Chirp.Razor.Domain_Model;

public class Cheep {
    [Key]
    public int CheepId { get; set; }
    public string Text { get; set; }
    public DateTime TimeStamp { get; set; }
    public Author Author { get; set; }
    
    public int AuthorId { get; set; }

}