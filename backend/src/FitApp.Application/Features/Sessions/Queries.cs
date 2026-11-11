using AutoMapper;
using AutoMapper.QueryableExtensions;
using FitApp.Application.Common.Exceptions;
using FitApp.Application.Common.Interfaces;
using FitApp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Sessions;

public record GetSessionsQuery(DateTime? From = null, DateTime? To = null)
    : IRequest<IReadOnlyList<SessionListItemDto>>;

public class GetSessionsQueryHandler
    : IRequestHandler<GetSessionsQuery, IReadOnlyList<SessionListItemDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public GetSessionsQueryHandler(IAppDbContext db, ICurrentUser currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<SessionListItemDto>> Handle(GetSessionsQuery request, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();

        var query = _db.WorkoutSessions.AsNoTracking().Where(x => x.UserId == userId);
        if (request.From is { } from) query = query.Where(x => x.StartedAt >= from);
        if (request.To is { } to) query = query.Where(x => x.StartedAt <= to);

        return await query
            .OrderByDescending(x => x.StartedAt)
            .ProjectTo<SessionListItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}

public record GetSessionByIdQuery(Guid Id) : IRequest<SessionDetailDto>;

public class GetSessionByIdQueryHandler : IRequestHandler<GetSessionByIdQuery, SessionDetailDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public GetSessionByIdQueryHandler(IAppDbContext db, ICurrentUser currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<SessionDetailDto> Handle(GetSessionByIdQuery request, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();

        var dto = await _db.WorkoutSessions
            .AsNoTracking()
            .Where(x => x.Id == request.Id && x.UserId == userId)
            .ProjectTo<SessionDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);

        return dto ?? throw NotFoundException.For("Antrenman", request.Id);
    }
}

/// <summary>Smart Resume: tamamlanmamış (InProgress) son oturumu döner; yoksa null.</summary>
public record GetActiveSessionQuery : IRequest<SessionDetailDto?>;

public class GetActiveSessionQueryHandler : IRequestHandler<GetActiveSessionQuery, SessionDetailDto?>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public GetActiveSessionQueryHandler(IAppDbContext db, ICurrentUser currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<SessionDetailDto?> Handle(GetActiveSessionQuery request, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();

        return await _db.WorkoutSessions
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Status == SessionStatus.InProgress)
            .OrderByDescending(x => x.StartedAt)
            .ProjectTo<SessionDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }
}
