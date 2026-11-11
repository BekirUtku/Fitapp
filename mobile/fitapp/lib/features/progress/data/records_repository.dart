import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/dio_client.dart';

class PersonalRecord {
  const PersonalRecord({
    required this.exerciseName,
    required this.recordType,
    required this.value,
    this.weight,
    this.reps,
    required this.achievedAt,
  });

  final String exerciseName;
  final String recordType;
  final double value;
  final double? weight;
  final int? reps;
  final DateTime achievedAt;

  factory PersonalRecord.fromJson(Map<String, dynamic> j) => PersonalRecord(
        exerciseName: j['exerciseName'] as String? ?? '',
        recordType: j['recordType'] as String? ?? '',
        value: (j['value'] as num).toDouble(),
        weight: (j['weight'] as num?)?.toDouble(),
        reps: (j['reps'] as num?)?.toInt(),
        achievedAt: DateTime.parse(j['achievedAt'] as String),
      );

  String get typeLabel => switch (recordType) {
        'Estimated1Rm' => 'Tahmini 1RM',
        'MaxWeight' => 'En Ağır',
        'MaxReps' => 'En Çok Tekrar',
        'MaxVolume' => 'En Yüksek Hacim',
        'MaxDuration' => 'En Uzun Süre',
        _ => recordType,
      };
}

final recordsRepositoryProvider = Provider<RecordsRepository>(
  (ref) => RecordsRepository(ref.read(dioProvider)),
);

class RecordsRepository {
  RecordsRepository(this._dio);
  final Dio _dio;

  Future<List<PersonalRecord>> current() async {
    final res = await _dio.get('/personal-records');
    return (res.data as List)
        .map((e) => PersonalRecord.fromJson(Map<String, dynamic>.from(e as Map)))
        .toList();
  }
}

final recordsProvider =
    FutureProvider<List<PersonalRecord>>((ref) => ref.read(recordsRepositoryProvider).current());
