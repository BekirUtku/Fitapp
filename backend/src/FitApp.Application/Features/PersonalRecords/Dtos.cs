namespace FitApp.Application.Features.PersonalRecords;

public record PersonalRecordDto(
    Guid Id,
    Guid ExerciseId,
    string ExerciseName,
    string RecordType,
    decimal Value,
    decimal? Weight,
    int? Reps,
    DateTime AchievedAt);
