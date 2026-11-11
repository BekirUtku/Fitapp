import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/dio_client.dart';
import '../domain/gamification_models.dart';

final gamificationRepositoryProvider = Provider<GamificationRepository>(
  (ref) => GamificationRepository(ref.read(dioProvider)),
);

class GamificationRepository {
  GamificationRepository(this._dio);
  final Dio _dio;

  Future<StreakData> streak() async {
    final res = await _dio.get('/gamification/streak');
    return StreakData.fromJson(Map<String, dynamic>.from(res.data as Map));
  }

  Future<List<MuscleHeat>> heatmap() async {
    final res = await _dio.get('/gamification/muscle-heatmap');
    return (res.data as List).map((e) => MuscleHeat.fromJson(Map<String, dynamic>.from(e as Map))).toList();
  }

  Future<List<GBadge>> badges() async {
    final res = await _dio.get('/gamification/badges');
    return (res.data as List).map((e) => GBadge.fromJson(Map<String, dynamic>.from(e as Map))).toList();
  }
}

final streakProvider = FutureProvider<StreakData>((ref) => ref.read(gamificationRepositoryProvider).streak());
final heatmapProvider = FutureProvider<List<MuscleHeat>>((ref) => ref.read(gamificationRepositoryProvider).heatmap());
final badgesProvider = FutureProvider<List<GBadge>>((ref) => ref.read(gamificationRepositoryProvider).badges());
