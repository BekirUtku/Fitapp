class Insight {
  const Insight({required this.code, required this.message});

  final String code;
  final String message;

  factory Insight.fromJson(Map<String, dynamic> j) =>
      Insight(code: j['code'] as String? ?? '', message: j['message'] as String? ?? '');
}
