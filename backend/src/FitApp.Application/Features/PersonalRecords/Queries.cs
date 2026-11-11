using FitApp.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.PersonalRecords;

/// <summary>Her hareket+tür için güncel en iyi rekor.</summary>
public record GetPersonalRecordsQuery : IRequest<IReadOnlyList<PersonalRecordDto>>;

public class GetPersonalRecordsQueryHandler
    : IRequestHandler<GetPersonalRecordsQuery, IReadOnlyList<PersonalRecordDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetPersonalRecordsQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<PersonalRecordDto>> Handle(GetPersonalRecordsQuery request, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();

        var rows = await _db.PersonalRecords
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => new
            {
                p.Id, p.ExerciseId, ExerciseName = p.Exercise!.Name,
                p.RecordType, p.Value, p.Weight, p.Reps, p.AchievedAt
            })
            .ToListAsync(ct);

        // Hareket + tür başına en yüksek değeri seç.
        return rows
            .GroupBy(r => new { r.ExerciseId, r.RecordType })
            .Select(g => g.OrderByDescending(x => x.Value).First())
            .OrderBy(r => r.ExerciseName).ThenBy(r => r.RecordType)
            .Select(r => new PersonalRecordDto(
                r.Id, r.ExerciseId, r.ExerciseName, r.RecordType.ToString(),
                r.Value, r.Weight, r.Reps, r.AchievedAt))
            .ToList();
    }
}

/// <summary>Bir harekette kırılan tüm PR'ların tarihçesi.</summary>
public record GetExercisePrHistoryQuery(Guid ExerciseId) : IRequest<IReadOnlyList<PersonalRecordDto>>;

public class GetExercisePrHistoryQueryHandler
    : IRequestHandler<GetExercisePrHistoryQuery, IReadOnlyList<PersonalRecordDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetExercisePrHistoryQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<PersonalRecordDto>> Handle(GetExercisePrHistoryQuery request, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();

        return await _db.PersonalRecords
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.ExerciseId == request.ExerciseId)
            .OrderByDescending(p => p.AchievedAt)
            .Select(p => new PersonalRecordDto(
                p.Id, p.ExerciseId, p.Exercise!.Name, p.RecordType.ToString(),
                p.Value, p.Weight, p.Reps, p.AchievedAt))
            .ToListAsync(ct);
    }
}
