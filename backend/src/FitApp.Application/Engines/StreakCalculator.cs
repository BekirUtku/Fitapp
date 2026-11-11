namespace FitApp.Application.Engines;

/// <summary>Antrenman yapılan günlerden seri (streak) hesaplayan saf yardımcı.</summary>
public static class StreakCalculator
{
    public static int CurrentStreak(IEnumerable<DateOnly> activeDays, DateOnly today)
    {
        var set = activeDays.ToHashSet();
        if (set.Count == 0) return 0;

        // Seri bugünden ya da dünden başlayabilir (bugün henüz çalışılmamış olabilir).
        var cursor = set.Contains(today) ? today : today.AddDays(-1);
        if (!set.Contains(cursor)) return 0;

        var streak = 0;
        while (set.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }
        return streak;
    }

    public static int LongestStreak(IEnumerable<DateOnly> activeDays)
    {
        var ordered = activeDays.Distinct().OrderBy(d => d).ToList();
        if (ordered.Count == 0) return 0;

        var longest = 1;
        var current = 1;
        for (var i = 1; i < ordered.Count; i++)
        {
            if (ordered[i] == ordered[i - 1].AddDays(1)) current++;
            else current = 1;
            longest = Math.Max(longest, current);
        }
        return longest;
    }
}
