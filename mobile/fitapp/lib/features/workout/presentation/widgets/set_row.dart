import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../../../../design_system/theme/app_colors.dart';
import '../../../../design_system/theme/app_spacing.dart';
import '../../domain/workout_models.dart';

/// Tek bir set satırı: [set no] [Kg] [Tekrar] [✓].
/// Placeholder olarak önceki değer gösterilir; kullanıcı dokunmadan ✓ ile geçebilir.
class SetRow extends StatefulWidget {
  const SetRow({
    super.key,
    required this.set,
    required this.onWeight,
    required this.onReps,
    required this.onToggle,
  });

  final WorkoutSetModel set;
  final ValueChanged<double?> onWeight;
  final ValueChanged<int?> onReps;
  final VoidCallback onToggle;

  @override
  State<SetRow> createState() => _SetRowState();
}

class _SetRowState extends State<SetRow> {
  late final TextEditingController _weight =
      TextEditingController(text: widget.set.weight?.toString() ?? '');
  late final TextEditingController _reps =
      TextEditingController(text: widget.set.reps?.toString() ?? '');

  @override
  void dispose() {
    _weight.dispose();
    _reps.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final done = widget.set.isCompleted;
    final rowColor = done ? AppColors.success.withOpacity(0.12) : Colors.transparent;

    return Container(
      decoration: BoxDecoration(
        color: rowColor,
        borderRadius: BorderRadius.circular(10),
      ),
      padding: const EdgeInsets.symmetric(horizontal: AppSpacing.xs, vertical: AppSpacing.xxs),
      margin: const EdgeInsets.only(bottom: AppSpacing.xxs),
      child: Row(
        children: [
          SizedBox(
            width: 34,
            child: Text('${widget.set.setNumber}',
                textAlign: TextAlign.center,
                style: theme.textTheme.titleMedium
                    ?.copyWith(color: theme.colorScheme.primary)),
          ),
          const SizedBox(width: AppSpacing.xs),
          Expanded(child: _numberField(_weight, 'kg', (v) => widget.onWeight(double.tryParse(v)))),
          const SizedBox(width: AppSpacing.xs),
          Expanded(child: _numberField(_reps, 'tekrar', (v) => widget.onReps(int.tryParse(v)))),
          const SizedBox(width: AppSpacing.xs),
          _CheckButton(done: done, onTap: widget.onToggle),
        ],
      ),
    );
  }

  Widget _numberField(TextEditingController c, String hint, ValueChanged<String> onChanged) {
    return TextField(
      controller: c,
      onChanged: onChanged,
      keyboardType: const TextInputType.numberWithOptions(decimal: true),
      inputFormatters: [FilteringTextInputFormatter.allow(RegExp(r'[0-9.,]'))],
      textAlign: TextAlign.center,
      decoration: InputDecoration(
        hintText: hint,
        isDense: true,
        filled: true,
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: BorderSide.none,
        ),
        contentPadding: const EdgeInsets.symmetric(vertical: 12),
      ),
    );
  }
}

class _CheckButton extends StatelessWidget {
  const _CheckButton({required this.done, required this.onTap});

  final bool done;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 180),
        width: 48,
        height: 48,
        decoration: BoxDecoration(
          color: done ? AppColors.success : Colors.transparent,
          border: Border.all(
            color: done ? AppColors.success : Theme.of(context).dividerColor,
            width: 2,
          ),
          borderRadius: BorderRadius.circular(12),
        ),
        child: Icon(Icons.check,
            color: done ? Colors.white : Theme.of(context).dividerColor),
      ),
    );
  }
}
