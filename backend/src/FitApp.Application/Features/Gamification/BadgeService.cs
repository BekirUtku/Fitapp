using FitApp.Application.Common.Interfaces;
using FitApp.Application.Engines;
using FitApp.Domain.Entities;
using FitApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Gamification;

/// <summary>Kural tabanlı rozet kazanımı. Antrenman bitince/sync'te değerlendirilir.</summary>
public class BadgeService
{
    private readonly IAppDbContext _db;

    public BadgeService(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Badge>> EvaluateAsync(Guid userId, CancellationToken ct)
    {
        var badges = await _db.Badges.AsNoTracking().ToListAsync(ct);
        if (badges.Count == 0) return Array.Empty<Badge>();

        var earnedCodes = await _db.UserBadges
            .Where(x => x.UserId == userId)
            .Join(_db.Badges, ub => ub.BadgeId, b => b.Id, (ub, b) => b.Code)
            .ToListAsync(ct);
        var earned = earnedCodes.ToHashSet();

        var completed = await _db.WorkoutSessions
            .Where(s => s.UserId == userId && s.Status == SessionStatus.Completed)
            .Select(s => new { s.StartedAt, s.TotalVolume })
            .ToListAsync(ct);

        var completedCount = completed.Count;
        var totalVolume = completed.Sum(x => x.TotalVolume);
        var activeDays = completed.Select(x => DateOnly.FromDateTime(x.StartedAt)).ToHashSet();
        var currentStreak = StreakCalculator.CurrentStreak(activeDays, DateOnly.FromDateTime(DateTime.UtcNow));
        var prCount = await _db.PersonalRecords.CountAsync(p => p.UserId == userId, ct);

        bool Meets(string code) => code switch
        {
            "first_workout" => completedCount >= 1,
            "streak_7" => currentStreak >= 7,
            "first_pr" => prCount >= 1,
            "workouts_50" => completedCount >= 50,
            "volume_100k" => totalVolume >= 100_000m,
            _ => false
        };

        var newlyEarned = new List<Badge>();
        foreach (var badge in badges)
        {
            if (earned.Contains(badge.Code) || !Meets(badge.Code)) continue;
            _db.UserBadges.Add(new UserBadge { UserId = userId, BadgeId = badge.Id });
            newlyEarned.Add(badge);
        }

        return newlyEarned;
    }
}
