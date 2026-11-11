using AutoMapper;
using AutoMapper.QueryableExtensions;
using FitApp.Application.Common.Exceptions;
using FitApp.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Templates;

public record GetTemplatesQuery : IRequest<IReadOnlyList<TemplateListItemDto>>;

public class GetTemplatesQueryHandler
    : IRequestHandler<GetTemplatesQuery, IReadOnlyList<TemplateListItemDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public GetTemplatesQueryHandler(IAppDbContext db, ICurrentUser currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<TemplateListItemDto>> Handle(GetTemplatesQuery request, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        return await _db.WorkoutTemplates
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.UpdatedAt)
            .ProjectTo<TemplateListItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}

public record GetTemplateByIdQuery(Guid Id) : IRequest<TemplateDetailDto>;

public class GetTemplateByIdQueryHandler : IRequestHandler<GetTemplateByIdQuery, TemplateDetailDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public GetTemplateByIdQueryHandler(IAppDbContext db, ICurrentUser currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<TemplateDetailDto> Handle(GetTemplateByIdQuery request, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();

        var dto = await _db.WorkoutTemplates
            .AsNoTracking()
            .Where(x => x.Id == request.Id && x.UserId == userId)
            .ProjectTo<TemplateDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);

        return dto ?? throw NotFoundException.For("Şablon", request.Id);
    }
}
