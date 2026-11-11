using AutoMapper;
using AutoMapper.QueryableExtensions;
using FitApp.Application.Common.Exceptions;
using FitApp.Application.Common.Interfaces;
using FitApp.Domain.Entities;
using FitApp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Sessions;

public record StartSessionCommand(StartSessionRequest Request) : IRequest<SessionDetailDto>;

public class StartSessionCommandHandler : IRequestHandler<StartSessionCommand, SessionDetailDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public StartSessionCommandHandler(IAppDbContext db, ICurrentUser currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<SessionDetailDto> Handle(StartSessionCommand command, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();
        var req = command.Request;

        var session = new WorkoutSession
        {
            UserId = userId,
            TemplateId = req.TemplateId,
            Status = SessionStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            Name = req.Name?.Trim() ?? "Antrenman"
        };

        if (req.TemplateId is { } templateId)
        {
            var template = await _db.WorkoutTemplates
                .AsNoTracking()
                .Include(t => t.Exercises).ThenInclude(e => e.Sets)
                .FirstOrDefaultAsync(t => t.Id == templateId && t.UserId == userId, ct)
                ?? throw NotFoundException.For("Şablon", templateId);

            if (string.IsNullOrWhiteSpace(req.Name))
                session.Name = template.Name;

            foreach (var te in template.Exercises.OrderBy(e => e.OrderIndex))
            {
                var se = new SessionExercise
                {
                    ExerciseId = te.ExerciseId,
                    OrderIndex = te.OrderIndex,
                    SupersetGroupId = te.SupersetGroupId,
                    Note = te.Note
                };

                if (te.Sets.Count > 0)
                {
                    foreach (var ts in te.Sets.OrderBy(s => s.SetNumber))
                        se.Sets.Add(new WorkoutSet
                        {
                            SetNumber = ts.SetNumber,
                            SetType = ts.SetType,
                            Weight = ts.TargetWeight,
                            Reps = ts.TargetReps
                        });
                }
                else
                {
                    for (var i = 1; i <= te.TargetSets; i++)
                        se.Sets.Add(new WorkoutSet
                        {
                            SetNumber = i,
                            SetType = SetType.Normal,
                            Weight = te.TargetWeight,
                            Reps = te.TargetReps
                        });
                }

                session.Exercises.Add(se);
            }
        }

        _db.WorkoutSessions.Add(session);
        await _db.SaveChangesAsync(ct);

        return await _db.WorkoutSessions
            .AsNoTracking()
            .Where(x => x.Id == session.Id)
            .ProjectTo<SessionDetailDto>(_mapper.ConfigurationProvider)
            .FirstAsync(ct);
    }
}
