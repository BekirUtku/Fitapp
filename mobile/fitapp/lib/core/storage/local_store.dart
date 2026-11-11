import 'package:hive_flutter/hive_flutter.dart';

/// Hive kutuları uygulamanın "source of truth"udur. Codegen'den kaçınmak için
/// kayıtlar JSON uyumlu Map olarak saklanır (id -> map).
class LocalStore {
  LocalStore._();
  static final LocalStore instance = LocalStore._();

  static const sessionsBox = 'sessions';
  static const exercisesBox = 'exercises';
  static const outboxBox = 'outbox';
  static const metaBox = 'meta';

  bool _ready = false;

  Future<void> init() async {
    if (_ready) return;
    await Hive.initFlutter();
    await Future.wait([
      Hive.openBox(sessionsBox),
      Hive.openBox(exercisesBox),
      Hive.openBox(outboxBox),
      Hive.openBox(metaBox),
    ]);
    _ready = true;
  }

  Box box(String name) => Hive.box(name);

  Map<String, dynamic>? read(String boxName, String id) {
    final raw = box(boxName).get(id);
    return raw == null ? null : Map<String, dynamic>.from(raw as Map);
  }

  Future<void> write(String boxName, String id, Map<String, dynamic> value) =>
      box(boxName).put(id, value);

  Future<void> delete(String boxName, String id) => box(boxName).delete(id);

  Iterable<Map<String, dynamic>> all(String boxName) =>
      box(boxName).values.map((e) => Map<String, dynamic>.from(e as Map));

  // --- meta ---
  DateTime? get lastSyncAt {
    final v = box(metaBox).get('lastSyncAt') as String?;
    return v == null ? null : DateTime.tryParse(v);
  }

  Future<void> setLastSyncAt(DateTime value) =>
      box(metaBox).put('lastSyncAt', value.toUtc().toIso8601String());
}
