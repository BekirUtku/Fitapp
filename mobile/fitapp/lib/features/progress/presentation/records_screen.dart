import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../design_system/components/app_card.dart';
import '../../../design_system/components/shimmer.dart';
import '../../../design_system/theme/app_spacing.dart';
import '../data/records_repository.dart';

class RecordsScreen extends ConsumerWidget {
  const RecordsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(recordsProvider);
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(title: const Text('Kişisel Rekorlar')),
      body: state.when(
        loading: () => const SkeletonList(),
        error: (_, __) => const Center(child: Text('Yüklenemedi')),
        data: (list) {
          if (list.isEmpty) {
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(AppSpacing.xl),
                child: Text('Henüz rekor yok. Antrenman yaptıkça PR\'ların burada birikir.',
                    textAlign: TextAlign.center, style: theme.textTheme.bodyMedium),
              ),
            );
          }

          // Harekete göre grupla.
          final byExercise = <String, List<PersonalRecord>>{};
          for (final r in list) {
            byExercise.putIfAbsent(r.exerciseName, () => []).add(r);
          }

          return ListView(
            padding: const EdgeInsets.all(AppSpacing.md),
            children: [
              for (final entry in byExercise.entries)
                Padding(
                  padding: const EdgeInsets.only(bottom: AppSpacing.sm),
                  child: AppCard(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            Icon(Icons.emoji_events, color: theme.colorScheme.secondary, size: 20),
                            const SizedBox(width: AppSpacing.xs),
                            Expanded(child: Text(entry.key, style: theme.textTheme.titleMedium)),
                          ],
                        ),
                        const SizedBox(height: AppSpacing.xs),
                        for (final r in entry.value)
                          Padding(
                            padding: const EdgeInsets.symmetric(vertical: 2),
                            child: Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                Text(r.typeLabel, style: theme.textTheme.bodyMedium),
                                Text(_valueText(r), style: theme.textTheme.labelLarge),
                              ],
                            ),
                          ),
                      ],
                    ),
                  ),
                ),
            ],
          );
        },
      ),
    );
  }

  String _valueText(PersonalRecord r) => switch (r.recordType) {
        'MaxReps' => '${r.value.toStringAsFixed(0)} tekrar',
        'MaxDuration' => '${r.value.toStringAsFixed(0)} sn',
        _ => '${r.value.toStringAsFixed(1)} kg',
      };
}
