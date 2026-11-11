using FitApp.Domain.Common;
using FitApp.Domain.Enums;

namespace FitApp.Domain.Entities;

public class WorkoutSet : BaseEntity
{
    public Guid SessionExerciseId { get; set; }

    public SessionExercise? SessionExercise { get; set; }

    public int SetNumber { get; set; }

    public SetType SetType { get; set; } = SetType.Normal;

    public decimal? Weight { get; set; }

    public int? Reps { get; set; }

    public int? DurationSeconds { get; set; }

    public decimal? DistanceMeters { get; set; }

    /// <summary>Rate of Perceived Exertion (1-10), opsiyonel.</summary>
    public int? Rpe { get; set; }

    public string? Note { get; set; }

    public bool IsCompleted { get; set; }

    public bool IsPr { get; set; }

    public DateTime? CompletedAt { get; set; }
}
