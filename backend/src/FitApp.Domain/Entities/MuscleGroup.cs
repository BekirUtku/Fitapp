using FitApp.Domain.Common;
using FitApp.Domain.Enums;

namespace FitApp.Domain.Entities;

public class MuscleGroup : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public BodyRegion BodyRegion { get; set; }

    public ICollection<Exercise> PrimaryExercises { get; set; } = new List<Exercise>();

    public ICollection<ExerciseSecondaryMuscle> SecondaryLinks { get; set; } = new List<ExerciseSecondaryMuscle>();
}
