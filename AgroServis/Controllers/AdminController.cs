using AgroServis.DAL;
using AgroServis.DAL.Entities;
using AgroServis.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroServis.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
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

        [HttpGet]
        public async Task<IActionResult> ApproveRegistration(int id)
        {
            var registration = await _context.PendingRegistrations.FindAsync(id);
            if (registration == null || registration.IsProcessed)
            {
                return NotFound();
            }

            return View(registration);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRegistrationConfirmed(int id)
        {
            var registration = await _context.PendingRegistrations.FindAsync(id);
            if (registration == null || registration.IsProcessed)
            {
                return NotFound();
            }

            try
            {
                var user = new ApplicationUser
                {
                    UserName = registration.Email,
                    Email = registration.Email,
                    EmailConfirmed = true,
                    FirstName = registration.FirstName,
                    LastName = registration.LastName,
                    IsApproved = true,
                };

                user.PasswordHash = registration.PasswordHash;

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    TempData["Error"] = $"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                    return RedirectToAction(nameof(PendingRegistrations));
                }

                await _userManager.AddToRoleAsync(user, "Worker");

                var worker = new Worker
                {
                    FirstName = registration.FirstName,
                    LastName = registration.LastName,
                    Email = registration.Email,
                    PhoneNumber = registration.PhoneNumber,
                    Position = registration.Position,
                    UserId = user.Id
                };

                _context.Workers.Add(worker);

                registration.IsProcessed = true;
                await _context.SaveChangesAsync();

                await _emailService.SendApprovalConfirmationAsync(registration.Email, registration.FirstName);

                TempData["Success"] = $"Worker {registration.FirstName} {registration.LastName} approved successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving registration {Id}", id);
                TempData["Error"] = "An error occurred while approving the registration.";
            }

            return RedirectToAction(nameof(PendingRegistrations));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRegistration(int id)
        {
            var registration = await _context.PendingRegistrations.FindAsync(id);
            if (registration == null || registration.IsProcessed)
            {
                return NotFound();
            }

            registration.IsProcessed = true;
            await _context.SaveChangesAsync();

            await _emailService.SendRejectionNotificationAsync(registration.Email, registration.FirstName);

            TempData["Success"] = "Registration request rejected.";
            return RedirectToAction(nameof(PendingRegistrations));
        }
    }
}