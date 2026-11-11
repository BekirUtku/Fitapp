using FitApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitApp.Infrastructure.Persistence.Configurations;

public class WorkoutTemplateConfiguration : IEntityTypeConfiguration<WorkoutTemplate>
{
    public void Configure(EntityTypeBuilder<WorkoutTemplate> builder)
    {
        builder.ToTable("workout_templates");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(160);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.UpdatedAt);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Exercises)
            .WithOne(x => x.Template!)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class TemplateExerciseConfiguration : IEntityTypeConfiguration<TemplateExercise>
{
    public void Configure(EntityTypeBuilder<TemplateExercise> builder)
    {
        builder.ToTable("template_exercises");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LoadType).HasConversion<int>();
        builder.Property(x => x.ProgressionRule).HasConversion<int>();
        builder.Property(x => x.TargetWeight).HasPrecision(7, 2);
        builder.Property(x => x.Percentage).HasPrecision(5, 2);
        builder.Property(x => x.Note).HasMaxLength(500);

        builder.HasIndex(x => x.TemplateId);

        builder.HasOne(x => x.Exercise)
            .WithMany()
            .HasForeignKey(x => x.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Sets)
            .WithOne(x => x.TemplateExercise!)
            .HasForeignKey(x => x.TemplateExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TemplateSetConfiguration : IEntityTypeConfiguration<TemplateSet>
{
    public void Configure(EntityTypeBuilder<TemplateSet> builder)
    {
        builder.ToTable("template_sets");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SetType).HasConversion<int>();
        builder.Property(x => x.TargetWeight).HasPrecision(7, 2);

        builder.HasIndex(x => x.TemplateExerciseId);
    }
}
