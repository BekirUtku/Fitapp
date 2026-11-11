namespace FitApp.Application.Features.Auth;

public record AuthResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    string Role,
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);
