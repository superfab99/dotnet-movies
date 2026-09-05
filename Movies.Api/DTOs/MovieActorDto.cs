namespace Movies.Api.DTOs;

public class MovieActorDto
{
    public int MovieId { get; set; }
    public int ActorId { get; set; }
    public ActorDto Actor { get; set; } = null!;
    public string? CharacterName { get; set; }
    public bool IsLeadRole { get; set; }
}
