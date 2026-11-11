using FitApp.Domain.Common;

namespace FitApp.Domain.Entities;

public class Equipment : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}
