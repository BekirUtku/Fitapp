using System.Text.Json;
using FitApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitApp.Infrastructure.Persistence.Configurations;

public class PersonalRecordConfiguration : IEntityTypeConfiguration<PersonalRecord>
{
    public void Configure(EntityTypeBuilder<PersonalRecord> builder)
    {
        builder.ToTable("personal_records");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RecordType).HasConversion<int>();
        builder.Property(x => x.Value).HasPrecision(10, 2);
        builder.Property(x => x.Weight).HasPrecision(7, 2);

        builder.HasIndex(x => new { x.UserId, x.ExerciseId, x.RecordType });
        builder.HasIndex(x => x.UpdatedAt);

        builder.HasOne(x => x.Exercise)
            .WithMany()
            .HasForeignKey(x => x.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class BodyMeasurementConfiguration : IEntityTypeConfiguration<BodyMeasurement>
{
    public void Configure(EntityTypeBuilder<BodyMeasurement> builder)
    {
        builder.ToTable("body_measurements");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Bodyweight).HasPrecision(6, 2);
        builder.Property(x => x.Note).HasMaxLength(500);

        var dictComparer = new ValueComparer<Dictionary<string, decimal>>(
            (a, b) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null)
                      == JsonSerializer.Serialize(b, (JsonSerializerOptions?)null),
            v => v == null ? 0 : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null).GetHashCode(),
            v => JsonSerializer.Deserialize<Dictionary<string, decimal>>(
                     JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null)
                 ?? new());

        builder.Property(x => x.Measurements)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, decimal>>(v, (JsonSerializerOptions?)null) ?? new())
            .Metadata.SetValueComparer(dictComparer);

        builder.HasIndex(x => new { x.UserId, x.MeasuredAt });

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
