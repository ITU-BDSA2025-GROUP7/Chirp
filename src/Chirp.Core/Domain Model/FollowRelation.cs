using System.ComponentModel.DataAnnotations;

namespace Chirp.Core.Domain_Model;

public class FollowRelation {
    [Key]
    public int FollowRelationId { get; set; }
    [Required]
    public required Author Follower{get; set;}

    [Required] 
    public required Author Followed { get; set; }
}