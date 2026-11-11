using FitApp.Domain.Common;

namespace FitApp.Domain.Entities;

public class BodyMeasurement : BaseEntity
{
    public Guid UserId { get; set; }

    public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;

    public decimal? Bodyweight { get; set; }

    /// <summary>Serbest ölçüler (göğüs, bel, kol...) — jsonb olarak saklanır.</summary>
    public Dictionary<string, decimal> Measurements { get; set; } = new();

    public string? Note { get; set; }
}
