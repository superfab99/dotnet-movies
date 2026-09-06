namespace Movies.Contracts.Users;

public sealed record UserRegistered(
    int UserId,
    string Username,
    string Email,
    DateTimeOffset OccurredAt);
