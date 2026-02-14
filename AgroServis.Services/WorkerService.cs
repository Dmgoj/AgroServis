using AgroServis.DAL;
using AgroServis.DAL.Entities;
using AgroServis.Services.DTO;
using AgroServis.Services.Exceptions;
using AgroServis.Services.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AgroServis.Services
{
    public class WorkerService : IWorkerService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;
        private readonly IPaginationService _paginationService;
        private readonly ILogger<WorkerService> _logger;
        private readonly IEmailService _emailService;

        public WorkerService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IPaginationService paginationService,
            IMemoryCache cache,
            ILogger<WorkerService> logger,
            IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _paginationService = paginationService;
            _cache = cache;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<PagedResult<WorkerDto>> GetAllAsync(
            int page,
            int pageSize,
            string? sortBy = null,
            string? sortDir = null,
            string? search = null)
        {
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
            "id",
            "firstname",
            "lastname",
            "email",
            "position"
            };

            var key = (sortBy ?? "lastname").Trim();
            if (!allowed.Contains(key))
                key = "lastname";

            var dir = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase)
                ? "desc"
                : "asc";

            var workerVersion = CacheVersionHelper.GetVersion(_cache, "Worker");

            var cacheKey = $"WorkerV_{workerVersion}_Page_{page}_Size_{pageSize}_Sort_{key}_{dir}_Q_{(search ?? "")}";

            if (_cache.TryGetValue(cacheKey, out PagedResult<WorkerDto>? cached) && cached != null)
            {
                _logger.LogDebug(
                    "Cache HIT: Returning workers page {Page} sort {Key} {Dir} search {Search}",
                    page,
                    key,
                    dir,
                    search
                );
                return cached;
            }

            _logger.LogDebug(
                "Cache MISS: Loading workers page {Page} sort {Key} {Dir} from DB",
                page,
                key,
                dir
            );

            var baseQuery = _context.Workers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                baseQuery = baseQuery.Where(w =>
                    EF.Functions.Like(w.FirstName, $"%{s}%") ||
                    EF.Functions.Like(w.LastName, $"%{s}%") ||
                    EF.Functions.Like(w.Email, $"%{s}%") ||
                    (w.Position != null && EF.Functions.Like(w.Position, $"%{s}%"))
                );
            }

            var desc = dir == "desc";
            switch (key.ToLowerInvariant())
            {
                case "firstname":
                    baseQuery = desc
                        ? baseQuery.OrderByDescending(w => w.FirstName)
                        : baseQuery.OrderBy(w => w.FirstName);
                    break;

                case "lastname":
                    baseQuery = desc
                        ? baseQuery.OrderByDescending(w => w.LastName)
                        : baseQuery.OrderBy(w => w.LastName);
                    break;

                case "email":
                    baseQuery = desc
                        ? baseQuery.OrderByDescending(w => w.Email)
                        : baseQuery.OrderBy(w => w.Email);
                    break;

                case "position":
                    baseQuery = desc
                        ? baseQuery.OrderByDescending(w => w.Position)
                        : baseQuery.OrderBy(w => w.Position);
                    break;

                default:
                    baseQuery = desc
                        ? baseQuery.OrderByDescending(w => w.Id)
                        : baseQuery.OrderBy(w => w.Id);
                    break;
            }

            var query = baseQuery.Select(w => new WorkerDto
            {
                Id = w.Id,
                FirstName = w.FirstName,
                LastName = w.LastName,
                Email = w.Email,
                PhoneNumber = w.PhoneNumber,
                Position = w.Position,
                UserId = w.UserId
            });

            var pagedResult = await _paginationService.GetPagedAsync(query, page, pageSize);

            var options = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
                .SetPriority(CacheItemPriority.Low);

            _cache.Set(cacheKey, pagedResult, options);

            _logger.LogInformation(
                "Loaded workers page {Page} with {Count}/{Total} items (sorted)",
                page,
                pagedResult.Items.Count,
                pagedResult.TotalItems
            );

            return pagedResult;
        }

        public async Task<int> CreateAsync(CreateWorkerDto dto)
        {
            _logger.LogInformation(
                "Creating worker: {FirstName} {LastName} ({Email})",
                dto.FirstName,
                dto.LastName,
                dto.Email
            );

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Attempted to create worker with existing email: {Email}", dto.Email);
                throw new DuplicateEntityException(dto.Email);
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user account for worker: {Errors}", errors);
                throw new InvalidOperationException($"Failed to create user account: {errors}");
            }

            await _userManager.AddToRoleAsync(user, "Worker");
            _logger.LogDebug("Assigned 'Worker' role to user {Email}", dto.Email);

            var worker = new Worker
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Position = dto.Position,
                UserId = user.Id
            };

            _context.Workers.Add(worker);
            await _context.SaveChangesAsync();

            CacheVersionHelper.BumpVersion(_cache, "Worker", _logger);

            _logger.LogInformation(
                "Worker created with ID {Id} ({FirstName} {LastName})",
                worker.Id,
                worker.FirstName,
                worker.LastName
            );

            return worker.Id;
        }

        public async Task<Worker?> GetByIdAsync(int id)
        {
            var version = CacheVersionHelper.GetVersion(_cache, "Worker");
            var cacheKey = $"Worker_v{version}_{id}";

            if (_cache.TryGetValue(cacheKey, out Worker? cached) && cached != null)
            {
                _logger.LogDebug("Cache HIT: Worker {Id}", id);
                return cached;
            }

            _logger.LogDebug("Cache MISS: Loading worker {Id} from DB", id);

            var worker = await _context.Workers.FindAsync(id);

            if (worker == null)
            {
                _logger.LogWarning("Worker {Id} not found", id);
                return null;
            }

            var options = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
                .SetPriority(CacheItemPriority.High);

            _cache.Set(cacheKey, worker, options);

            _logger.LogInformation(
                "Worker {Id} ({FirstName} {LastName}) loaded and cached",
                id,
                worker.FirstName,
                worker.LastName
            );

            return worker;
        }

        public async Task<Worker?> GetByUserIdAsync(string userId)
        {
            var version = CacheVersionHelper.GetVersion(_cache, "Worker");
            var cacheKey = $"Worker_v{version}_UserId_{userId}";

            if (_cache.TryGetValue(cacheKey, out Worker? cached) && cached != null)
            {
                _logger.LogDebug("Cache HIT: Worker with UserId {UserId}", userId);
                return cached;
            }

            _logger.LogDebug("Cache MISS: Loading worker with UserId {UserId} from DB", userId);

            var worker = await _context.Workers.FirstOrDefaultAsync(w => w.UserId == userId);

            if (worker == null)
            {
                _logger.LogWarning("Worker with UserId {UserId} not found", userId);
                return null;
            }

            var options = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
                .SetPriority(CacheItemPriority.High);

            _cache.Set(cacheKey, worker, options);

            return worker;
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogDebug("Deleting worker {Id}", id);

            var worker = await _context.Workers.FindAsync(id);

            if (worker == null)
            {
                _logger.LogWarning("Attempted to delete non-existing worker with ID {Id}", id);
                throw new EntityNotFoundException("Worker", id);
            }

            var user = await _userManager.FindByIdAsync(worker.UserId);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to delete user account for worker {Id}: {Errors}", id, errors);
                    throw new InvalidOperationException($"Failed to delete user account: {errors}");
                }
                _logger.LogDebug("Deleted user account for worker {Id}", id);
            }

            _context.Workers.Remove(worker);
            await _context.SaveChangesAsync();

            CacheVersionHelper.BumpVersion(_cache, "Worker", _logger);

            _logger.LogInformation(
                "Worker {Id} ({FirstName} {LastName}) deleted",
                id,
                worker.FirstName,
                worker.LastName
            );
        }

        public async Task<(bool Success, string Message, string? FirstName)> ApproveRegistrationByTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return (false, "Invalid approval link.", null);
                }

                var registration = await _context.PendingRegistrations
                    .FirstOrDefaultAsync(p => p.ApprovalToken == token && !p.IsProcessed);

                if (registration == null)
                {
                    return (false, "Registration request not found or already processed.", null);
                }

                if (registration.TokenExpiresAt < DateTime.UtcNow)
                {
                    return (false, "This approval link has expired.", null);
                }

                // Create user
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
                    return (false, $"Failed to create user: {errors}", null);
                }

                await _userManager.AddToRoleAsync(user, "Worker");

                // Create worker
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

                CacheVersionHelper.BumpVersion(_cache, "Worker", _logger);

                // Send confirmation email
                await _emailService.SendApprovalConfirmationAsync(registration.Email, registration.FirstName);

                _logger.LogInformation("Registration approved via token for {Email}", registration.Email);

                return (true, "Registration approved successfully.", registration.FirstName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving registration by token");
                return (false, "An error occurred while processing the approval.", null);
            }
        }

        public async Task<(bool Success, string Message, string? FirstName)> RejectRegistrationByTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return (false, "Invalid rejection link.", null);
                }

                var registration = await _context.PendingRegistrations
                    .FirstOrDefaultAsync(p => p.ApprovalToken == token && !p.IsProcessed);

                if (registration == null)
                {
                    return (false, "Registration request not found or already processed.", null);
                }

                if (registration.TokenExpiresAt < DateTime.UtcNow)
                {
                    return (false, "This rejection link has expired.", null);
                }

                var firstName = registration.FirstName;

                // Send rejection email
                await _emailService.SendRejectionNotificationAsync(registration.Email, registration.FirstName);

                _context.PendingRegistrations.Remove(registration);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Registration rejected via token for {Email}", registration.Email);

                return (true, "Registration rejected successfully.", firstName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting registration by token");
                return (false, "An error occurred while processing the rejection.", null);
            }
        }

        public async Task<(bool Success, string Message, string? FirstName)> ApproveRegistrationByIdAsync(int id)
        {
            try
            {
                var registration = await _context.PendingRegistrations.FindAsync(id);
                if (registration == null || registration.IsProcessed)
                {
                    return (false, "Registration request not found or already processed.", null);
                }

                // Create user
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
                    return (false, $"Failed to create user: {errors}", null);
                }

                await _userManager.AddToRoleAsync(user, "Worker");

                // Create worker
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

                CacheVersionHelper.BumpVersion(_cache, "Worker", _logger);

                // Send confirmation email
                await _emailService.SendApprovalConfirmationAsync(registration.Email, registration.FirstName);

                _logger.LogInformation("Registration approved via admin dashboard for {Email}", registration.Email);

                return (true, $"Worker {registration.FirstName} {registration.LastName} approved successfully.", registration.FirstName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving registration by ID {Id}", id);
                return (false, "An error occurred while approving the registration.", null);
            }
        }

        public async Task<(bool Success, string Message, string? FirstName)> RejectRegistrationByIdAsync(int id)
        {
            try
            {
                var registration = await _context.PendingRegistrations.FindAsync(id);
                if (registration == null || registration.IsProcessed)
                {
                    return (false, "Registration request not found or already processed.", null);
                }

                var firstName = registration.FirstName;

                // Send rejection email
                await _emailService.SendRejectionNotificationAsync(registration.Email, registration.FirstName);

                _context.PendingRegistrations.Remove(registration);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Registration rejected via admin dashboard for {Email}", registration.Email);

                return (true, $"Registration from {registration.FirstName} {registration.LastName} has been rejected.", firstName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting registration by ID {Id}", id);
                return (false, "An error occurred while rejecting the registration.", null);
            }
        }
    }
}