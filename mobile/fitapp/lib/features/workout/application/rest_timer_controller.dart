import 'dart:async';

import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/notifications/notification_service.dart';

class RestTimerState {
  const RestTimerState({
    this.isActive = false,
    this.isRunning = false,
    this.remaining = 0,
    this.total = 0,
  });

  final bool isActive;
  final bool isRunning;
  final int remaining; // saniye
  final int total;

  double get progress => total == 0 ? 0 : (total - remaining) / total;

  RestTimerState copyWith({bool? isActive, bool? isRunning, int? remaining, int? total}) =>
      RestTimerState(
        isActive: isActive ?? this.isActive,
        isRunning: isRunning ?? this.isRunning,
        remaining: remaining ?? this.remaining,
        total: total ?? this.total,
      );
}

class RestTimerController extends StateNotifier<RestTimerState> {
  RestTimerController() : super(const RestTimerState());

  Timer? _ticker;
  DateTime? _endTime;

  void start(int seconds) {
    _endTime = DateTime.now().add(Duration(seconds: seconds));
    state = RestTimerState(isActive: true, isRunning: true, remaining: seconds, total: seconds);
    NotificationService.instance.scheduleRestEnd(_endTime!);
    _startTicker();
  }

  void addSeconds(int delta) {
    if (!state.isActive) return;
    final newTotal = (state.total + delta).clamp(5, 3600);
    _endTime = DateTime.now().add(Duration(seconds: (state.remaining + delta).clamp(0, 3600)));
    state = state.copyWith(total: newTotal, remaining: (state.remaining + delta).clamp(0, 3600));
    if (state.isRunning) NotificationService.instance.scheduleRestEnd(_endTime!);
  }

  void pause() {
    _ticker?.cancel();
    NotificationService.instance.cancelRest();
    state = state.copyWith(isRunning: false);
  }

  void resume() {
    if (!state.isActive) return;
    _endTime = DateTime.now().add(Duration(seconds: state.remaining));
    NotificationService.instance.scheduleRestEnd(_endTime!);
    state = state.copyWith(isRunning: true);
    _startTicker();
  }

  void stop() {
    _ticker?.cancel();
    NotificationService.instance.cancelRest();
    _endTime = null;
    state = const RestTimerState();
  }

  /// Uygulama öne geldiğinde kalan süreyi endTime üzerinden yeniden hesaplar.
  void resync() {
    if (!state.isActive || !state.isRunning || _endTime == null) return;
    final remaining = _endTime!.difference(DateTime.now()).inSeconds;
    if (remaining <= 0) {
      stop();
    } else {
      state = state.copyWith(remaining: remaining);
    }
  }

  void _startTicker() {
    _ticker?.cancel();
    _ticker = Timer.periodic(const Duration(seconds: 1), (_) {
      final remaining = _endTime!.difference(DateTime.now()).inSeconds;
      if (remaining <= 0) {
        stop();
      } else {
        state = state.copyWith(remaining: remaining);
      }
    });
  }

  @override
  void dispose() {
    _ticker?.cancel();
    super.dispose();
  }
}

final restTimerProvider =
    StateNotifierProvider<RestTimerController, RestTimerState>((ref) => RestTimerController());
