import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/navigation.dart';
import '../../../design_system/components/app_card.dart';
import '../../../design_system/components/fade_in.dart';
import '../../../design_system/components/gradient_card.dart';
import '../../../design_system/theme/app_colors.dart';
import '../../../design_system/theme/app_spacing.dart';
import '../../gamification/data/gamification_repository.dart';
import '../../insights/data/insights_repository.dart';
import '../../insights/presentation/insight_card.dart';

class DashboardScreen extends ConsumerWidget {
  const DashboardScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final now = DateTime.now();
    final streak = ref.watch(streakProvider);
    final insights = ref.watch(insightsSummaryProvider);

    return RefreshIndicator(
      onRefresh: () async {
        ref.invalidate(streakProvider);
        ref.invalidate(insightsSummaryProvider);
      },
      child: ListView(
        padding: const EdgeInsets.all(AppSpacing.md),
        children: [
          FadeIn(
            child: Text(_greeting(now.hour), style: theme.textTheme.headlineMedium),
          ),
          Text(_formatDate(now),
              style: theme.textTheme.bodyMedium?.copyWith(color: theme.colorScheme.onSurfaceVariant)),
          const SizedBox(height: AppSpacing.md),

          // Antrenmana başla — enerjik hero kart
          FadeIn(
            delayMs: 60,
            child: GradientCard(
              onTap: () => ref.read(selectedTabProvider.notifier).state = 1,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: const [
                      Icon(Icons.bolt, color: Colors.white),
                      SizedBox(width: AppSpacing.xs),
                      Text('BUGÜN ANTRENMAN ZAMANI',
                          style: TextStyle(color: Colors.white, fontWeight: FontWeight.w800, letterSpacing: 0.5)),
                    ],
                  ),
                  const SizedBox(height: AppSpacing.xs),
                  const Text('Başlamak için dokun',
                      style: TextStyle(color: Colors.white70, fontSize: 14)),
                  const SizedBox(height: AppSpacing.md),
                  Row(
                    children: const [
                      Text('Antrenmana Başla',
                          style: TextStyle(color: Colors.white, fontSize: 20, fontWeight: FontWeight.w800)),
                      SizedBox(width: AppSpacing.xs),
                      Icon(Icons.arrow_forward, color: Colors.white),
                    ],
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: AppSpacing.md),

          // Seri
          streak.maybeWhen(
            data: (s) => FadeIn(
              delayMs: 120,
              child: AppCard(
                child: Row(
                  children: [
                    Container(
                      width: 44,
                      height: 44,
                      decoration: BoxDecoration(
                        gradient: AppColors.energyGradient,
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: const Icon(Icons.local_fire_department, color: Colors.white),
                    ),
                    const SizedBox(width: AppSpacing.md),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text('${s.current} günlük seri', style: theme.textTheme.titleMedium),
                          Text('En uzun: ${s.longest} gün', style: theme.textTheme.labelSmall),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),
            orElse: () => const SizedBox.shrink(),
          ),
          const SizedBox(height: AppSpacing.lg),

          Text('İçgörüler', style: theme.textTheme.titleMedium),
          const SizedBox(height: AppSpacing.sm),
          insights.maybeWhen(
            data: (list) => list.isEmpty
                ? Text('Antrenman kaydettikçe içgörüler burada belirecek.',
                    style: theme.textTheme.bodyMedium)
                : Column(children: [for (final i in list.take(4)) InsightCard(insight: i)]),
            orElse: () => const Padding(
              padding: EdgeInsets.all(AppSpacing.md),
              child: Center(child: CircularProgressIndicator()),
            ),
          ),
        ],
      ),
    );
  }

  String _greeting(int hour) {
    if (hour < 6) return 'İyi geceler 🌙';
    if (hour < 12) return 'Günaydın ☀️';
    if (hour < 18) return 'İyi günler 💪';
    return 'İyi akşamlar 🌆';
  }

  String _formatDate(DateTime d) {
    const months = [
      'Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran',
      'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'
    ];
    const days = ['Pazartesi', 'Salı', 'Çarşamba', 'Perşembe', 'Cuma', 'Cumartesi', 'Pazar'];
    return '${days[d.weekday - 1]}, ${d.day} ${months[d.month - 1]} ${d.year}';
  }
}
