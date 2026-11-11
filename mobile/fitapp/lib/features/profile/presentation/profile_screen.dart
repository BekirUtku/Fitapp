import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../design_system/components/app_button.dart';
import '../../../design_system/components/app_card.dart';
import '../../../design_system/theme/app_spacing.dart';
import '../../auth/application/auth_controller.dart';
import '../../export/presentation/export_screen.dart';
import '../../gamification/presentation/gamification_screen.dart';
import '../../insights/presentation/insights_screen.dart';
import '../../nutrition/presentation/nutrition_screen.dart';
import '../../progress/presentation/measurements_screen.dart';
import '../../progress/presentation/records_screen.dart';

/// Profil menüsü. Bölümler ilgili fazlarda dolduruluyor.
class ProfileScreen extends ConsumerWidget {
  const ProfileScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final items = <(IconData, String, Widget?)>[
      (Icons.restaurant, 'Beslenme', const NutritionScreen()),
      (Icons.straighten, 'Vücut Ölçüleri', const MeasurementsScreen()),
      (Icons.emoji_events, 'Kişisel Rekorlar', const RecordsScreen()),
      (Icons.insights, 'İçgörüler', const InsightsScreen()),
      (Icons.local_fire_department, 'Seri & Rozetler', const GamificationScreen()),
      (Icons.ios_share, 'Dışa Aktar', const ExportScreen()),
    ];

    return ListView(
      padding: const EdgeInsets.all(AppSpacing.md),
      children: [
        Text('Profil', style: Theme.of(context).textTheme.headlineMedium),
        const SizedBox(height: AppSpacing.md),
        for (final (icon, title, screen) in items)
          Padding(
            padding: const EdgeInsets.only(bottom: AppSpacing.xs),
            child: AppCard(
              onTap: screen == null
                  ? null
                  : () => Navigator.of(context).push(MaterialPageRoute(builder: (_) => screen)),
              child: Row(
                children: [
                  Icon(icon, color: Theme.of(context).colorScheme.primary),
                  const SizedBox(width: AppSpacing.md),
                  Expanded(child: Text(title, style: Theme.of(context).textTheme.titleMedium)),
                  Icon(screen == null ? Icons.lock_outline : Icons.chevron_right,
                      color: Theme.of(context).dividerColor),
                ],
              ),
            ),
          ),
        const SizedBox(height: AppSpacing.lg),
        AppButton(
          label: 'Çıkış Yap',
          icon: Icons.logout,
          variant: AppButtonVariant.outlined,
          onPressed: () => ref.read(authControllerProvider.notifier).logout(),
        ),
      ],
    );
  }
}
