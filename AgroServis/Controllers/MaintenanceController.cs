using AgroServis.Services;
using AgroServis.Services.DTOs;
using AgroServis.Services.Exceptions;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgroServis.Controllers
{
    public class MaintenanceController : Controller
    {
        private readonly IMaintenanceService _service;
        public MaintenanceController(IMaintenanceService service)
        {
            _service = service;
        }

        // GET: MaintenanceController
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var maintenances = await _service.GetAllAsync(pageNumber, pageSize);
            return View(maintenances);
        }

        // GET: MaintenanceController/Details/5
        public ActionResult Details(int id)
        {
            return View();
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
                var id = await _service.CreateAsync(dto);

                TempData["Success"] = "Maintenance record created successfully!";
                return RedirectToAction(nameof(Details), new { id });
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
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: MaintenanceController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: MaintenanceController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: MaintenanceController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}