using FitApp.Domain.Common;
using FitApp.Domain.Enums;

namespace FitApp.Domain.Entities;

public class WorkoutSession : BaseEntity
{
    public Guid UserId { get; set; }

    public User? User { get; set; }

    /// <summary>Null = manuel (şablonsuz) antrenman.</summary>
    public Guid? TemplateId { get; set; }

    public string Name { get; set; } = string.Empty;

    public SessionStatus Status { get; set; } = SessionStatus.InProgress;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? EndedAt { get; set; }

    public decimal TotalVolume { get; set; }

    public int TotalSets { get; set; }

    public int TotalReps { get; set; }

    public int EstimatedCalories { get; set; }

    public string? Note { get; set; }

    public ICollection<SessionExercise> Exercises { get; set; } = new List<SessionExercise>();
}
