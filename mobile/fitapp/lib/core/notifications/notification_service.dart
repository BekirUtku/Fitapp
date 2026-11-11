import 'package:flutter/foundation.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:timezone/data/latest_all.dart' as tz;
import 'package:timezone/timezone.dart' as tz;

/// Dinlenme sayacı için local notification servisi.
///
/// Arka plan/kilit ekranı kısıtlamalarına uygun yaklaşım: sayaç başladığında
/// bitiş anına önceden planlanmış bir bildirim kurulur. Uygulama öne gelince
/// kalan süre endTime üzerinden yeniden hesaplanır.
class NotificationService {
  NotificationService._();
  static final NotificationService instance = NotificationService._();

  final FlutterLocalNotificationsPlugin _plugin = FlutterLocalNotificationsPlugin();
  static const int _restNotificationId = 1001;
  bool _initialized = false;

  Future<void> init() async {
    if (kIsWeb || _initialized) return; // web'de local notification desteklenmez
    tz.initializeTimeZones();

    const android = AndroidInitializationSettings('@mipmap/ic_launcher');
    const ios = DarwinInitializationSettings(
      requestAlertPermission: true,
      requestBadgePermission: true,
      requestSoundPermission: true,
    );
    await _plugin.initialize(
      const InitializationSettings(android: android, iOS: ios),
    );
    _initialized = true;
  }

  Future<void> scheduleRestEnd(DateTime endTime) async {
    await init();
    await cancelRest();

    const details = NotificationDetails(
      android: AndroidNotificationDetails(
        'rest_timer',
        'Dinlenme Sayacı',
        channelDescription: 'Dinlenme süresi bitiş uyarısı',
        importance: Importance.max,
        priority: Priority.high,
        playSound: true,
        enableVibration: true,
        category: AndroidNotificationCategory.alarm,
      ),
      iOS: DarwinNotificationDetails(presentSound: true),
    );

    await _plugin.zonedSchedule(
      _restNotificationId,
      'Dinlenme bitti 💪',
      'Sıradaki sete hazır mısın?',
      tz.TZDateTime.from(endTime, tz.local),
      details,
      androidScheduleMode: AndroidScheduleMode.exactAllowWhileIdle,
      uiLocalNotificationDateInterpretation:
          UILocalNotificationDateInterpretation.absoluteTime,
    );
  }

  Future<void> cancelRest() async => _plugin.cancel(_restNotificationId);
}
