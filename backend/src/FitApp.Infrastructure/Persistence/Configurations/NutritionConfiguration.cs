using FitApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitApp.Infrastructure.Persistence.Configurations;

public class NutritionLogConfiguration : IEntityTypeConfiguration<NutritionLog>
{
    public void Configure(EntityTypeBuilder<NutritionLog> builder)
    {
        builder.ToTable("nutrition_logs");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.UserId, x.LogDate }).IsUnique();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class NutritionTargetConfiguration : IEntityTypeConfiguration<NutritionTarget>
{
    public void Configure(EntityTypeBuilder<NutritionTarget> builder)
    {
        builder.ToTable("nutrition_targets");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.UserId, x.EffectiveFrom });

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
