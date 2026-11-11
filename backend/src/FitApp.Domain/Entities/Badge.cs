using FitApp.Domain.Common;

namespace FitApp.Domain.Entities;

public class Badge : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
}

public class UserBadge : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid BadgeId { get; set; }

    public Badge? Badge { get; set; }

    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
}
