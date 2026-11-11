import 'package:flutter/material.dart';

import '../../../design_system/theme/app_spacing.dart';
import '../domain/gamification_models.dart';

/// GitHub katkı takvimine benzer seri görselleştirmesi: her gün bir kutucuk.
class StreakCalendar extends StatelessWidget {
  const StreakCalendar({super.key, required this.days});

  final List<StreakDay> days;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    // Haftalara böl (7'şerli sütunlar).
    final weeks = <List<StreakDay>>[];
    for (var i = 0; i < days.length; i += 7) {
      weeks.add(days.sublist(i, (i + 7).clamp(0, days.length)));
    }

    Color cellColor(int count) {
      if (count <= 0) return theme.dividerColor.withOpacity(0.4);
      final opacity = (0.35 + 0.22 * count).clamp(0.35, 1.0);
      return theme.colorScheme.primary.withOpacity(opacity);
    }

    return SingleChildScrollView(
      scrollDirection: Axis.horizontal,
      reverse: true,
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          for (final week in weeks)
            Column(
              children: [
                for (final day in week)
                  Container(
                    width: 14,
                    height: 14,
                    margin: const EdgeInsets.all(2),
                    decoration: BoxDecoration(
                      color: cellColor(day.count),
                      borderRadius: BorderRadius.circular(3),
                    ),
                  ),
              ],
            ),
        ],
      ),
    );
  }
}
