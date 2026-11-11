import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../design_system/components/app_button.dart';
import '../../../design_system/components/app_card.dart';
import '../../../design_system/components/line_chart_card.dart';
import '../../../design_system/components/shimmer.dart';
import '../../../design_system/theme/app_spacing.dart';
import '../data/measurements_repository.dart';

class MeasurementsScreen extends ConsumerWidget {
  const MeasurementsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(measurementsProvider);
    return Scaffold(
      appBar: AppBar(title: const Text('Vücut Ölçüleri')),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _addSheet(context, ref),
        icon: const Icon(Icons.add),
        label: const Text('Ölçüm Ekle'),
      ),
      body: state.when(
        loading: () => const SkeletonList(count: 4),
        error: (_, __) => const Center(child: Text('Yüklenemedi')),
        data: (list) {
          final weights = list
              .where((m) => m.bodyweight != null)
              .toList()
              .reversed // eskiden yeniye
              .map((m) => m.bodyweight!)
              .toList();

          return ListView(
            padding: const EdgeInsets.all(AppSpacing.md),
            children: [
              LineChartCard(title: 'Kilo Trendi', values: weights, unit: 'kg'),
              const SizedBox(height: AppSpacing.md),
              Text('Kayıtlar', style: Theme.of(context).textTheme.titleMedium),
              const SizedBox(height: AppSpacing.sm),
              if (list.isEmpty)
                Text('Henüz ölçüm yok.', style: Theme.of(context).textTheme.bodyMedium),
              for (final m in list)
                Padding(
                  padding: const EdgeInsets.only(bottom: AppSpacing.xs),
                  child: AppCard(
                    child: Row(
                      children: [
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(m.bodyweight != null ? '${m.bodyweight} kg' : 'Ölçüm',
                                  style: Theme.of(context).textTheme.titleMedium),
                              Text(_date(m.measuredAt), style: Theme.of(context).textTheme.labelSmall),
                              if (m.note != null && m.note!.isNotEmpty)
                                Text(m.note!, style: Theme.of(context).textTheme.bodyMedium),
                            ],
                          ),
                        ),
                        IconButton(
                          icon: const Icon(Icons.delete_outline),
                          onPressed: () async {
                            await ref.read(measurementsRepositoryProvider).remove(m.id);
                            ref.invalidate(measurementsProvider);
                          },
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

  String _date(DateTime d) => '${d.day}.${d.month}.${d.year}';

  void _addSheet(BuildContext context, WidgetRef ref) {
    final weight = TextEditingController();
    final note = TextEditingController();
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
            Text('Yeni Ölçüm', style: Theme.of(context).textTheme.titleLarge),
            const SizedBox(height: AppSpacing.md),
            TextField(
              controller: weight,
              keyboardType: const TextInputType.numberWithOptions(decimal: true),
              inputFormatters: [FilteringTextInputFormatter.allow(RegExp(r'[0-9.]'))],
              decoration: const InputDecoration(labelText: 'Kilo (kg)', border: OutlineInputBorder()),
            ),
            const SizedBox(height: AppSpacing.xs),
            TextField(
              controller: note,
              decoration: const InputDecoration(labelText: 'Not (opsiyonel)', border: OutlineInputBorder()),
            ),
            const SizedBox(height: AppSpacing.md),
            AppButton(
              label: 'Kaydet',
              icon: Icons.check,
              onPressed: () async {
                await ref.read(measurementsRepositoryProvider).add(
                      bodyweight: double.tryParse(weight.text),
                      note: note.text.trim().isEmpty ? null : note.text.trim(),
                    );
                ref.invalidate(measurementsProvider);
                if (context.mounted) Navigator.pop(context);
              },
            ),
          ],
        ),
      ),
    );
  }
}
