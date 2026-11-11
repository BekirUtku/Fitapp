import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../design_system/components/shimmer.dart';
import '../../../design_system/theme/app_spacing.dart';
import '../data/insights_repository.dart';
import 'insight_card.dart';

class InsightsScreen extends ConsumerWidget {
  const InsightsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(insightsSummaryProvider);
    return Scaffold(
      appBar: AppBar(title: const Text('İçgörüler')),
      body: RefreshIndicator(
        onRefresh: () async => ref.refresh(insightsSummaryProvider.future),
        child: state.when(
          loading: () => const SkeletonList(),
          error: (e, _) => _empty(context, 'Şimdilik içgörü yok.'),
          data: (items) => items.isEmpty
              ? _empty(context, 'Daha fazla antrenman kaydettikçe içgörüler burada görünecek.')
              : ListView(
                  padding: const EdgeInsets.all(AppSpacing.md),
                  children: [for (final i in items) InsightCard(insight: i)],
                ),
        ),
      ),
    );
  }

  Widget _empty(BuildContext context, String msg) => ListView(
        children: [
          const SizedBox(height: 120),
          Icon(Icons.insights, size: 48, color: Theme.of(context).colorScheme.primary),
          const SizedBox(height: AppSpacing.md),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: AppSpacing.xl),
            child: Text(msg, textAlign: TextAlign.center,
                style: Theme.of(context).textTheme.bodyMedium),
          ),
        ],
      );
}
