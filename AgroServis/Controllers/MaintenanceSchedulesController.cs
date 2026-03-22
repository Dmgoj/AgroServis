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

        public async Task<IActionResult> Index(int? equipmentId)
        {
            var schedules = await _service.GetAllAsync(equipmentId);

            ViewBag.EquipmentId = equipmentId;

            return View(schedules);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? equipmentId)
        {
            var dto = await _service.GetForCreateAsync();

            if (equipmentId.HasValue)
            {
                dto = dto with { EquipmentId = equipmentId.Value };
            }

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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _service.GetForEditAsync(id);
            if (dto == null)
                return NotFound();

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MaintenanceScheduleUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var reloadDto = await _service.GetForEditAsync(dto.Id);
                if (reloadDto == null)
                    return NotFound();

                dto = dto with { AvailableEquipment = reloadDto.AvailableEquipment };
                return View(dto);
            }

            try
            {
                await _service.UpdateAsync(dto);
                TempData["Success"] = "Maintenance schedule updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }
    }
}