import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/navigation.dart';
import '../../../design_system/components/app_card.dart';
import '../../../design_system/components/shimmer.dart';
import '../../../design_system/theme/app_spacing.dart';
import '../../workout/application/workout_controller.dart';
import '../../workout/domain/workout_models.dart';
import '../data/programs_repository.dart';
import '../domain/template_models.dart';
import 'template_editor_screen.dart';

class ProgramsScreen extends ConsumerWidget {
  const ProgramsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final templates = ref.watch(templatesProvider);
    final theme = Theme.of(context);

    return Scaffold(
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => Navigator.of(context).push(
            MaterialPageRoute(builder: (_) => const TemplateEditorScreen())),
        icon: const Icon(Icons.add),
        label: const Text('Yeni Program'),
      ),
      body: RefreshIndicator(
        onRefresh: () async => ref.refresh(templatesProvider.future),
        child: templates.when(
          loading: () => const SkeletonList(),
          error: (_, __) => _empty(context, 'Programlar yüklenemedi. Giriş yaptın mı?'),
          data: (list) => list.isEmpty
              ? _empty(context, 'Henüz şablon yok. Swagger veya program editöründen ekleyebilirsin.')
              : ListView(
                  padding: const EdgeInsets.all(AppSpacing.md),
                  children: [
                    Text('Programlarım', style: theme.textTheme.headlineMedium),
                    const SizedBox(height: AppSpacing.md),
                    for (final t in list)
                      Padding(
                        padding: const EdgeInsets.only(bottom: AppSpacing.xs),
                        child: AppCard(
                          onTap: () => _start(context, ref, t),
                          child: Row(
                            children: [
                              Icon(Icons.list_alt, color: theme.colorScheme.primary),
                              const SizedBox(width: AppSpacing.md),
                              Expanded(
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Text(t.name, style: theme.textTheme.titleMedium),
                                    Text('${t.exerciseCount} hareket', style: theme.textTheme.labelSmall),
                                  ],
                                ),
                              ),
                              const Icon(Icons.play_arrow),
                              PopupMenuButton<String>(
                                onSelected: (v) {
                                  if (v == 'edit') {
                                    Navigator.of(context).push(MaterialPageRoute(
                                        builder: (_) => TemplateEditorScreen(templateId: t.id)));
                                  } else if (v == 'delete') {
                                    _confirmDelete(context, ref, t);
                                  }
                                },
                                itemBuilder: (_) => const [
                                  PopupMenuItem(value: 'edit', child: Text('Düzenle')),
                                  PopupMenuItem(value: 'delete', child: Text('Sil')),
                                ],
                              ),
                            ],
                          ),
                        ),
                      ),
                  ],
                ),
        ),
      ),
    );
  }

  Future<void> _start(BuildContext context, WidgetRef ref, TemplateListItem t) async {
    showDialog<void>(
      context: context,
      barrierDismissible: false,
      builder: (_) => const Center(child: CircularProgressIndicator()),
    );
    try {
      final detail = await ref.read(programsRepositoryProvider).detail(t.id);
      final exercises = _toSession(detail);
      ref.read(workoutProvider.notifier).startWithExercises(detail.name, exercises);
      if (context.mounted) Navigator.pop(context); // dialog
      ref.read(selectedTabProvider.notifier).state = 1; // Workout sekmesi
    } catch (_) {
      if (context.mounted) {
        Navigator.pop(context);
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Antrenman başlatılamadı.')),
        );
      }
    }
  }

  Future<void> _confirmDelete(BuildContext context, WidgetRef ref, TemplateListItem t) async {
    final ok = await showDialog<bool>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('Programı sil'),
        content: Text('"${t.name}" silinsin mi?'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('Vazgeç')),
          TextButton(onPressed: () => Navigator.pop(context, true), child: const Text('Sil')),
        ],
      ),
    );
    if (ok != true) return;
    try {
      await ref.read(programsRepositoryProvider).remove(t.id);
      ref.invalidate(templatesProvider);
    } catch (_) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Silinemedi.')));
      }
    }
  }

  List<SessionExerciseModel> _toSession(TemplateDetail d) {
    return [
      for (final ex in d.exercises)
        SessionExerciseModel(
          exerciseId: ex.exerciseId,
          exerciseName: ex.exerciseName,
          muscleGroup: '',
          sets: ex.sets.isNotEmpty
              ? [for (final s in ex.sets) WorkoutSetModel(setNumber: s.setNumber, weight: s.weight, reps: s.reps)]
              : [
                  for (var i = 1; i <= ex.targetSets; i++)
                    WorkoutSetModel(setNumber: i, weight: ex.targetWeight, reps: ex.targetReps),
                ],
        ),
    ];
  }

  Widget _empty(BuildContext context, String msg) => ListView(
        children: [
          const SizedBox(height: 140),
          Icon(Icons.calendar_month, size: 48, color: Theme.of(context).colorScheme.primary),
          const SizedBox(height: AppSpacing.md),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: AppSpacing.xl),
            child: Text(msg, textAlign: TextAlign.center, style: Theme.of(context).textTheme.bodyMedium),
          ),
        ],
      );
}
