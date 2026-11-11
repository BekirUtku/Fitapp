import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/storage/local_store.dart';
import '../../../core/sync/outbox.dart';
import '../../../core/sync/sync_engine.dart';
import '../domain/workout_models.dart';

final sessionRepositoryProvider = Provider<SessionRepository>(
  (ref) => SessionRepository(LocalStore.instance, ref.read(outboxProvider)),
);

/// Oturumu Hive'a (source of truth) yazar ve outbox'a değişiklik ekler.
class SessionRepository {
  SessionRepository(this._store, this._outbox);

  final LocalStore _store;
  final Outbox _outbox;

  Future<void> saveFinished(WorkoutSessionModel session, {String? note}) async {
    final now = DateTime.now().toUtc();
    final payload = _toPayload(session, status: 'Completed', endedAt: now, note: note);

    await _store.write(LocalStore.sessionsBox, session.id, {
      'id': session.id,
      'updatedAt': now.toIso8601String(),
      'isSynced': false,
      ...payload,
    });

    _outbox.enqueueUpsert('session', session.id, payload, updatedAt: now);
  }

  Map<String, dynamic> _toPayload(
    WorkoutSessionModel session, {
    required String status,
    DateTime? endedAt,
    String? note,
  }) {
    return {
      'templateId': null,
      'name': session.name,
      'status': status,
      'startedAt': session.startedAt.toUtc().toIso8601String(),
      'endedAt': endedAt?.toIso8601String(),
      'note': note,
      'exercises': [
        for (var i = 0; i < session.exercises.length; i++)
          _exercisePayload(session.exercises[i], i),
      ],
    };
  }

  Map<String, dynamic> _exercisePayload(SessionExerciseModel e, int order) => {
        'id': e.id,
        'exerciseId': e.exerciseId,
        'orderIndex': order,
        'supersetGroupId': e.supersetGroupId,
        'note': null,
        'sets': [
          for (final s in e.sets)
            {
              'id': s.id,
              'setNumber': s.setNumber,
              'setType': s.setType.apiValue,
              'weight': s.weight,
              'reps': s.reps,
              'durationSeconds': null,
              'distanceMeters': null,
              'rpe': s.rpe,
              'note': s.note,
              'isCompleted': s.isCompleted,
              'isPr': false,
              'completedAt': null,
            },
        ],
      };
}
