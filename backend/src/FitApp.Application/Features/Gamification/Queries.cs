using FitApp.Application.Common.Interfaces;
using FitApp.Application.Engines;
using FitApp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Gamification;

public record StreakDayDto(DateOnly Date, int Count);
public record StreakDto(int CurrentStreak, int LongestStreak, IReadOnlyList<StreakDayDto> Days);

public record MuscleHeatDto(Guid MuscleGroupId, string MuscleGroupName, int Sets, decimal Volume, double Intensity);

public record BadgeDto(string Code, string Name, string Description, bool Earned, DateTime? EarnedAt);

// ---------- Streak ----------

public record GetStreakQuery(int Days = 120) : IRequest<StreakDto>;

public class GetStreakQueryHandler : IRequestHandler<GetStreakQuery, StreakDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetStreakQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<StreakDto> Handle(GetStreakQuery request, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var days = Math.Clamp(request.Days, 7, 365);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = today.AddDays(-(days - 1));

        var starts = await _db.WorkoutSessions.AsNoTracking()
            .Where(s => s.UserId == userId && s.Status == SessionStatus.Completed)
            .Select(s => s.StartedAt)
            .ToListAsync(ct);

        var byDay = starts
            .Select(DateOnly.FromDateTime)
            .GroupBy(d => d)
            .ToDictionary(g => g.Key, g => g.Count());

        var grid = new List<StreakDayDto>();
        for (var d = from; d <= today; d = d.AddDays(1))
            grid.Add(new StreakDayDto(d, byDay.TryGetValue(d, out var c) ? c : 0));

        var activeDays = byDay.Keys;
        return new StreakDto(
            StreakCalculator.CurrentStreak(activeDays, today),
            StreakCalculator.LongestStreak(activeDays),
            grid);
    }
}

// ---------- Muscle heatmap ----------

public record GetMuscleHeatmapQuery(int Days = 30) : IRequest<IReadOnlyList<MuscleHeatDto>>;

public class GetMuscleHeatmapQueryHandler
    : IRequestHandler<GetMuscleHeatmapQuery, IReadOnlyList<MuscleHeatDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetMuscleHeatmapQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<MuscleHeatDto>> Handle(GetMuscleHeatmapQuery request, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var since = DateTime.UtcNow.AddDays(-Math.Clamp(request.Days, 7, 180));

        var rows = await _db.WorkoutSets.AsNoTracking()
            .Where(s => s.IsCompleted && !s.IsDeleted && s.SetType != SetType.WarmUp
                && s.SessionExercise!.Session!.UserId == userId
                && !s.SessionExercise.Session.IsDeleted
                && s.SessionExercise.Session.StartedAt >= since)
            .Select(s => new
            {
                MgId = s.SessionExercise!.Exercise!.PrimaryMuscleGroupId,
                MgName = s.SessionExercise.Exercise.PrimaryMuscleGroup!.Name,
                Volume = (s.Weight ?? 0m) * (s.Reps ?? 0)
            })
            .ToListAsync(ct);

        var grouped = rows
            .GroupBy(r => new { r.MgId, r.MgName })
            .Select(g => (g.Key.MgId, g.Key.MgName, Sets: g.Count(), Volume: g.Sum(x => x.Volume)))
            .ToList();

        var maxVolume = grouped.Count == 0 ? 0m : grouped.Max(x => x.Volume);

        return grouped
            .OrderByDescending(x => x.Volume)
            .Select(x => new MuscleHeatDto(
                x.MgId, x.MgName, x.Sets, x.Volume,
                maxVolume <= 0 ? 0 : (double)(x.Volume / maxVolume)))
            .ToList();
    }
}

// ---------- Badges ----------

public record GetBadgesQuery : IRequest<IReadOnlyList<BadgeDto>>;

public class GetBadgesQueryHandler : IRequestHandler<GetBadgesQuery, IReadOnlyList<BadgeDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetBadgesQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<BadgeDto>> Handle(GetBadgesQuery request, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();

        var earned = await _db.UserBadges.AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToDictionaryAsync(x => x.BadgeId, x => x.EarnedAt, ct);

        var badges = await _db.Badges.AsNoTracking().OrderBy(b => b.Name).ToListAsync(ct);

        return badges
            .Select(b => new BadgeDto(
                b.Code, b.Name, b.Description,
                earned.ContainsKey(b.Id),
                earned.TryGetValue(b.Id, out var at) ? at : null))
            .ToList();
    }
}
