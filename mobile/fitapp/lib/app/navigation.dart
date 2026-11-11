import 'package:flutter_riverpod/flutter_riverpod.dart';

/// Alt sekme index'i. Programlar ekranından antrenman başlatınca Workout'a geçmek için paylaşılır.
final selectedTabProvider = StateProvider<int>((_) => 0);
