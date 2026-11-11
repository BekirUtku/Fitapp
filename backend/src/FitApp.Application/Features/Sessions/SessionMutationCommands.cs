using AutoMapper;
using AutoMapper.QueryableExtensions;
using FitApp.Application.Common.Exceptions;
using FitApp.Application.Common.Interfaces;
using FitApp.Domain.Entities;
using FitApp.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Sessions;

// ---------- Plansız hareket ekle ----------

public record AddSessionExerciseCommand(Guid SessionId, AddSessionExerciseRequest Request)
    : IRequest<SessionExerciseDto>;

public class AddSessionExerciseCommandHandler
    : IRequestHandler<AddSessionExerciseCommand, SessionExerciseDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public AddSessionExerciseCommandHandler(IAppDbContext db, ICurrentUser currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<SessionExerciseDto> Handle(AddSessionExerciseCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var session = await SessionAccess.LoadOwnedSessionAsync(_db, userId, command.SessionId, withGraph: true, ct);

        var exercise = await _db.Exercises
            .FirstOrDefaultAsync(x => x.Id == command.Request.ExerciseId
                && (x.OwnerUserId == null || x.OwnerUserId == userId), ct)
            ?? throw NotFoundException.For("Hareket", command.Request.ExerciseId);

        var nextOrder = session.Exercises.Count == 0 ? 0 : session.Exercises.Max(e => e.OrderIndex) + 1;

        var se = new SessionExercise
        {
            SessionId = session.Id,
            ExerciseId = exercise.Id,
            OrderIndex = nextOrder,
            SupersetGroupId = command.Request.SupersetGroupId
        };

        _db.SessionExercises.Add(se);
        await _db.SaveChangesAsync(ct);

        // Ad/metricType için hareketi bağla (map projeksiyonu yerine elle döndürüyoruz).
        return new SessionExerciseDto(
            se.Id, exercise.Id, exercise.Name, exercise.MetricType.ToString(),
            se.OrderIndex, se.SupersetGroupId, se.Note, Array.Empty<WorkoutSetDto>());
    }
}

// ---------- Set ekle ----------

public record AddSetCommand(Guid SessionExerciseId, AddSetRequest Request) : IRequest<WorkoutSetDto>;

public class AddSetCommandValidator : AbstractValidator<AddSetCommand>
{
    public AddSetCommandValidator()
    {
        RuleFor(x => x.Request.SetType).Must(v => Enum.TryParse<SetType>(v, out _))
            .WithMessage("Geçersiz setType.");
        RuleFor(x => x.Request.Rpe).InclusiveBetween(1, 10).When(x => x.Request.Rpe.HasValue);
    }
}

public class AddSetCommandHandler : IRequestHandler<AddSetCommand, WorkoutSetDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public AddSetCommandHandler(IAppDbContext db, ICurrentUser currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<WorkoutSetDto> Handle(AddSetCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var se = await SessionAccess.LoadOwnedExerciseAsync(_db, userId, command.SessionExerciseId, ct);

        var nextNumber = se.Sets.Count == 0 ? 1 : se.Sets.Max(s => s.SetNumber) + 1;
        var r = command.Request;

        var set = new WorkoutSet
        {
            SessionExerciseId = se.Id,
            SetNumber = nextNumber,
            SetType = Enum.Parse<SetType>(r.SetType),
            Weight = r.Weight,
            Reps = r.Reps,
            DurationSeconds = r.DurationSeconds,
            DistanceMeters = r.DistanceMeters,
            Rpe = r.Rpe,
            Note = r.Note
        };

        _db.WorkoutSets.Add(set);
        await _db.SaveChangesAsync(ct);

        return _mapper.Map<WorkoutSetDto>(set);
    }
}

// ---------- Set güncelle ----------

public record UpdateSetCommand(Guid SetId, UpdateSetRequest Request) : IRequest<WorkoutSetDto>;

