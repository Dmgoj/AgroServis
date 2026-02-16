using AgroServis.DAL;
using AgroServis.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroServis.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWorkerService _workerService;

        public AdminController(
            ApplicationDbContext context,
            IWorkerService workerService)
        {
            _context = context;
            _workerService = workerService;
        }

        [HttpGet]
        public async Task<IActionResult> PendingRegistrations()
        {
            var pending = await _context.PendingRegistrations
                .Where(p => !p.IsProcessed)
                .OrderByDescending(p => p.RequestedAt)
                .ToListAsync();

            return View(pending);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRegistration(int id)
        {
            var (success, message, _) = await _workerService.ApproveRegistrationByIdAsync(id);

            TempData[success ? "Success" : "Error"] = message;

            return RedirectToAction("Index", "Workers", new { tab = "pending" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRegistration(int id)
        {
            var (success, message, _) = await _workerService.RejectRegistrationByIdAsync(id);

            TempData[success ? "Success" : "Error"] = message;

            return RedirectToAction("Index", "Workers", new { tab = "pending" });
        }
    }
}