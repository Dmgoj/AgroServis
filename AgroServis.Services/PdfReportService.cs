using AgroServis.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace AgroServis.Services
{
    public class PdfReportService : IPdfReportService
    {
        public byte[] GenerateMaintenanceReport(IEnumerable<MaintenanceDto> items)
            => GenerateMaintenanceReport(items, new[] { "Date", "Equipment", "Serial", "Description", "Cost", "PerformedBy" });

        public byte[] GenerateMaintenanceReport(IEnumerable<MaintenanceDto> items, IEnumerable<string> selectedFields)
        {
            var list = (items ?? Enumerable.Empty<MaintenanceDto>()).ToList();
            var fields = new HashSet<string>(selectedFields ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);

            bool showDate = fields.Contains("Date");
            bool showEquipment = fields.Contains("Equipment");
            bool showSerial = fields.Contains("Serial");
            bool showDescription = fields.Contains("Description");
            bool showType = fields.Contains("Type");
            bool showStatus = fields.Contains("Status");
            bool showCost = fields.Contains("Cost");
            bool showPerformedBy = fields.Contains("PerformedBy");
            bool showNotes = fields.Contains("Notes");
            bool showCompletedAt = fields.Contains("CompletedAt");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Row(row =>
                        {
                            row.RelativeColumn().Column(col =>
                            {
                                col.Item().Text("Maintenance Report").FontSize(16).SemiBold();
                                col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(9).FontColor(Colors.Grey.Darken2);
                            });

                            row.ConstantColumn(120).AlignRight().Text($"Items: {list.Count}").FontSize(10);
                        });

                    page.Content().PaddingVertical(8).Column(col =>
                    {
                        if (!list.Any())
                        {
                            col.Item().AlignCenter().Text("No maintenance records found").FontColor(Colors.Grey.Darken2);
                            return;
                        }

                        // Header row: build columns dynamically
                        col.Item().BorderBottom(1).PaddingBottom(6).Row(header =>
                        {
                            if (showDate) header.ConstantColumn(60).Text("Date").SemiBold();
                            if (showEquipment) header.RelativeColumn().Text("Equipment").SemiBold();
                            if (showSerial) header.ConstantColumn(80).AlignRight().Text("Serial").SemiBold();
                            if (showDescription) header.RelativeColumn().Text("Description").SemiBold();
                            if (showType) header.ConstantColumn(60).AlignRight().Text("Type").SemiBold();
                            if (showStatus) header.ConstantColumn(60).AlignRight().Text("Status").SemiBold();
                            if (showCost) header.ConstantColumn(70).AlignRight().Text("Cost").SemiBold();
                            if (showPerformedBy) header.ConstantColumn(100).AlignRight().Text("Performed By").SemiBold();
                            if (showNotes) header.ConstantColumn(120).AlignRight().Text("Notes").SemiBold();
                            if (showCompletedAt) header.ConstantColumn(80).AlignRight().Text("Completed").SemiBold();
                        });

                        foreach (var m in list)
                        {
                            col.Item().PaddingVertical(6).Row(r =>
                            {
                                if (showDate) r.ConstantColumn(60).Text(m.MaintenanceDate.ToString("dd-MM-yyyy"));
                                if (showEquipment) r.RelativeColumn().Text($"{m.EquipmentName}");
                                if (showSerial) r.ConstantColumn(80).AlignRight().Text(m.EquipmentSerialNumber);
                                if (showDescription) r.RelativeColumn().Text(m.Description).FontSize(9).FontColor(Colors.Grey.Darken2);
                                if (showType) r.ConstantColumn(60).AlignRight().Text(m.Type.ToString());
                                if (showStatus) r.ConstantColumn(60).AlignRight().Text(m.Status.ToString());
                                if (showCost) r.ConstantColumn(70).AlignRight().Text(m.Cost.HasValue ? $"{m.Cost.Value:C2}" : "N/A");
                                if (showPerformedBy) r.ConstantColumn(100).AlignRight().Text(m.PerformedBy ?? string.Empty);
                                if (showNotes) r.ConstantColumn(120).AlignRight().Text(m.Notes ?? string.Empty).FontSize(9);
                                if (showCompletedAt) r.ConstantColumn(80).AlignRight().Text(m.CompletedAt?.ToString("dd-MM-yyyy") ?? "-");
                            });

                            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        }

                        // Totals row if cost selected
                        if (showCost)
                        {
                            var total = list.Where(x => x.Cost.HasValue).Sum(x => x.Cost.Value);
                            col.Item().PaddingTop(8).Row(t =>
                            {
                                t.RelativeColumn().Text(string.Empty);
                                t.ConstantColumn(150).Row(r =>
                                {
                                    r.ConstantColumn(80).Text("Total:").SemiBold();
                                    r.ConstantColumn(70).AlignRight().Text($"{total:C2}").SemiBold();
                                });
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text(x => x.Span("Page ").CurrentPageNumber().Span(" / ").TotalPages());
                });
            });

            return document.GeneratePdf();
        }
    }