using FitApp.Application.Common.Exceptions;
using FitApp.Application.Common.Interfaces;
using FitApp.Domain.Entities;
using FitApp.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Exercises.Commands;

public record UpdateExerciseCommand(Guid Id, UpdateExerciseRequest Request) : IRequest<Unit>;

public class UpdateExerciseCommandValidator : AbstractValidator<UpdateExerciseCommand>
{
    public UpdateExerciseCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Request.PrimaryMuscleGroupId).NotEmpty();
        RuleFor(x => x.Request.MetricType)
            .Must(v => Enum.TryParse<MetricType>(v, out _))
            .WithMessage("Geçersiz metricType.");
    }
}

public class UpdateExerciseCommandHandler : IRequestHandler<UpdateExerciseCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpdateExerciseCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateExerciseCommand command, CancellationToken ct)
    {
        var exercise = await _db.Exercises
            .Include(x => x.SecondaryMuscles)
            .FirstOrDefaultAsync(x => x.Id == command.Id, ct)
            ?? throw NotFoundException.For("Hareket", command.Id);

        EnsureCanModify(exercise);

        var req = command.Request;
        exercise.Name = req.Name.Trim();
        exercise.PrimaryMuscleGroupId = req.PrimaryMuscleGroupId;
        exercise.EquipmentId = req.EquipmentId;
        exercise.MetricType = Enum.Parse<MetricType>(req.MetricType);
        exercise.VideoUrl = req.VideoUrl;
        exercise.ImageUrl = req.ImageUrl;
        exercise.Description = req.Description;

        exercise.SecondaryMuscles.Clear();
        foreach (var mgId in req.SecondaryMuscleGroupIds ?? Array.Empty<Guid>())
            exercise.SecondaryMuscles.Add(new ExerciseSecondaryMuscle
            {
                ExerciseId = exercise.Id,
                MuscleGroupId = mgId
            });

        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }

    private void EnsureCanModify(Exercise exercise)
    {
        var userId = _currentUser.RequireUserId();

        if (exercise.OwnerUserId is null)
        {
            if (!_currentUser.IsAdmin)
                throw new ForbiddenException("Sistem hareketi yalnızca Admin tarafından düzenlenebilir.");
        }
        else if (exercise.OwnerUserId != userId)
        {
            throw new ForbiddenException("Yalnızca kendi hareketinizi düzenleyebilirsiniz.");
        }
    }
}
