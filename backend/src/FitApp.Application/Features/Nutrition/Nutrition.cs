using FitApp.Application.Common.Interfaces;
using FitApp.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Nutrition;

public record NutritionLogDto(DateOnly Date, int Calories, int ProteinG, int CarbsG, int FatG, int WaterMl);

public record NutritionTargetDto(int Calories, int ProteinG, int CarbsG, int FatG, int WaterMl);

public record NutritionRemainingDto(int Calories, int ProteinG, int CarbsG, int FatG, int WaterMl);

public record NutritionSummaryDto(
    DateOnly Date, NutritionLogDto Log, NutritionTargetDto? Target, NutritionRemainingDto? Remaining);

public record SaveLogRequest(DateOnly Date, int Calories, int ProteinG, int CarbsG, int FatG, int WaterMl);

public record AddWaterRequest(DateOnly Date, int Ml);

public record SaveTargetRequest(int Calories, int ProteinG, int CarbsG, int FatG, int WaterMl);

internal static class NutritionMapper
{
    public static NutritionLogDto ToDto(NutritionLog? log, DateOnly date) => log is null
        ? new NutritionLogDto(date, 0, 0, 0, 0, 0)
        : new NutritionLogDto(log.LogDate, log.Calories, log.ProteinG, log.CarbsG, log.FatG, log.WaterMl);

    public static NutritionTargetDto? ToDto(NutritionTarget? t) => t is null
        ? null
        : new NutritionTargetDto(t.Calories, t.ProteinG, t.CarbsG, t.FatG, t.WaterMl);
}

// ---------- Queries ----------

public record GetNutritionSummaryQuery(DateOnly Date) : IRequest<NutritionSummaryDto>;

public class GetNutritionSummaryQueryHandler : IRequestHandler<GetNutritionSummaryQuery, NutritionSummaryDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetNutritionSummaryQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<NutritionSummaryDto> Handle(GetNutritionSummaryQuery request, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();

        var log = await _db.NutritionLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.LogDate == request.Date, ct);

        var target = await _db.NutritionTargets
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefaultAsync(ct);

        var logDto = NutritionMapper.ToDto(log, request.Date);
        var targetDto = NutritionMapper.ToDto(target);

        NutritionRemainingDto? remaining = targetDto is null
            ? null
            : new NutritionRemainingDto(
                targetDto.Calories - logDto.Calories,
                targetDto.ProteinG - logDto.ProteinG,
                targetDto.CarbsG - logDto.CarbsG,
                targetDto.FatG - logDto.FatG,
                targetDto.WaterMl - logDto.WaterMl);

        return new NutritionSummaryDto(request.Date, logDto, targetDto, remaining);
    }
}

public record GetCurrentTargetQuery : IRequest<NutritionTargetDto?>;

public class GetCurrentTargetQueryHandler : IRequestHandler<GetCurrentTargetQuery, NutritionTargetDto?>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetCurrentTargetQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<NutritionTargetDto?> Handle(GetCurrentTargetQuery request, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var target = await _db.NutritionTargets
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefaultAsync(ct);
        return NutritionMapper.ToDto(target);
    }
}

// ---------- Commands ----------

public record UpsertNutritionLogCommand(SaveLogRequest Request) : IRequest<Unit>;

public class UpsertNutritionLogCommandValidator : AbstractValidator<UpsertNutritionLogCommand>
{
    public UpsertNutritionLogCommandValidator()
    {
        RuleFor(x => x.Request.Calories).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.ProteinG).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.WaterMl).GreaterThanOrEqualTo(0);
    }
}

public class UpsertNutritionLogCommandHandler : IRequestHandler<UpsertNutritionLogCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpsertNutritionLogCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpsertNutritionLogCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var r = command.Request;

        var log = await _db.NutritionLogs
            .FirstOrDefaultAsync(x => x.UserId == userId && x.LogDate == r.Date, ct);

        if (log is null)
        {
            log = new NutritionLog { UserId = userId, LogDate = r.Date };
            _db.NutritionLogs.Add(log);
        }

        log.Calories = r.Calories;
        log.ProteinG = r.ProteinG;
        log.CarbsG = r.CarbsG;
        log.FatG = r.FatG;
        log.WaterMl = r.WaterMl;

        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public record AddWaterCommand(AddWaterRequest Request) : IRequest<int>;

public class AddWaterCommandHandler : IRequestHandler<AddWaterCommand, int>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public AddWaterCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(AddWaterCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var r = command.Request;

        var log = await _db.NutritionLogs
            .FirstOrDefaultAsync(x => x.UserId == userId && x.LogDate == r.Date, ct);

        if (log is null)
        {
            log = new NutritionLog { UserId = userId, LogDate = r.Date };
            _db.NutritionLogs.Add(log);
        }

        log.WaterMl = Math.Max(0, log.WaterMl + r.Ml);
        await _db.SaveChangesAsync(ct);
        return log.WaterMl;
    }
}

public record SetTargetCommand(SaveTargetRequest Request) : IRequest<Unit>;

public class SetTargetCommandHandler : IRequestHandler<SetTargetCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SetTargetCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(SetTargetCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var r = command.Request;

        _db.NutritionTargets.Add(new NutritionTarget
        {
            UserId = userId,
            Calories = r.Calories,
            ProteinG = r.ProteinG,
            CarbsG = r.CarbsG,
            FatG = r.FatG,
            WaterMl = r.WaterMl,
            EffectiveFrom = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
