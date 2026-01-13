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

        public async Task<int> CreateAsync(MaintenanceCreateDto dto)
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
                Type = dto.Type,
                Status = dto.Status,
                Cost = dto.Cost,
                Notes = dto.Notes,
                CreatedAt = DateTime.Now,
            };

            _context.MaintenanceRecords.Add(maintenance);
            await _context.SaveChangesAsync();

            CacheHelper.InvalidatePaginationCaches(_cache, _logger, "Maintenance");
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
            _cache.Remove($"Maintenance_{id}");

            CacheHelper.InvalidatePaginationCaches(_cache, _logger, "Maintenance");
        }

        public async Task<PagedResult<MaintenanceDto>> GetAllAsync(int page, int pageSize)
        {
            var cacheKey = $"MaintenancePage_{page}_Size_{pageSize}";

            if (
                _cache.TryGetValue(cacheKey, out PagedResult<MaintenanceDto>? cached)
                && cached != null
            )
            {
                _logger.LogDebug("Cache HIT: Maintenance page {Page}", page);
                return cached;
            }

            _logger.LogDebug("Cache MISS: Maintenance page {Page}, querying database", page);

            var query = _context
                .MaintenanceRecords.OrderBy(e => e.Id)
                .Include(m => m.Equipment)
                .ThenInclude(e => e.EquipmentType)
                .ThenInclude(et => et.EquipmentCategory)
                .OrderByDescending(m => m.MaintenanceDate)
                .Select(m => new MaintenanceDto
                {
                    Id = m.Id,
                    EquipmentId = m.EquipmentId,
                    EquipmentName = $"{m.Equipment.Manufacturer} {m.Equipment.Model}",
                    EquipmentSerialNumber = m.Equipment.SerialNumber,
                    MaintenanceDate = m.MaintenanceDate,
                    Description = m.Description,
                    Type = m.Type,
                    Status = m.Status,
                    Cost = m.Cost,
                    Notes = m.Notes,
                    PerformedBy = m.PerformedBy,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt,
                });

            var result = await _paginationService.GetPagedAsync(query, page, pageSize);

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                .SetPriority(CacheItemPriority.Low);

            _cache.Set(cacheKey, result, cacheOptions);

            _logger.LogInformation(
                "Maintenance page {Page} loaded and cached with {Count} items",
                page,
                result.Items.Count
            );

            return result;
        }

        public async Task<MaintenanceDto> GetByIdAsync(int id)
        {
            var cacheKey = $"Maintenance_{id}";

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
                    PerformedBy = m.PerformedBy,

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
            var maintenance = await _context.MaintenanceRecords.FirstOrDefaultAsync(e =>
                e.Id == id
            );

            if (maintenance == null)
            {
                throw new EntityNotFoundException("Maintenance", id);
            }

            MaintenanceEditDto maintenanceEditDto = new MaintenanceEditDto
            {
                Id = maintenance.Id,
                EquipmentId = maintenance.EquipmentId,
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

        public Task UpdateAsync(MaintenanceUpdateDto dto)
        {
            throw new NotImplementedException();
        }
    }
}