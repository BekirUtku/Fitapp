using FitApp.Domain.Common;

namespace FitApp.Domain.Entities;

public class NutritionLog : BaseEntity
{
    public Guid UserId { get; set; }

    public DateOnly LogDate { get; set; }

    public int Calories { get; set; }

    public int ProteinG { get; set; }

    public int CarbsG { get; set; }

    public int FatG { get; set; }

    public int WaterMl { get; set; }
}
