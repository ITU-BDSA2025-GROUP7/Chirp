using System.ComponentModel.DataAnnotations;

namespace Chirp.Core.Domain_Model;

public class Author {
    [Key]
    public int AuthorId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }

    public List<Cheep> Cheeps { get; set; } = [];
}