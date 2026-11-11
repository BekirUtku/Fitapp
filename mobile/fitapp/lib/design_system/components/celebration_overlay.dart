import 'dart:math';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

/// Hafif konfeti kutlaması + titreşim. Yeni PR / antrenman tamamlamada tetiklenir.
/// Ek paket gerektirmez (AnimationController + CustomPainter).
class CelebrationOverlay extends StatefulWidget {
  const CelebrationOverlay({super.key, this.trigger = true});

  final bool trigger;

  @override
  State<CelebrationOverlay> createState() => _CelebrationOverlayState();
}

class _CelebrationOverlayState extends State<CelebrationOverlay>
    with SingleTickerProviderStateMixin {
  late final AnimationController _controller =
      AnimationController(vsync: this, duration: const Duration(milliseconds: 2200));
  final _rng = Random();
  late final List<_Particle> _particles;

  @override
  void initState() {
    super.initState();
    _particles = List.generate(80, (_) => _Particle.random(_rng));
    if (widget.trigger) {
      HapticFeedback.mediumImpact();
      _controller.forward();
    }
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return IgnorePointer(
      child: AnimatedBuilder(
        animation: _controller,
        builder: (_, __) => _controller.isAnimating || _controller.isCompleted
            ? CustomPaint(
                size: Size.infinite,
                painter: _ConfettiPainter(_particles, _controller.value),
              )
            : const SizedBox.shrink(),
      ),
    );
  }
}

class _Particle {
  _Particle(this.x, this.dx, this.color, this.size, this.rotationSpeed);

  final double x; // 0..1 başlangıç yatay konumu
  final double dx; // yatay sürüklenme
  final Color color;
  final double size;
  final double rotationSpeed;

  static const _colors = [
    Color(0xFF3D5AFE), Color(0xFF00E5A0), Color(0xFFF59E0B),
    Color(0xFFEF4444), Color(0xFF22C55E),
  ];

  factory _Particle.random(Random r) => _Particle(
        r.nextDouble(),
        (r.nextDouble() - 0.5) * 0.4,
        _colors[r.nextInt(_colors.length)],
        4 + r.nextDouble() * 6,
        (r.nextDouble() - 0.5) * 8,
      );
}

class _ConfettiPainter extends CustomPainter {
  _ConfettiPainter(this.particles, this.t);

  final List<_Particle> particles;
  final double t;

  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint();
    for (final p in particles) {
      final y = (t * 1.2 - 0.1) * size.height + p.size;
      final x = (p.x + p.dx * t) * size.width;
      paint.color = p.color.withOpacity((1 - t).clamp(0.0, 1.0));
      canvas.save();
      canvas.translate(x, y);
      canvas.rotate(p.rotationSpeed * t);
      canvas.drawRect(
        Rect.fromCenter(center: Offset.zero, width: p.size, height: p.size * 0.6),
        paint,
      );
      canvas.restore();
    }
  }

  @override
  bool shouldRepaint(_ConfettiPainter old) => old.t != t;
}
