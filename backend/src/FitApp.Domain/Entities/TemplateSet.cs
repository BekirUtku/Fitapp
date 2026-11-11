using FitApp.Domain.Common;
using FitApp.Domain.Enums;

namespace FitApp.Domain.Entities;

public class TemplateSet : BaseEntity
{
    public Guid TemplateExerciseId { get; set; }

    public TemplateExercise? TemplateExercise { get; set; }

    public int SetNumber { get; set; }

    public SetType SetType { get; set; } = SetType.Normal;

    public decimal? TargetWeight { get; set; }

    public int? TargetReps { get; set; }

    public int? RestSeconds { get; set; }
}
