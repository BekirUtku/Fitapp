using FitApp.Application.Common.Interfaces;
using FitApp.Domain.Entities;
using FitApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Sync.Handlers;

public record ExerciseSyncPayload(
    string Name,
    Guid PrimaryMuscleGroupId,
    Guid? EquipmentId,
    string MetricType,
    string? VideoUrl,
    string? ImageUrl,
    string? Description,
    IReadOnlyList<Guid>? SecondaryMuscleGroupIds);

/// <summary>Yalnızca kullanıcının kendi özel hareketleri senkronlanır. Sistem hareketleri sync dışıdır.</summary>
public class ExerciseSyncHandler : ISyncAggregateHandler
{
    private readonly IAppDbContext _db;

    public ExerciseSyncHandler(IAppDbContext db) => _db = db;

    public string EntityType => "exercise";
    public int Order => 1;

    public async Task<SyncOutcome> UpsertAsync(Guid userId, SyncChange change, CancellationToken ct)
    {
        var existing = await _db.Exercises
            .IgnoreQueryFilters()
            .Include(x => x.SecondaryMuscles)
            .FirstOrDefaultAsync(x => x.Id == change.EntityId, ct);

        if (existing is not null && existing.OwnerUserId != userId)
            return SyncOutcome.Error; // başka kullanıcının / sistem hareketi

        if (existing is not null && existing.UpdatedAt >= change.UpdatedAt)
            return SyncOutcome.SkippedLww;

        var p = SyncJson.Deserialize<ExerciseSyncPayload>(change.Payload);

        var entity = existing ?? new Exercise { Id = change.EntityId, IsCustom = true, OwnerUserId = userId };

        entity.Name = p.Name;
        entity.PrimaryMuscleGroupId = p.PrimaryMuscleGroupId;
        entity.EquipmentId = p.EquipmentId;
        entity.MetricType = Enum.Parse<MetricType>(p.MetricType);
        entity.VideoUrl = p.VideoUrl;
        entity.ImageUrl = p.ImageUrl;
        entity.Description = p.Description;
        entity.IsDeleted = false;
        entity.UpdatedAt = change.UpdatedAt;

        entity.SecondaryMuscles.Clear();
        foreach (var mg in p.SecondaryMuscleGroupIds ?? Array.Empty<Guid>())
            entity.SecondaryMuscles.Add(new ExerciseSecondaryMuscle { ExerciseId = entity.Id, MuscleGroupId = mg });

        if (existing is null)
        {
            entity.CreatedAt = change.UpdatedAt;
            _db.Exercises.Add(entity);
        }

        return SyncOutcome.Applied;
    }

    public async Task<SyncOutcome> DeleteAsync(Guid userId, SyncChange change, CancellationToken ct)
    {
        var entity = await _db.Exercises
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == change.EntityId, ct);

        if (entity is null) return SyncOutcome.NotFound;
        if (entity.OwnerUserId != userId) return SyncOutcome.Error;
        if (entity.UpdatedAt >= change.UpdatedAt) return SyncOutcome.SkippedLww;

        entity.IsDeleted = true;
        entity.UpdatedAt = change.UpdatedAt;
        return SyncOutcome.Applied;
    }

    public async Task<IReadOnlyList<SyncChange>> PullAsync(Guid userId, DateTime since, CancellationToken ct)
    {
        var rows = await _db.Exercises
            .IgnoreQueryFilters()
            .Include(x => x.SecondaryMuscles)
            .Where(x => x.OwnerUserId == userId && x.UpdatedAt > since)
            .ToListAsync(ct);

        return rows.Select(x => x.IsDeleted
            ? new SyncChange(EntityType, x.Id, SyncOperation.Delete, x.UpdatedAt, null)
            : new SyncChange(EntityType, x.Id, SyncOperation.Upsert, x.UpdatedAt,
                SyncJson.ToElement(new ExerciseSyncPayload(
                    x.Name, x.PrimaryMuscleGroupId, x.EquipmentId, x.MetricType.ToString(),
                    x.VideoUrl, x.ImageUrl, x.Description,
                    x.SecondaryMuscles.Select(m => m.MuscleGroupId).ToList()))))
            .ToList();
    }
}
