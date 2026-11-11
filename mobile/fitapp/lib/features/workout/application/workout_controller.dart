import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../domain/workout_models.dart';
import 'rest_timer_controller.dart';

/// Aktif antrenman oturumunu yöneten controller.
///
/// Faz 3'te state bellek içindedir; Faz 4'te Hive (source of truth) + sync
/// kuyruğu ile kalıcı hale getirilecektir.
class WorkoutController extends StateNotifier<WorkoutSessionModel?> {
  WorkoutController(this._ref) : super(null);

  final Ref _ref;

  void startManual({String name = 'Antrenman'}) {
    state = WorkoutSessionModel(name: name, startedAt: DateTime.now());
  }

  void startWithExercises(String name, List<SessionExerciseModel> exercises) {
    state = WorkoutSessionModel(name: name, startedAt: DateTime.now(), exercises: exercises);
  }

  void addExercise({required String exerciseId, required String name, required String muscleGroup}) {
    final s = state;
    if (s == null) return;
    s.exercises.add(SessionExerciseModel(
      exerciseId: exerciseId,
      exerciseName: name,
      muscleGroup: muscleGroup,
      sets: [WorkoutSetModel(setNumber: 1)],
    ));
    _emit();
  }

  void addSet(String exerciseId) {
    final ex = _find(exerciseId);
    if (ex == null) return;
    final next = ex.sets.isEmpty ? 1 : ex.sets.last.setNumber + 1;
    ex.sets.add(ex.sets.isEmpty
        ? WorkoutSetModel(setNumber: next)
        : ex.sets.last.copyPlaceholder(next));
    _emit();
  }

  void updateSet(String exerciseId, String setId,
      {double? weight, int? reps, int? rpe, SetType? setType, String? note}) {
    final set = _findSet(exerciseId, setId);
    if (set == null) return;
    if (weight != null) set.weight = weight;
    if (reps != null) set.reps = reps;
    if (rpe != null) set.rpe = rpe;
    if (setType != null) set.setType = setType;
    if (note != null) set.note = note;
    _emit();
  }

  /// Tek set ✓ → dinlenme sayacı başlar. Undo → false.
  void toggleComplete(String exerciseId, String setId) {
    final set = _findSet(exerciseId, setId);
    if (set == null) return;
    set.isCompleted = !set.isCompleted;
    if (set.isCompleted) {
      _ref.read(restTimerProvider.notifier).start(set.setType.suggestedRestSeconds);
    }
    _emit();
  }

  /// Tümünü Onayla → sayaç BAŞLAMAZ (spec).
  void completeAll(String exerciseId) {
    final ex = _find(exerciseId);
    if (ex == null) return;
    for (final s in ex.sets) {
      s.isCompleted = true;
    }
    _emit();
  }

  void removeSet(String exerciseId, String setId) {
    final ex = _find(exerciseId);
    if (ex == null) return;
    ex.sets.removeWhere((s) => s.id == setId);
    for (var i = 0; i < ex.sets.length; i++) {
      ex.sets[i].setNumber = i + 1;
    }
    _emit();
  }

  void finish() {
    _ref.read(restTimerProvider.notifier).stop();
    state = null;
  }

  void discard() {
    _ref.read(restTimerProvider.notifier).stop();
    state = null;
  }

  SessionExerciseModel? _find(String exerciseId) {
    final s = state;
    if (s == null) return null;
    for (final e in s.exercises) {
      if (e.id == exerciseId) return e;
    }
    return null;
  }

  WorkoutSetModel? _findSet(String exerciseId, String setId) {
    final ex = _find(exerciseId);
    if (ex == null) return null;
    for (final s in ex.sets) {
      if (s.id == setId) return s;
    }
    return null;
  }

  void _emit() {
    final s = state;
    if (s == null) return;
    // Aynı referansı yeni bir sarmalayıcı ile yeniden yayınlıyoruz (rebuild tetikler).
    state = WorkoutSessionModel(
      id: s.id,
      name: s.name,
      startedAt: s.startedAt,
      exercises: s.exercises,
    );
  }
}

final workoutProvider =
    StateNotifierProvider<WorkoutController, WorkoutSessionModel?>((ref) => WorkoutController(ref));
