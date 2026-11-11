import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/dio_client.dart';
import '../data/auth_repository.dart';

/// Oturum durumu: true = giriş yapılmış. Uygulama açılışında token varlığından türetilir.
final authControllerProvider =
    AsyncNotifierProvider<AuthController, bool>(AuthController.new);

class AuthController extends AsyncNotifier<bool> {
  AuthRepository get _repo => ref.read(authRepositoryProvider);

  @override
  Future<bool> build() async {
    final token = await ref.read(authTokenStoreProvider).accessToken;
    return token != null;
  }

  Future<void> login(String email, String password) async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() async {
      await _repo.login(email.trim(), password);
      return true;
    });
  }

  Future<void> register(String email, String password, String displayName) async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(() async {
      await _repo.register(email.trim(), password, displayName.trim());
      return true;
    });
  }

  Future<void> logout() async {
    await _repo.logout();
    state = const AsyncValue.data(false);
  }
}
