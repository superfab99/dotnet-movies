namespace Movies.Contracts.Movies;

public sealed record MovieUpdated(
    int MovieId,
    string Title,
    string Genre,
    DateTime ReleaseDate,
    DateTimeOffset OccurredAt);