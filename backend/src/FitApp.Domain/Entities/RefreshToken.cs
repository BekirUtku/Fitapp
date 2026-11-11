using FitApp.Domain.Common;

namespace FitApp.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }

    public User? User { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool Revoked { get; set; }

    public bool IsActive => !Revoked && DateTime.UtcNow < ExpiresAt;
}
