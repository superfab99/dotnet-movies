namespace Movies.Contracts.Movies;

public sealed record MovieCreated(
    int MovieId,
    string Title,
    string Genre,
    DateTime ReleaseDate,
    DateTimeOffset OccurredAt);
