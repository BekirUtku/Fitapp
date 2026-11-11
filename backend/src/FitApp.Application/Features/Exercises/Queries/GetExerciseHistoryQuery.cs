using FitApp.Application.Common.Exceptions;
using FitApp.Application.Common.Interfaces;
using FitApp.Application.Features.PersonalRecords;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Exercises.Queries;

public record GetExerciseHistoryQuery(Guid ExerciseId) : IRequest<ExerciseHistoryDto>;

public class GetExerciseHistoryQueryHandler : IRequestHandler<GetExerciseHistoryQuery, ExerciseHistoryDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetExerciseHistoryQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ExerciseHistoryDto> Handle(GetExerciseHistoryQuery request, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();

        var exercise = await _db.Exercises
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ExerciseId
                && (x.OwnerUserId == null || x.OwnerUserId == userId), ct)
            ?? throw NotFoundException.For("Hareket", request.ExerciseId);

        var sessionExercises = await _db.SessionExercises
            .AsNoTracking()
            .Include(se => se.Sets)
            .Include(se => se.Session)
            .Where(se => se.ExerciseId == request.ExerciseId
                && se.Session!.UserId == userId
                && !se.Session.IsDeleted)
            .ToListAsync(ct);

        var entries = sessionExercises
            .Select(se =>
            {
                var completed = se.Sets.Where(s => s.IsCompleted && !s.IsDeleted).ToList();
                var top = completed
                    .Where(s => s.Weight is > 0m && s.Reps is > 0)
                    .Select(s => PersonalRecordService.Estimated1Rm(s.Weight!.Value, s.Reps!.Value))
                    .DefaultIfEmpty(0m).Max();
                var volume = completed.Sum(s => (s.Weight ?? 0m) * (s.Reps ?? 0));

                return new ExerciseHistoryEntryDto(
                    se.Session!.Id,
                    se.Session.StartedAt,
                    volume,
                    top,
                    completed
                        .OrderBy(s => s.SetNumber)
                        .Select(s => new HistorySetDto(s.SetNumber, s.SetType.ToString(), s.Weight, s.Reps))
                        .ToList());
            })
            .Where(e => e.Sets.Count > 0)
            .OrderByDescending(e => e.Date)
            .ToList();

        var records = await _db.PersonalRecords
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.ExerciseId == request.ExerciseId)
            .OrderByDescending(p => p.AchievedAt)
            .Select(p => new PersonalRecordDto(
                p.Id, p.ExerciseId, exercise.Name, p.RecordType.ToString(),
                p.Value, p.Weight, p.Reps, p.AchievedAt))
            .ToListAsync(ct);

        return new ExerciseHistoryDto(exercise.Id, exercise.Name, entries, records);
    }
}
