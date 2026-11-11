import 'package:flutter/material.dart';

/// Enerjik & sporcu palet. Koyu tema ağırlıklı, canlı vurgular + gradyanlar.
abstract final class AppColors {
  // Marka — enerjik indigo → mor
  static const Color primary = Color(0xFF5B6CFF);
  static const Color primaryDark = Color(0xFF7B8CFF);
  static const Color gradientStart = Color(0xFF5B6CFF);
  static const Color gradientEnd = Color(0xFF9B5CFF);

  // Vurgular
  static const Color accent = Color(0xFF00E5A0); // enerjik yeşil
  static const Color energy = Color(0xFFFF7A45); // turuncu (seri/ateş)

  // Durum
  static const Color success = Color(0xFF22C55E);
  static const Color warning = Color(0xFFF59E0B);
  static const Color danger = Color(0xFFFF4D5E);

  // Dark (ana)
  static const Color darkBackground = Color(0xFF0A0D14);
  static const Color darkSurface = Color(0xFF141A24);
  static const Color darkSurfaceAlt = Color(0xFF1C2330);
  static const Color darkTextPrimary = Color(0xFFF4F6FB);
  static const Color darkTextSecondary = Color(0xFF8A94A6);
  static const Color darkBorder = Color(0xFF232B3A);

  // Light (ikincil)
  static const Color lightBackground = Color(0xFFF4F6FB);
  static const Color lightSurface = Color(0xFFFFFFFF);
  static const Color lightTextPrimary = Color(0xFF0E1320);
  static const Color lightTextSecondary = Color(0xFF5B6475);
  static const Color lightBorder = Color(0xFFE5E8F0);

  // Enerji gradyanı (CTA, hero, başlık vurgusu)
  static const LinearGradient brandGradient = LinearGradient(
    colors: [gradientStart, gradientEnd],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );

  static const LinearGradient energyGradient = LinearGradient(
    colors: [Color(0xFFFF7A45), Color(0xFFFF4D8D)],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );
}
