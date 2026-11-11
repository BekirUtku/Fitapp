using FitApp.Domain.Common;
using FitApp.Domain.Enums;

namespace FitApp.Domain.Entities;

public class Exercise : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public Guid PrimaryMuscleGroupId { get; set; }

    public MuscleGroup? PrimaryMuscleGroup { get; set; }

    public Guid? EquipmentId { get; set; }

    public Equipment? Equipment { get; set; }

    public MetricType MetricType { get; set; } = MetricType.WeightReps;

    public string? VideoUrl { get; set; }

    public string? ImageUrl { get; set; }

    public string? Description { get; set; }

    public bool IsCustom { get; set; }

    /// <summary>Null = sistem hareketi (Admin tarafından yönetilir).</summary>
    public Guid? OwnerUserId { get; set; }

    public User? OwnerUser { get; set; }

    public ICollection<ExerciseSecondaryMuscle> SecondaryMuscles { get; set; } = new List<ExerciseSecondaryMuscle>();
}
