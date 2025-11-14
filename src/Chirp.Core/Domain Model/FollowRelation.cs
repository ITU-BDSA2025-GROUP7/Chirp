using System.ComponentModel.DataAnnotations;

namespace Chirp.Core.Domain_Model;

public class FollowRelation {
    [Required]
    public Author follower{get; set;}

    [Required] 
    public Author followed { get; set; }
}