using FitApp.Application.Common.Interfaces;
using FitApp.Application.Engines;
using MediatR;

namespace FitApp.Application.Features.Insights;

public record GetInsightsSummaryQuery : IRequest<IReadOnlyList<InsightDto>>;

public class GetInsightsSummaryQueryHandler
    : IRequestHandler<GetInsightsSummaryQuery, IReadOnlyList<InsightDto>>
{
    private readonly InsightService _insights;
    private readonly ICurrentUser _currentUser;

    public GetInsightsSummaryQueryHandler(InsightService insights, ICurrentUser currentUser)
    {
        _insights = insights;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyList<InsightDto>> Handle(GetInsightsSummaryQuery request, CancellationToken ct)
        => _insights.BuildSummaryAsync(_currentUser.RequireUserId(), ct);
}

public record GetSessionInsightsQuery(Guid SessionId) : IRequest<IReadOnlyList<InsightDto>>;

public class GetSessionInsightsQueryHandler
    : IRequestHandler<GetSessionInsightsQuery, IReadOnlyList<InsightDto>>
{
    private readonly InsightService _insights;
    private readonly ICurrentUser _currentUser;

    public GetSessionInsightsQueryHandler(InsightService insights, ICurrentUser currentUser)
    {
        _insights = insights;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyList<InsightDto>> Handle(GetSessionInsightsQuery request, CancellationToken ct)
        => _insights.BuildSessionAsync(_currentUser.RequireUserId(), request.SessionId, ct);
}
