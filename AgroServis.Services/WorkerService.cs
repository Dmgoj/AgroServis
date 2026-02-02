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

        public WorkerService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IPaginationService paginationService,
            IMemoryCache cache,
            ILogger<WorkerService> logger)
        {
            _context = context;
            _userManager = userManager;
            _paginationService = paginationService;
            _cache = cache;
            _logger = logger;
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
    }
}
}