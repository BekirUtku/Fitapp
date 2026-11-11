using AutoMapper;
using AutoMapper.QueryableExtensions;
using FitApp.Application.Common.Exceptions;
using FitApp.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.WeeklyPlans;

public record GetWeeklyPlansQuery : IRequest<IReadOnlyList<WeeklyPlanListItemDto>>;

public class GetWeeklyPlansQueryHandler
    : IRequestHandler<GetWeeklyPlansQuery, IReadOnlyList<WeeklyPlanListItemDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public GetWeeklyPlansQueryHandler(IAppDbContext db, ICurrentUser currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<WeeklyPlanListItemDto>> Handle(GetWeeklyPlansQuery request, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        return await _db.WeeklyPlans
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsActive).ThenBy(x => x.Name)
            .ProjectTo<WeeklyPlanListItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}

public record GetWeeklyPlanByIdQuery(Guid Id) : IRequest<WeeklyPlanDetailDto>;

public class GetWeeklyPlanByIdQueryHandler : IRequestHandler<GetWeeklyPlanByIdQuery, WeeklyPlanDetailDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public GetWeeklyPlanByIdQueryHandler(IAppDbContext db, ICurrentUser currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<WeeklyPlanDetailDto> Handle(GetWeeklyPlanByIdQuery request, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();

        var dto = await _db.WeeklyPlans
            .AsNoTracking()
            .Where(x => x.Id == request.Id && x.UserId == userId)
            .ProjectTo<WeeklyPlanDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);

        return dto ?? throw NotFoundException.For("Haftalık plan", request.Id);
    }
}
