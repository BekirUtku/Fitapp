using FitApp.Application.Common.Interfaces;
using FitApp.Application.Features.Sessions;
using FitApp.Domain.Entities;
using FitApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Sync.Handlers;

public record SessionSetSyncPayload(
    Guid Id, int SetNumber, string SetType, decimal? Weight, int? Reps,
    int? DurationSeconds, decimal? DistanceMeters, int? Rpe, string? Note,
    bool IsCompleted, bool IsPr, DateTime? CompletedAt);

public record SessionExerciseSyncPayload(
    Guid Id, Guid ExerciseId, int OrderIndex, Guid? SupersetGroupId, string? Note,
    IReadOnlyList<SessionSetSyncPayload> Sets);

public record SessionSyncPayload(
    Guid? TemplateId, string Name, string Status, DateTime StartedAt, DateTime? EndedAt, string? Note,
    IReadOnlyList<SessionExerciseSyncPayload> Exercises);

/// <summary>Oturum aggregate'ı (hareketler + setler dahil) tek parça senkronlanır — FK sırası derdi olmaz.</summary>
public class SessionSyncHandler : ISyncAggregateHandler
{
    private readonly IAppDbContext _db;
    private readonly FitApp.Application.Features.Gamification.BadgeService _badges;

    public SessionSyncHandler(IAppDbContext db, FitApp.Application.Features.Gamification.BadgeService badges)
    {
        _db = db;
        _badges = badges;
    }

    public string EntityType => "session";
    public int Order => 4;

    public async Task<SyncOutcome> UpsertAsync(Guid userId, SyncChange change, CancellationToken ct)
    {
        var existing = await _db.WorkoutSessions
            .IgnoreQueryFilters()
            .Include(s => s.Exercises).ThenInclude(e => e.Sets)
            .FirstOrDefaultAsync(s => s.Id == change.EntityId, ct);

        if (existing is not null && existing.UserId != userId)
            return SyncOutcome.Error;

        if (existing is not null && existing.UpdatedAt >= change.UpdatedAt)
            return SyncOutcome.SkippedLww;

        var p = SyncJson.Deserialize<SessionSyncPayload>(change.Payload);
        var session = existing ?? new WorkoutSession { Id = change.EntityId, UserId = userId };

        session.TemplateId = p.TemplateId;
        session.Name = p.Name;
        session.Status = Enum.Parse<SessionStatus>(p.Status);
        session.StartedAt = p.StartedAt;
        session.EndedAt = p.EndedAt;
        session.Note = p.Note;
        session.IsDeleted = false;
        session.UpdatedAt = change.UpdatedAt;

        if (existing is not null)
        {
            _db.WorkoutSets.RemoveRange(existing.Exercises.SelectMany(e => e.Sets));
            _db.SessionExercises.RemoveRange(existing.Exercises);
            session.Exercises.Clear();
        }
        else
        {
            session.CreatedAt = change.UpdatedAt;
        }

        foreach (var ex in p.Exercises)
        {
            var se = new SessionExercise
            {
                Id = ex.Id,
                SessionId = session.Id,
                ExerciseId = ex.ExerciseId,
                OrderIndex = ex.OrderIndex,
                SupersetGroupId = ex.SupersetGroupId,
                Note = ex.Note
            };
            foreach (var s in ex.Sets)
                se.Sets.Add(new WorkoutSet
                {
                    Id = s.Id,
                    SessionExerciseId = se.Id,
                    SetNumber = s.SetNumber,
                    SetType = Enum.Parse<SetType>(s.SetType),
                    Weight = s.Weight,
                    Reps = s.Reps,
                    DurationSeconds = s.DurationSeconds,
                    DistanceMeters = s.DistanceMeters,
                    Rpe = s.Rpe,
                    Note = s.Note,
                    IsCompleted = s.IsCompleted,
                    IsPr = s.IsPr,
                    CompletedAt = s.CompletedAt
                });
            session.Exercises.Add(se);
        }

        if (existing is null)
            _db.WorkoutSessions.Add(session);

        if (session.Status == SessionStatus.Completed)
        {
            SessionCalculator.ApplyTotals(session);
            await PersonalRecords.PersonalRecordService.DetectAsync(_db, userId, session, ct);
            await _db.SaveChangesAsync(ct); // sayımların bu oturumu kapsaması için
            await _badges.EvaluateAsync(userId, ct);
        }

        return SyncOutcome.Applied;
    }

    public async Task<SyncOutcome> DeleteAsync(Guid userId, SyncChange change, CancellationToken ct)
    {
        var session = await _db.WorkoutSessions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == change.EntityId, ct);

        if (session is null) return SyncOutcome.NotFound;
        if (session.UserId != userId) return SyncOutcome.Error;
        if (session.UpdatedAt >= change.UpdatedAt) return SyncOutcome.SkippedLww;

        session.IsDeleted = true;
        session.UpdatedAt = change.UpdatedAt;
        return SyncOutcome.Applied;
    }

    public async Task<IReadOnlyList<SyncChange>> PullAsync(Guid userId, DateTime since, CancellationToken ct)
    {
        var rows = await _db.WorkoutSessions
            .IgnoreQueryFilters()
            .Include(s => s.Exercises).ThenInclude(e => e.Sets)
            .Where(s => s.UserId == userId && s.UpdatedAt > since)
            .ToListAsync(ct);

        return rows.Select(s => s.IsDeleted
            ? new SyncChange(EntityType, s.Id, SyncOperation.Delete, s.UpdatedAt, null)
            : new SyncChange(EntityType, s.Id, SyncOperation.Upsert, s.UpdatedAt,
                SyncJson.ToElement(new SessionSyncPayload(
                    s.TemplateId, s.Name, s.Status.ToString(), s.StartedAt, s.EndedAt, s.Note,
                    s.Exercises.OrderBy(e => e.OrderIndex).Select(e => new SessionExerciseSyncPayload(
                        e.Id, e.ExerciseId, e.OrderIndex, e.SupersetGroupId, e.Note,
                        e.Sets.OrderBy(x => x.SetNumber).Select(x => new SessionSetSyncPayload(
                            x.Id, x.SetNumber, x.SetType.ToString(), x.Weight, x.Reps,
                            x.DurationSeconds, x.DistanceMeters, x.Rpe, x.Note,
                            x.IsCompleted, x.IsPr, x.CompletedAt)).ToList())).ToList()))))
            .ToList();
    }
}
