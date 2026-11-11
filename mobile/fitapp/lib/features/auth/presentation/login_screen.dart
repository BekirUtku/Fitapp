import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../design_system/components/app_button.dart';
import '../../../design_system/components/fade_in.dart';
import '../../../design_system/theme/app_colors.dart';
import '../../../design_system/theme/app_spacing.dart';
import '../application/auth_controller.dart';

class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({super.key});

  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  final _email = TextEditingController();
  final _password = TextEditingController();
  final _name = TextEditingController();
  bool _registerMode = false;

  @override
  void dispose() {
    _email.dispose();
    _password.dispose();
    _name.dispose();
    super.dispose();
  }

  void _submit() {
    final ctrl = ref.read(authControllerProvider.notifier);
    if (_registerMode) {
      ctrl.register(_email.text, _password.text, _name.text);
    } else {
      ctrl.login(_email.text, _password.text);
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final state = ref.watch(authControllerProvider);
    final loading = state.isLoading;

    ref.listen(authControllerProvider, (_, next) {
      if (next.hasError) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(_errorText(next.error))),
        );
      }
    });

    return Scaffold(
      body: Stack(
        children: [
          // Enerjik arka plan parıltısı
          Positioned(
            top: -120,
            right: -80,
            child: _glow(AppColors.primary.withOpacity(0.35), 320),
          ),
          Positioned(
            bottom: -140,
            left: -90,
            child: _glow(AppColors.accent.withOpacity(0.18), 300),
          ),
          SafeArea(
            child: Center(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(AppSpacing.lg),
                child: FadeIn(
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      Center(
                        child: Container(
                          width: 84,
                          height: 84,
                          decoration: BoxDecoration(
                            gradient: AppColors.brandGradient,
                            borderRadius: BorderRadius.circular(24),
                            boxShadow: [
                              BoxShadow(
                                  color: AppColors.primary.withOpacity(0.5),
                                  blurRadius: 30,
                                  offset: const Offset(0, 12)),
                            ],
                          ),
                          child: const Icon(Icons.fitness_center, size: 44, color: Colors.white),
                        ),
                      ),
                      const SizedBox(height: AppSpacing.lg),
                      Text('FitApp', textAlign: TextAlign.center, style: theme.textTheme.displaySmall),
                      const SizedBox(height: AppSpacing.xs),
                      Text(
                        _registerMode
                            ? 'Hesabını oluştur, hemen başla.'
                            : 'Antrenmanını kaydet · gelişimini gör · rekor kır.',
                        textAlign: TextAlign.center,
                        style: theme.textTheme.bodyMedium?.copyWith(color: theme.colorScheme.onSurfaceVariant),
                      ),
                      const SizedBox(height: AppSpacing.xl),
                      if (_registerMode) ...[
                        _field(_name, 'Ad', Icons.person_outline),
                        const SizedBox(height: AppSpacing.sm),
                      ],
                      _field(_email, 'E-posta', Icons.mail_outline,
                          keyboard: TextInputType.emailAddress),
                      const SizedBox(height: AppSpacing.sm),
                      _field(_password, 'Şifre', Icons.lock_outline, obscure: true),
                      const SizedBox(height: AppSpacing.lg),
                      AppButton(
                        label: _registerMode ? 'Kayıt Ol' : 'Giriş Yap',
                        icon: Icons.arrow_forward,
                        loading: loading,
                        gradient: true,
                        onPressed: _submit,
                      ),
                      const SizedBox(height: AppSpacing.xs),
                      AppButton(
                        label: _registerMode
                            ? 'Zaten hesabın var mı? Giriş yap'
                            : 'Hesabın yok mu? Kayıt ol',
                        variant: AppButtonVariant.text,
                        onPressed: loading ? null : () => setState(() => _registerMode = !_registerMode),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _glow(Color color, double size) => Container(
        width: size,
        height: size,
        decoration: BoxDecoration(
          shape: BoxShape.circle,
          gradient: RadialGradient(colors: [color, color.withOpacity(0)]),
        ),
      );

  Widget _field(TextEditingController c, String label, IconData icon,
          {bool obscure = false, TextInputType? keyboard}) =>
      TextField(
        controller: c,
        obscureText: obscure,
        keyboardType: keyboard,
        decoration: InputDecoration(labelText: label, prefixIcon: Icon(icon)),
      );

  String _errorText(Object? error) {
    if (error is DioException) {
      final data = error.response?.data;
      if (data is Map) {
        final errors = data['errors'];
        if (errors is Map && errors.isNotEmpty) {
          final first = errors.values.first;
          if (first is List && first.isNotEmpty) return first.first.toString();
        }
        if (error.response?.statusCode == 401) return 'E-posta veya şifre hatalı.';
        if (data['title'] is String) return data['title'] as String;
      }
      if (error.response?.statusCode == 401) return 'E-posta veya şifre hatalı.';
      return 'Bağlantı hatası. Sunucu çalışıyor mu?';
    }
    return 'Bir hata oluştu.';
  }
}
