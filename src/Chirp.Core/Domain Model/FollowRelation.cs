using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chirp.Core.Domain_Model;

public class FollowRelation {
    [Key]
    public int FollowRelationId { get; set; }

    [Required]
    public required Author Follower { get; set; }

    [Required]
    public required Author Followed { get; set; }
}