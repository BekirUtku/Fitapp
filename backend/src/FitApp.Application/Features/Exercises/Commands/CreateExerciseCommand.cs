using FitApp.Application.Common.Exceptions;
using FitApp.Application.Common.Interfaces;
using FitApp.Domain.Entities;
using FitApp.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Exercises.Commands;

/// <param name="AsSystem">true ise sistem hareketi (Admin gerekir); false ise kullanıcı özel hareketi.</param>
public record CreateExerciseCommand(CreateExerciseRequest Request, bool AsSystem) : IRequest<Guid>;

public class CreateExerciseCommandValidator : AbstractValidator<CreateExerciseCommand>
{
    public CreateExerciseCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Request.PrimaryMuscleGroupId).NotEmpty();
        RuleFor(x => x.Request.MetricType)
            .Must(v => Enum.TryParse<MetricType>(v, out _))
            .WithMessage("Geçersiz metricType.");
    }
}

public class CreateExerciseCommandHandler : IRequestHandler<CreateExerciseCommand, Guid>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CreateExerciseCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateExerciseCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var req = command.Request;

        if (command.AsSystem && !_currentUser.IsAdmin)
            throw new ForbiddenException("Sistem hareketi yalnızca Admin tarafından eklenebilir.");

        var mgExists = await _db.MuscleGroups.AnyAsync(x => x.Id == req.PrimaryMuscleGroupId, ct);
        if (!mgExists)
            throw NotFoundException.For("Kas grubu", req.PrimaryMuscleGroupId);

        var exercise = new Exercise
        {
            Name = req.Name.Trim(),
            PrimaryMuscleGroupId = req.PrimaryMuscleGroupId,
            EquipmentId = req.EquipmentId,
            MetricType = Enum.Parse<MetricType>(req.MetricType),
            VideoUrl = req.VideoUrl,
            ImageUrl = req.ImageUrl,
            Description = req.Description,
            IsCustom = !command.AsSystem,
            OwnerUserId = command.AsSystem ? null : userId
        };

        foreach (var mgId in req.SecondaryMuscleGroupIds ?? Array.Empty<Guid>())
            exercise.SecondaryMuscles.Add(new ExerciseSecondaryMuscle { MuscleGroupId = mgId });

        _db.Exercises.Add(exercise);
        await _db.SaveChangesAsync(ct);

        return exercise.Id;
    }
}
