import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/dio_client.dart';
import '../domain/insight.dart';

final insightsRepositoryProvider = Provider<InsightsRepository>(
  (ref) => InsightsRepository(ref.read(dioProvider)),
);

class InsightsRepository {
  InsightsRepository(this._dio);

  final Dio _dio;

  Future<List<Insight>> summary() async {
    final res = await _dio.get('/insights/summary');
    return _parse(res.data);
  }

  Future<List<Insight>> session(String sessionId) async {
    final res = await _dio.get('/insights/session/$sessionId');
    return _parse(res.data);
  }

  List<Insight> _parse(dynamic data) =>
      (data as List).map((e) => Insight.fromJson(Map<String, dynamic>.from(e as Map))).toList();
}

/// Antrenman sonu analizi (best-effort; sunucuda oturum yoksa boş döner).
final sessionInsightsProvider = FutureProvider.family<List<Insight>, String>((ref, sessionId) async {
  try {
    return await ref.read(insightsRepositoryProvider).session(sessionId);
  } catch (_) {
    return const <Insight>[];
  }
});

final insightsSummaryProvider = FutureProvider<List<Insight>>((ref) async {
  try {
    return await ref.read(insightsRepositoryProvider).summary();
  } catch (_) {
    return const <Insight>[];
  }
});
