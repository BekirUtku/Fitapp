import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../data/nutrition_repository.dart';
import '../domain/nutrition_models.dart';

final nutritionControllerProvider =
    AsyncNotifierProvider<NutritionController, NutritionSummary>(NutritionController.new);

class NutritionController extends AsyncNotifier<NutritionSummary> {
  NutritionRepository get _repo => ref.read(nutritionRepositoryProvider);

  @override
  Future<NutritionSummary> build() => _repo.summary();

  Future<void> addWater(int ml) async {
    await _repo.addWater(ml);
    await _reload();
  }

  Future<void> saveLog(MacroSet macros) async {
    await _repo.saveLog(macros);
    await _reload();
  }

  Future<void> saveTarget(MacroSet target) async {
    await _repo.setTarget(target);
    await _reload();
  }

  Future<void> _reload() async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(_repo.summary);
  }
}
