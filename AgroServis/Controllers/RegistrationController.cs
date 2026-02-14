using AgroServis.DAL;
using AgroServis.DAL.Entities;
using AgroServis.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroServis.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<RegistrationController> _logger;

        public RegistrationController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ILogger<RegistrationController> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Approve(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return View("ApprovalError", "Invalid approval link.");
            }

            var registration = await _context.PendingRegistrations
                .FirstOrDefaultAsync(p => p.ApprovalToken == token && !p.IsProcessed);

            if (registration == null)
            {
                return View("ApprovalError", "Registration request not found or already processed.");
            }

            if (registration.TokenExpiresAt < DateTime.UtcNow)
            {
                return View("ApprovalError", "This approval link has expired.");
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
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create user: {Errors}", errors);
                    return View("ApprovalError", $"Failed to create user: {errors}");
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
                _context.PendingRegistrations.Remove(registration);
                await _context.SaveChangesAsync();

                await _emailService.SendApprovalConfirmationAsync(registration.Email, registration.FirstName);

                _logger.LogInformation("Registration approved for {Email}", registration.Email);

                return View("ApprovalSuccess", registration.FirstName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving registration");
                return View("ApprovalError", "An error occurred while processing the approval.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Reject(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return View("ApprovalError", "Invalid rejection link.");
            }

            var registration = await _context.PendingRegistrations
                .FirstOrDefaultAsync(p => p.ApprovalToken == token && !p.IsProcessed);

            if (registration == null)
            {
                return View("ApprovalError", "Registration request not found or already processed.");
            }

            if (registration.TokenExpiresAt < DateTime.UtcNow)
            {
                return View("ApprovalError", "This rejection link has expired.");
            }

            try
            {
                await _emailService.SendRejectionNotificationAsync(registration.Email, registration.FirstName);

                _context.PendingRegistrations.Remove(registration);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Registration rejected for {Email}", registration.Email);

                return View("RejectionSuccess", registration.FirstName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting registration");
                return View("ApprovalError", "An error occurred while processing the rejection.");
            }
        }
    }
}