using FitApp.Application.Common.Exceptions;
using FitApp.Application.Common.Interfaces;
using FitApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Features.Sessions;

/// <summary>Oturum alt kayıtlarına sahiplik doğrulamalı erişim yardımcıları.</summary>
internal static class SessionAccess
{
    public static async Task<WorkoutSession> LoadOwnedSessionAsync(
        IAppDbContext db, Guid userId, Guid sessionId, bool withGraph, CancellationToken ct)
    {
        var query = db.WorkoutSessions.AsQueryable();
        if (withGraph)
            query = query.Include(s => s.Exercises).ThenInclude(e => e.Sets);

        var session = await query.FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId, ct)
            ?? throw NotFoundException.For("Antrenman", sessionId);

        return session;
    }

    public static async Task<SessionExercise> LoadOwnedExerciseAsync(
        IAppDbContext db, Guid userId, Guid sessionExerciseId, CancellationToken ct)
    {
        var se = await db.SessionExercises
            .Include(x => x.Sets)
            .Include(x => x.Session)
            .FirstOrDefaultAsync(x => x.Id == sessionExerciseId, ct)
            ?? throw NotFoundException.For("Antrenman hareketi", sessionExerciseId);

        if (se.Session is null || se.Session.UserId != userId)
            throw new ForbiddenException();

        return se;
    }

    public static async Task<WorkoutSet> LoadOwnedSetAsync(
        IAppDbContext db, Guid userId, Guid setId, CancellationToken ct)
    {
        var set = await db.WorkoutSets
            .Include(x => x.SessionExercise!).ThenInclude(e => e.Session)
            .FirstOrDefaultAsync(x => x.Id == setId, ct)
            ?? throw NotFoundException.For("Set", setId);

        if (set.SessionExercise?.Session is null || set.SessionExercise.Session.UserId != userId)
            throw new ForbiddenException();

        return set;
    }
}
