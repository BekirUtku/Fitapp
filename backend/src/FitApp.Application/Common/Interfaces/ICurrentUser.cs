using FitApp.Domain.Enums;

namespace FitApp.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin => Role == UserRole.Admin;

    Guid RequireUserId();
}
