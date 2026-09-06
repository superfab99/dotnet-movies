namespace Movies.Contracts.Movies;

public sealed record MovieDeleted(
    int MovieId,
    DateTimeOffset OccurredAt);
