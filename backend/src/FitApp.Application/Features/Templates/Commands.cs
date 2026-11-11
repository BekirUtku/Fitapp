using FitApp.Application.Common.Exceptions;
using FitApp.Application.Common.Interfaces;
using FitApp.Domain.Entities;
using FitApp.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Templates;

// ---------- Ortak doğrulama ----------

public class SaveTemplateRequestValidator : AbstractValidator<SaveTemplateRequest>
{
    public SaveTemplateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(160);
        RuleForEach(x => x.Exercises).ChildRules(ex =>
        {
            ex.RuleFor(e => e.ExerciseId).NotEmpty();
            ex.RuleFor(e => e.TargetSets).GreaterThan(0).LessThanOrEqualTo(20);
            ex.RuleFor(e => e.RestSeconds).GreaterThanOrEqualTo(0).LessThanOrEqualTo(600);
            ex.RuleFor(e => e.LoadType).Must(v => Enum.TryParse<LoadType>(v, out _))
                .WithMessage("Geçersiz loadType.");
            ex.RuleFor(e => e.ProgressionRule).Must(v => Enum.TryParse<ProgressionRule>(v, out _))
                .WithMessage("Geçersiz progressionRule.");
        });
    }
}

// ---------- Ortak entity kurucu ----------

internal static class TemplateExerciseFactory
{
    public static TemplateExercise Build(TemplateExerciseInput input)
    {
        var te = new TemplateExercise
        {
            ExerciseId = input.ExerciseId,
            OrderIndex = input.OrderIndex,
            TargetSets = input.TargetSets,
            TargetReps = input.TargetReps,
            LoadType = Enum.Parse<LoadType>(input.LoadType),
            TargetWeight = input.TargetWeight,
            Percentage = input.Percentage,
            RestSeconds = input.RestSeconds,
            Note = input.Note,
            SupersetGroupId = input.SupersetGroupId,
            ProgressionRule = Enum.Parse<ProgressionRule>(input.ProgressionRule)
        };

        foreach (var s in input.Sets ?? Array.Empty<TemplateSetInput>())
            te.Sets.Add(new TemplateSet
            {
                SetNumber = s.SetNumber,
                SetType = Enum.Parse<SetType>(s.SetType),
                TargetWeight = s.TargetWeight,
                TargetReps = s.TargetReps,
                RestSeconds = s.RestSeconds
            });

        return te;
    }
}

// ---------- Create ----------

public record CreateTemplateCommand(SaveTemplateRequest Request) : IRequest<Guid>;

public class CreateTemplateCommandValidator : AbstractValidator<CreateTemplateCommand>
{
    public CreateTemplateCommandValidator()
        => RuleFor(x => x.Request).SetValidator(new SaveTemplateRequestValidator());
}

public class CreateTemplateCommandHandler : IRequestHandler<CreateTemplateCommand, Guid>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateTemplateCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateTemplateCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var req = command.Request;

        await EnsureExercisesVisible(_db, userId, req.Exercises.Select(e => e.ExerciseId), ct);

        var template = new WorkoutTemplate
        {
            UserId = userId,
            Name = req.Name.Trim(),
            Notes = req.Notes
        };

        foreach (var input in req.Exercises)
            template.Exercises.Add(TemplateExerciseFactory.Build(input));

        _db.WorkoutTemplates.Add(template);
        await _db.SaveChangesAsync(ct);

        return template.Id;
    }

    internal static async Task EnsureExercisesVisible(
        IAppDbContext db, Guid userId, IEnumerable<Guid> exerciseIds, CancellationToken ct)
    {
        var ids = exerciseIds.Distinct().ToList();
        if (ids.Count == 0) return;

        var visibleCount = await db.Exercises
            .Where(x => ids.Contains(x.Id) && (x.OwnerUserId == null || x.OwnerUserId == userId))
            .CountAsync(ct);

        if (visibleCount != ids.Count)
            throw new NotFoundException("Şablondaki hareketlerden biri bulunamadı veya erişilebilir değil.");
    }
}

// ---------- Update (tam değiştirme) ----------

public record UpdateTemplateCommand(Guid Id, SaveTemplateRequest Request) : IRequest<Unit>;

public class UpdateTemplateCommandValidator : AbstractValidator<UpdateTemplateCommand>
{
    public UpdateTemplateCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request).SetValidator(new SaveTemplateRequestValidator());
    }
}

public class UpdateTemplateCommandHandler : IRequestHandler<UpdateTemplateCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpdateTemplateCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateTemplateCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();

        var template = await _db.WorkoutTemplates
            .Include(x => x.Exercises).ThenInclude(e => e.Sets)
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.UserId == userId, ct)
            ?? throw NotFoundException.For("Şablon", command.Id);

        var req = command.Request;
        await CreateTemplateCommandHandler.EnsureExercisesVisible(
            _db, userId, req.Exercises.Select(e => e.ExerciseId), ct);

        template.Name = req.Name.Trim();
        template.Notes = req.Notes;

        // Çocukları tamamen değiştir (cascade delete + yeniden ekleme).
        _db.TemplateSets.RemoveRange(template.Exercises.SelectMany(e => e.Sets));
        _db.TemplateExercises.RemoveRange(template.Exercises);
        template.Exercises.Clear();

        foreach (var input in req.Exercises)
            template.Exercises.Add(TemplateExerciseFactory.Build(input));

        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

// ---------- Delete ----------

public record DeleteTemplateCommand(Guid Id) : IRequest<Unit>;

public class DeleteTemplateCommandHandler : IRequestHandler<DeleteTemplateCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public DeleteTemplateCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteTemplateCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();

        var template = await _db.WorkoutTemplates
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.UserId == userId, ct)
            ?? throw NotFoundException.For("Şablon", command.Id);

        template.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
