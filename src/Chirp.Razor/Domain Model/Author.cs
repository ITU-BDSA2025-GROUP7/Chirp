using System.ComponentModel.DataAnnotations;

namespace Chirp.Razor.Domain_Model;

public class Author {
    [Key]
    public int AuthorId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}