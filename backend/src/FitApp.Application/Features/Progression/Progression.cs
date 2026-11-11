using FitApp.Application.Common.Exceptions;
using FitApp.Application.Common.Interfaces;
using FitApp.Application.Engines;
using FitApp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Progression;

public record ProgressionSuggestionDto(
    Guid ExerciseId, string Action, decimal? SuggestedWeight, int? SuggestedReps, string Rationale);

public record WarmupSetDto(int SetNumber, decimal Weight, int Reps, int Percent);

// ---------- Bir sonraki set önerisi ----------

public record GetProgressionSuggestionQuery(Guid ExerciseId) : IRequest<ProgressionSuggestionDto>;

public class GetProgressionSuggestionQueryHandler
    : IRequestHandler<GetProgressionSuggestionQuery, ProgressionSuggestionDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetProgressionSuggestionQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ProgressionSuggestionDto> Handle(GetProgressionSuggestionQuery request, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();

        var exercise = await _db.Exercises.AsNoTracking()
            .Include(x => x.PrimaryMuscleGroup)
            .FirstOrDefaultAsync(x => x.Id == request.ExerciseId
                && (x.OwnerUserId == null || x.OwnerUserId == userId), ct)
            ?? throw NotFoundException.For("Hareket", request.ExerciseId);

        var increment = exercise.PrimaryMuscleGroup?.BodyRegion == BodyRegion.Lower ? 5m : 2.5m;

        var lastExercise = await _db.SessionExercises.AsNoTracking()
            .Include(se => se.Sets)
            .Include(se => se.Session)
            .Where(se => se.ExerciseId == request.ExerciseId
                && se.Session!.UserId == userId
                && se.Session.Status == SessionStatus.Completed)
            .OrderByDescending(se => se.Session!.StartedAt)
            .FirstOrDefaultAsync(ct);

        var working = lastExercise?.Sets
            .Where(s => s.IsCompleted && !s.IsDeleted && s.SetType != SetType.WarmUp
                && s.Weight is > 0m && s.Reps is > 0)
            .Select(s => new SetPerformance(s.Weight!.Value, s.Reps!.Value))
            .ToList() ?? new List<SetPerformance>();

        // Hedef tekrar aralığı: bu hareketi içeren en güncel şablondan.
        var targetReps = await _db.TemplateExercises.AsNoTracking()
            .Where(te => te.ExerciseId == request.ExerciseId && te.Template!.UserId == userId)
            .OrderByDescending(te => te.Template!.UpdatedAt)
            .Select(te => (int?)te.TargetReps)
            .FirstOrDefaultAsync(ct);

        var maxReps = targetReps is > 0 ? targetReps.Value : 12;
        var minReps = Math.Max(1, maxReps - 4);

        var s = ProgressionEngine.SuggestNext(working, minReps, maxReps, increment);
        return new ProgressionSuggestionDto(
            request.ExerciseId, s.Action.ToString(), s.SuggestedWeight, s.SuggestedReps, s.Rationale);
    }
}

// ---------- Otomatik ısınma ----------

public record GetWarmupQuery(decimal WorkingWeight, Guid? ExerciseId) : IRequest<IReadOnlyList<WarmupSetDto>>;

public class GetWarmupQueryHandler : IRequestHandler<GetWarmupQuery, IReadOnlyList<WarmupSetDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetWarmupQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<WarmupSetDto>> Handle(GetWarmupQuery request, CancellationToken ct)
    {
        var increment = 2.5m;
        if (request.ExerciseId is { } exId)
        {
            var region = await _db.Exercises.AsNoTracking()
                .Where(x => x.Id == exId)
                .Select(x => x.PrimaryMuscleGroup!.BodyRegion)
                .FirstOrDefaultAsync(ct);
            if (region == BodyRegion.Lower) increment = 5m;
        }

        return ProgressionEngine.BuildWarmup(request.WorkingWeight, increment)
            .Select(w => new WarmupSetDto(w.SetNumber, w.Weight, w.Reps, w.Percent))
            .ToList();
    }
}
