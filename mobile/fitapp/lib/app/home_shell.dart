import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../features/dashboard/presentation/dashboard_screen.dart';
import '../features/profile/presentation/profile_screen.dart';
import '../features/programs/presentation/programs_screen.dart';
import '../features/workout/presentation/focus_mode_screen.dart';
import 'navigation.dart';

/// 4 ana sekmeli kabuk. Sekme index'i paylaşılan provider üzerinden yönetilir.
class HomeShell extends ConsumerWidget {
  const HomeShell({super.key});

  static const _tabs = [
    DashboardScreen(),
    FocusModeScreen(),
    ProgramsScreen(),
    ProfileScreen(),
  ];

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final index = ref.watch(selectedTabProvider);

    return Scaffold(
      body: SafeArea(child: _tabs[index]),
      bottomNavigationBar: NavigationBar(
        selectedIndex: index,
        onDestinationSelected: (i) => ref.read(selectedTabProvider.notifier).state = i,
        destinations: const [
          NavigationDestination(icon: Icon(Icons.dashboard_outlined), label: 'Dashboard'),
          NavigationDestination(icon: Icon(Icons.fitness_center_outlined), label: 'Workout'),
          NavigationDestination(icon: Icon(Icons.calendar_month_outlined), label: 'Programs'),
          NavigationDestination(icon: Icon(Icons.person_outline), label: 'Profile'),
        ],
      ),
    );
  }
}
