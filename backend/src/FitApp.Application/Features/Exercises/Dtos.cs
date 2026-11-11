namespace FitApp.Application.Features.Exercises;

public record ExerciseListItemDto(
    Guid Id,
    string Name,
    string MetricType,
    Guid PrimaryMuscleGroupId,
    string PrimaryMuscleGroupName,
    Guid? EquipmentId,
    string? EquipmentName,
    string? ImageUrl,
    bool IsCustom);

public record ExerciseDetailDto(
    Guid Id,
    string Name,
    string MetricType,
    Guid PrimaryMuscleGroupId,
    string PrimaryMuscleGroupName,
    Guid? EquipmentId,
    string? EquipmentName,
    string? VideoUrl,
    string? ImageUrl,
    string? Description,
    bool IsCustom,
    Guid? OwnerUserId,
    IReadOnlyList<Guid> SecondaryMuscleGroupIds);

public record CreateExerciseRequest(
    string Name,
    Guid PrimaryMuscleGroupId,
    Guid? EquipmentId,
    string MetricType,
    string? VideoUrl,
    string? ImageUrl,
    string? Description,
    IReadOnlyList<Guid>? SecondaryMuscleGroupIds);

public record UpdateExerciseRequest(
    string Name,
    Guid PrimaryMuscleGroupId,
    Guid? EquipmentId,
    string MetricType,
    string? VideoUrl,
    string? ImageUrl,
    string? Description,
    IReadOnlyList<Guid>? SecondaryMuscleGroupIds);
