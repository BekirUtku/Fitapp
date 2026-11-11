using FitApp.Domain.Common;
using FitApp.Domain.Enums;

namespace FitApp.Domain.Entities;

public class PersonalRecord : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid ExerciseId { get; set; }

    public Exercise? Exercise { get; set; }

    public RecordType RecordType { get; set; }

    public decimal Value { get; set; }

    public decimal? Weight { get; set; }

    public int? Reps { get; set; }

    public Guid? SourceSetId { get; set; }

    public DateTime AchievedAt { get; set; } = DateTime.UtcNow;
}
