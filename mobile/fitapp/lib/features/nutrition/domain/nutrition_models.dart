class MacroSet {
  const MacroSet({
    this.calories = 0,
    this.proteinG = 0,
    this.carbsG = 0,
    this.fatG = 0,
    this.waterMl = 0,
  });

  final int calories;
  final int proteinG;
  final int carbsG;
  final int fatG;
  final int waterMl;

  factory MacroSet.fromJson(Map<String, dynamic> j) => MacroSet(
        calories: (j['calories'] as num?)?.toInt() ?? 0,
        proteinG: (j['proteinG'] as num?)?.toInt() ?? 0,
        carbsG: (j['carbsG'] as num?)?.toInt() ?? 0,
        fatG: (j['fatG'] as num?)?.toInt() ?? 0,
        waterMl: (j['waterMl'] as num?)?.toInt() ?? 0,
      );

  Map<String, dynamic> toJson() => {
        'calories': calories,
        'proteinG': proteinG,
        'carbsG': carbsG,
        'fatG': fatG,
        'waterMl': waterMl,
      };
}

class NutritionSummary {
  const NutritionSummary({required this.date, required this.log, this.target});

  final String date;
  final MacroSet log;
  final MacroSet? target;

  factory NutritionSummary.fromJson(Map<String, dynamic> j) => NutritionSummary(
        date: j['date'] as String,
        log: MacroSet.fromJson(Map<String, dynamic>.from(j['log'] as Map)),
        target: j['target'] == null
            ? null
            : MacroSet.fromJson(Map<String, dynamic>.from(j['target'] as Map)),
      );
}
