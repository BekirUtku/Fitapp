namespace FitApp.Application.Features.Sessions;

public record WorkoutSetDto(
    Guid Id,
    int SetNumber,
    string SetType,
    decimal? Weight,
    int? Reps,
    int? DurationSeconds,
    decimal? DistanceMeters,
    int? Rpe,
    string? Note,
    bool IsCompleted,
    bool IsPr,
    DateTime? CompletedAt);

public record SessionExerciseDto(
    Guid Id,
    Guid ExerciseId,
    string ExerciseName,
    string MetricType,
    int OrderIndex,
    Guid? SupersetGroupId,
    string? Note,
    IReadOnlyList<WorkoutSetDto> Sets);

public record SessionListItemDto(
    Guid Id,
    string Name,
    string Status,
    DateTime StartedAt,
    DateTime? EndedAt,
    decimal TotalVolume,
    int TotalSets);

public record SessionDetailDto(
    Guid Id,
    string Name,
    Guid? TemplateId,
    string Status,
    DateTime StartedAt,
    DateTime? EndedAt,
    decimal TotalVolume,
    int TotalSets,
    int TotalReps,
    int EstimatedCalories,
    string? Note,
    IReadOnlyList<SessionExerciseDto> Exercises);

// --- Yazma modelleri ---

public record StartSessionRequest(Guid? TemplateId, string? Name);

public record AddSessionExerciseRequest(Guid ExerciseId, Guid? SupersetGroupId);

public record AddSetRequest(
    string SetType,
    decimal? Weight,
    int? Reps,
    int? DurationSeconds,
    decimal? DistanceMeters,
    int? Rpe,
    string? Note);

public record UpdateSetRequest(
    string SetType,
    decimal? Weight,
    int? Reps,
    int? DurationSeconds,
    decimal? DistanceMeters,
    int? Rpe,
    string? Note);
