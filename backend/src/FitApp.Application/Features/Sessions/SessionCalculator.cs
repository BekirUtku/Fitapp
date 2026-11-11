using FitApp.Domain.Entities;
using FitApp.Domain.Enums;

namespace FitApp.Application.Features.Sessions;

/// <summary>Antrenman toplamlarını ve tahmini kaloriyi deterministik hesaplar (kural tabanlı).</summary>
public static class SessionCalculator
{
    public static void ApplyTotals(WorkoutSession session)
    {
        var completed = session.Exercises
            .SelectMany(e => e.Sets)
            .Where(s => s.IsCompleted && !s.IsDeleted)
            .ToList();

        session.TotalSets = completed.Count;
        session.TotalReps = completed.Sum(s => s.Reps ?? 0);

        session.TotalVolume = completed
            .Where(s => s.SetType != SetType.WarmUp)
            .Sum(s => (s.Weight ?? 0m) * (s.Reps ?? 0));

        var end = session.EndedAt ?? DateTime.UtcNow;
        var minutes = Math.Max(0, (end - session.StartedAt).TotalMinutes);

        // Basit heuristik: ~6 kcal/dk direnç antrenmanı. Faz 7'de kişiselleştirilecek.
        session.EstimatedCalories = (int)Math.Round(minutes * 6.0);
    }
}
