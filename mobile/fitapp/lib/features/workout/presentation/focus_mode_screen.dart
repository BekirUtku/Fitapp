import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../design_system/components/app_button.dart';
import '../../../design_system/theme/app_spacing.dart';
import '../../../core/sync/sync_engine.dart';
import '../../exercises/presentation/exercise_picker.dart';
import '../application/rest_timer_controller.dart';
import '../application/workout_controller.dart';
import '../data/session_repository.dart';
import '../domain/workout_models.dart';
import 'widgets/exercise_card.dart';
import 'widgets/rest_timer_bar.dart';
import 'workout_summary_screen.dart';

/// Uygulamanın kalbi. Faz 3'te bellek içi state ile çalışır (Faz 4'te Hive+sync).
class FocusModeScreen extends ConsumerStatefulWidget {
  const FocusModeScreen({super.key});

  @override
  ConsumerState<FocusModeScreen> createState() => _FocusModeScreenState();
}

class _FocusModeScreenState extends ConsumerState<FocusModeScreen>
    with WidgetsBindingObserver {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    // Öne geldiğinde dinlenme sayacını endTime üzerinden yeniden hesapla.
    if (state == AppLifecycleState.resumed) {
      ref.read(restTimerProvider.notifier).resync();
    }
  }

  @override
  Widget build(BuildContext context) {
    final session = ref.watch(workoutProvider);
    return session == null ? const _EmptyState() : _ActiveSession(session: session);
  }
}

class _EmptyState extends ConsumerWidget {
  const _EmptyState();

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    return Padding(
      padding: const EdgeInsets.all(AppSpacing.lg),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.fitness_center, size: 56, color: theme.colorScheme.primary),
          const SizedBox(height: AppSpacing.md),
          Text('Antrenmana hazır mısın?', style: theme.textTheme.headlineSmall),
          const SizedBox(height: AppSpacing.xs),
          Text('Programından başlat ya da serbest antrenman aç.',
              textAlign: TextAlign.center, style: theme.textTheme.bodyMedium),
          const SizedBox(height: AppSpacing.lg),
          AppButton(
            label: 'Manuel Antrenman Başlat',
            icon: Icons.play_arrow,
            onPressed: () => ref.read(workoutProvider.notifier).startManual(),
          ),
        ],
      ),
    );
  }
}

class _ActiveSession extends ConsumerWidget {
  const _ActiveSession({required this.session});

  final WorkoutSessionModel session;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final ctrl = ref.read(workoutProvider.notifier);

    return Column(
      children: [
        const RestTimerBar(),
        Padding(
          padding: const EdgeInsets.fromLTRB(AppSpacing.md, AppSpacing.sm, AppSpacing.md, 0),
          child: Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(session.name, style: theme.textTheme.headlineSmall),
                    Text('${session.totalSets} set · ${session.totalVolume.toStringAsFixed(0)} kg hacim',
                        style: theme.textTheme.bodyMedium),
                  ],
                ),
              ),
              TextButton(
                onPressed: () => _confirmFinish(context, ref),
                child: const Text('Bitir'),
              ),
            ],
          ),
        ),
        Expanded(
          child: session.exercises.isEmpty
              ? Center(
                  child: Text('Henüz hareket yok. Aşağıdan ekle.',
                      style: theme.textTheme.bodyMedium),
                )
              : ListView.separated(
                  padding: const EdgeInsets.all(AppSpacing.md),
                  itemCount: session.exercises.length,
                  separatorBuilder: (_, __) => const SizedBox(height: AppSpacing.sm),
                  itemBuilder: (_, i) => ExerciseCard(exercise: session.exercises[i]),
                ),
        ),
        SafeArea(
          top: false,
          child: Padding(
            padding: const EdgeInsets.all(AppSpacing.md),
            child: AppButton(
              label: 'Hareket Ekle',
              icon: Icons.add,
              variant: AppButtonVariant.tonal,
              onPressed: () => _addExercise(context, ctrl),
            ),
          ),
        ),
      ],
    );
  }

  Future<void> _addExercise(BuildContext context, WorkoutController ctrl) async {
    final picked = await showExercisePicker(context);
    if (picked == null) return;
    ctrl.addExercise(
      exerciseId: picked.id,
      name: picked.name,
      muscleGroup: picked.muscleGroupName,
    );
  }

  void _confirmFinish(BuildContext context, WidgetRef ref) {
    showModalBottomSheet<void>(
      context: context,
      builder: (_) => SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.lg),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text('Antrenmanı bitir', style: Theme.of(context).textTheme.titleLarge),
              const SizedBox(height: AppSpacing.md),
              AppButton(
                label: 'Bitir ve Kaydet',
                icon: Icons.check,
                onPressed: () async {
                  // Önce Hive'a yaz + outbox'a ekle (local-first), sonra sync dene.
                  await ref.read(sessionRepositoryProvider).saveFinished(session);
                  final finished = session;
                  ref.read(workoutProvider.notifier).finish();
                  unawaited(ref.read(syncEngineProvider).sync());
                  if (!context.mounted) return;
                  Navigator.pop(context); // bottom sheet
                  Navigator.of(context).push(MaterialPageRoute(
                    builder: (_) => WorkoutSummaryScreen(session: finished),
                  ));
                },
              ),
              const SizedBox(height: AppSpacing.xs),
              AppButton(
                label: 'Vazgeç',
                variant: AppButtonVariant.text,
                onPressed: () => Navigator.pop(context),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
