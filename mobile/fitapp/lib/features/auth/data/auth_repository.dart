import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/auth_token_store.dart';
import '../../../core/network/dio_client.dart';

class AuthUser {
  const AuthUser({required this.userId, required this.email, required this.displayName, required this.role});
  final String userId;
  final String email;
  final String displayName;
  final String role;

  factory AuthUser.fromJson(Map<String, dynamic> j) => AuthUser(
        userId: j['userId'] as String,
        email: j['email'] as String,
        displayName: j['displayName'] as String? ?? '',
        role: j['role'] as String? ?? 'User',
      );
}

final authRepositoryProvider = Provider<AuthRepository>(
  (ref) => AuthRepository(ref.read(dioProvider), ref.read(authTokenStoreProvider)),
);

class AuthRepository {
  AuthRepository(this._dio, this._tokens);

  final Dio _dio;
  final AuthTokenStore _tokens;

  Future<AuthUser> login(String email, String password) =>
      _auth('/auth/login', {'email': email, 'password': password});

  Future<AuthUser> register(String email, String password, String displayName) =>
      _auth('/auth/register', {'email': email, 'password': password, 'displayName': displayName});

  Future<void> logout() async {
    final refresh = await _tokens.refreshToken;
    try {
      if (refresh != null) await _dio.post('/auth/logout', data: {'refreshToken': refresh});
    } catch (_) {
      // sunucuya ulaşılamasa da yerel token'ları temizle
    }
    await _tokens.clear();
  }

  Future<AuthUser> _auth(String path, Map<String, dynamic> body) async {
    final res = await _dio.post(path, data: body);
    final data = Map<String, dynamic>.from(res.data as Map);
    await _tokens.save(
      accessToken: data['accessToken'] as String,
      refreshToken: data['refreshToken'] as String,
    );
    return AuthUser.fromJson(data);
  }
}
