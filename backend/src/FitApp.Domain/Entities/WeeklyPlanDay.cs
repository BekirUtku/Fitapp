using FitApp.Domain.Common;

namespace FitApp.Domain.Entities;

public class WeeklyPlanDay : BaseEntity
{
    public Guid WeeklyPlanId { get; set; }

    public WeeklyPlan? WeeklyPlan { get; set; }

    /// <summary>1 = Pazartesi ... 7 = Pazar (ISO-8601).</summary>
    public int DayOfWeek { get; set; }

    /// <summary>Null = dinlenme günü.</summary>
    public Guid? TemplateId { get; set; }

    public WorkoutTemplate? Template { get; set; }
}
