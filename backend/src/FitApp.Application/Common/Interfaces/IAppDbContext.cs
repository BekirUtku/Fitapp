using FitApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitApp.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<MuscleGroup> MuscleGroups { get; }
    DbSet<Equipment> Equipment { get; }
    DbSet<Exercise> Exercises { get; }
    DbSet<ExerciseSecondaryMuscle> ExerciseSecondaryMuscles { get; }
    DbSet<WorkoutTemplate> WorkoutTemplates { get; }
    DbSet<TemplateExercise> TemplateExercises { get; }
    DbSet<TemplateSet> TemplateSets { get; }
    DbSet<WeeklyPlan> WeeklyPlans { get; }
    DbSet<WeeklyPlanDay> WeeklyPlanDays { get; }
    DbSet<WorkoutSession> WorkoutSessions { get; }
    DbSet<SessionExercise> SessionExercises { get; }
    DbSet<WorkoutSet> WorkoutSets { get; }
    DbSet<PersonalRecord> PersonalRecords { get; }
    DbSet<BodyMeasurement> BodyMeasurements { get; }
    DbSet<NutritionLog> NutritionLogs { get; }
    DbSet<NutritionTarget> NutritionTargets { get; }
    DbSet<Badge> Badges { get; }
    DbSet<UserBadge> UserBadges { get; }

    /// <summary>false iken SaveChanges CreatedAt/UpdatedAt'i otomatik güncellemez (sync'te client zamanını korumak için).</summary>
    bool AutoTimestamps { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
