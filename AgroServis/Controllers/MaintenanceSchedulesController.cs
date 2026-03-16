using AgroServis.Services;
using AgroServis.Services.DTO;
using AgroServis.Services.Exceptions;
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

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var dto = await _service.GetForCreateAsync();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaintenanceScheduleCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var reloadDto = await _service.GetForCreateAsync();
                dto = dto with { AvailableEquipment = reloadDto.AvailableEquipment };
                return View(dto);
            }

            try
            {
                await _service.CreateAsync(dto);
                TempData["Success"] = "Maintenance schedule created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (EntityNotFoundException)
            {
                ModelState.AddModelError(nameof(dto.EquipmentId), "Selected equipment was not found.");

                var reloadDto = await _service.GetForCreateAsync();
                dto = dto with { AvailableEquipment = reloadDto.AvailableEquipment };

                return View(dto);
            }
        }
    }
}