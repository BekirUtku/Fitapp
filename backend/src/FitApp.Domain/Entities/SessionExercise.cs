using FitApp.Domain.Common;

namespace FitApp.Domain.Entities;

public class SessionExercise : BaseEntity
{
    public Guid SessionId { get; set; }

    public WorkoutSession? Session { get; set; }

    public Guid ExerciseId { get; set; }

    public Exercise? Exercise { get; set; }

    public int OrderIndex { get; set; }

    /// <summary>Superset grubu; null = solo.</summary>
    public Guid? SupersetGroupId { get; set; }

    public string? Note { get; set; }

    public ICollection<WorkoutSet> Sets { get; set; } = new List<WorkoutSet>();
}
