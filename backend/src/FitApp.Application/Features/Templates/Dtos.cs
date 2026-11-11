namespace FitApp.Application.Features.Templates;

public record TemplateListItemDto(
    Guid Id,
    string Name,
    string? Notes,
    int ExerciseCount,
    DateTime UpdatedAt);

public record TemplateSetDto(
    int SetNumber,
    string SetType,
    decimal? TargetWeight,
    int? TargetReps,
    int? RestSeconds);

public record TemplateExerciseDto(
    Guid Id,
    Guid ExerciseId,
    string ExerciseName,
    int OrderIndex,
    int TargetSets,
    int? TargetReps,
    string LoadType,
    decimal? TargetWeight,
    decimal? Percentage,
    int RestSeconds,
    string? Note,
    Guid? SupersetGroupId,
    string ProgressionRule,
    IReadOnlyList<TemplateSetDto> Sets);

public record TemplateDetailDto(
    Guid Id,
    string Name,
    string? Notes,
    DateTime UpdatedAt,
    IReadOnlyList<TemplateExerciseDto> Exercises);

// --- Yazma modelleri ---

public record TemplateSetInput(
    int SetNumber,
    string SetType,
    decimal? TargetWeight,
    int? TargetReps,
    int? RestSeconds);

public record TemplateExerciseInput(
    Guid ExerciseId,
    int OrderIndex,
    int TargetSets,
    int? TargetReps,
    string LoadType,
    decimal? TargetWeight,
    decimal? Percentage,
    int RestSeconds,
    string? Note,
    Guid? SupersetGroupId,
    string ProgressionRule,
    IReadOnlyList<TemplateSetInput>? Sets);

public record SaveTemplateRequest(
    string Name,
    string? Notes,
    IReadOnlyList<TemplateExerciseInput> Exercises);
