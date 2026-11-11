using FitApp.Application.Common.Interfaces;
using FitApp.Domain.Entities;
using FitApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.PersonalRecords;

/// <summary>Kural tabanlı kişisel rekor tespiti. Yeni PR kırılan setleri işaretler ve PR tarihçesine kayıt ekler.</summary>
public static class PersonalRecordService
{
    public static decimal Estimated1Rm(decimal weight, int reps) =>
        Math.Round(weight * (1m + reps / 30m), 1);

    public static async Task<List<PersonalRecord>> DetectAsync(
        IAppDbContext db, Guid userId, WorkoutSession session, CancellationToken ct)
    {
        var created = new List<PersonalRecord>();

        var sets = session.Exercises
            .SelectMany(e => e.Sets.Select(s => (e.ExerciseId, Set: s)))
            .Where(t => !t.Set.IsDeleted && t.Set.IsCompleted && t.Set.SetType != SetType.WarmUp)
            .ToList();

        if (sets.Count == 0) return created;

        var exerciseIds = sets.Select(t => t.ExerciseId).Distinct().ToList();

        var existing = await db.PersonalRecords
            .Where(p => p.UserId == userId && exerciseIds.Contains(p.ExerciseId))
            .ToListAsync(ct);

        var running = new Dictionary<(Guid, RecordType), decimal>();

        decimal StoredBest(Guid ex, RecordType rt) => existing
            .Where(p => p.ExerciseId == ex && p.RecordType == rt)
            .Select(p => p.Value).DefaultIfEmpty(0m).Max();

        decimal CurrentBest(Guid ex, RecordType rt) =>
            Math.Max(StoredBest(ex, rt), running.TryGetValue((ex, rt), out var v) ? v : 0m);

        void Consider(Guid ex, RecordType rt, decimal value, WorkoutSet src, decimal? weight, int? reps)
        {
            if (value <= CurrentBest(ex, rt)) return;
            running[(ex, rt)] = value;
            var pr = new PersonalRecord
            {
                UserId = userId,
                ExerciseId = ex,
                RecordType = rt,
                Value = value,
                Weight = weight,
                Reps = reps,
                SourceSetId = src.Id,
                AchievedAt = session.EndedAt ?? DateTime.UtcNow
            };
            db.PersonalRecords.Add(pr);
            created.Add(pr);
            src.IsPr = true;
        }

        foreach (var (ex, s) in sets)
        {
            if (s.Weight is > 0m && s.Reps is > 0)
            {
                Consider(ex, RecordType.Estimated1Rm, Estimated1Rm(s.Weight.Value, s.Reps.Value), s, s.Weight, s.Reps);
                Consider(ex, RecordType.MaxWeight, s.Weight.Value, s, s.Weight, s.Reps);
                Consider(ex, RecordType.MaxReps, s.Reps.Value, s, s.Weight, s.Reps);
                Consider(ex, RecordType.MaxVolume, s.Weight.Value * s.Reps.Value, s, s.Weight, s.Reps);
            }

            if (s.DurationSeconds is > 0)
                Consider(ex, RecordType.MaxDuration, s.DurationSeconds.Value, s, null, null);
        }

        return created;
    }
}
