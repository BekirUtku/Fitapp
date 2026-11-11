import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../network/dio_client.dart';
import '../storage/local_store.dart';
import 'outbox.dart';
import 'sync_change.dart';

final outboxProvider = Provider<Outbox>((ref) => Outbox(LocalStore.instance));

final syncEngineProvider = Provider<SyncEngine>((ref) => SyncEngine(
      dio: ref.read(dioProvider),
      outbox: ref.read(outboxProvider),
      store: LocalStore.instance,
    ));

class SyncResultSummary {
  const SyncResultSummary({this.pushed = 0, this.pulled = 0, this.failed = 0, this.error});
  final int pushed;
  final int pulled;
  final int failed;
  final String? error;
  bool get ok => error == null;
}

/// Local-first sync: önce yerel değişiklikleri gönder (push), sonra sunucudan çek (pull).
class SyncEngine {
  SyncEngine({required Dio dio, required Outbox outbox, required LocalStore store})
      : _dio = dio,
        _outbox = outbox,
        _store = store;

  final Dio _dio;
  final Outbox _outbox;
  final LocalStore _store;
  bool _running = false;

  static const _typeToBox = {
    'session': LocalStore.sessionsBox,
    'exercise': LocalStore.exercisesBox,
  };

  Future<SyncResultSummary> sync() async {
    if (_running) return const SyncResultSummary();
    _running = true;
    try {
      final pushed = await _push();
      final pulled = await _pull();
      return SyncResultSummary(pushed: pushed.applied, failed: pushed.failed, pulled: pulled);
    } on DioException catch (e) {
      return SyncResultSummary(error: e.message ?? 'Ağ hatası');
    } finally {
      _running = false;
    }
  }

  Future<({int applied, int failed})> _push() async {
    final pending = _outbox.pending();
    if (pending.isEmpty) return (applied: 0, failed: 0);

    final res = await _dio.post('/sync/push', data: {
      'changes': pending.map((c) => c.toApi()).toList(),
    });

    final results = (res.data['results'] as List).cast<Map<String, dynamic>>();
    var applied = 0;
    var failed = 0;

    for (final r in results) {
      final id = r['entityId'] as String;
      final type = r['entityType'] as String;
      final status = (r['status'] as String).toLowerCase();
      final key = '$type:$id';

      if (status == 'error') {
        failed++;
        for (final c in pending) {
          if (c.key == key) {
            await _outbox.markRetried(c);
            break;
          }
        }
      } else {
        // Applied / SkippedLww / NotFound → yerel değişiklik çözüldü.
        applied++;
        await _outbox.remove(key);
      }
    }
    return (applied: applied, failed: failed);
  }

  Future<int> _pull() async {
    final since = _store.lastSyncAt;
    final res = await _dio.get('/sync/pull', queryParameters: {
      if (since != null) 'since': since.toUtc().toIso8601String(),
    });

    final changes = (res.data['changes'] as List).cast<Map<String, dynamic>>();
    for (final c in changes) {
      final type = c['entityType'] as String;
      final id = c['entityId'] as String;
      final op = (c['operation'] as String).toLowerCase();
      final boxName = _typeToBox[type];
      if (boxName == null) continue;

      if (op == 'delete') {
        await _store.delete(boxName, id);
      } else {
        final payload = Map<String, dynamic>.from(c['payload'] as Map);
        await _store.write(boxName, id, {
          'id': id,
          'updatedAt': c['updatedAt'],
          'isSynced': true,
          ...payload,
        });
      }
    }

    final serverTime = DateTime.tryParse(res.data['serverTime'] as String? ?? '');
    if (serverTime != null) await _store.setLastSyncAt(serverTime);
    return changes.length;
  }
}