public class UpdateSetCommandValidator : AbstractValidator<UpdateSetCommand>
{
    public UpdateSetCommandValidator()
    {
        RuleFor(x => x.Request.SetType).Must(v => Enum.TryParse<SetType>(v, out _))
            .WithMessage("Geçersiz setType.");
        RuleFor(x => x.Request.Rpe).InclusiveBetween(1, 10).When(x => x.Request.Rpe.HasValue);
    }
}

public class UpdateSetCommandHandler : IRequestHandler<UpdateSetCommand, WorkoutSetDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public UpdateSetCommandHandler(IAppDbContext db, ICurrentUser currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<WorkoutSetDto> Handle(UpdateSetCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var set = await SessionAccess.LoadOwnedSetAsync(_db, userId, command.SetId, ct);

        var r = command.Request;
        set.SetType = Enum.Parse<SetType>(r.SetType);
        set.Weight = r.Weight;
        set.Reps = r.Reps;
        set.DurationSeconds = r.DurationSeconds;
        set.DistanceMeters = r.DistanceMeters;
        set.Rpe = r.Rpe;
        set.Note = r.Note;

        await _db.SaveChangesAsync(ct);
        return _mapper.Map<WorkoutSetDto>(set);
    }
}

// ---------- Set onayla / geri al (undo) ----------

public record SetCompletionCommand(Guid SetId, bool Completed) : IRequest<WorkoutSetDto>;

public class SetCompletionCommandHandler : IRequestHandler<SetCompletionCommand, WorkoutSetDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public SetCompletionCommandHandler(IAppDbContext db, ICurrentUser currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<WorkoutSetDto> Handle(SetCompletionCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var set = await SessionAccess.LoadOwnedSetAsync(_db, userId, command.SetId, ct);

        set.IsCompleted = command.Completed;
        set.CompletedAt = command.Completed ? DateTime.UtcNow : null;

        await _db.SaveChangesAsync(ct);
        return _mapper.Map<WorkoutSetDto>(set);
    }
}

// ---------- Set sil (tombstone) ----------

public record DeleteSetCommand(Guid SetId) : IRequest<Unit>;

public class DeleteSetCommandHandler : IRequestHandler<DeleteSetCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public DeleteSetCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteSetCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var set = await SessionAccess.LoadOwnedSetAsync(_db, userId, command.SetId, ct);

        set.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

// ---------- Antrenmanı bitir ----------

public record FinishSessionCommand(Guid SessionId, string? Note) : IRequest<SessionDetailDto>;

public class FinishSessionCommandHandler : IRequestHandler<FinishSessionCommand, SessionDetailDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;
    private readonly Gamification.BadgeService _badges;

    public FinishSessionCommandHandler(
        IAppDbContext db, ICurrentUser currentUser, IMapper mapper, Gamification.BadgeService badges)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
        _badges = badges;
    }

    public async Task<SessionDetailDto> Handle(FinishSessionCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var session = await SessionAccess.LoadOwnedSessionAsync(_db, userId, command.SessionId, withGraph: true, ct);

        session.Status = SessionStatus.Completed;
        session.EndedAt = DateTime.UtcNow;
        session.Note = command.Note;

        SessionCalculator.ApplyTotals(session);
        await PersonalRecords.PersonalRecordService.DetectAsync(_db, userId, session, ct);
        await _db.SaveChangesAsync(ct);

        // Rozet/seri sayımının bu antrenmanı da kapsaması için kayıttan sonra değerlendir.
        await _badges.EvaluateAsync(userId, ct);
        await _db.SaveChangesAsync(ct);

        return await _db.WorkoutSessions
            .AsNoTracking()
            .Where(x => x.Id == session.Id)
            .ProjectTo<SessionDetailDto>(_mapper.ConfigurationProvider)
            .FirstAsync(ct);
    }
}

// ---------- Antrenmanı sil ----------

public record DeleteSessionCommand(Guid SessionId) : IRequest<Unit>;

public class DeleteSessionCommandHandler : IRequestHandler<DeleteSessionCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public DeleteSessionCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteSessionCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var session = await SessionAccess.LoadOwnedSessionAsync(_db, userId, command.SessionId, withGraph: false, ct);

        session.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
