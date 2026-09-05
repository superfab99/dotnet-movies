using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movies.Api.Models;

namespace Movies.Api.ModelConfigurations;

internal sealed class MovieActorConfiguration : IEntityTypeConfiguration<MovieActor>
{
    public void Configure(EntityTypeBuilder<MovieActor> builder)
    {
        builder.HasIndex(movieActor => new
        {
            movieActor.MovieId,
            movieActor.ActorId
        }).IsUnique();

        builder.HasOne(movieActor => movieActor.Movie)
            .WithMany(movie => movie.MovieActors)
            .HasForeignKey(movieActor => movieActor.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(movieActor => movieActor.Actor)
            .WithMany(actor => actor.MovieActors)
            .HasForeignKey(movieActor => movieActor.ActorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}