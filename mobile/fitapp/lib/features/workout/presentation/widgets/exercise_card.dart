import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../design_system/components/app_button.dart';
import '../../../../design_system/components/app_card.dart';
import '../../../../design_system/theme/app_spacing.dart';
import '../../application/workout_controller.dart';
import '../../domain/workout_models.dart';
import 'set_row.dart';

class ExerciseCard extends ConsumerWidget {
  const ExerciseCard({super.key, required this.exercise});

  final SessionExerciseModel exercise;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final ctrl = ref.read(workoutProvider.notifier);

    return AppCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(exercise.exerciseName, style: theme.textTheme.titleLarge),
                    Text(exercise.muscleGroup,
                        style: theme.textTheme.labelSmall
                            ?.copyWith(color: theme.colorScheme.primary)),
                  ],
                ),
              ),
              if (exercise.supersetGroupId != null)
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                  decoration: BoxDecoration(
                    color: theme.colorScheme.secondary.withOpacity(0.15),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Text('Superset', style: theme.textTheme.labelSmall),
                ),
            ],
          ),
          const SizedBox(height: AppSpacing.sm),
          ...exercise.sets.map((s) => SetRow(
                key: ValueKey(s.id),
                set: s,
                onWeight: (v) => ctrl.updateSet(exercise.id, s.id, weight: v),
                onReps: (v) => ctrl.updateSet(exercise.id, s.id, reps: v),
                onToggle: () => ctrl.toggleComplete(exercise.id, s.id),
              )),
          const SizedBox(height: AppSpacing.xs),
          Row(
            children: [
              Expanded(
                child: AppButton(
                  label: 'Set Ekle',
                  icon: Icons.add,
                  variant: AppButtonVariant.tonal,
                  onPressed: () => ctrl.addSet(exercise.id),
                ),
              ),
              const SizedBox(width: AppSpacing.xs),
              Expanded(
                child: AppButton(
                  label: 'Tümünü Onayla',
                  icon: Icons.done_all,
                  variant: AppButtonVariant.outlined,
                  onPressed: () => ctrl.completeAll(exercise.id),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
