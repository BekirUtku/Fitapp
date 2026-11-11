using FitApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitApp.Infrastructure.Persistence.Configurations;

public class MuscleGroupConfiguration : IEntityTypeConfiguration<MuscleGroup>
{
    public void Configure(EntityTypeBuilder<MuscleGroup> builder)
    {
        builder.ToTable("muscle_groups");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(80);
        builder.Property(x => x.BodyRegion).HasConversion<int>();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("equipment");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(80);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("exercises");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(160);
        builder.Property(x => x.MetricType).HasConversion<int>();
        builder.Property(x => x.Description).HasMaxLength(2000);

        builder.HasIndex(x => x.PrimaryMuscleGroupId);
        builder.HasIndex(x => x.OwnerUserId);
        builder.HasIndex(x => x.UpdatedAt);

        builder.HasOne(x => x.PrimaryMuscleGroup)
            .WithMany(x => x.PrimaryExercises)
            .HasForeignKey(x => x.PrimaryMuscleGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Equipment)
            .WithMany(x => x.Exercises)
            .HasForeignKey(x => x.EquipmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.OwnerUser)
            .WithMany(x => x.CustomExercises)
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class ExerciseSecondaryMuscleConfiguration : IEntityTypeConfiguration<ExerciseSecondaryMuscle>
{
    public void Configure(EntityTypeBuilder<ExerciseSecondaryMuscle> builder)
    {
        builder.ToTable("exercise_secondary_muscles");
        builder.HasKey(x => new { x.ExerciseId, x.MuscleGroupId });

        builder.HasOne(x => x.Exercise)
            .WithMany(x => x.SecondaryMuscles)
            .HasForeignKey(x => x.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.MuscleGroup)
            .WithMany(x => x.SecondaryLinks)
            .HasForeignKey(x => x.MuscleGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
