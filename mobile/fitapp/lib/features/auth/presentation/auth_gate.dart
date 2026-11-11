import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/home_shell.dart';
import '../application/auth_controller.dart';
import 'login_screen.dart';

/// Token varsa uygulamaya, yoksa giriş ekranına yönlendirir.
class AuthGate extends ConsumerWidget {
  const AuthGate({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final auth = ref.watch(authControllerProvider);
    return auth.when(
      loading: () => const Scaffold(body: Center(child: CircularProgressIndicator())),
      error: (_, __) => const LoginScreen(),
      data: (authenticated) => authenticated ? const HomeShell() : const LoginScreen(),
    );
  }
}
