namespace Movies.Api.Models
{
    public class MovieActor : BaseModel
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;

        public int ActorId { get; set; }
        public Actor Actor { get; set; } = null!;

        public bool IsLeadRole { get; set; }
        public String? CharacterName { get; set; }
    }
}