import 'dart:async';

import 'package:connectivity_plus/connectivity_plus.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'sync_engine.dart';

/// Bağlantı geri geldiğinde bekleyen değişiklikleri otomatik senkronlar.
/// Uygulama başında bir kez okunarak (watch) aktive edilir.
final connectivitySyncProvider = Provider<ConnectivitySync>((ref) {
  final service = ConnectivitySync(ref);
  ref.onDispose(service.dispose);
  service.start();
  return service;
});

class ConnectivitySync {
  ConnectivitySync(this._ref);

  final Ref _ref;
  StreamSubscription<List<ConnectivityResult>>? _sub;

  void start() {
    _sub = Connectivity().onConnectivityChanged.listen((results) {
      final online = results.any((r) => r != ConnectivityResult.none);
      if (online) {
        _ref.read(syncEngineProvider).sync();
      }
    });
  }

  void dispose() => _sub?.cancel();
}
