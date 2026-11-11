import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../design_system/components/app_button.dart';
import '../../../design_system/components/app_card.dart';
import '../../../design_system/theme/app_spacing.dart';
import '../../exercises/presentation/exercise_picker.dart';
import '../data/programs_repository.dart';

class _EditorExercise {
  _EditorExercise({required this.exerciseId, required this.name, this.sets = 3, this.reps = 10, this.weight, this.rest = 90});
  final String exerciseId;
  final String name;
  int sets;
  int? reps;
  double? weight;
  int rest;
}

/// Şablon oluşturma/düzenleme ekranı. [templateId] verilirse düzenleme modunda açılır.
class TemplateEditorScreen extends ConsumerStatefulWidget {
  const TemplateEditorScreen({super.key, this.templateId});

  final String? templateId;

  @override
  ConsumerState<TemplateEditorScreen> createState() => _TemplateEditorScreenState();
}

class _TemplateEditorScreenState extends ConsumerState<TemplateEditorScreen> {
  final _name = TextEditingController();
  final List<_EditorExercise> _exercises = [];
  bool _saving = false;
  bool _loading = false;

  bool get _isEdit => widget.templateId != null;

  @override
  void initState() {
    super.initState();
    if (_isEdit) _loadExisting();
  }

  Future<void> _loadExisting() async {
    setState(() => _loading = true);
    try {
      final d = await ref.read(programsRepositoryProvider).detail(widget.templateId!);
      _name.text = d.name;
      _exercises.addAll(d.exercises.map((e) => _EditorExercise(
            exerciseId: e.exerciseId,
            name: e.exerciseName,
            sets: e.targetSets,
            reps: e.targetReps,
            weight: e.targetWeight,
          )));
    } catch (_) {
      // yeni oluşturma gibi devam
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  void dispose() {
    _name.dispose();
    super.dispose();
  }

  Future<void> _addExercise() async {
    final picked = await showExercisePicker(context);
    if (picked == null) return;
    setState(() => _exercises.add(_EditorExercise(exerciseId: picked.id, name: picked.name)));
  }

  Map<String, dynamic> _body() => {
        'name': _name.text.trim(),
        'notes': null,
        'exercises': [
          for (var i = 0; i < _exercises.length; i++)
            {
              'exerciseId': _exercises[i].exerciseId,
              'orderIndex': i,
              'targetSets': _exercises[i].sets,
              'targetReps': _exercises[i].reps,
              'loadType': 'Absolute',
              'targetWeight': _exercises[i].weight,
              'percentage': null,
              'restSeconds': _exercises[i].rest,
              'note': null,
              'supersetGroupId': null,
              'progressionRule': 'None',
              'sets': null,
            },
        ],
      };

  Future<void> _save() async {
    if (_name.text.trim().isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Program adı gerekli.')));
      return;
    }
    setState(() => _saving = true);
    try {
      final repo = ref.read(programsRepositoryProvider);
      if (_isEdit) {
        await repo.update(widget.templateId!, _body());
      } else {
        await repo.create(_body());
      }
      ref.invalidate(templatesProvider);
      if (mounted) Navigator.pop(context);
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Kaydedilemedi.')));
      }
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    return Scaffold(
      appBar: AppBar(
        title: Text(_isEdit ? 'Programı Düzenle' : 'Yeni Program'),
        actions: [
          TextButton(
            onPressed: _saving ? null : _save,
            child: _saving
                ? const SizedBox(width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2))
                : const Text('Kaydet'),
          ),
        ],
      ),
      body: ListView(
        padding: const EdgeInsets.all(AppSpacing.md),
        children: [
          TextField(
            controller: _name,
            decoration: const InputDecoration(labelText: 'Program adı', border: OutlineInputBorder()),
          ),
          const SizedBox(height: AppSpacing.md),
          for (var i = 0; i < _exercises.length; i++) _exerciseCard(i),
          const SizedBox(height: AppSpacing.sm),
          AppButton(
            label: 'Hareket Ekle',
            icon: Icons.add,
            variant: AppButtonVariant.tonal,
            onPressed: _addExercise,
          ),
        ],
      ),
    );
  }

  Widget _exerciseCard(int i) {
    final e = _exercises[i];
    return Padding(
      padding: const EdgeInsets.only(bottom: AppSpacing.xs),
      child: AppCard(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(child: Text(e.name, style: Theme.of(context).textTheme.titleMedium)),
                IconButton(
                  icon: const Icon(Icons.delete_outline),
                  onPressed: () => setState(() => _exercises.removeAt(i)),
                ),
              ],
            ),
            Row(
              children: [
                _numField('Set', e.sets.toString(), (v) => e.sets = int.tryParse(v) ?? e.sets),
                const SizedBox(width: AppSpacing.xs),
                _numField('Tekrar', e.reps?.toString() ?? '', (v) => e.reps = int.tryParse(v)),
                const SizedBox(width: AppSpacing.xs),
                _numField('Kg', e.weight?.toString() ?? '', (v) => e.weight = double.tryParse(v)),
                const SizedBox(width: AppSpacing.xs),
                _numField('Dinlenme', e.rest.toString(), (v) => e.rest = int.tryParse(v) ?? e.rest),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _numField(String label, String initial, ValueChanged<String> onChanged) => Expanded(
        child: TextFormField(
          initialValue: initial,
          keyboardType: const TextInputType.numberWithOptions(decimal: true),
          inputFormatters: [FilteringTextInputFormatter.allow(RegExp(r'[0-9.]'))],
          onChanged: onChanged,
          decoration: InputDecoration(labelText: label, isDense: true, border: const OutlineInputBorder()),
        ),
      );
}
