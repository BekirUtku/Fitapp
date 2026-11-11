using FitApp.Application.Common.Exceptions;
using FitApp.Application.Common.Interfaces;
using FitApp.Domain.Entities;
using FitApp.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Auth.Commands;

public record RegisterCommand(string Email, string Password, string DisplayName) : IRequest<AuthResponse>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(120);
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtService _jwt;

    public RegisterCommandHandler(IAppDbContext db, IPasswordHasher hasher, IJwtService jwt)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var exists = await _db.Users.IgnoreQueryFilters().AnyAsync(x => x.Email == email, ct);
        if (exists)
            throw new ConflictException("Bu e-posta zaten kayıtlı.");

        var user = new User
        {
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            Role = UserRole.User,
            PasswordHash = _hasher.Hash(request.Password)
        };

        _db.Users.Add(user);

        var (refresh, refreshExp) = _jwt.GenerateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _jwt.HashRefreshToken(refresh),
            ExpiresAt = refreshExp
        });

        await _db.SaveChangesAsync(ct);

        var access = _jwt.GenerateAccessToken(user);
        return new AuthResponse(user.Id, user.Email, user.DisplayName, user.Role.ToString(),
            access, refresh, refreshExp);
    }
}
