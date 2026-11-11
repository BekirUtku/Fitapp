import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/dio_client.dart';
import '../domain/template_models.dart';

final programsRepositoryProvider = Provider<ProgramsRepository>(
  (ref) => ProgramsRepository(ref.read(dioProvider)),
);

class ProgramsRepository {
  ProgramsRepository(this._dio);
  final Dio _dio;

  Future<List<TemplateListItem>> list() async {
    final res = await _dio.get('/templates');
    return (res.data as List)
        .map((e) => TemplateListItem.fromJson(Map<String, dynamic>.from(e as Map)))
        .toList();
  }

  Future<TemplateDetail> detail(String id) async {
    final res = await _dio.get('/templates/$id');
    return TemplateDetail.fromJson(Map<String, dynamic>.from(res.data as Map));
  }

  Future<void> create(Map<String, dynamic> body) => _dio.post('/templates', data: body);

  Future<void> update(String id, Map<String, dynamic> body) => _dio.put('/templates/$id', data: body);

  Future<void> remove(String id) => _dio.delete('/templates/$id');
}

final templatesProvider = FutureProvider<List<TemplateListItem>>(
  (ref) => ref.read(programsRepositoryProvider).list(),
);
