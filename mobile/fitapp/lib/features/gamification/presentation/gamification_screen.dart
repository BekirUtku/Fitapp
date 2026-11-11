import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../design_system/components/app_card.dart';
import '../../../design_system/theme/app_spacing.dart';
import '../data/gamification_repository.dart';
import '../domain/gamification_models.dart';
import 'streak_calendar.dart';

class GamificationScreen extends ConsumerWidget {
  const GamificationScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final streak = ref.watch(streakProvider);
    final heat = ref.watch(heatmapProvider);
    final badges = ref.watch(badgesProvider);
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(title: const Text('Seri & Rozetler')),
      body: ListView(
        padding: const EdgeInsets.all(AppSpacing.md),
        children: [
          AppCard(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                streak.when(
                  loading: () => const SizedBox(height: 60, child: Center(child: CircularProgressIndicator())),
                  error: (_, __) => const Text('Seri yüklenemedi'),
                  data: (s) => Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Icon(Icons.local_fire_department, color: theme.colorScheme.secondary),
                          const SizedBox(width: AppSpacing.xs),
                          Text('${s.current} günlük seri',
                              style: theme.textTheme.titleLarge),
                          const Spacer(),
                          Text('En uzun: ${s.longest}', style: theme.textTheme.labelLarge),
                        ],
                      ),
                      const SizedBox(height: AppSpacing.md),
                      StreakCalendar(days: s.days),
                    ],
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: AppSpacing.md),
          Text('Kas Isı Haritası (30 gün)', style: theme.textTheme.titleMedium),
          const SizedBox(height: AppSpacing.sm),
          heat.when(
            loading: () => const Center(child: CircularProgressIndicator()),
            error: (_, __) => const Text('Yüklenemedi'),
            data: (list) => list.isEmpty
                ? const Text('Henüz veri yok')
                : Column(children: [for (final m in list) _HeatRow(heat: m)]),
          ),
          const SizedBox(height: AppSpacing.lg),
          Text('Rozetler', style: theme.textTheme.titleMedium),
          const SizedBox(height: AppSpacing.sm),
          badges.when(
            loading: () => const Center(child: CircularProgressIndicator()),
            error: (_, __) => const Text('Yüklenemedi'),
            data: (list) => Column(children: [for (final b in list) _BadgeRow(badge: b)]),
          ),
        ],
      ),
    );
  }
}

class _HeatRow extends StatelessWidget {
  const _HeatRow({required this.heat});
  final MuscleHeat heat;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: AppSpacing.xxs),
      child: Row(
        children: [
          SizedBox(width: 110, child: Text(heat.name, style: theme.textTheme.bodyMedium)),
          Expanded(
            child: ClipRRect(
              borderRadius: BorderRadius.circular(6),
              child: LinearProgressIndicator(
                value: heat.intensity == 0 ? 0.02 : heat.intensity,
                minHeight: 10,
                backgroundColor: theme.dividerColor,
                color: Color.lerp(theme.colorScheme.secondary, theme.colorScheme.error, heat.intensity),
              ),
            ),
          ),
          const SizedBox(width: AppSpacing.xs),
          Text('${heat.sets} set', style: theme.textTheme.labelSmall),
        ],
      ),
    );
  }
}

class _BadgeRow extends StatelessWidget {
  const _BadgeRow({required this.badge});
  final GBadge badge;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final earned = badge.earned;
    return Padding(
      padding: const EdgeInsets.only(bottom: AppSpacing.xs),
      child: AppCard(
        child: Row(
          children: [
            Icon(earned ? Icons.emoji_events : Icons.lock_outline,
                color: earned ? theme.colorScheme.secondary : theme.dividerColor),
            const SizedBox(width: AppSpacing.md),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(badge.name,
                      style: theme.textTheme.titleMedium?.copyWith(
                          color: earned ? null : theme.disabledColor)),
                  Text(badge.description, style: theme.textTheme.labelSmall),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
