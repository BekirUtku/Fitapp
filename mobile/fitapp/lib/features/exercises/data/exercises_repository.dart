import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/dio_client.dart';
import '../domain/exercise_models.dart';

final exercisesRepositoryProvider = Provider<ExercisesRepository>(
  (ref) => ExercisesRepository(ref.read(dioProvider)),
);

class ExercisesRepository {
  ExercisesRepository(this._dio);
  final Dio _dio;

  Future<List<ExerciseListItem>> list({String? search, String? muscleGroupId}) async {
    final res = await _dio.get('/exercises', queryParameters: {
      if (search != null && search.isNotEmpty) 'search': search,
      if (muscleGroupId != null) 'muscleGroup': muscleGroupId,
      'pageSize': 50,
    });
    final items = (res.data['items'] as List).cast<Map<String, dynamic>>();
    return items.map(ExerciseListItem.fromJson).toList();
  }

  Future<List<MuscleGroupItem>> muscleGroups() async {
    final res = await _dio.get('/muscle-groups');
    return (res.data as List)
        .map((e) => MuscleGroupItem.fromJson(Map<String, dynamic>.from(e as Map)))
        .toList();
  }
}

final muscleGroupsProvider = FutureProvider<List<MuscleGroupItem>>(
  (ref) => ref.read(exercisesRepositoryProvider).muscleGroups(),
);
