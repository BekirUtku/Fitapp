import 'package:flutter/material.dart';

/// Enerjik tipografi — kalın, iddialı başlıklar; okunaklı gövde.
abstract final class AppTypography {
  static const String fontFamily = 'Roboto';

  static const TextTheme textTheme = TextTheme(
    displaySmall: TextStyle(fontSize: 34, fontWeight: FontWeight.w800, height: 1.1, letterSpacing: -0.8),
    headlineMedium: TextStyle(fontSize: 26, fontWeight: FontWeight.w800, height: 1.15, letterSpacing: -0.6),
    headlineSmall: TextStyle(fontSize: 21, fontWeight: FontWeight.w700, height: 1.2, letterSpacing: -0.3),
    titleLarge: TextStyle(fontSize: 18, fontWeight: FontWeight.w700, letterSpacing: -0.2),
    titleMedium: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
    bodyLarge: TextStyle(fontSize: 16, fontWeight: FontWeight.w400, height: 1.4),
    bodyMedium: TextStyle(fontSize: 14, fontWeight: FontWeight.w400, height: 1.4),
    labelLarge: TextStyle(fontSize: 14, fontWeight: FontWeight.w700, letterSpacing: 0.2),
    labelSmall: TextStyle(fontSize: 12, fontWeight: FontWeight.w600, letterSpacing: 0.3),
  );
}
