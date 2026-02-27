using AgroServis.Services;
using AgroServis.Services.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AgroServis.Controllers
{
    public class ReportController : Controller
    {
        private readonly IPdfReportService _pdfReportService;

        public ReportController(IPdfReportService pdfReportService)
        {
            _pdfReportService = pdfReportService;
        }

        // GET /Reports/MaintenanceReport
        public IActionResult MaintenanceReport()
        {
            // Replace with your repository/DbContext call to load maintenances
            var sample = new[]
            {
                new MaintenanceDto { Id = 1, Title = "Oil Change", Description = "Engine oil and filter", Date = DateTime.UtcNow.AddDays(-10), PerformedBy = "Tech A", Cost = 120m },
                new MaintenanceDto { Id = 2, Title = "Belt Replacement", Description = "Drive belt replaced", Date = DateTime.UtcNow.AddDays(-5), PerformedBy = "Tech B", Cost = 75.5m }
            };

            var pdfBytes = _pdfReportService.GenerateMaintenanceReport(sample);
            return File(pdfBytes, "application/pdf", "maintenance-report.pdf");
        }
    }
}