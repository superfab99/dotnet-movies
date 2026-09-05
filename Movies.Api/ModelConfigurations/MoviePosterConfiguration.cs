using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movies.Api.Models;

namespace Movies.Api.ModelConfigurations
{
    internal sealed class MoviePosterConfiguration : IEntityTypeConfiguration<MoviePoster>
    {
        public void Configure(EntityTypeBuilder<MoviePoster> builder)
        {
            builder.HasKey(mp => mp.Id);

            builder.HasOne(mp => mp.Movie)
            .WithOne(m => m.MoviePoster)
            .HasForeignKey<MoviePoster>(mp => mp.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}