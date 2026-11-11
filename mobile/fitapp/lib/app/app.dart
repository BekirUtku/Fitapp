import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../core/sync/connectivity_sync.dart';
import '../design_system/theme/app_theme.dart';
import '../features/auth/presentation/auth_gate.dart';

class FitApp extends ConsumerWidget {
  const FitApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    ref.watch(connectivitySyncProvider); // bağlantı-bazlı otomatik sync'i aktive et
    return MaterialApp(
      title: 'FitApp',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.light,
      darkTheme: AppTheme.dark,
      themeMode: ThemeMode.light,
      // Geniş ekranlarda (web/masaüstü) içeriği telefon genişliğinde ortalar.
      builder: (context, child) => ColoredBox(
        color: const Color(0xFFDDE3EC),
        child: Center(
          child: ClipRect(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 480),
              child: child,
            ),
          ),
        ),
      ),
      home: const AuthGate(),
    );
  }
}
