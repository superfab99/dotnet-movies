namespace Movies.Contracts.Reviews;

public sealed record ReviewCreated(
    int ReviewId,
    int MovieId,
    int Rating,
    DateTimeOffset OccurredAt);
