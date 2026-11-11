using FitApp.Domain.Common;

namespace FitApp.Domain.Entities;

public class WorkoutTemplate : BaseEntity
{
    public Guid UserId { get; set; }

    public User? User { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public ICollection<TemplateExercise> Exercises { get; set; } = new List<TemplateExercise>();
}
