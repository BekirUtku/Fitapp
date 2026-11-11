using FitApp.Application.Common.Interfaces;
using FitApp.Application.Features.Insights;
using FitApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Engines;

/// <summary>Geçmiş antrenman verisinden deterministik istatistik + şablon cümle üretir (LLM yok).</summary>
public class InsightService
{
    private readonly IAppDbContext _db;

    public InsightService(IAppDbContext db) => _db = db;

    private record SetFact(
        DateTime Date, Guid ExerciseId, string ExerciseName,
        Guid MuscleGroupId, string MuscleGroupName,
        decimal? Weight, int? Reps, DateTime? CompletedAt, SetType SetType);

    public async Task<IReadOnlyList<InsightDto>> BuildSummaryAsync(Guid userId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var since = now.AddDays(-30);

        var facts = await _db.WorkoutSets.AsNoTracking()
            .Where(s => s.IsCompleted && !s.IsDeleted
                && s.SessionExercise!.Session!.UserId == userId
                && !s.SessionExercise.Session.IsDeleted
                && s.SessionExercise.Session.StartedAt >= since)
            .Select(s => new SetFact(
                s.SessionExercise!.Session!.StartedAt,
                s.SessionExercise.ExerciseId,
                s.SessionExercise.Exercise!.Name,
                s.SessionExercise.Exercise.PrimaryMuscleGroupId,
                s.SessionExercise.Exercise.PrimaryMuscleGroup!.Name,
                s.Weight, s.Reps, s.CompletedAt, s.SetType))
            .ToListAsync(ct);

        var insights = new List<InsightDto>();

        AddIfNotNull(insights, WeeklyVolumeChange(facts, now));
        AddIfNotNull(insights, MostImproved(facts));
        AddIfNotNull(insights, StrongestMuscle(facts));
        AddIfNotNull(insights, NeglectedMuscle(facts, now));
        AddIfNotNull(insights, AverageRest(facts));
        AddIfNotNull(insights, await ClosestToPrAsync(userId, facts, ct));

        return insights;
    }

    private static void AddIfNotNull(List<InsightDto> list, InsightDto? item)
    {
        if (item is not null) list.Add(item);
    }

    /// <summary>Antrenman sonu analizi: PR'lar, hacim karşılaştırması, protein hedefi.</summary>
    public async Task<IReadOnlyList<InsightDto>> BuildSessionAsync(Guid userId, Guid sessionId, CancellationToken ct)
    {
        var session = await _db.WorkoutSessions.AsNoTracking()
            .Include(s => s.Exercises).ThenInclude(e => e.Sets)
            .Include(s => s.Exercises).ThenInclude(e => e.Exercise)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, ct);
        if (session is null) return Array.Empty<InsightDto>();

        var result = new List<InsightDto>();

        // Yeni PR'lar
        var prExercises = session.Exercises
            .Where(e => e.Sets.Any(s => s.IsPr && !s.IsDeleted))
            .Select(e => e.Exercise?.Name)
            .Where(n => n is not null)
            .Distinct()
            .ToList();
        foreach (var name in prExercises)
            result.Add(new InsightDto("session_pr", $"{name}'de yeni kişisel rekor! 🎉"));

        // Hacim karşılaştırması (önceki tamamlanmış antrenman)
        decimal SessionVolume(IEnumerable<Domain.Entities.WorkoutSet> sets) =>
            sets.Where(s => s.IsCompleted && !s.IsDeleted && s.SetType != SetType.WarmUp)
                .Sum(s => (s.Weight ?? 0m) * (s.Reps ?? 0));

        var thisVolume = SessionVolume(session.Exercises.SelectMany(e => e.Sets));

        var prev = await _db.WorkoutSessions.AsNoTracking()
            .Include(s => s.Exercises).ThenInclude(e => e.Sets)
            .Where(s => s.UserId == userId && s.Status == SessionStatus.Completed
                && s.StartedAt < session.StartedAt && s.Id != session.Id)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(ct);

        if (prev is not null)
        {
            var prevVolume = SessionVolume(prev.Exercises.SelectMany(e => e.Sets));
            if (prevVolume > 0)
            {
                var pct = (int)Math.Round((thisVolume - prevVolume) / prevVolume * 100);
                if (pct != 0)
                {
                    var dir = pct > 0 ? "daha fazla" : "daha az";
                    result.Add(new InsightDto("session_volume",
                        $"Geçen antrenmana göre %{Math.Abs(pct)} {dir} hacim yaptın."));
                }
            }
        }

