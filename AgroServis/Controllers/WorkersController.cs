using AgroServis.Services;
using AgroServis.Services.DTO;
using AgroServis.Services.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace AgroServis.Controllers
{
    [Authorize(Roles = "Admin")]
    public class WorkersController : Controller
    {
        private readonly IWorkerService _service;

        public WorkersController(IWorkerService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            int pageSize = 10,
            string? sortBy = null,
            string? sortDir = null,
            string? search = null
        )
        {
            var workers = await _service.GetAllAsync(
                pageNumber,
                pageSize,
                sortBy,
                sortDir,
                search
            );

            var pendingRegistrations = await _service.GetPendingRegistrationsAsync();

            ViewBag.PendingRegistrations = pendingRegistrations;
            ViewBag.PendingCount = pendingRegistrations.Count;

            ViewData["SortBy"] = sortBy ?? "";
            ViewData["SortDir"] = sortDir ?? "";
            ViewData["Search"] = search ?? "";

            return View(workers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkerCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var createdId = await _service.CreateAsync(dto);

                var created = await _service.GetByIdAsync(createdId);
                TempData["CreatedWorkerName"] = $"{created.FirstName} {created.LastName}";

                ModelState.Clear();
                return View(new WorkerCreateDto());
            }
            catch (DuplicateEntityException ex)
            {
                ModelState.AddModelError(nameof(dto.Email), ex.Message);
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                TempData["Success"] = "Worker deleted successfully!";
            }
            catch (EntityNotFoundException)
            {
                TempData["Error"] = "Worker not found.";
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while deleting the worker.";
            }

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int id)
        {
            var worker = await _service.GetDtoByIdAsync(id);
            if (worker == null)
            {
                return NotFound();
            }
            return View(worker);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var worker = await _service.GetByIdAsync(id);
            if (worker == null)
            {
                return NotFound();
            }
            var dto = new WorkerUpdateDto
            {
                Id = worker.Id,
                FirstName = worker.FirstName,
                LastName = worker.LastName,
                Email = worker.Email,
                PhoneNumber = worker.PhoneNumber,
                Position = worker.Position
            };
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(WorkerUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                await _service.UpdateAsync(dto);
                TempData["Success"] = "Worker updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DuplicateEntityException ex)
            {
                ModelState.AddModelError(nameof(dto.Email), ex.Message);
                return View(dto);
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }
    }
}