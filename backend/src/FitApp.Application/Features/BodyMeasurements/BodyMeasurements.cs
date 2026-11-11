using FitApp.Application.Common.Exceptions;
using FitApp.Application.Common.Interfaces;
using FitApp.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.BodyMeasurements;

public record BodyMeasurementDto(
    Guid Id,
    DateTime MeasuredAt,
    decimal? Bodyweight,
    IReadOnlyDictionary<string, decimal> Measurements,
    string? Note);

public record SaveBodyMeasurementRequest(
    DateTime? MeasuredAt,
    decimal? Bodyweight,
    Dictionary<string, decimal>? Measurements,
    string? Note);

// ---------- Query ----------

public record GetBodyMeasurementsQuery : IRequest<IReadOnlyList<BodyMeasurementDto>>;

public class GetBodyMeasurementsQueryHandler
    : IRequestHandler<GetBodyMeasurementsQuery, IReadOnlyList<BodyMeasurementDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetBodyMeasurementsQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<BodyMeasurementDto>> Handle(GetBodyMeasurementsQuery request, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var rows = await _db.BodyMeasurements
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.MeasuredAt)
            .ToListAsync(ct);

        return rows
            .Select(x => new BodyMeasurementDto(x.Id, x.MeasuredAt, x.Bodyweight, x.Measurements, x.Note))
            .ToList();
    }
}

// ---------- Create ----------

public record CreateBodyMeasurementCommand(SaveBodyMeasurementRequest Request) : IRequest<Guid>;

public class CreateBodyMeasurementCommandHandler : IRequestHandler<CreateBodyMeasurementCommand, Guid>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateBodyMeasurementCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateBodyMeasurementCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var r = command.Request;

        var entity = new BodyMeasurement
        {
            UserId = userId,
            MeasuredAt = r.MeasuredAt ?? DateTime.UtcNow,
            Bodyweight = r.Bodyweight,
            Measurements = r.Measurements ?? new(),
            Note = r.Note
        };

        _db.BodyMeasurements.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }
}

// ---------- Update ----------

public record UpdateBodyMeasurementCommand(Guid Id, SaveBodyMeasurementRequest Request) : IRequest<Unit>;

public class UpdateBodyMeasurementCommandHandler : IRequestHandler<UpdateBodyMeasurementCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpdateBodyMeasurementCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateBodyMeasurementCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var entity = await _db.BodyMeasurements
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.UserId == userId, ct)
            ?? throw NotFoundException.For("Ölçüm", command.Id);

        var r = command.Request;
        if (r.MeasuredAt is { } m) entity.MeasuredAt = m;
        entity.Bodyweight = r.Bodyweight;
        if (r.Measurements is not null) entity.Measurements = r.Measurements;
        entity.Note = r.Note;

        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

// ---------- Delete ----------

public record DeleteBodyMeasurementCommand(Guid Id) : IRequest<Unit>;

public class DeleteBodyMeasurementCommandHandler : IRequestHandler<DeleteBodyMeasurementCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public DeleteBodyMeasurementCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteBodyMeasurementCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var entity = await _db.BodyMeasurements
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.UserId == userId, ct)
            ?? throw NotFoundException.For("Ölçüm", command.Id);

        entity.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
