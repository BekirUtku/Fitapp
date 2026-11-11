import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../design_system/theme/app_spacing.dart';
import '../data/exercises_repository.dart';
import '../domain/exercise_models.dart';

/// Hareket havuzundan seçim yapılan bottom sheet. Seçilen hareketi döner.
Future<ExerciseListItem?> showExercisePicker(BuildContext context) {
  return showModalBottomSheet<ExerciseListItem>(
    context: context,
    isScrollControlled: true,
    builder: (_) => const FractionallySizedBox(
      heightFactor: 0.85,
      child: _ExercisePickerSheet(),
    ),
  );
}

class _ExercisePickerSheet extends ConsumerStatefulWidget {
  const _ExercisePickerSheet();

  @override
  ConsumerState<_ExercisePickerSheet> createState() => _ExercisePickerSheetState();
}

class _ExercisePickerSheetState extends ConsumerState<_ExercisePickerSheet> {
  final _search = TextEditingController();
  String? _muscleGroupId;
  Timer? _debounce;
  late Future<List<ExerciseListItem>> _future;

  @override
  void initState() {
    super.initState();
    _future = _load();
  }

  Future<List<ExerciseListItem>> _load() =>
      ref.read(exercisesRepositoryProvider).list(search: _search.text, muscleGroupId: _muscleGroupId);

  void _reload() => setState(() => _future = _load());

  void _onSearch(String _) {
    _debounce?.cancel();
    _debounce = Timer(const Duration(milliseconds: 300), _reload);
  }

  @override
  void dispose() {
    _debounce?.cancel();
    _search.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final groups = ref.watch(muscleGroupsProvider);

    return Padding(
      padding: EdgeInsets.only(bottom: MediaQuery.of(context).viewInsets.bottom),
      child: Column(
        children: [
          const SizedBox(height: AppSpacing.sm),
          Container(width: 40, height: 4, decoration: BoxDecoration(
              color: theme.dividerColor, borderRadius: BorderRadius.circular(2))),
          Padding(
            padding: const EdgeInsets.all(AppSpacing.md),
            child: TextField(
              controller: _search,
              onChanged: _onSearch,
              autofocus: true,
              decoration: const InputDecoration(
                hintText: 'Hareket ara...',
                prefixIcon: Icon(Icons.search),
                border: OutlineInputBorder(),
              ),
            ),
          ),
          groups.maybeWhen(
            data: (list) => SizedBox(
              height: 40,
              child: ListView(
                scrollDirection: Axis.horizontal,
                padding: const EdgeInsets.symmetric(horizontal: AppSpacing.md),
                children: [
                  _chip('Tümü', _muscleGroupId == null, () {
                    setState(() => _muscleGroupId = null);
                    _reload();
                  }),
                  for (final g in list)
                    _chip(g.name, _muscleGroupId == g.id, () {
                      setState(() => _muscleGroupId = g.id);
                      _reload();
                    }),
                ],
              ),
            ),
            orElse: () => const SizedBox.shrink(),
          ),
          const Divider(height: AppSpacing.md),
          Expanded(
            child: FutureBuilder<List<ExerciseListItem>>(
              future: _future,
              builder: (context, snap) {
                if (snap.connectionState == ConnectionState.waiting) {
                  return const Center(child: CircularProgressIndicator());
                }
                final items = snap.data ?? const [];
                if (items.isEmpty) {
                  return Center(child: Text('Hareket bulunamadı', style: theme.textTheme.bodyMedium));
                }
                return ListView.builder(
                  itemCount: items.length,
                  itemBuilder: (_, i) {
                    final e = items[i];
                    return ListTile(
                      title: Text(e.name),
                      subtitle: Text(e.muscleGroupName),
                      trailing: e.isCustom
                          ? const Icon(Icons.person, size: 16)
                          : const Icon(Icons.chevron_right),
                      onTap: () => Navigator.pop(context, e),
                    );
                  },
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _chip(String label, bool selected, VoidCallback onTap) => Padding(
        padding: const EdgeInsets.only(right: AppSpacing.xs),
        child: ChoiceChip(label: Text(label), selected: selected, onSelected: (_) => onTap()),
      );
}
