class StreakDay {
  const StreakDay(this.date, this.count);
  final DateTime date;
  final int count;

  factory StreakDay.fromJson(Map<String, dynamic> j) =>
      StreakDay(DateTime.parse(j['date'] as String), (j['count'] as num).toInt());
}

class StreakData {
  const StreakData({required this.current, required this.longest, required this.days});
  final int current;
  final int longest;
  final List<StreakDay> days;

  factory StreakData.fromJson(Map<String, dynamic> j) => StreakData(
        current: (j['currentStreak'] as num).toInt(),
        longest: (j['longestStreak'] as num).toInt(),
        days: (j['days'] as List)
            .map((e) => StreakDay.fromJson(Map<String, dynamic>.from(e as Map)))
            .toList(),
      );
}

class MuscleHeat {
  const MuscleHeat({required this.name, required this.sets, required this.volume, required this.intensity});
  final String name;
  final int sets;
  final double volume;
  final double intensity;

  factory MuscleHeat.fromJson(Map<String, dynamic> j) => MuscleHeat(
        name: j['muscleGroupName'] as String,
        sets: (j['sets'] as num).toInt(),
        volume: (j['volume'] as num).toDouble(),
        intensity: (j['intensity'] as num).toDouble(),
      );
}

class GBadge {
  const GBadge({required this.code, required this.name, required this.description, required this.earned});
  final String code;
  final String name;
  final String description;
  final bool earned;

  factory GBadge.fromJson(Map<String, dynamic> j) => GBadge(
        code: j['code'] as String,
        name: j['name'] as String,
        description: j['description'] as String? ?? '',
        earned: j['earned'] as bool? ?? false,
      );
}
