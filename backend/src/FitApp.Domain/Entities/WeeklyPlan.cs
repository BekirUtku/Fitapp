using FitApp.Domain.Common;

namespace FitApp.Domain.Entities;

public class WeeklyPlan : BaseEntity
{
    public Guid UserId { get; set; }

    public User? User { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public ICollection<WeeklyPlanDay> Days { get; set; } = new List<WeeklyPlanDay>();
}
