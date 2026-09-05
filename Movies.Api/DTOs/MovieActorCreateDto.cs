using System.ComponentModel.DataAnnotations;

namespace Movies.Api.DTOs;

public class MovieActorCreateDto
{
    [Required]
    public int ActorId { get; set; }

    [StringLength(200)]
    public string? CharacterName { get; set; }

    public bool IsLeadRole { get; set; }
}
