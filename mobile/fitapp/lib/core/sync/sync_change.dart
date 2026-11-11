/// Sunucu sözleşmesiyle uyumlu değişiklik zarfı.
class SyncChange {
  SyncChange({
    required this.entityType,
    required this.entityId,
    required this.operation, // 'Upsert' | 'Delete'
    required this.updatedAt,
    this.payload,
    this.retryCount = 0,
  });

  final String entityType;
  final String entityId;
  final String operation;
  final DateTime updatedAt;
  final Map<String, dynamic>? payload;
  final int retryCount;

  /// Aynı varlığın değişiklikleri outbox'ta tek anahtarda birleşsin.
  String get key => '$entityType:$entityId';

  Map<String, dynamic> toApi() => {
        'entityType': entityType,
        'entityId': entityId,
        'operation': operation,
        'updatedAt': updatedAt.toUtc().toIso8601String(),
        'payload': payload,
      };

  Map<String, dynamic> toLocal() => {...toApi(), 'retryCount': retryCount};

  factory SyncChange.fromLocal(Map<String, dynamic> m) => SyncChange(
        entityType: m['entityType'] as String,
        entityId: m['entityId'] as String,
        operation: m['operation'] as String,
        updatedAt: DateTime.parse(m['updatedAt'] as String),
        payload: m['payload'] == null ? null : Map<String, dynamic>.from(m['payload'] as Map),
        retryCount: (m['retryCount'] as int?) ?? 0,
      );

  SyncChange withRetry() => SyncChange(
        entityType: entityType,
        entityId: entityId,
        operation: operation,
        updatedAt: updatedAt,
        payload: payload,
        retryCount: retryCount + 1,
      );
}

/// Push bağımlılık sırası (küçük önce). Sunucu tarafıyla aynı.
const Map<String, int> kSyncOrder = {
  'exercise': 1,
  'template': 2,
  'weeklyPlan': 3,
  'session': 4,
};
