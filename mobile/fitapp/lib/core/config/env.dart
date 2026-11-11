import 'package:flutter/foundation.dart';

/// Ortam yapılandırması. Backend base URL'i platforma göre otomatik seçilir.
abstract final class Env {
  static String get apiBaseUrl {
    const override = String.fromEnvironment('API_BASE_URL');
    if (override.isNotEmpty) return override;

    // Web ve masaüstü: doğrudan localhost.
    if (kIsWeb) return 'http://localhost:5001/api';

    // Android emülatör host makinesine 10.0.2.2 üzerinden erişir.
    return defaultTargetPlatform == TargetPlatform.android
        ? 'http://10.0.2.2:5001/api'
        : 'http://localhost:5001/api';
  }

  static const Duration connectTimeout = Duration(seconds: 15);
  static const Duration receiveTimeout = Duration(seconds: 20);
}
