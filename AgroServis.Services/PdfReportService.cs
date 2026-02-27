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
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Text("Maintenance Report")
                        .FontSize(18)
                        .SemiBold()
                        .AlignCenter();

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Spacing(8);

                        // Header row
                        col.Item().Row(r =>
                        {
                            r.ConstantColumn(100).Text("Date").SemiBold();
                            r.RelativeColumn().Text("Details").SemiBold();
                            r.ConstantColumn(80).AlignRight().Text("Cost").SemiBold();
                        });

                        foreach (var m in items)
                        {
                            col.Item().Row(row =>
                            {
                                row.ConstantColumn(100).Text(m.Date.ToString("yyyy-MM-dd"));
                                row.RelativeColumn().Column(c =>
                                {
                                    c.Item().Text($"{m.Title}").SemiBold();
                                    c.Item().Text(m.Description).FontColor(Colors.Grey.Darken2).FontSize(10);
                                    c.Item().Text($"Performed by: {m.PerformedBy}").FontSize(10);
                                });
                                row.ConstantColumn(80).AlignRight().Text($"{m.Cost:C}");
                            });

                            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(x => x.Span("Page ").CurrentPageNumber().Span(" / ").TotalPages());
                });
            });

            return document.GeneratePdf();
        }
    }
}