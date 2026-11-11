import 'dart:io';

import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:path_provider/path_provider.dart';

import '../../../core/network/dio_client.dart';

final exportRepositoryProvider = Provider<ExportRepository>(
  (ref) => ExportRepository(ref.read(dioProvider)),
);

class ExportRepository {
  ExportRepository(this._dio);

  final Dio _dio;

  /// Verilen formatta veriyi indirir, geçici bir dosyaya yazar ve yolunu döner.
  Future<String> download(String format) async {
    final res = await _dio.get<List<int>>(
      '/export',
      queryParameters: {'format': format},
      options: Options(responseType: ResponseType.bytes),
    );

    final dir = await getTemporaryDirectory();
    final stamp = DateTime.now().toIso8601String().substring(0, 10);
    final path = '${dir.path}/fitapp_export_$stamp.$format';
    final file = File(path);
    await file.writeAsBytes(res.data ?? const []);
    return path;
  }
}
