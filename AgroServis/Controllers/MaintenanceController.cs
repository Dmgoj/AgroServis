using AgroServis.Services;
using AgroServis.Services.DTO;
using AgroServis.Services.DTOs;
using AgroServis.Services.Exceptions;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgroServis.Controllers
{
    [Authorize]
    public class MaintenanceController : Controller
    {
        private readonly IMaintenanceService _service;

        public MaintenanceController(IMaintenanceService service)
        {
            _service = service;
        }

        // GET: MaintenanceController
        [AllowAnonymous]
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            int pageSize = 10,
            string? sortBy = null,
            string? sortDir = null,
            string? search = null,
            int? equipmentId = null,
            string? type = null,
            string? status = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null
        )
        {
            var maintenances = await _service.GetAllAsync(
                pageNumber,
                pageSize,
                sortBy,
                sortDir,
                search,
                equipmentId,
                type,
                status,
                dateFrom,
                dateTo
            );

            var createModel = await _service.GetForCreateAsync();
            ViewBag.AvailableEquipment =
                createModel.AvailableEquipment
                ?? Enumerable.Empty<AgroServis.Services.DTO.EquipmentDto>();

            ViewData["SortBy"] = sortBy ?? "";
            ViewData["SortDir"] = sortDir ?? "";
            ViewData["Search"] = search ?? "";
            ViewData["EquipmentId"] = equipmentId?.ToString() ?? "";
            ViewData["Type"] = type ?? "";
            ViewData["Status"] = status ?? "";
            ViewData["DateFrom"] = dateFrom?.ToString("yyyy-MM-dd") ?? "";
            ViewData["DateTo"] = dateTo?.ToString("yyyy-MM-dd") ?? "";

            return View(maintenances);
        }

        // GET: MaintenanceController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
                return NotFound();

            return View(dto);
        }

        // GET: MaintenanceController/Create
        public async Task<IActionResult> Create(int? equipmentId = null)
        {
            var model = await _service.GetForCreateAsync();

            if (equipmentId.HasValue)
            {
                model.EquipmentId = equipmentId.Value;
            }

            return View(model);
        }

        // POST: MaintenanceController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaintenanceCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var model = await _service.GetForCreateAsync();
                dto.AvailableEquipment = model.AvailableEquipment;
                return View(dto);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var id = await _service.CreateAsync(dto, userId);

                TempData["Success"] = "Maintenance record created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (EntityNotFoundException ex)
            {
                TempData["Error"] = ex.Message;

                var model = await _service.GetForCreateAsync();
                dto.AvailableEquipment = model.AvailableEquipment;
                return View(dto);
            }
        }

        // GET: MaintenanceController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var maintenance = await _service.GetByIdForEditAsync(id);
            if (maintenance == null)
            {
                return NotFound();
            }
            return View(maintenance);
        }

        // POST: MaintenanceController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MaintenanceEditDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var updateDto = new MaintenanceUpdateDto
                {
                    Id = dto.Id,
                    EquipmentId = dto.EquipmentId,
                    MaintenanceDate = dto.MaintenanceDate,
                    Description = dto.Description,
                    Type = dto.Type,
                    Status = dto.Status,
                    Cost = dto.Cost,
                    Notes = dto.Notes,
                    PerformedBy = dto.PerformedBy,
                };

                await _service.UpdateAsync(updateDto);

                TempData["Success"] = "Maintenance record updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: MaintenanceController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}