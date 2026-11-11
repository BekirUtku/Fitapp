import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../design_system/theme/app_spacing.dart';
import '../../application/rest_timer_controller.dart';

/// Aktif dinlenme sayacı çubuğu: geri sayım + ±15sn + duraklat/devam + kapat.
class RestTimerBar extends ConsumerWidget {
  const RestTimerBar({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(restTimerProvider);
    final ctrl = ref.read(restTimerProvider.notifier);
    if (!state.isActive) return const SizedBox.shrink();

    final theme = Theme.of(context);
    final m = (state.remaining ~/ 60).toString().padLeft(2, '0');
    final s = (state.remaining % 60).toString().padLeft(2, '0');

    return Material(
      color: theme.colorScheme.primary,
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: AppSpacing.md, vertical: AppSpacing.xs),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            LinearProgressIndicator(
              value: state.progress,
              backgroundColor: Colors.white24,
              color: Colors.white,
              minHeight: 4,
            ),
            const SizedBox(height: AppSpacing.xs),
            Row(
              children: [
                Text('Dinlenme  $m:$s',
                    style: theme.textTheme.titleMedium?.copyWith(color: Colors.white)),
                const Spacer(),
                _iconBtn(Icons.remove, () => ctrl.addSeconds(-15)),
                _iconBtn(Icons.add, () => ctrl.addSeconds(15)),
                _iconBtn(state.isRunning ? Icons.pause : Icons.play_arrow,
                    () => state.isRunning ? ctrl.pause() : ctrl.resume()),
                _iconBtn(Icons.close, ctrl.stop),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _iconBtn(IconData icon, VoidCallback onTap) =>
      IconButton(onPressed: onTap, icon: Icon(icon, color: Colors.white));
}
