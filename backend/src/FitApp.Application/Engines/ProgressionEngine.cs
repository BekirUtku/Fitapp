namespace FitApp.Application.Engines;

public record WarmupSet(int SetNumber, decimal Weight, int Reps, int Percent);

public record SetPerformance(decimal Weight, int Reps);

public enum ProgressionAction { IncreaseWeight, IncreaseReps, Hold, NoData }

public record ProgressionSuggestion(
    ProgressionAction Action,
    decimal? SuggestedWeight,
    int? SuggestedReps,
    string Rationale);

/// <summary>Saf, deterministik antrenman zekâsı. LLM yok — tamamen kural tabanlı.</summary>
public static class ProgressionEngine
{
    public static decimal RoundToIncrement(decimal weight, decimal increment)
        => increment <= 0 ? weight : Math.Round(weight / increment) * increment;

    public static decimal Estimated1Rm(decimal weight, int reps)
        => Math.Round(weight * (1m + reps / 30m), 1);

    /// <summary>Çalışma ağırlığından kademeli ısınma setleri üretir (40/60/80%).</summary>
    public static IReadOnlyList<WarmupSet> BuildWarmup(decimal workingWeight, decimal increment = 2.5m)
    {
        if (workingWeight < 20m) return Array.Empty<WarmupSet>();

        var plan = new[] { (0.40, 8), (0.60, 5), (0.80, 3) };
        var result = new List<WarmupSet>();
        var n = 1;
        foreach (var (pct, reps) in plan)
        {
            var w = RoundToIncrement((decimal)pct * workingWeight, increment);
            if (w <= 0) continue;
            result.Add(new WarmupSet(n++, w, reps, (int)(pct * 100)));
        }
        return result;
    }

    /// <summary>
    /// Double progression: tüm çalışma setlerinde üst tekrar sınırına ulaşıldıysa ağırlık artır,
    /// aksi halde aynı ağırlıkta tekrarı yükseltmeye çalış.
    /// </summary>
    public static ProgressionSuggestion SuggestNext(
        IReadOnlyList<SetPerformance> lastWorkingSets,
        int minReps,
        int maxReps,
        decimal increment)
    {
        if (lastWorkingSets.Count == 0)
            return new ProgressionSuggestion(ProgressionAction.NoData, null, null,
                "Öneri için geçmiş veri yok.");

        var topWeight = lastWorkingSets.Max(s => s.Weight);
        var setsAtTop = lastWorkingSets.Where(s => s.Weight >= topWeight).ToList();
        var hitTopReps = setsAtTop.All(s => s.Reps >= maxReps);

        if (hitTopReps)
        {
            var next = RoundToIncrement(topWeight + increment, increment);
            return new ProgressionSuggestion(ProgressionAction.IncreaseWeight, next, minReps,
                $"Tüm setlerde {maxReps} tekrarı geçtin — ağırlığı {next:0.##} kg'a çıkar, {minReps} tekrarla başla.");
        }

        var bestReps = setsAtTop.Max(s => s.Reps);
        if (bestReps < maxReps)
            return new ProgressionSuggestion(ProgressionAction.IncreaseReps, topWeight, bestReps + 1,
                $"Aynı ağırlıkta ({topWeight:0.##} kg) {bestReps + 1} tekrar hedefle.");

        return new ProgressionSuggestion(ProgressionAction.Hold, topWeight, maxReps,
            $"{topWeight:0.##} kg ile {maxReps} tekrarı koru.");
    }

    /// <summary>Son N oturumdaki en iyi 1RM ilerlemiyorsa plato/deload sinyali.</summary>
    public static bool IsPlateau(IReadOnlyList<decimal> recentTop1Rms, int window = 3)
    {
        if (recentTop1Rms.Count < window) return false;
        var lastWindow = recentTop1Rms.TakeLast(window).ToList();
        var best = lastWindow.Max();
        // İlk değer pencerenin en iyisi kadar iyiyse ilerleme yok demektir.
        return lastWindow[0] >= best;
    }
}
