using AgroServis.Services;
using AgroServis.Services.DTO;
using Microsoft.AspNetCore.Mvc;

namespace AgroServis.Controllers
{
    public class ReportController : Controller
    {
        private readonly IPdfReportService _pdfReportService;
        private readonly IMaintenanceService _maintenanceService;

        public ReportController(IPdfReportService pdfReportService, IMaintenanceService maintenanceService)
        {
            _pdfReportService = pdfReportService;
            _maintenanceService = maintenanceService;
        }

        [HttpGet]
        public async Task<IActionResult> MaintenanceReport()
        {
            var vm = new MaintenanceReportOptionsDto();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MaintenanceReport(MaintenanceReportOptionsDto options)
        {
            var data = await _maintenanceService.GetForReportAsync(options);
            var pdfBytes = _pdfReportService.GenerateMaintenanceReport(data, options);

            return File(pdfBytes, "application/pdf", "maintenance-report.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> MaintenanceSingleReport(int id)
        {
            var item = await _maintenanceService.GetByIdAsync(id);

            if (item == null)
                return NotFound();

            var data = new List<MaintenanceDto> { item };

            var options = new MaintenanceReportOptionsDto
            {
                IncludeEquipmentName = true,
                IncludeSerialNumber = true,
                IncludeMaintenanceDate = true,
                IncludeType = true,
                IncludeStatus = true,
                IncludeCost = true,
                IncludePerformedBy = true,
                IncludeDescription = true,
                IncludeNotes = true
            };

            var pdfBytes = _pdfReportService.GenerateMaintenanceReport(data, options);

            return File(pdfBytes, "application/pdf", $"maintenance-{id}.pdf");
        }
    }
}