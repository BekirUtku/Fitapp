class ExerciseListItem {
  const ExerciseListItem({
    required this.id,
    required this.name,
    required this.muscleGroupId,
    required this.muscleGroupName,
    required this.isCustom,
  });

  final String id;
  final String name;
  final String muscleGroupId;
  final String muscleGroupName;
  final bool isCustom;

  factory ExerciseListItem.fromJson(Map<String, dynamic> j) => ExerciseListItem(
        id: j['id'] as String,
        name: j['name'] as String,
        muscleGroupId: j['primaryMuscleGroupId'] as String? ?? '',
        muscleGroupName: j['primaryMuscleGroupName'] as String? ?? '',
        isCustom: j['isCustom'] as bool? ?? false,
      );
}

class MuscleGroupItem {
  const MuscleGroupItem({required this.id, required this.name});
  final String id;
  final String name;

  factory MuscleGroupItem.fromJson(Map<String, dynamic> j) =>
      MuscleGroupItem(id: j['id'] as String, name: j['name'] as String);
}
