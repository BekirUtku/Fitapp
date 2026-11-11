using FitApp.Domain.Entities;

namespace FitApp.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);

    (string token, DateTime expiresAt) GenerateRefreshToken();

    string HashRefreshToken(string token);
}
