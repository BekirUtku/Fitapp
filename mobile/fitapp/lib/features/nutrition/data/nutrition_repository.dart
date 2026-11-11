import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/dio_client.dart';
import '../domain/nutrition_models.dart';

final nutritionRepositoryProvider = Provider<NutritionRepository>(
  (ref) => NutritionRepository(ref.read(dioProvider)),
);

class NutritionRepository {
  NutritionRepository(this._dio);

  final Dio _dio;

  String _today() => DateTime.now().toIso8601String().substring(0, 10);

  Future<NutritionSummary> summary([String? date]) async {
    final res = await _dio.get('/nutrition/summary', queryParameters: {'date': date ?? _today()});
    return NutritionSummary.fromJson(Map<String, dynamic>.from(res.data as Map));
  }

  Future<void> saveLog(MacroSet macros, {String? date}) async {
    await _dio.put('/nutrition/logs', data: {'date': date ?? _today(), ...macros.toJson()});
  }

  Future<int> addWater(int ml, {String? date}) async {
    final res = await _dio.post('/nutrition/logs/water', data: {'date': date ?? _today(), 'ml': ml});
    return (res.data['waterMl'] as num).toInt();
  }

  Future<void> setTarget(MacroSet target) async {
    await _dio.put('/nutrition/targets', data: target.toJson());
  }
}
