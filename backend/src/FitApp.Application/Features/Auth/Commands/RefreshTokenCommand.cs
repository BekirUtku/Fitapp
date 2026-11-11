using FitApp.Application.Common.Exceptions;
using FitApp.Application.Common.Interfaces;
using FitApp.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IAppDbContext _db;
    private readonly IJwtService _jwt;

    public RefreshTokenCommandHandler(IAppDbContext db, IJwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var hash = _jwt.HashRefreshToken(request.RefreshToken);

        var token = await _db.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

        if (token is null || !token.IsActive || token.User is null)
            throw new UnauthorizedException("Geçersiz veya süresi dolmuş oturum.");

        // Rotasyon: eskiyi iptal et, yenisini üret.
        token.Revoked = true;

        var (refresh, refreshExp) = _jwt.GenerateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = token.UserId,
            TokenHash = _jwt.HashRefreshToken(refresh),
            ExpiresAt = refreshExp
        });

        await _db.SaveChangesAsync(ct);

        var access = _jwt.GenerateAccessToken(token.User);
        return new AuthResponse(token.User.Id, token.User.Email, token.User.DisplayName,
            token.User.Role.ToString(), access, refresh, refreshExp);
    }
}
