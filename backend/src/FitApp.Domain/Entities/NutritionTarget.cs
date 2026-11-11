using FitApp.Domain.Common;

namespace FitApp.Domain.Entities;

public class NutritionTarget : BaseEntity
{
    public Guid UserId { get; set; }

    public int Calories { get; set; }

    public int ProteinG { get; set; }

    public int CarbsG { get; set; }

    public int FatG { get; set; }

    public int WaterMl { get; set; }

    /// <summary>Bu tarihten itibaren geçerli. En güncel olan aktif hedeftir.</summary>
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
}
