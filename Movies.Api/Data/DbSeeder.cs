using Microsoft.EntityFrameworkCore;
using Movies.Api.Enums;
using Movies.Api.Models;

namespace Movies.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MoviesApiDbContext>();

        await dbContext.Database.MigrateAsync();

        var movies = new[]
        {
            new Movie
            {
                Title = "The Last Horizon",
                Description = "A pilot searches for a way home after a mission beyond the known frontier.",
                ReleaseDate = new DateTime(2024, 5, 17),
                Genre = "Science Fiction",
                Rating = 8.4,
                DurationMinutes = 128
            },
            new Movie
            {
                Title = "Whispers in Winter",
                Description = "A journalist returns to her hometown to uncover a long-buried story.",
                ReleaseDate = new DateTime(2023, 11, 3),
                Genre = "Drama",
                Rating = 7.8,
                DurationMinutes = 116
            },
            new Movie
            {
                Title = "The Clockmaker's Secret",
                Description = "An apprentice discovers that a family clock hides a dangerous secret.",
                ReleaseDate = new DateTime(2022, 8, 26),
                Genre = "Mystery",
                Rating = 8.1,
                DurationMinutes = 104
            }
        };

        if (!await dbContext.Movies.AnyAsync())
        {
            foreach (var movie in movies)
            {
                dbContext.Movies.Add(movie);
            }

            await dbContext.SaveChangesAsync();
        }

        var actors = new[]
        {
            new Actor { FirstName = "Maya", LastName = "Stone", Gender = Gender.Female },
            new Actor { FirstName = "Elias", LastName = "Grant", Gender = Gender.Male },
            new Actor { FirstName = "Noah", LastName = "Reed", Gender = Gender.Male },
            new Actor { FirstName = "Ava", LastName = "Cole", Gender = Gender.Female }
        };

        if (!await dbContext.Actors.AnyAsync())
        {
            foreach (var actor in actors)
            {
                dbContext.Actors.Add(actor);
            }

            await dbContext.SaveChangesAsync();
        }

        var savedMovies = await dbContext.Movies
            .Where(movie => movies.Select(seedMovie => seedMovie.Title).Contains(movie.Title))
            .ToDictionaryAsync(movie => movie.Title);
        var savedActors = await dbContext.Actors
            .Where(actor => actors.Select(seedActor => seedActor.FirstName + " " + seedActor.LastName)
                .Contains(actor.FirstName + " " + actor.LastName))
            .ToDictionaryAsync(actor => actor.FirstName + " " + actor.LastName);

        var movieActors = new[]
        {
            (MovieTitle: "The Last Horizon", ActorName: "Maya Stone", IsLeadRole: true, CharacterName: "Commander Elara Voss"),
            (MovieTitle: "The Last Horizon", ActorName: "Elias Grant", IsLeadRole: false, CharacterName: "Jon Bell"),
            (MovieTitle: "Whispers in Winter", ActorName: "Noah Reed", IsLeadRole: true, CharacterName: "Daniel Frost"),
            (MovieTitle: "Whispers in Winter", ActorName: "Ava Cole", IsLeadRole: false, CharacterName: "Mara Wells"),
            (MovieTitle: "The Clockmaker's Secret", ActorName: "Maya Stone", IsLeadRole: true, CharacterName: "Clara Vale")
        };

        if (!await dbContext.MovieActors.AnyAsync()
            && savedMovies.Count == movies.Length
            && savedActors.Count == actors.Length)
        {
            foreach (var movieActor in movieActors)
            {
                var movie = savedMovies[movieActor.MovieTitle];
                var actor = savedActors[movieActor.ActorName];

                dbContext.MovieActors.Add(new MovieActor
                {
                    MovieId = movie.Id,
                    ActorId = actor.Id,
                    IsLeadRole = movieActor.IsLeadRole,
                    CharacterName = movieActor.CharacterName
                });
            }

            await dbContext.SaveChangesAsync();
        }
    }
}