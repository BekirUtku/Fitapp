using FitApp.Application.Common.Exceptions;
using FitApp.Application.Common.Interfaces;
using FitApp.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.WeeklyPlans;

// ---------- Create (7 boş günle) ----------

public record CreateWeeklyPlanCommand(CreateWeeklyPlanRequest Request) : IRequest<Guid>;

public class CreateWeeklyPlanCommandValidator : AbstractValidator<CreateWeeklyPlanCommand>
{
    public CreateWeeklyPlanCommandValidator()
        => RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(160);
}

public class CreateWeeklyPlanCommandHandler : IRequestHandler<CreateWeeklyPlanCommand, Guid>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateWeeklyPlanCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateWeeklyPlanCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var hasAny = await _db.WeeklyPlans.AnyAsync(x => x.UserId == userId, ct);

        var plan = new WeeklyPlan
        {
            UserId = userId,
            Name = command.Request.Name.Trim(),
            IsActive = !hasAny // ilk plan otomatik aktif
        };

        for (var day = 1; day <= 7; day++)
            plan.Days.Add(new WeeklyPlanDay { DayOfWeek = day, TemplateId = null });

        _db.WeeklyPlans.Add(plan);
        await _db.SaveChangesAsync(ct);
        return plan.Id;
    }
}

// ---------- Update (ad + aktiflik) ----------

public record UpdateWeeklyPlanCommand(Guid Id, UpdateWeeklyPlanRequest Request) : IRequest<Unit>;

public class UpdateWeeklyPlanCommandValidator : AbstractValidator<UpdateWeeklyPlanCommand>
{
    public UpdateWeeklyPlanCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(160);
    }
}

public class UpdateWeeklyPlanCommandHandler : IRequestHandler<UpdateWeeklyPlanCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpdateWeeklyPlanCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateWeeklyPlanCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();

        var plan = await _db.WeeklyPlans
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.UserId == userId, ct)
            ?? throw NotFoundException.For("Haftalık plan", command.Id);

        plan.Name = command.Request.Name.Trim();

        if (command.Request.IsActive && !plan.IsActive)
        {
            // Tek aktif plan garantisi: diğerlerini pasifleştir.
            var others = await _db.WeeklyPlans
                .Where(x => x.UserId == userId && x.IsActive && x.Id != plan.Id)
                .ToListAsync(ct);
            foreach (var o in others) o.IsActive = false;
        }

        plan.IsActive = command.Request.IsActive;

        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

// ---------- Gün → şablon atama ----------

public record AssignDayCommand(Guid PlanId, AssignDayRequest Request) : IRequest<Unit>;

public class AssignDayCommandValidator : AbstractValidator<AssignDayCommand>
{
    public AssignDayCommandValidator()
    {
        RuleFor(x => x.PlanId).NotEmpty();
        RuleFor(x => x.Request.DayOfWeek).InclusiveBetween(1, 7);
    }
}

public class AssignDayCommandHandler : IRequestHandler<AssignDayCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public AssignDayCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(AssignDayCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();

        var plan = await _db.WeeklyPlans
            .Include(x => x.Days)
            .FirstOrDefaultAsync(x => x.Id == command.PlanId && x.UserId == userId, ct)
            ?? throw NotFoundException.For("Haftalık plan", command.PlanId);

        var templateId = command.Request.TemplateId;
        if (templateId is { } tid)
        {
            var templateOk = await _db.WorkoutTemplates
                .AnyAsync(x => x.Id == tid && x.UserId == userId, ct);
            if (!templateOk)
                throw NotFoundException.For("Şablon", tid);
        }

        var day = plan.Days.FirstOrDefault(d => d.DayOfWeek == command.Request.DayOfWeek);
        if (day is null)
        {
            plan.Days.Add(new WeeklyPlanDay
            {
                DayOfWeek = command.Request.DayOfWeek,
                TemplateId = templateId
            });
        }
        else
        {
            day.TemplateId = templateId; // null = dinlenme günü
        }

        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

// ---------- Delete ----------

public record DeleteWeeklyPlanCommand(Guid Id) : IRequest<Unit>;

public class DeleteWeeklyPlanCommandHandler : IRequestHandler<DeleteWeeklyPlanCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public DeleteWeeklyPlanCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteWeeklyPlanCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();

        var plan = await _db.WeeklyPlans
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.UserId == userId, ct)
            ?? throw NotFoundException.For("Haftalık plan", command.Id);

        plan.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
