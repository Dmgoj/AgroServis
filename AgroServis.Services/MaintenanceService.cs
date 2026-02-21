using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgroServis.DAL;
using AgroServis.DAL.Entities;
using AgroServis.Services.DTO;
using AgroServis.Services.DTOs;
using AgroServis.Services.Exceptions;
using AgroServis.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AgroServis.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaginationService _paginationService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<MaintenanceService> _logger;

        public MaintenanceService(
            ApplicationDbContext context,
            IPaginationService paginationService,
            IMemoryCache cache,
            ILogger<MaintenanceService> logger
        )
        {
            _context = context;
            _paginationService = paginationService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<int> CreateAsync(MaintenanceCreateDto dto, string? performedByUserId)
        {
            _logger.LogInformation(
                "Creating maintenance for equipment {EquipmentId}",
                dto.EquipmentId
            );

            var equipmentExists = await _context.Equipment.AnyAsync(e => e.Id == dto.EquipmentId);

            if (!equipmentExists)
            {
                throw new EntityNotFoundException("equipment", dto.EquipmentId);
            }

            var maintenance = new Maintenance
            {
                EquipmentId = dto.EquipmentId,
                MaintenanceDate = dto.MaintenanceDate,
                Description = dto.Description,
                PerformedBy = performedByUserId,
                Type = dto.Type,
                Status = dto.Status,
                Cost = dto.Cost,
                Notes = dto.Notes,
                CreatedAt = DateTime.Now,
            };

            _context.MaintenanceRecords.Add(maintenance);
            await _context.SaveChangesAsync();
            CacheVersionHelper.BumpVersion(_cache, "Equipment", _logger);
            CacheVersionHelper.BumpVersion(_cache, "Maintenance", _logger);
            _logger.LogInformation("Maintenance created with ID {Id}", maintenance.Id);

            return maintenance.Id;
        }

        public async Task DeleteAsync(int id)
        {
            var maintenance = await _context.MaintenanceRecords.FirstOrDefaultAsync(m =>
                m.Id == id
            );

            if (maintenance == null)
            {
                throw new KeyNotFoundException($"Maintenance with ID {id} not found.");
            }

            maintenance.DeletedAt = DateTime.UtcNow;
            maintenance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            CacheVersionHelper.BumpVersion(_cache, "Maintenance", _logger);
        }

        public async Task<PagedResult<MaintenanceDto>> GetAllAsync(
            int page,
            int pageSize,
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
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "maintenancedate",
                "type",
                "status",
                "equipment",
                "createdat",
            };
            var key = (sortBy ?? "maintenancedate").Trim();
            if (!allowed.Contains(key))
                key = "maintenancedate";
            var dir = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase)
                ? "desc"
                : "asc";
            var cacheKey =
                $"MaintenancePage_{page}_Size_{pageSize}_Sort_{key}_{dir}_Q_{(search ?? "")}_E_{(equipmentId?.ToString() ?? "")}_T_{(type ?? "")}_S_{(status ?? "")}_DF_{(dateFrom?.ToString("s") ?? "")}_DT_{(dateTo?.ToString("s") ?? "")}";

            if (
                _cache.TryGetValue(cacheKey, out PagedResult<MaintenanceDto>? cached)
                && cached != null
            )
            {
                return cached;
            }

            var baseQuery = _context
                .MaintenanceRecords.Include(m => m.Equipment)
                .ThenInclude(e => e.EquipmentType)
                .ThenInclude(et => et.EquipmentCategory)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                baseQuery = baseQuery.Where(m =>
                    EF.Functions.Like(m.Description, $"%{s}%")
                    || EF.Functions.Like(m.Notes, $"%{s}%")
                    || EF.Functions.Like(m.PerformedBy, $"%{s}%")
                );
            }

            if (equipmentId.HasValue)
                baseQuery = baseQuery.Where(m => m.EquipmentId == equipmentId.Value);
            if (!string.IsNullOrWhiteSpace(type))
                baseQuery = baseQuery.Where(m => m.Type.ToString() == type);
            if (!string.IsNullOrWhiteSpace(status))
                baseQuery = baseQuery.Where(m => m.Status.ToString() == status);
            if (dateFrom.HasValue)
                baseQuery = baseQuery.Where(m => m.MaintenanceDate >= dateFrom.Value);
            if (dateTo.HasValue)
                baseQuery = baseQuery.Where(m => m.MaintenanceDate <= dateTo.Value);

            var desc = dir == "desc";
            switch (key.ToLowerInvariant())
            {
                case "maintenancedate":
                    baseQuery = desc
                        ? baseQuery.OrderByDescending(m => m.MaintenanceDate)
                        : baseQuery.OrderBy(m => m.MaintenanceDate);
                    break;

                case "type":
                    baseQuery = desc
                        ? baseQuery.OrderByDescending(m => m.Type)
                        : baseQuery.OrderBy(m => m.Type);
                    break;

                case "status":
                    baseQuery = desc
                        ? baseQuery.OrderByDescending(m => m.Status)
                        : baseQuery.OrderBy(m => m.Status);
                    break;

                case "equipment":
                    baseQuery = desc
                        ? baseQuery
                            .OrderByDescending(m => m.Equipment.Manufacturer)
                            .ThenByDescending(m => m.Equipment.Model)
                        : baseQuery
                            .OrderBy(m => m.Equipment.Manufacturer)
                            .ThenBy(m => m.Equipment.Model);
                    break;

                case "createdat":
                    baseQuery = desc
                        ? baseQuery.OrderByDescending(m => m.CreatedAt)
                        : baseQuery.OrderBy(m => m.CreatedAt);
                    break;

                default:
                    baseQuery = baseQuery.OrderByDescending(m => m.MaintenanceDate);
                    break;
            }

            var query = baseQuery.Select(m => new MaintenanceDto
            {
                Id = m.Id,
                EquipmentId = m.EquipmentId,
                EquipmentName = $"{m.Equipment.Manufacturer} {m.Equipment.Model}",
                EquipmentSerialNumber = m.Equipment.SerialNumber,
                MaintenanceDate = m.MaintenanceDate,
                Type = m.Type,
                Status = m.Status,
                Cost = m.Cost,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
            });

            var result = await _paginationService.GetPagedAsync(query, page, pageSize);

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                .SetPriority(CacheItemPriority.Low);

            _cache.Set(cacheKey, result, cacheOptions);
            return result;
        }

        public async Task<MaintenanceDto> GetByIdAsync(int id)
        {
            var version = CacheVersionHelper.GetVersion(_cache, "Maintenance");
            var cacheKey = $"Maintenance_v{version}_{id}";

            if (_cache.TryGetValue(cacheKey, out MaintenanceDto cached))
            {
                _logger.LogDebug("Cache HIT: Maintenance {Id}", id);
                return cached;
            }

            var maintenance = await _context
                .MaintenanceRecords.AsNoTracking()
                .Where(m => m.Id == id)
                .Select(m => new MaintenanceDto
                {
                    Id = m.Id,
                    EquipmentId = m.EquipmentId,
                    EquipmentName = m.Equipment.Model,
                    EquipmentSerialNumber = m.Equipment.SerialNumber,

                    MaintenanceDate = m.MaintenanceDate,
                    Description = m.Description,
                    Type = m.Type,
                    Status = m.Status,

                    Cost = m.Cost,
                    Notes = m.Notes,
                    PerformedBy = m.PerformedByUser != null ? m.PerformedByUser.UserName : null,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt,
                })
                .FirstOrDefaultAsync();

            if (maintenance == null)
            {
                throw new EntityNotFoundException("Maintenance", id);
            }

            _cache.Set(
                cacheKey,
                maintenance,
                new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5),
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
                }
            );

            return maintenance;
        }

        public async Task<MaintenanceEditDto> GetByIdForEditAsync(int id)
        {
            var maintenance = await _context
                .MaintenanceRecords.Include(e => e.Equipment)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (maintenance == null)
            {
                throw new EntityNotFoundException("Maintenance", id);
            }

            MaintenanceEditDto maintenanceEditDto = new MaintenanceEditDto
            {
                Id = maintenance.Id,
                EquipmentId = maintenance.EquipmentId,
                EquipmentName =
                    $"{maintenance.Equipment.Manufacturer} {maintenance.Equipment.Model}",
                MaintenanceDate = maintenance.MaintenanceDate,
                Description = maintenance.Description,
                Type = maintenance.Type,
                Status = maintenance.Status,
                Cost = maintenance.Cost,
                Notes = maintenance.Notes,
                PerformedBy = maintenance.PerformedBy,
            };

            return maintenanceEditDto;
        }

        public async Task<MaintenanceCreateDto> GetForCreateAsync()
        {
            var availableEquipment = await _context
                .Equipment.Select(e => new EquipmentDto
                {
                    Id = e.Id,
                    Manufacturer = e.Manufacturer,
                    Model = e.Model,
                    SerialNumber = e.SerialNumber,
                    EquipmentTypeId = e.EquipmentTypeId,
                    EquipmentType = e.EquipmentType.Type,
                    EquipmentCategory = e.EquipmentType.EquipmentCategory.Category,
                })
                .ToListAsync();

            return new MaintenanceCreateDto { AvailableEquipment = availableEquipment };
        }

        public async Task UpdateAsync(MaintenanceUpdateDto dto)
        {
            _logger.LogDebug("Updating maintenance {Id}", dto.Id);

            var maintenance = await _context.MaintenanceRecords.FindAsync(dto.Id);

            if (maintenance == null)
            {
                _logger.LogWarning("Cannot update maintenance {Id} - not found", dto.Id);
                throw new EntityNotFoundException("Maintenance", dto.Id);
            }

            maintenance.EquipmentId = dto.EquipmentId;
            maintenance.MaintenanceDate = dto.MaintenanceDate;
            maintenance.Description = dto.Description;
            maintenance.Type = dto.Type;
            maintenance.Status = dto.Status;
            maintenance.Cost = dto.Cost;
            maintenance.Notes = dto.Notes;
            maintenance.PerformedBy = dto.PerformedBy;
            maintenance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            CacheVersionHelper.BumpVersion(_cache, "Maintenance", _logger);

            _logger.LogInformation("Maintenance {Id} updated successfully", dto.Id);
        }
    }
}