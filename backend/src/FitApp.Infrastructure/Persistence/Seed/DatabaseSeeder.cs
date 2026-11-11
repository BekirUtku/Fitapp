using FitApp.Domain.Entities;
using FitApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FitApp.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // Geliştirici test aşaması: şemayı migration yerine model'den doğrudan kur.
        // (Store'a yaklaşınca MigrateAsync + migration'lara geçilecek.)
        await db.Database.EnsureCreatedAsync();

        await SeedAdminAsync(db, config);
        await SeedMuscleGroupsAsync(db);
        await SeedEquipmentAsync(db);
        await SeedBadgesAsync(db);
        await db.SaveChangesAsync();

        // Hareketler kas grubu/ekipman ID'lerine ihtiyaç duyar; onlar kaydedildikten sonra.
        await SeedExercisesAsync(db);
        await db.SaveChangesAsync();
    }

    private static async Task SeedAdminAsync(AppDbContext db, IConfiguration config)
    {
        var email = config["Seed:AdminEmail"] ?? "admin@fitapp.local";
        if (await db.Users.IgnoreQueryFilters().AnyAsync(x => x.Email == email))
            return;

        var password = config["Seed:AdminPassword"] ?? "ChangeMe_123!";
        db.Users.Add(new User
        {
            Email = email,
            DisplayName = "Admin",
            Role = UserRole.Admin,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        });
    }

    private static async Task SeedMuscleGroupsAsync(AppDbContext db)
    {
        if (await db.MuscleGroups.AnyAsync()) return;

        var groups = new (string Name, BodyRegion Region)[]
        {
            ("Göğüs", BodyRegion.Upper),
            ("Sırt", BodyRegion.Upper),
            ("Omuz", BodyRegion.Upper),
            ("Biceps", BodyRegion.Upper),
            ("Triceps", BodyRegion.Upper),
            ("Ön Kol", BodyRegion.Upper),
            ("Quadriceps", BodyRegion.Lower),
            ("Hamstring", BodyRegion.Lower),
            ("Kalça", BodyRegion.Lower),
            ("Baldır", BodyRegion.Lower),
            ("Karın", BodyRegion.Core),
            ("Bel / Core", BodyRegion.Core)
        };

        foreach (var (name, region) in groups)
            db.MuscleGroups.Add(new MuscleGroup { Name = name, BodyRegion = region });
    }

    private static async Task SeedEquipmentAsync(AppDbContext db)
    {
        if (await db.Equipment.AnyAsync()) return;

        var items = new[]
        {
            "Barbell", "Dumbbell", "Makine", "Kablo", "Vücut Ağırlığı",
            "Kettlebell", "Smith Machine", "Bant", "Plaka", "EZ Bar"
        };

        foreach (var name in items)
            db.Equipment.Add(new Equipment { Name = name });
    }

    private static async Task SeedBadgesAsync(AppDbContext db)
    {
        var defs = new (string Code, string Name, string Description)[]
        {
            ("first_workout", "İlk Adım", "İlk antrenmanını tamamladın."),
            ("streak_7", "Ateşli Hafta", "7 gün üst üste antrenman serisi."),
            ("first_pr", "Rekor Kırıcı", "İlk kişisel rekorunu kırdın."),
            ("workouts_50", "Azimli", "50 antrenman tamamladın."),
            ("volume_100k", "Ton Kaldıran", "Toplam 100.000 kg hacim topladın.")
        };

        foreach (var (code, name, desc) in defs)
        {
            if (!await db.Badges.AnyAsync(b => b.Code == code))
                db.Badges.Add(new Badge { Code = code, Name = name, Description = desc });
        }
    }

    private static async Task SeedExercisesAsync(AppDbContext db)
    {
        if (await db.Exercises.IgnoreQueryFilters().AnyAsync(x => x.OwnerUserId == null))
            return;

        var mg = await db.MuscleGroups.ToDictionaryAsync(x => x.Name, x => x.Id);
        var eq = await db.Equipment.ToDictionaryAsync(x => x.Name, x => x.Id);

        // (Hareket, Kas Grubu, Ekipman, Ölçüm Tipi)
        var defs = new (string Name, string Muscle, string? Equip, MetricType Metric)[]
        {
            ("Bench Press", "Göğüs", "Barbell", MetricType.WeightReps),
            ("Incline Dumbbell Press", "Göğüs", "Dumbbell", MetricType.WeightReps),
            ("Dumbbell Fly", "Göğüs", "Dumbbell", MetricType.WeightReps),
            ("Cable Crossover", "Göğüs", "Kablo", MetricType.WeightReps),
            ("Şınav", "Göğüs", "Vücut Ağırlığı", MetricType.BodyweightReps),
            ("Deadlift", "Sırt", "Barbell", MetricType.WeightReps),
            ("Barbell Row", "Sırt", "Barbell", MetricType.WeightReps),
            ("Lat Pulldown", "Sırt", "Kablo", MetricType.WeightReps),
            ("Seated Cable Row", "Sırt", "Kablo", MetricType.WeightReps),
            ("Barfiks", "Sırt", "Vücut Ağırlığı", MetricType.WeightedBodyweight),
            ("Overhead Press", "Omuz", "Barbell", MetricType.WeightReps),
            ("Dumbbell Shoulder Press", "Omuz", "Dumbbell", MetricType.WeightReps),
            ("Lateral Raise", "Omuz", "Dumbbell", MetricType.WeightReps),
            ("Face Pull", "Omuz", "Kablo", MetricType.WeightReps),
            ("Barbell Curl", "Biceps", "EZ Bar", MetricType.WeightReps),
            ("Dumbbell Curl", "Biceps", "Dumbbell", MetricType.WeightReps),
            ("Hammer Curl", "Biceps", "Dumbbell", MetricType.WeightReps),
            ("Tricep Pushdown", "Triceps", "Kablo", MetricType.WeightReps),
            ("Overhead Tricep Extension", "Triceps", "Dumbbell", MetricType.WeightReps),
            ("Dips", "Triceps", "Vücut Ağırlığı", MetricType.WeightedBodyweight),
            ("Squat", "Quadriceps", "Barbell", MetricType.WeightReps),
            ("Leg Press", "Quadriceps", "Makine", MetricType.WeightReps),
            ("Leg Extension", "Quadriceps", "Makine", MetricType.WeightReps),
            ("Lunge", "Quadriceps", "Dumbbell", MetricType.WeightReps),
            ("Romanian Deadlift", "Hamstring", "Barbell", MetricType.WeightReps),
            ("Leg Curl", "Hamstring", "Makine", MetricType.WeightReps),
            ("Hip Thrust", "Kalça", "Barbell", MetricType.WeightReps),
            ("Calf Raise", "Baldır", "Makine", MetricType.WeightReps),
            ("Plank", "Karın", "Vücut Ağırlığı", MetricType.Duration),
            ("Crunch", "Karın", "Vücut Ağırlığı", MetricType.BodyweightReps),
            ("Hanging Leg Raise", "Karın", "Vücut Ağırlığı", MetricType.BodyweightReps),
        };

        foreach (var (name, muscle, equip, metric) in defs)
        {
            if (!mg.TryGetValue(muscle, out var muscleId)) continue;
            Guid? equipId = equip is not null && eq.TryGetValue(equip, out var e) ? e : null;

            db.Exercises.Add(new Exercise
            {
                Name = name,
                PrimaryMuscleGroupId = muscleId,
                EquipmentId = equipId,
                MetricType = metric,
                IsCustom = false,
                OwnerUserId = null
            });
        }
    }
}
