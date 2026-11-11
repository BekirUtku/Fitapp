using AutoMapper;
using AutoMapper.QueryableExtensions;
using FitApp.Application.Common.Exceptions;
using FitApp.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Exercises.Queries;

public record GetExerciseByIdQuery(Guid Id) : IRequest<ExerciseDetailDto>;

public class GetExerciseByIdQueryHandler : IRequestHandler<GetExerciseByIdQuery, ExerciseDetailDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public GetExerciseByIdQueryHandler(IAppDbContext db, ICurrentUser currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<ExerciseDetailDto> Handle(GetExerciseByIdQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId;

        var dto = await _db.Exercises
            .AsNoTracking()
            .Where(x => x.Id == request.Id && (x.OwnerUserId == null || x.OwnerUserId == userId))
            .ProjectTo<ExerciseDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);

        return dto ?? throw NotFoundException.For("Hareket", request.Id);
    }
}
