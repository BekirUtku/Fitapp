using FitApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitApp.Infrastructure.Persistence.Configurations;

public class WeeklyPlanConfiguration : IEntityTypeConfiguration<WeeklyPlan>
{
    public void Configure(EntityTypeBuilder<WeeklyPlan> builder)
    {
        builder.ToTable("weekly_plans");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(160);
        builder.HasIndex(x => x.UserId);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Days)
            .WithOne(x => x.WeeklyPlan!)
            .HasForeignKey(x => x.WeeklyPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class WeeklyPlanDayConfiguration : IEntityTypeConfiguration<WeeklyPlanDay>
{
    public void Configure(EntityTypeBuilder<WeeklyPlanDay> builder)
    {
        builder.ToTable("weekly_plan_days");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.WeeklyPlanId, x.DayOfWeek }).IsUnique();

        builder.HasOne(x => x.Template)
            .WithMany()
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
