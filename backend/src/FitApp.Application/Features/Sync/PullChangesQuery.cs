using FitApp.Application.Common.Interfaces;
using MediatR;

namespace FitApp.Application.Features.Sync;

public record PullChangesQuery(DateTime? Since) : IRequest<PullResponse>;

public class PullChangesQueryHandler : IRequestHandler<PullChangesQuery, PullResponse>
{
    private readonly ICurrentUser _currentUser;
    private readonly IEnumerable<ISyncAggregateHandler> _handlers;

    public PullChangesQueryHandler(ICurrentUser currentUser, IEnumerable<ISyncAggregateHandler> handlers)
    {
        _currentUser = currentUser;
        _handlers = handlers;
    }

    public async Task<PullResponse> Handle(PullChangesQuery request, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var since = request.Since ?? DateTime.MinValue;

        var all = new List<SyncChange>();
        foreach (var handler in _handlers.OrderBy(h => h.Order))
            all.AddRange(await handler.PullAsync(userId, since, ct));

        var ordered = all.OrderBy(c => c.UpdatedAt).ToList();
        return new PullResponse(ordered, DateTime.UtcNow);
    }
}