        // Protein hedefi
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var target = await _db.NutritionTargets.AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.EffectiveFrom)
            .FirstOrDefaultAsync(ct);
        if (target is { ProteinG: > 0 })
        {
            var log = await _db.NutritionLogs.AsNoTracking()
                .FirstOrDefaultAsync(l => l.UserId == userId && l.LogDate == today, ct);
            var consumed = log?.ProteinG ?? 0;
            var remaining = target.ProteinG - consumed;
            if (remaining > 0)
                result.Add(new InsightDto("protein_remaining", $"Protein hedefine {remaining} g kaldı."));
        }

        return result;
    }

    private static IEnumerable<SetFact> Working(IEnumerable<SetFact> f) =>
        f.Where(x => x.SetType != SetType.WarmUp && x.Weight is > 0m && x.Reps is > 0);

    private static decimal Volume(SetFact s) => (s.Weight ?? 0m) * (s.Reps ?? 0);

    private static InsightDto? WeeklyVolumeChange(List<SetFact> facts, DateTime now)
    {
        var thisWeek = Working(facts).Where(s => s.Date >= now.AddDays(-7)).Sum(Volume);
        var lastWeek = Working(facts).Where(s => s.Date >= now.AddDays(-14) && s.Date < now.AddDays(-7)).Sum(Volume);
        if (lastWeek <= 0) return null;

        var pct = (int)Math.Round((thisWeek - lastWeek) / lastWeek * 100);
        if (pct == 0) return null;
        var dir = pct > 0 ? "daha fazla" : "daha az";
        return new InsightDto("weekly_volume",
            $"Bu hafta geçen haftaya göre %{Math.Abs(pct)} {dir} hacim yaptın.");
    }

    private static InsightDto? MostImproved(List<SetFact> facts)
    {
        decimal Best1Rm(IEnumerable<SetFact> f) =>
            f.Select(s => ProgressionEngine.Estimated1Rm(s.Weight!.Value, s.Reps!.Value))
             .DefaultIfEmpty(0m).Max();

        var best = Working(facts)
            .GroupBy(s => new { s.ExerciseId, s.ExerciseName })
            .Select(g =>
            {
                var ordered = g.OrderBy(x => x.Date).ToList();
                var firstDay = ordered.First().Date.Date;
                var lastDay = ordered.Last().Date.Date;
                if (firstDay == lastDay) return (g.Key.ExerciseName, Delta: 0m);
                var startBest = Best1Rm(ordered.Where(x => x.Date.Date == firstDay));
                var endBest = Best1Rm(ordered.Where(x => x.Date.Date == lastDay));
                return (g.Key.ExerciseName, Delta: endBest - startBest);
            })
            .Where(x => x.Delta > 0)
            .OrderByDescending(x => x.Delta)
            .FirstOrDefault();

        if (best.ExerciseName is null) return null;
        return new InsightDto("most_improved",
            $"Son 30 günde en çok gelişen hareketin: {best.ExerciseName} (+{best.Delta:0.#} kg tahmini 1RM).");
    }

    private static InsightDto? StrongestMuscle(List<SetFact> facts)
    {
        var top = Working(facts)
            .GroupBy(s => s.MuscleGroupName)
            .Select(g => (Name: g.Key, Volume: g.Sum(Volume)))
            .OrderByDescending(x => x.Volume)
            .FirstOrDefault();

        if (top.Name is null || top.Volume <= 0) return null;
        return new InsightDto("strongest_muscle", $"En güçlü kas grubun: {top.Name}.");
    }

    private static InsightDto? NeglectedMuscle(List<SetFact> facts, DateTime now)
    {
        var recentCutoff = now.AddDays(-14);
        var trainedBefore = facts.Where(s => s.Date < recentCutoff)
            .Select(s => s.MuscleGroupName).Distinct();
        var trainedRecently = facts.Where(s => s.Date >= recentCutoff)
            .Select(s => s.MuscleGroupName).ToHashSet();

        var neglected = trainedBefore.FirstOrDefault(m => !trainedRecently.Contains(m));
        if (neglected is null) return null;
        return new InsightDto("neglected_muscle", $"{neglected} kasını 2 haftadır çalışmadın.");
    }

    private async Task<InsightDto?> ClosestToPrAsync(Guid userId, List<SetFact> facts, CancellationToken ct)
    {
        var prs = await _db.PersonalRecords.AsNoTracking()
            .Where(p => p.UserId == userId && p.RecordType == RecordType.Estimated1Rm)
            .GroupBy(p => p.ExerciseId)
            .Select(g => new { ExerciseId = g.Key, Best = g.Max(x => x.Value) })
            .ToListAsync(ct);
        if (prs.Count == 0) return null;

        var recent = Working(facts)
            .GroupBy(s => s.ExerciseId)
            .ToDictionary(g => g.Key,
                g => g.Max(s => ProgressionEngine.Estimated1Rm(s.Weight!.Value, s.Reps!.Value)));

        var names = facts.GroupBy(s => s.ExerciseId).ToDictionary(g => g.Key, g => g.First().ExerciseName);

        var candidate = prs
            .Where(p => recent.ContainsKey(p.ExerciseId))
            .Select(p => new { p.ExerciseId, Gap = p.Best - recent[p.ExerciseId] })
            .Where(x => x.Gap > 0)
            .OrderBy(x => x.Gap)
            .FirstOrDefault();

        if (candidate is null || !names.TryGetValue(candidate.ExerciseId, out var name)) return null;
        return new InsightDto("closest_pr", $"{name} hareketinde PR'a {candidate.Gap:0.#} kg kaldı.");
    }

    private static InsightDto? AverageRest(List<SetFact> facts)
    {
        var withTime = facts.Where(s => s.CompletedAt is not null)
            .OrderBy(s => s.CompletedAt)
            .ToList();
        if (withTime.Count < 3) return null;

        var gaps = new List<double>();
        for (var i = 1; i < withTime.Count; i++)
        {
            var gap = (withTime[i].CompletedAt!.Value - withTime[i - 1].CompletedAt!.Value).TotalSeconds;
            if (gap is > 5 and < 600) gaps.Add(gap); // makul dinlenme aralığı
        }
        if (gaps.Count == 0) return null;

        var avg = (int)Math.Round(gaps.Average());
        return new InsightDto("avg_rest", $"Ortalama dinlenme süren {avg / 60}:{avg % 60:00} dakika.");
    }
}
