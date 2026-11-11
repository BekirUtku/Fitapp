import '../storage/local_store.dart';
import 'sync_change.dart';

/// Senkronize edilmemiş yerel değişiklikleri tutar. Anahtar = entityType:entityId,
/// böylece aynı varlığın ardışık düzenlemeleri tek kayıtta birleşir (son yazan kalır).
class Outbox {
  Outbox(this._store);

  final LocalStore _store;

  void enqueueUpsert(String entityType, String entityId, Map<String, dynamic> payload,
      {DateTime? updatedAt}) {
    _put(SyncChange(
      entityType: entityType,
      entityId: entityId,
      operation: 'Upsert',
      updatedAt: updatedAt ?? DateTime.now().toUtc(),
      payload: payload,
    ));
  }

  void enqueueDelete(String entityType, String entityId, {DateTime? updatedAt}) {
    _put(SyncChange(
      entityType: entityType,
      entityId: entityId,
      operation: 'Delete',
      updatedAt: updatedAt ?? DateTime.now().toUtc(),
    ));
  }

  List<SyncChange> pending() {
    final items = _store
        .all(LocalStore.outboxBox)
        .map(SyncChange.fromLocal)
        .toList();
    items.sort((a, b) =>
        (kSyncOrder[a.entityType] ?? 99).compareTo(kSyncOrder[b.entityType] ?? 99));
    return items;
  }

  Future<void> remove(String key) => _store.delete(LocalStore.outboxBox, key);

  Future<void> markRetried(SyncChange change) => _put(change.withRetry());

  Future<void> _put(SyncChange change) =>
      _store.write(LocalStore.outboxBox, change.key, change.toLocal());
}
