using AutoMapper;
using AutoMapper.QueryableExtensions;
using FitApp.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Catalog;

public record GetMuscleGroupsQuery : IRequest<IReadOnlyList<MuscleGroupDto>>;

public class GetMuscleGroupsQueryHandler
    : IRequestHandler<GetMuscleGroupsQuery, IReadOnlyList<MuscleGroupDto>>
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public GetMuscleGroupsQueryHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<MuscleGroupDto>> Handle(GetMuscleGroupsQuery request, CancellationToken ct)
        => await _db.MuscleGroups
            .OrderBy(x => x.Name)
            .ProjectTo<MuscleGroupDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
}

public record GetEquipmentQuery : IRequest<IReadOnlyList<EquipmentDto>>;

public class GetEquipmentQueryHandler
    : IRequestHandler<GetEquipmentQuery, IReadOnlyList<EquipmentDto>>
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public GetEquipmentQueryHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<EquipmentDto>> Handle(GetEquipmentQuery request, CancellationToken ct)
        => await _db.Equipment
            .OrderBy(x => x.Name)
            .ProjectTo<EquipmentDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
}
