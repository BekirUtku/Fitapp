using FitApp.Application.Common.Exceptions;
using FitApp.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Exercises.Commands;

public record DeleteExerciseCommand(Guid Id) : IRequest<Unit>;

public class DeleteExerciseCommandHandler : IRequestHandler<DeleteExerciseCommand, Unit>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public DeleteExerciseCommandHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteExerciseCommand command, CancellationToken ct)
    {
        var exercise = await _db.Exercises.FirstOrDefaultAsync(x => x.Id == command.Id, ct)
            ?? throw NotFoundException.For("Hareket", command.Id);

        var userId = _currentUser.RequireUserId();

        if (exercise.OwnerUserId is null)
        {
            if (!_currentUser.IsAdmin)
                throw new ForbiddenException("Sistem hareketi yalnızca Admin tarafından silinebilir.");
        }
        else if (exercise.OwnerUserId != userId)
        {
            throw new ForbiddenException("Yalnızca kendi hareketinizi silebilirsiniz.");
        }

        // Tombstone (soft delete) — sync ile tutarlı.
        exercise.IsDeleted = true;
        await _db.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
