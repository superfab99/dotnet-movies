using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movies.Api.Models;

namespace Movies.Api.ModelConfigurations;

internal sealed class ActorConfiguration : IEntityTypeConfiguration<Actor>
{
    public void Configure(EntityTypeBuilder<Actor> builder)
    {
        builder.HasKey(actor => actor.Id);

        builder.Property(actor => actor.Gender)
            .HasConversion<string>()
            .HasMaxLength(30);

    }
}