import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../design_system/components/app_button.dart';
import '../../../design_system/components/app_card.dart';
import '../../../design_system/components/shimmer.dart';
import '../../../design_system/theme/app_colors.dart';
import '../../../design_system/theme/app_spacing.dart';
import '../application/nutrition_controller.dart';
import '../domain/nutrition_models.dart';

class NutritionScreen extends ConsumerWidget {
  const NutritionScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(nutritionControllerProvider);
    return Scaffold(
      appBar: AppBar(title: const Text('Beslenme')),
      body: state.when(
        loading: () => const SkeletonList(count: 3),
        error: (e, _) => Center(child: Text('Yüklenemedi: $e')),
        data: (s) => _Content(summary: s),
      ),
    );
  }
}

class _Content extends ConsumerWidget {
  const _Content({required this.summary});

  final NutritionSummary summary;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final ctrl = ref.read(nutritionControllerProvider.notifier);
    final log = summary.log;
    final target = summary.target;

    return ListView(
      padding: const EdgeInsets.all(AppSpacing.md),
      children: [
        AppCard(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text('Makrolar', style: Theme.of(context).textTheme.titleLarge),
                  TextButton(
                    onPressed: () => _editMacros(context, ctrl, log),
                    child: const Text('Düzenle'),
                  ),
                ],
              ),
              const SizedBox(height: AppSpacing.sm),
              _MacroBar(label: 'Kalori', value: log.calories, target: target?.calories, unit: 'kcal', color: AppColors.primary),
              _MacroBar(label: 'Protein', value: log.proteinG, target: target?.proteinG, unit: 'g', color: AppColors.accent),
              _MacroBar(label: 'Karbonhidrat', value: log.carbsG, target: target?.carbsG, unit: 'g', color: AppColors.warning),
              _MacroBar(label: 'Yağ', value: log.fatG, target: target?.fatG, unit: 'g', color: AppColors.danger),
              if (target == null)
                Padding(
                  padding: const EdgeInsets.only(top: AppSpacing.sm),
                  child: AppButton(
                    label: 'Hedef Belirle',
                    variant: AppButtonVariant.tonal,
                    onPressed: () => _editTarget(context, ctrl, log),
                  ),
                ),
            ],
          ),
        ),
        const SizedBox(height: AppSpacing.md),
        _WaterCard(current: log.waterMl, target: target?.waterMl, onAdd: ctrl.addWater),
      ],
    );
  }

  void _editMacros(BuildContext context, NutritionController ctrl, MacroSet current) =>
      _macroSheet(context, 'Makroları Gir', current, (m) => ctrl.saveLog(m));

  void _editTarget(BuildContext context, NutritionController ctrl, MacroSet current) =>
      _macroSheet(context, 'Günlük Hedef', current, (m) => ctrl.saveTarget(m));

  void _macroSheet(BuildContext context, String title, MacroSet initial, Future<void> Function(MacroSet) onSave) {
    final cal = TextEditingController(text: initial.calories == 0 ? '' : '${initial.calories}');
    final pro = TextEditingController(text: initial.proteinG == 0 ? '' : '${initial.proteinG}');
    final car = TextEditingController(text: initial.carbsG == 0 ? '' : '${initial.carbsG}');
    final fat = TextEditingController(text: initial.fatG == 0 ? '' : '${initial.fatG}');

    showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      builder: (_) => Padding(
        padding: EdgeInsets.only(
          bottom: MediaQuery.of(context).viewInsets.bottom + AppSpacing.lg,
          left: AppSpacing.lg, right: AppSpacing.lg, top: AppSpacing.lg,
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(title, style: Theme.of(context).textTheme.titleLarge),
            const SizedBox(height: AppSpacing.md),
            _numField(cal, 'Kalori (kcal)'),
            _numField(pro, 'Protein (g)'),
            _numField(car, 'Karbonhidrat (g)'),
            _numField(fat, 'Yağ (g)'),
            const SizedBox(height: AppSpacing.md),
            AppButton(
              label: 'Kaydet',
              icon: Icons.check,
              onPressed: () async {
                await onSave(MacroSet(
                  calories: int.tryParse(cal.text) ?? 0,
                  proteinG: int.tryParse(pro.text) ?? 0,
                  carbsG: int.tryParse(car.text) ?? 0,
                  fatG: int.tryParse(fat.text) ?? 0,
                  waterMl: initial.waterMl,
                ));
                if (context.mounted) Navigator.pop(context);
              },
            ),
          ],
        ),
      ),
    );
  }

  Widget _numField(TextEditingController c, String label) => Padding(
        padding: const EdgeInsets.only(bottom: AppSpacing.xs),
        child: TextField(
          controller: c,
          keyboardType: TextInputType.number,
          inputFormatters: [FilteringTextInputFormatter.digitsOnly],
          decoration: InputDecoration(labelText: label, border: const OutlineInputBorder()),
        ),
      );
}

class _MacroBar extends StatelessWidget {
  const _MacroBar({
    required this.label,
    required this.value,
    required this.target,
    required this.unit,
    required this.color,
  });

  final String label;
  final int value;
  final int? target;
  final String unit;
  final Color color;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final ratio = (target == null || target == 0) ? 0.0 : (value / target!).clamp(0.0, 1.0);
    final text = target == null ? '$value $unit' : '$value / $target $unit';

    return Padding(
      padding: const EdgeInsets.symmetric(vertical: AppSpacing.xs),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(label, style: theme.textTheme.bodyMedium),
              Text(text, style: theme.textTheme.labelLarge),
            ],
          ),
          const SizedBox(height: AppSpacing.xxs),
          ClipRRect(
            borderRadius: BorderRadius.circular(8),
            child: LinearProgressIndicator(
              value: ratio,
              minHeight: 8,
              backgroundColor: theme.dividerColor,
              color: color,
            ),
          ),
        ],
      ),
    );
  }
}

class _WaterCard extends StatelessWidget {
  const _WaterCard({required this.current, required this.target, required this.onAdd});

  final int current;
  final int? target;
  final Future<void> Function(int) onAdd;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final ratio = (target == null || target == 0) ? 0.0 : (current / target!).clamp(0.0, 1.0);

    return AppCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text('Su', style: theme.textTheme.titleLarge),
              Text(target == null ? '$current ml' : '$current / $target ml',
                  style: theme.textTheme.titleMedium?.copyWith(color: theme.colorScheme.primary)),
            ],
          ),
          const SizedBox(height: AppSpacing.sm),
          ClipRRect(
            borderRadius: BorderRadius.circular(8),
            child: LinearProgressIndicator(value: ratio, minHeight: 8, backgroundColor: theme.dividerColor),
          ),
          const SizedBox(height: AppSpacing.sm),
          Row(
            children: [
              Expanded(
                child: AppButton(
                  label: '+250 ml',
                  variant: AppButtonVariant.tonal,
                  onPressed: () => onAdd(250),
                ),
              ),
              const SizedBox(width: AppSpacing.xs),
              Expanded(
                child: AppButton(
                  label: '+500 ml',
                  variant: AppButtonVariant.tonal,
                  onPressed: () => onAdd(500),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
