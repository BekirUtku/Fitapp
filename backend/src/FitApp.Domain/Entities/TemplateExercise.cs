using FitApp.Domain.Common;
using FitApp.Domain.Enums;

namespace FitApp.Domain.Entities;

public class TemplateExercise : BaseEntity
{
    public Guid TemplateId { get; set; }

    public WorkoutTemplate? Template { get; set; }

    public Guid ExerciseId { get; set; }

    public Exercise? Exercise { get; set; }

    public int OrderIndex { get; set; }

    public int TargetSets { get; set; }

    public int? TargetReps { get; set; }

    public LoadType LoadType { get; set; } = LoadType.Absolute;

    public decimal? TargetWeight { get; set; }

    /// <summary>LoadType = PercentageOf1Rm iken kullanılır (0-100).</summary>
    public decimal? Percentage { get; set; }

    public int RestSeconds { get; set; }

    public string? Note { get; set; }

    /// <summary>Aynı superset grubuna ait hareketler bu değeri paylaşır. Null = solo.</summary>
    public Guid? SupersetGroupId { get; set; }

    public ProgressionRule ProgressionRule { get; set; } = ProgressionRule.None;

    /// <summary>Opsiyonel set-bazlı override (warm-up vb.). Boşsa TargetSets adet varsayılan set kullanılır.</summary>
    public ICollection<TemplateSet> Sets { get; set; } = new List<TemplateSet>();
}
