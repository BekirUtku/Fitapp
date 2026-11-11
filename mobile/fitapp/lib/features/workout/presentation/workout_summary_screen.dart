import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../design_system/components/app_button.dart';
import '../../../design_system/components/app_card.dart';
import '../../../design_system/components/celebration_overlay.dart';
import '../../../design_system/theme/app_spacing.dart';
import '../../insights/data/insights_repository.dart';
import '../../insights/presentation/insight_card.dart';
import '../domain/workout_models.dart';

/// Antrenman sonu özet ekranı. Kutlama animasyonu Faz 8'de eklenecek.
class WorkoutSummaryScreen extends ConsumerWidget {
  const WorkoutSummaryScreen({super.key, required this.session});

  final WorkoutSessionModel session;

  int get _estimatedCalories => (session.elapsed.inMinutes * 6);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final insights = ref.watch(sessionInsightsProvider(session.id));
    final d = session.elapsed;
    final duration = '${d.inMinutes} dk ${d.inSeconds % 60} sn';

    return Scaffold(
      appBar: AppBar(title: const Text('Antrenman Özeti')),
      body: Stack(children: [
      ListView(
        padding: const EdgeInsets.all(AppSpacing.md),
        children: [
          Text(session.name, style: theme.textTheme.headlineMedium),
          const SizedBox(height: AppSpacing.md),
          _StatGrid(items: [
            ('Süre', duration),
            ('Toplam Set', '${session.totalSets}'),
            ('Toplam Tekrar', '${session.totalReps}'),
            ('Hacim', '${session.totalVolume.toStringAsFixed(0)} kg'),
            ('Tahmini Kalori', '$_estimatedCalories kcal'),
            ('Hareket', '${session.exercises.length}'),
          ]),
          const SizedBox(height: AppSpacing.lg),
          Text('Hareket Bazlı En İyi (tahmini 1RM)', style: theme.textTheme.titleMedium),
          const SizedBox(height: AppSpacing.sm),
          ...session.exercises.map(_exerciseRow),
          ...insights.maybeWhen<List<Widget>>(
            data: (list) => list.isEmpty
                ? <Widget>[]
                : <Widget>[
                    const SizedBox(height: AppSpacing.lg),
                    Text('Analiz', style: theme.textTheme.titleMedium),
                    const SizedBox(height: AppSpacing.sm),
                    ...list.map((i) => InsightCard(insight: i)),
                  ],
            orElse: () => <Widget>[],
          ),
          const SizedBox(height: AppSpacing.xl),
          AppButton(
            label: 'Kapat',
            icon: Icons.check,
            onPressed: () => Navigator.of(context).pop(),
          ),
        ],
      ),
      const Positioned.fill(child: CelebrationOverlay()),
      ]),
    );
  }

  Widget _exerciseRow(SessionExerciseModel e) {
    final best = e.sets
        .where((s) => s.isCompleted && s.setType != SetType.warmUp && (s.weight ?? 0) > 0 && (s.reps ?? 0) > 0)
        .map((s) => s.weight! * (1 + s.reps! / 30))
        .fold<double>(0, (a, b) => b > a ? b : a);

    return Padding(
      padding: const EdgeInsets.only(bottom: AppSpacing.xs),
      child: AppCard(
        child: Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Expanded(child: Text(e.exerciseName)),
            Text(best > 0 ? '${best.toStringAsFixed(1)} kg' : '—'),
          ],
        ),
      ),
    );
  }
}

class _StatGrid extends StatelessWidget {
  const _StatGrid({required this.items});

  final List<(String, String)> items;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return GridView.count(
      crossAxisCount: 2,
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      mainAxisSpacing: AppSpacing.xs,
      crossAxisSpacing: AppSpacing.xs,
      childAspectRatio: 2.4,
      children: [
        for (final (label, value) in items)
          AppCard(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Text(value, style: theme.textTheme.titleLarge),
                Text(label, style: theme.textTheme.labelSmall),
              ],
            ),
          ),
      ],
    );
  }
}
