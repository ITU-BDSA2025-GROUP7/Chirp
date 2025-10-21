using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor.Domain_Model;

[Index(nameof(Email), IsUnique = true)]
public class Author
{
    [Key]
    public int AuthorId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }

    public List<Cheep> Cheeps { get; set; } = [];
}