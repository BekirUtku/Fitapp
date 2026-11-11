using FitApp.Application.Common.Interfaces;
using MediatR;

namespace FitApp.Application.Features.Sync;

public record PushChangesCommand(PushRequest Request) : IRequest<PushResponse>;

public class PushChangesCommandHandler : IRequestHandler<PushChangesCommand, PushResponse>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IReadOnlyDictionary<string, ISyncAggregateHandler> _handlers;

    public PushChangesCommandHandler(
        IAppDbContext db, ICurrentUser currentUser, IEnumerable<ISyncAggregateHandler> handlers)
    {
        _db = db;
        _currentUser = currentUser;
        _handlers = handlers.ToDictionary(h => h.EntityType, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<PushResponse> Handle(PushChangesCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var results = new List<SyncResult>();

        // Client zaman damgasını koru (LWW gerçek düzenleme zamanına göre çalışsın).
        _db.AutoTimestamps = false;

        // Upsert'ler bağımlılık sırasında, delete'ler tersine.
        var upserts = command.Request.Changes
            .Where(c => c.Operation == SyncOperation.Upsert)
            .OrderBy(c => HandlerOrder(c.EntityType));

        var deletes = command.Request.Changes
            .Where(c => c.Operation == SyncOperation.Delete)
            .OrderByDescending(c => HandlerOrder(c.EntityType));

        foreach (var change in upserts.Concat(deletes))
        {
            if (!_handlers.TryGetValue(change.EntityType, out var handler))
            {
                results.Add(new SyncResult(change.EntityId, change.EntityType, "Error", "Bilinmeyen tür."));
                continue;
            }

            try
            {
                var outcome = change.Operation == SyncOperation.Upsert
                    ? await handler.UpsertAsync(userId, change, ct)
                    : await handler.DeleteAsync(userId, change, ct);

                results.Add(new SyncResult(change.EntityId, change.EntityType, outcome.ToString()));
            }
            catch (Exception ex)
            {
                results.Add(new SyncResult(change.EntityId, change.EntityType, "Error", ex.Message));
            }
        }

        await _db.SaveChangesAsync(ct);
        _db.AutoTimestamps = true;

        return new PushResponse(results, DateTime.UtcNow);
    }

    private int HandlerOrder(string type) =>
        _handlers.TryGetValue(type, out var h) ? h.Order : int.MaxValue;
}
