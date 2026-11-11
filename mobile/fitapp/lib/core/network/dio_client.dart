import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import '../config/env.dart';
import 'auth_token_store.dart';

final authTokenStoreProvider = Provider<AuthTokenStore>(
  (ref) => AuthTokenStore(const FlutterSecureStorage()),
);

final dioProvider = Provider<Dio>((ref) {
  final tokens = ref.read(authTokenStoreProvider);

  final dio = Dio(BaseOptions(
    baseUrl: Env.apiBaseUrl,
    connectTimeout: Env.connectTimeout,
    receiveTimeout: Env.receiveTimeout,
    contentType: 'application/json',
  ));

  dio.interceptors.add(_AuthInterceptor(dio, tokens));
  return dio;
});

/// Her isteğe Bearer token ekler; 401'de bir kez refresh dener.
class _AuthInterceptor extends QueuedInterceptor {
  _AuthInterceptor(this._dio, this._tokens);

  final Dio _dio;
  final AuthTokenStore _tokens;

  @override
  Future<void> onRequest(RequestOptions options, RequestInterceptorHandler handler) async {
    final token = await _tokens.accessToken;
    if (token != null) options.headers['Authorization'] = 'Bearer $token';
    handler.next(options);
  }

  @override
  Future<void> onError(DioException err, ErrorInterceptorHandler handler) async {
    final is401 = err.response?.statusCode == 401;
    final alreadyRetried = err.requestOptions.extra['retried'] == true;

    if (!is401 || alreadyRetried) return handler.next(err);

    final refresh = await _tokens.refreshToken;
    if (refresh == null) return handler.next(err);

    try {
      final res = await Dio(BaseOptions(baseUrl: Env.apiBaseUrl))
          .post('/auth/refresh', data: {'refreshToken': refresh});
      final data = res.data as Map<String, dynamic>;
      await _tokens.save(
        accessToken: data['accessToken'] as String,
        refreshToken: data['refreshToken'] as String,
      );

      final opts = err.requestOptions;
      opts.extra['retried'] = true;
      opts.headers['Authorization'] = 'Bearer ${data['accessToken']}';
      final clone = await _dio.fetch(opts);
      return handler.resolve(clone);
    } catch (_) {
      await _tokens.clear();
      return handler.next(err);
    }
  }
}
