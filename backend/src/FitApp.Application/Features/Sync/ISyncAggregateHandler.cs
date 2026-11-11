namespace FitApp.Application.Features.Sync;

/// <summary>Bir aggregate türü için sync uygula/çek mantığı.</summary>
public interface ISyncAggregateHandler
{
    string EntityType { get; }

    /// <summary>Push sırasındaki bağımlılık sırası (küçük önce gider; delete'lerde tersine işlenir).</summary>
    int Order { get; }

    Task<SyncOutcome> UpsertAsync(Guid userId, SyncChange change, CancellationToken ct);

    Task<SyncOutcome> DeleteAsync(Guid userId, SyncChange change, CancellationToken ct);

    Task<IReadOnlyList<SyncChange>> PullAsync(Guid userId, DateTime since, CancellationToken ct);
}
