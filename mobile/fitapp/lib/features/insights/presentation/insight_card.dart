import 'package:flutter/material.dart';

import '../../../design_system/components/app_card.dart';
import '../../../design_system/theme/app_spacing.dart';
import '../domain/insight.dart';

class InsightCard extends StatelessWidget {
  const InsightCard({super.key, required this.insight});

  final Insight insight;

  IconData get _icon => switch (insight.code) {
        'weekly_volume' => Icons.trending_up,
        'most_improved' => Icons.auto_graph,
        'strongest_muscle' => Icons.bolt,
        'neglected_muscle' => Icons.warning_amber,
        'avg_rest' => Icons.timer_outlined,
        'closest_pr' => Icons.emoji_events_outlined,
        'session_pr' => Icons.emoji_events,
        'session_volume' => Icons.fitness_center,
        'protein_remaining' => Icons.restaurant,
        _ => Icons.insights,
      };

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Padding(
      padding: const EdgeInsets.only(bottom: AppSpacing.xs),
      child: AppCard(
        child: Row(
          children: [
            Container(
              width: 40,
              height: 40,
              decoration: BoxDecoration(
                color: theme.colorScheme.primary.withOpacity(0.12),
                borderRadius: BorderRadius.circular(10),
              ),
              child: Icon(_icon, color: theme.colorScheme.primary, size: 22),
            ),
            const SizedBox(width: AppSpacing.md),
            Expanded(child: Text(insight.message, style: theme.textTheme.bodyLarge)),
          ],
        ),
      ),
    );
  }
}
