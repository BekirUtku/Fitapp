namespace FitApp.Domain.Entities;

/// <summary>Exercise ↔ MuscleGroup çoktan-çoğa (ikincil kaslar) ara tablosu.</summary>
public class ExerciseSecondaryMuscle
{
    public Guid ExerciseId { get; set; }

    public Exercise? Exercise { get; set; }

    public Guid MuscleGroupId { get; set; }

    public MuscleGroup? MuscleGroup { get; set; }
}
