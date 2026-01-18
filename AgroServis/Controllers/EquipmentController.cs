using AgroServis.Services;
using AgroServis.Services.DTO;
using Microsoft.AspNetCore.Mvc;

namespace AgroServis.Controllers
{
    public class EquipmentController : Controller
    {
        private readonly IEquipmentService _service;

        public EquipmentController(IEquipmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            int pageSize = 10,
            string? sortBy = null,
            string? sortDir = null,
            string? search = null,
            int? equipmentTypeId = null,
            int? categoryId = null
        )
        {
            var equipment = await _service.GetAllAsync(
                pageNumber,
                pageSize,
                sortBy,
                sortDir,
                search,
                equipmentTypeId,
                categoryId
            );

            var createModel = await _service.GetForCreateAsync();
            ViewBag.EquipmentTypes = createModel.EquipmentTypes ?? Enumerable.Empty<object>();
            ViewBag.EquipmentCategories =
                createModel.EquipmentCategories ?? Enumerable.Empty<object>();

            ViewData["SortBy"] = sortBy ?? "";
            ViewData["SortDir"] = sortDir ?? "";
            ViewData["Search"] = search ?? "";
            ViewData["EquipmentTypeId"] = equipmentTypeId?.ToString() ?? "";
            ViewData["CategoryId"] = categoryId?.ToString() ?? "";

            return View(equipment);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var equipment = await _service.GetByIdAsync(id);
            if (equipment == null)
            {
                return NotFound();
            }
            return View(equipment);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var equipment = await _service.GetByIdForEditAsync(id);
            if (equipment == null)
            {
                return NotFound();
            }
            return View(equipment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EquipmentUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var editDto = await _service.GetByIdForEditAsync(dto.Id);

                editDto.Manufacturer = dto.Manufacturer;
                editDto.Model = dto.Model;
                editDto.SerialNumber = dto.SerialNumber;
                editDto.EquipmentTypeId = dto.EquipmentTypeId;

                return View(editDto);
            }
            await _service.UpdateAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = await _service.GetForCreateAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EquipmentCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var model = await _service.GetForCreateAsync();
                model.Manufacturer = dto.Manufacturer;
                model.Model = dto.Model;
                model.SerialNumber = dto.SerialNumber;
                model.EquipmentTypeId = dto.EquipmentTypeId;
                return View(model);
            }
            var createdId = await _service.CreateAsync(dto);

            var created = await _service.GetByIdAsync(createdId);
            TempData["CreatedEquipmentName"] = $"{created.Manufacturer} {created.Model}";

            var freshModel = await _service.GetForCreateAsync();
            ModelState.Clear();
            return View(freshModel);
        }

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