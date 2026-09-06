using Microsoft.EntityFrameworkCore;
using Movies.Review.Models;

namespace Movies.Review.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MoviesReviewDbContext>();

        await dbContext.Database.MigrateAsync();

        var reviews = new[]
        {
            new MovieReview
            {
                MovieId = 1,
                Comment = "A thoughtful science-fiction story with strong performances.",
                Rating = 5
            },
            new MovieReview
            {
                MovieId = 2,
                Comment = "The atmosphere and mystery kept me engaged from beginning to end.",
                Rating = 4
            },
            new MovieReview
            {
                MovieId = 3,
                Comment = "A clever mystery with a satisfying final reveal.",
                Rating = 5
            }
        };

        if (!await dbContext.MovieReviews.AnyAsync())
        {
            foreach (var review in reviews)
            {
                dbContext.MovieReviews.Add(review);
            }

            await dbContext.SaveChangesAsync();
        }
    }
}