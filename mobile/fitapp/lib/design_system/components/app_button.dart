import 'package:flutter/material.dart';

import '../theme/app_colors.dart';
import '../theme/app_radius.dart';
import '../theme/app_spacing.dart';

enum AppButtonVariant { filled, tonal, outlined, text }

/// Salon kullanımı için büyük dokunma alanlı ortak buton.
/// [gradient] true iken enerjik marka gradyanıyla dolu "hero" CTA olur.
class AppButton extends StatelessWidget {
  const AppButton({
    super.key,
    required this.label,
    this.onPressed,
    this.icon,
    this.variant = AppButtonVariant.filled,
    this.expand = true,
    this.loading = false,
    this.gradient = false,
  });

  final String label;
  final VoidCallback? onPressed;
  final IconData? icon;
  final AppButtonVariant variant;
  final bool expand;
  final bool loading;
  final bool gradient;

  @override
  Widget build(BuildContext context) {
    if (gradient) return _GradientButton(this);

    final child = loading
        ? const SizedBox(
            height: 20,
            width: 20,
            child: CircularProgressIndicator(strokeWidth: 2.4),
          )
        : _content();

    final onTap = loading ? null : onPressed;

    final button = switch (variant) {
      AppButtonVariant.filled => FilledButton(onPressed: onTap, child: child),
      AppButtonVariant.tonal => FilledButton.tonal(onPressed: onTap, child: child),
      AppButtonVariant.outlined => OutlinedButton(onPressed: onTap, child: child),
      AppButtonVariant.text => TextButton(onPressed: onTap, child: child),
    };

    return expand ? SizedBox(width: double.infinity, child: button) : button;
  }

  Widget _content() => Row(
        mainAxisSize: expand ? MainAxisSize.max : MainAxisSize.min,
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          if (icon != null) ...[
            Icon(icon, size: 20),
            const SizedBox(width: AppSpacing.xs),
          ],
          Text(label),
        ],
      );
}

class _GradientButton extends StatelessWidget {
  const _GradientButton(this.config);
  final AppButton config;

  @override
  Widget build(BuildContext context) {
    final enabled = !config.loading && config.onPressed != null;

    final content = config.loading
        ? const SizedBox(
            height: 22, width: 22,
            child: CircularProgressIndicator(strokeWidth: 2.6, color: Colors.white),
          )
        : Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              if (config.icon != null) ...[
                Icon(config.icon, size: 22, color: Colors.white),
                const SizedBox(width: AppSpacing.xs),
              ],
              Text(config.label,
                  style: const TextStyle(
                      color: Colors.white, fontSize: 16, fontWeight: FontWeight.w800, letterSpacing: 0.2)),
            ],
          );

    final button = Opacity(
      opacity: enabled ? 1 : 0.55,
      child: DecoratedBox(
        decoration: BoxDecoration(
          gradient: AppColors.brandGradient,
          borderRadius: AppRadius.button,
          boxShadow: [
            BoxShadow(
              color: AppColors.primary.withOpacity(0.45),
              blurRadius: 24,
              offset: const Offset(0, 10),
            ),
          ],
        ),
        child: Material(
          color: Colors.transparent,
          borderRadius: AppRadius.button,
          child: InkWell(
            borderRadius: AppRadius.button,
            onTap: enabled ? config.onPressed : null,
            child: Container(
              height: 56,
              alignment: Alignment.center,
              child: content,
            ),
          ),
        ),
      ),
    );

    return config.expand ? SizedBox(width: double.infinity, child: button) : button;
  }
}
