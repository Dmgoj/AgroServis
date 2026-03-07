using AgroServis.Services.DTO;

using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AgroServis.Services
{
    public class PdfReportService : IPdfReportService
    {
        public byte[] GenerateMaintenanceReport(IReadOnlyList<MaintenanceDto> data, MaintenanceReportOptionsDto options)
        {
            var now = DateTime.Now;

            QuestPDF.Infrastructure.IDocument document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(header =>
                    {
                        header.Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Maintenance Report").FontSize(18).SemiBold();
                                col.Item().Text($"Generated: {now:yyyy-MM-dd HH:mm}").FontSize(9).FontColor(Colors.Grey.Darken2);
                                col.Item().Text($"Records: {data.Count}").FontSize(9).FontColor(Colors.Grey.Darken2);
                            });
                        });
                    });

                    page.Content().PaddingTop(10).Element(content =>
                    {
                        content.Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                if (options.IncludeEquipmentName) columns.RelativeColumn(2);
                                if (options.IncludeSerialNumber) columns.RelativeColumn(2);
                                if (options.IncludeMaintenanceDate) columns.RelativeColumn(1);
                                if (options.IncludeType) columns.RelativeColumn(1);
                                if (options.IncludeStatus) columns.RelativeColumn(1);
                                if (options.IncludeCost) columns.RelativeColumn(1);
                                if (options.IncludePerformedBy) columns.RelativeColumn(2);
                                if (options.IncludeDescription) columns.RelativeColumn(3);
                                if (options.IncludeNotes) columns.RelativeColumn(3);
                            });

                            // Header row
                            table.Header(header =>
                            {
                                static IContainer HeaderCell(IContainer c) =>
                                    c.DefaultTextStyle(x => x.SemiBold())
                                     .PaddingVertical(6)
                                     .PaddingHorizontal(4)
                                     .Background(Colors.Grey.Lighten3)
                                     .BorderBottom(1)
                                     .BorderColor(Colors.Grey.Medium);

                                if (options.IncludeEquipmentName) header.Cell().Element(HeaderCell).Text("Equipment");
                                if (options.IncludeSerialNumber) header.Cell().Element(HeaderCell).Text("Serial #");
                                if (options.IncludeMaintenanceDate) header.Cell().Element(HeaderCell).Text("Date");
                                if (options.IncludeType) header.Cell().Element(HeaderCell).Text("Type");
                                if (options.IncludeStatus) header.Cell().Element(HeaderCell).Text("Status");
                                if (options.IncludeCost) header.Cell().Element(HeaderCell).AlignRight().Text("Cost");
                                if (options.IncludePerformedBy) header.Cell().Element(HeaderCell).Text("Performed By");
                                if (options.IncludeDescription) header.Cell().Element(HeaderCell).Text("Description");
                                if (options.IncludeNotes) header.Cell().Element(HeaderCell).Text("Notes");
                            });

                            // Body rows
                            foreach (var m in data)
                            {
                                static IContainer Cell(IContainer c) =>
                                    c.PaddingVertical(4)
                                     .PaddingHorizontal(4)
                                     .BorderBottom(1)
                                     .BorderColor(Colors.Grey.Lighten2);

                                if (options.IncludeEquipmentName) table.Cell().Element(Cell).Text(m.EquipmentName);
                                if (options.IncludeSerialNumber) table.Cell().Element(Cell).Text(m.EquipmentSerialNumber);
                                if (options.IncludeMaintenanceDate) table.Cell().Element(Cell).Text(m.FormattedDate);
                                if (options.IncludeType) table.Cell().Element(Cell).Text(m.Type.ToString());
                                if (options.IncludeStatus) table.Cell().Element(Cell).Text(m.Status.ToString());

                                if (options.IncludeCost)
                                    table.Cell().Element(Cell).AlignRight().Text(m.FormattedCost);

                                if (options.IncludePerformedBy)
                                    table.Cell().Element(Cell).Text(m.PerformedBy ?? "N/A");

                                if (options.IncludeDescription)
                                    table.Cell().Element(Cell).Text(m.Description);

                                if (options.IncludeNotes)
                                    table.Cell().Element(Cell).Text(string.IsNullOrWhiteSpace(m.Notes) ? "—" : m.Notes);
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}