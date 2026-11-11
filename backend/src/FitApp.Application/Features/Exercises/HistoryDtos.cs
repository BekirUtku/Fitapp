using FitApp.Application.Features.PersonalRecords;

namespace FitApp.Application.Features.Exercises;

public record HistorySetDto(int SetNumber, string SetType, decimal? Weight, int? Reps);

public record ExerciseHistoryEntryDto(
    Guid SessionId,
    DateTime Date,
    decimal Volume,
    decimal TopEstimated1Rm,
    IReadOnlyList<HistorySetDto> Sets);

public record ExerciseHistoryDto(
    Guid ExerciseId,
    string ExerciseName,
    IReadOnlyList<ExerciseHistoryEntryDto> Entries,
    IReadOnlyList<PersonalRecordDto> Records);
