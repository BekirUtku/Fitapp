import 'package:uuid/uuid.dart';

const _uuid = Uuid();

/// Backend SetType ile birebir uyumlu.
enum SetType { normal, warmUp, dropSet, failure, amrap, restPause, cluster, tempo }

extension SetTypeX on SetType {
  String get label => switch (this) {
        SetType.normal => 'Normal',
        SetType.warmUp => 'Isınma',
        SetType.dropSet => 'Drop',
        SetType.failure => 'Failure',
        SetType.amrap => 'AMRAP',
        SetType.restPause => 'Rest-Pause',
        SetType.cluster => 'Cluster',
        SetType.tempo => 'Tempo',
      };

  String get apiValue => switch (this) {
        SetType.normal => 'Normal',
        SetType.warmUp => 'WarmUp',
        SetType.dropSet => 'DropSet',
        SetType.failure => 'Failure',
        SetType.amrap => 'Amrap',
        SetType.restPause => 'RestPause',
        SetType.cluster => 'Cluster',
        SetType.tempo => 'Tempo',
      };

  /// Set türüne göre dinamik dinlenme önerisi (saniye).
  int get suggestedRestSeconds => switch (this) {
        SetType.warmUp => 45,
        SetType.failure => 180,
        SetType.amrap => 150,
        SetType.dropSet => 120,
        SetType.restPause => 120,
        SetType.cluster => 120,
        SetType.normal => 90,
        SetType.tempo => 90,
      };
}

class WorkoutSetModel {
  WorkoutSetModel({
    String? id,
    required this.setNumber,
    this.setType = SetType.normal,
    this.weight,
    this.reps,
    this.rpe,
    this.note,
    this.isCompleted = false,
  }) : id = id ?? _uuid.v4();

  final String id;
  int setNumber;
  SetType setType;
  double? weight;
  int? reps;
  int? rpe;
  String? note;
  bool isCompleted;

  double get volume => (weight ?? 0) * (reps ?? 0);

  WorkoutSetModel copyPlaceholder(int newNumber) => WorkoutSetModel(
        setNumber: newNumber,
        setType: setType,
        weight: weight,
        reps: reps,
      );
}

class SessionExerciseModel {
  SessionExerciseModel({
    String? id,
    required this.exerciseId,
    required this.exerciseName,
    required this.muscleGroup,
    this.supersetGroupId,
    List<WorkoutSetModel>? sets,
  })  : id = id ?? _uuid.v4(),
        sets = sets ?? [];

  final String id;
  final String exerciseId;
  final String exerciseName;
  final String muscleGroup;
  String? supersetGroupId;
  final List<WorkoutSetModel> sets;

  bool get allCompleted => sets.isNotEmpty && sets.every((s) => s.isCompleted);
}

class WorkoutSessionModel {
  WorkoutSessionModel({
    String? id,
    required this.name,
    required this.startedAt,
    List<SessionExerciseModel>? exercises,
  })  : id = id ?? _uuid.v4(),
        exercises = exercises ?? [];

  final String id;
  String name;
  final DateTime startedAt;
  final List<SessionExerciseModel> exercises;

  int get totalSets =>
      exercises.expand((e) => e.sets).where((s) => s.isCompleted).length;

  int get totalReps => exercises
      .expand((e) => e.sets)
      .where((s) => s.isCompleted)
      .fold(0, (sum, s) => sum + (s.reps ?? 0));

  double get totalVolume => exercises
      .expand((e) => e.sets)
      .where((s) => s.isCompleted && s.setType != SetType.warmUp)
      .fold(0.0, (sum, s) => sum + s.volume);

  Duration get elapsed => DateTime.now().difference(startedAt);
}
