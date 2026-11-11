using AutoMapper;
using AutoMapper.QueryableExtensions;
using FitApp.Application.Common.Interfaces;
using FitApp.Application.Common.Models;
using FitApp.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Exercises.Queries;

public record GetExercisesQuery(
    Guid? MuscleGroupId = null,
    Guid? EquipmentId = null,
    string? Search = null,
    bool? CustomOnly = null,
    int Page = 1,
    int PageSize = 30) : IRequest<PagedResult<ExerciseListItemDto>>;

public class GetExercisesQueryHandler
    : IRequestHandler<GetExercisesQuery, PagedResult<ExerciseListItemDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public GetExercisesQueryHandler(IAppDbContext db, ICurrentUser currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<PagedResult<ExerciseListItemDto>> Handle(GetExercisesQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId;

        IQueryable<Exercise> query = _db.Exercises
            .AsNoTracking()
            // Görünürlük: sistem hareketleri + kullanıcının kendi özel hareketleri.
            .Where(x => x.OwnerUserId == null || x.OwnerUserId == userId);

        if (request.MuscleGroupId is { } mg)
            query = query.Where(x => x.PrimaryMuscleGroupId == mg
                || x.SecondaryMuscles.Any(s => s.MuscleGroupId == mg));

        if (request.EquipmentId is { } eq)
            query = query.Where(x => x.EquipmentId == eq);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(term));
        }

        if (request.CustomOnly == true)
            query = query.Where(x => x.OwnerUserId == userId);

        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 100);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * size)
            .Take(size)
            .ProjectTo<ExerciseListItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedResult<ExerciseListItemDto>(items, page, size, total);
    }
}
