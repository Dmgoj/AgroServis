using AgroServis.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgroServis.Controllers
{
    public class MaintenanceSchedulesController : Controller
    {
        private readonly IMaintenanceScheduleService _service;

        public MaintenanceSchedulesController(IMaintenanceScheduleService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var schedules = await _service.GetAllAsync();
            return View(schedules);
        }
    }
}