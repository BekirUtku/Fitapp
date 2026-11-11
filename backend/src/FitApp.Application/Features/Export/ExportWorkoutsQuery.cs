using System.Globalization;
using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using FitApp.Application.Common.Interfaces;
using FitApp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FitApp.Application.Features.Export;

public record ExportResult(byte[] Content, string ContentType, string FileName);

/// <param name="Format">json | csv</param>
public record ExportWorkoutsQuery(string Format) : IRequest<ExportResult>;

public class ExportWorkoutsQueryHandler : IRequestHandler<ExportWorkoutsQuery, ExportResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    static ExportWorkoutsQueryHandler()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public ExportWorkoutsQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private record SetRow(
        DateTime Date, string Session, string Exercise, int SetNumber, string SetType,
        decimal? Weight, int? Reps, int? Rpe, bool Completed, bool IsPr);

    public async Task<ExportResult> Handle(ExportWorkoutsQuery request, CancellationToken ct)
    {
        var userId = _currentUser.RequireUserId();

        var rows = await _db.WorkoutSets.AsNoTracking()
            .Where(s => !s.IsDeleted
                && s.SessionExercise!.Session!.UserId == userId
                && !s.SessionExercise.Session.IsDeleted)
            .OrderBy(s => s.SessionExercise!.Session!.StartedAt)
            .Select(s => new SetRow(
                s.SessionExercise!.Session!.StartedAt,
                s.SessionExercise.Session.Name,
                s.SessionExercise.Exercise!.Name,
                s.SetNumber,
                s.SetType.ToString(),
                s.Weight, s.Reps, s.Rpe, s.IsCompleted, s.IsPr))
            .ToListAsync(ct);

        var stamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var format = request.Format.Trim().ToLowerInvariant();

        return format switch
        {
            "csv" => new ExportResult(BuildCsv(rows), "text/csv", $"fitapp_export_{stamp}.csv"),
            "json" => new ExportResult(BuildJson(rows), "application/json", $"fitapp_export_{stamp}.json"),
            "xlsx" => new ExportResult(BuildXlsx(rows),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"fitapp_export_{stamp}.xlsx"),
            "pdf" => new ExportResult(BuildPdf(rows), "application/pdf", $"fitapp_export_{stamp}.pdf"),
            _ => throw new Common.Exceptions.AppValidationException(
                new Dictionary<string, string[]> { ["format"] = new[] { "Geçersiz format (json|csv|xlsx|pdf)." } })
        };
    }

    private static byte[] BuildXlsx(IReadOnlyList<SetRow> rows)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Antrenmanlar");

        var headers = new[] { "Tarih", "Antrenman", "Hareket", "Set No", "Set Türü", "Ağırlık", "Tekrar", "RPE", "Tamamlandı", "PR" };
        for (var c = 0; c < headers.Length; c++)
            ws.Cell(1, c + 1).Value = headers[c];
        ws.Row(1).Style.Font.Bold = true;

        var r = 2;
        foreach (var row in rows)
        {
            ws.Cell(r, 1).Value = row.Date;
            ws.Cell(r, 2).Value = row.Session;
            ws.Cell(r, 3).Value = row.Exercise;
            ws.Cell(r, 4).Value = row.SetNumber;
            ws.Cell(r, 5).Value = row.SetType;
            ws.Cell(r, 6).Value = row.Weight;
            ws.Cell(r, 7).Value = row.Reps;
            ws.Cell(r, 8).Value = row.Rpe;
            ws.Cell(r, 9).Value = row.Completed ? "Evet" : "Hayır";
            ws.Cell(r, 10).Value = row.IsPr ? "PR" : "";
            r++;
        }

        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    private static byte[] BuildPdf(IReadOnlyList<SetRow> rows)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Text($"FitApp Antrenman Raporu  •  {DateTime.UtcNow:yyyy-MM-dd}")
                    .SemiBold().FontSize(14);

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2); // tarih
                        cols.RelativeColumn(2); // hareket
                        cols.RelativeColumn(1); // set
                        cols.RelativeColumn(1); // ağırlık
                        cols.RelativeColumn(1); // tekrar
                        cols.RelativeColumn(1); // pr
                    });

                    void Head(string t) => table.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(t).SemiBold();
                    Head("Tarih"); Head("Hareket"); Head("Set"); Head("Kg"); Head("Tekrar"); Head("PR");

                    foreach (var row in rows)
                    {
                        table.Cell().Padding(3).Text(row.Date.ToString("yyyy-MM-dd"));
                        table.Cell().Padding(3).Text(row.Exercise);
                        table.Cell().Padding(3).Text(row.SetNumber.ToString());
                        table.Cell().Padding(3).Text(row.Weight?.ToString("0.##") ?? "");
                        table.Cell().Padding(3).Text(row.Reps?.ToString() ?? "");
                        table.Cell().Padding(3).Text(row.IsPr ? "★" : "");
                    }
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Sayfa ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private static byte[] BuildJson(IReadOnlyList<SetRow> rows)
        => JsonSerializer.SerializeToUtf8Bytes(rows,
            new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true });

    private static byte[] BuildCsv(IReadOnlyList<SetRow> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Tarih,Antrenman,Hareket,SetNo,SetTuru,Agirlik,Tekrar,RPE,Tamamlandi,PR");

        foreach (var r in rows)
        {
            var inv = CultureInfo.InvariantCulture;
            sb.Append(r.Date.ToString("yyyy-MM-dd HH:mm", inv)).Append(',')
              .Append(Csv(r.Session)).Append(',')
              .Append(Csv(r.Exercise)).Append(',')
              .Append(r.SetNumber).Append(',')
              .Append(r.SetType).Append(',')
              .Append(r.Weight?.ToString(inv) ?? "").Append(',')
              .Append(r.Reps?.ToString(inv) ?? "").Append(',')
              .Append(r.Rpe?.ToString(inv) ?? "").Append(',')
              .Append(r.Completed ? "1" : "0").Append(',')
              .Append(r.IsPr ? "1" : "0")
              .AppendLine();
        }

        // Excel'in UTF-8 Türkçe karakterleri doğru okuması için BOM ekle.
        var preamble = Encoding.UTF8.GetPreamble();
        var body = Encoding.UTF8.GetBytes(sb.ToString());
        return preamble.Concat(body).ToArray();
    }

    private static string Csv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
