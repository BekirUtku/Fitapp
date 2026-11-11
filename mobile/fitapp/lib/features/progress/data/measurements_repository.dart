import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/dio_client.dart';

class BodyMeasurement {
  const BodyMeasurement({required this.id, required this.measuredAt, this.bodyweight, this.note});
  final String id;
  final DateTime measuredAt;
  final double? bodyweight;
  final String? note;

  factory BodyMeasurement.fromJson(Map<String, dynamic> j) => BodyMeasurement(
        id: j['id'] as String,
        measuredAt: DateTime.parse(j['measuredAt'] as String),
        bodyweight: (j['bodyweight'] as num?)?.toDouble(),
        note: j['note'] as String?,
      );
}

final measurementsRepositoryProvider = Provider<MeasurementsRepository>(
  (ref) => MeasurementsRepository(ref.read(dioProvider)),
);

class MeasurementsRepository {
  MeasurementsRepository(this._dio);
  final Dio _dio;

  Future<List<BodyMeasurement>> list() async {
    final res = await _dio.get('/body-measurements');
    return (res.data as List)
        .map((e) => BodyMeasurement.fromJson(Map<String, dynamic>.from(e as Map)))
        .toList();
  }

  Future<void> add({double? bodyweight, String? note}) =>
      _dio.post('/body-measurements', data: {'bodyweight': bodyweight, 'note': note});

  Future<void> remove(String id) => _dio.delete('/body-measurements/$id');
}

final measurementsProvider =
    FutureProvider<List<BodyMeasurement>>((ref) => ref.read(measurementsRepositoryProvider).list());
