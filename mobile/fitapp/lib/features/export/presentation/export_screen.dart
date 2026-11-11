import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:share_plus/share_plus.dart';

import '../../../design_system/components/app_card.dart';
import '../../../design_system/theme/app_spacing.dart';
import '../data/export_repository.dart';

class ExportScreen extends ConsumerStatefulWidget {
  const ExportScreen({super.key});

  @override
  ConsumerState<ExportScreen> createState() => _ExportScreenState();
}

class _ExportScreenState extends ConsumerState<ExportScreen> {
  String? _busy;

  Future<void> _export(String format) async {
    setState(() => _busy = format);
    try {
      final path = await ref.read(exportRepositoryProvider).download(format);
      await Share.shareXFiles([XFile(path)], text: 'FitApp verilerim ($format)');
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Dışa aktarma başarısız. Bağlantını kontrol et.')),
        );
      }
    } finally {
      if (mounted) setState(() => _busy = null);
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final formats = <(String, String, IconData)>[
      ('xlsx', 'Excel (.xlsx)', Icons.table_chart_outlined),
      ('pdf', 'PDF Rapor', Icons.picture_as_pdf_outlined),
      ('csv', 'CSV', Icons.grid_on_outlined),
      ('json', 'JSON (yedek)', Icons.data_object_outlined),
    ];

    return Scaffold(
      appBar: AppBar(title: const Text('Dışa Aktar')),
      body: ListView(
        padding: const EdgeInsets.all(AppSpacing.md),
        children: [
          Text('Tüm antrenman verini seçtiğin formatta dışa aktar ve paylaş.',
              style: theme.textTheme.bodyMedium),
          const SizedBox(height: AppSpacing.md),
          for (final (format, label, icon) in formats)
            Padding(
              padding: const EdgeInsets.only(bottom: AppSpacing.xs),
              child: AppCard(
                onTap: _busy == null ? () => _export(format) : null,
                child: Row(
                  children: [
                    Icon(icon, color: theme.colorScheme.primary),
                    const SizedBox(width: AppSpacing.md),
                    Expanded(child: Text(label, style: theme.textTheme.titleMedium)),
                    if (_busy == format)
                      const SizedBox(
                          width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2))
                    else
                      Icon(Icons.download, color: theme.dividerColor),
                  ],
                ),
              ),
            ),
        ],
      ),
    );
  }
}
