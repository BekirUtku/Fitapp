using FitApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitApp.Infrastructure.Persistence.Configurations;

public class WorkoutSessionConfiguration : IEntityTypeConfiguration<WorkoutSession>
{
    public void Configure(EntityTypeBuilder<WorkoutSession> builder)
    {
        builder.ToTable("workout_sessions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(160);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.TotalVolume).HasPrecision(10, 2);
        builder.Property(x => x.Note).HasMaxLength(1000);

        builder.HasIndex(x => new { x.UserId, x.StartedAt });
        builder.HasIndex(x => new { x.UserId, x.Status });
        builder.HasIndex(x => x.UpdatedAt);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Exercises)
            .WithOne(x => x.Session!)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class SessionExerciseConfiguration : IEntityTypeConfiguration<SessionExercise>
{
    public void Configure(EntityTypeBuilder<SessionExercise> builder)
    {
        builder.ToTable("session_exercises");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Note).HasMaxLength(500);
        builder.HasIndex(x => x.SessionId);

        builder.HasOne(x => x.Exercise)
            .WithMany()
            .HasForeignKey(x => x.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Sets)
            .WithOne(x => x.SessionExercise!)
            .HasForeignKey(x => x.SessionExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkoutSetConfiguration : IEntityTypeConfiguration<WorkoutSet>
{
    public void Configure(EntityTypeBuilder<WorkoutSet> builder)
    {
        builder.ToTable("workout_sets");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SetType).HasConversion<int>();
        builder.Property(x => x.Weight).HasPrecision(7, 2);
        builder.Property(x => x.DistanceMeters).HasPrecision(9, 2);
        builder.Property(x => x.Note).HasMaxLength(500);

        builder.HasIndex(x => x.SessionExerciseId);
        builder.HasIndex(x => x.UpdatedAt);
    }
}
