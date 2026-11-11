class TemplateListItem {
  const TemplateListItem({required this.id, required this.name, required this.exerciseCount});
  final String id;
  final String name;
  final int exerciseCount;

  factory TemplateListItem.fromJson(Map<String, dynamic> j) => TemplateListItem(
        id: j['id'] as String,
        name: j['name'] as String,
        exerciseCount: (j['exerciseCount'] as num?)?.toInt() ?? 0,
      );
}

class TemplateSetPlan {
  const TemplateSetPlan({required this.setNumber, this.weight, this.reps});
  final int setNumber;
  final double? weight;
  final int? reps;

  factory TemplateSetPlan.fromJson(Map<String, dynamic> j) => TemplateSetPlan(
        setNumber: (j['setNumber'] as num).toInt(),
        weight: (j['targetWeight'] as num?)?.toDouble(),
        reps: (j['targetReps'] as num?)?.toInt(),
      );
}

class TemplateExercisePlan {
  const TemplateExercisePlan({
    required this.exerciseId,
    required this.exerciseName,
    required this.targetSets,
    this.targetReps,
    this.targetWeight,
    required this.sets,
  });

  final String exerciseId;
  final String exerciseName;
  final int targetSets;
  final int? targetReps;
  final double? targetWeight;
  final List<TemplateSetPlan> sets;

  factory TemplateExercisePlan.fromJson(Map<String, dynamic> j) => TemplateExercisePlan(
        exerciseId: j['exerciseId'] as String,
        exerciseName: j['exerciseName'] as String? ?? '',
        targetSets: (j['targetSets'] as num?)?.toInt() ?? 1,
        targetReps: (j['targetReps'] as num?)?.toInt(),
        targetWeight: (j['targetWeight'] as num?)?.toDouble(),
        sets: (j['sets'] as List? ?? [])
            .map((e) => TemplateSetPlan.fromJson(Map<String, dynamic>.from(e as Map)))
            .toList(),
      );
}

class TemplateDetail {
  const TemplateDetail({required this.id, required this.name, required this.exercises});
  final String id;
  final String name;
  final List<TemplateExercisePlan> exercises;

  factory TemplateDetail.fromJson(Map<String, dynamic> j) => TemplateDetail(
        id: j['id'] as String,
        name: j['name'] as String,
        exercises: (j['exercises'] as List? ?? [])
            .map((e) => TemplateExercisePlan.fromJson(Map<String, dynamic>.from(e as Map)))
            .toList(),
      );
}
