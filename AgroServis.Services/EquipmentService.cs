using AgroServis.DAL;
using AgroServis.DAL.Entities;
using AgroServis.Services.DTO;
using AgroServis.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using AgroServis.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroServis.Services
{
    public class EquipmentService : IEquipmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaginationService _paginationService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<EquipmentService> _logger;

        public EquipmentService(ApplicationDbContext context, IPaginationService paginationService, IMemoryCache cache, ILogger<EquipmentService> logger)
        {
            _context = context;
            _paginationService = paginationService;
            _cache = cache;
            _logger = logger;
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogDebug("Deleting equipment {Id}", id);

            var forDelete = await _context.Equipment.FindAsync(id);

            if (forDelete == null)
            {
                _logger.LogWarning("Attempted to delete non-existing equipment with ID {Id}.", id);
                throw new EntityNotFoundException("Equipment", id);
            }

            _context.Remove(forDelete);
            await _context.SaveChangesAsync();

            _cache.Remove($"Equipment_{id}");

            CacheHelper.InvalidatePaginationCaches(_cache, _logger, "Equipment");

            _logger.LogInformation(
               "Equipment {Id} ({Manufacturer} {Model}) deleted",
               id,
               forDelete.Manufacturer,
               forDelete.Model);
        }
        public async Task<PagedResult<EquipmentDto>> GetAllAsync(int page, int pageSize)
        {
            var cacheKey = $"EquipmentPage_{page}_Size_{pageSize}";

            if (_cache.TryGetValue(cacheKey, out PagedResult<EquipmentDto>? cachedResult) && cachedResult != null)
            {
                _logger.LogDebug("Cache HIT: Returning page {Page} (size {PageSize}) from cache", page, pageSize);
                return cachedResult!;
            }

            _logger.LogDebug("Cache MISS: Loading page {Page} (size {PageSize}) from database", page, pageSize);

            var query = _context.Equipment
             .OrderBy(e => e.Id)
             .Select(e => new EquipmentDto
             {
                 Id = e.Id,
                 Manufacturer = e.Manufacturer,
                 Model = e.Model,
                 SerialNumber = e.SerialNumber,
                 EquipmentTypeId = e.EquipmentTypeId,
                 EquipmentType = e.EquipmentType.Type,
                 EquipmentCategory = e.EquipmentType.EquipmentCategory.Category,
                 LastMaintenanceDate = e.MaintenanceRecords
                     .OrderByDescending(m => m.MaintenanceDate)
                     .Select(m => (DateTime?)m.MaintenanceDate)
                     .FirstOrDefault()
             });

            var pagedResult = await _paginationService.GetPagedAsync(query, page, pageSize);

            var options = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
                .SetPriority(CacheItemPriority.Low);

            _cache.Set(cacheKey, pagedResult, options);

            _logger.LogInformation("Loaded equipment page {Page} with {ItemCount}/{TotalItems} items", page, pagedResult.Items.Count, pagedResult.TotalItems);

            return pagedResult;
        }
        public async Task<EquipmentDto> GetByIdAsync(int id)
        {
            var cacheKey = $"Equipment_{id}";

            if (_cache.TryGetValue(cacheKey, out EquipmentDto? cachedResult) && cachedResult != null)
            {
                _logger.LogDebug("Cache HIT: Equipment {Id}", id);
                return cachedResult!;
            }

            var equipment = await _context.Equipment
               .Include(e => e.EquipmentType)
               .ThenInclude(et => et.EquipmentCategory)
               .FirstOrDefaultAsync(e => e.Id == id);

            if (equipment == null)
            {
                _logger.LogWarning("Equipment {Id} not found", id);
                return null;
            }

            var equipmentDto = new EquipmentDto
            {
                Id = equipment.Id,
                Manufacturer = equipment.Manufacturer,
                Model = equipment.Model,
                SerialNumber = equipment.SerialNumber,
                EquipmentTypeId = equipment.EquipmentTypeId,
                EquipmentType = equipment.EquipmentType.Type,
                EquipmentCategory = equipment.EquipmentType.EquipmentCategory.Category
            };

            var options = new MemoryCacheEntryOptions()
               .SetSlidingExpiration(TimeSpan.FromMinutes(2))
               .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
               .SetPriority(CacheItemPriority.High);

            _cache.Set(cacheKey, equipmentDto, options);

            _logger.LogInformation(
                "Equipment {Id} ({Manufacturer} {Model}) loaded and cached",
                id,
                equipment.Manufacturer,
                equipment.Model);

            return equipmentDto;
        }

        public async Task<EquipmentEditDto> GetByIdForEditAsync(int id)
        {
            var equipment = await _context.Equipment
                .Include(e => e.EquipmentType)
                .ThenInclude(et => et.EquipmentCategory)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipment == null)
            {
                throw new EntityNotFoundException("Equipment", id);
            }

            var types = await GetCachedEquipmentTypesAsync();
            var categories = await GetCachedEquipmentCategoriesAsync();

            EquipmentEditDto equipmentEditDto = new EquipmentEditDto
            {
                Id = equipment.Id,
                Manufacturer = equipment.Manufacturer,
                Model = equipment.Model,
                SerialNumber = equipment.SerialNumber,
                EquipmentTypeId = equipment.EquipmentTypeId,
                EquipmentType = equipment.EquipmentType,
                EquipmentCategory = equipment.EquipmentType.EquipmentCategory,
                EquipmentTypes = types,
                EquipmentCategories = categories
            };

            return equipmentEditDto;
        }

        public async Task<EquipmentEditDto> GetForCreateAsync()
        {
            var types = await GetCachedEquipmentTypesAsync();

            var categories = await GetCachedEquipmentCategoriesAsync();

            return new EquipmentEditDto
            {
                EquipmentTypes = types,
                EquipmentCategories = categories
            };
        }

        public async Task<int> CreateAsync(EquipmentCreateDto dto)
        {
            _logger.LogInformation(
               "Creating equipment: {Manufacturer} {Model}",
               dto.Manufacturer,
               dto.Model);

            var existingEquipment = await _context.Equipment
                .AnyAsync(e => e.SerialNumber == dto.SerialNumber);

            if (existingEquipment)
            {
                throw new DuplicateEntityException(dto.SerialNumber);
            }

            var equipment = new Equipment
            {
                Manufacturer = dto.Manufacturer,
                Model = dto.Model,
                SerialNumber = dto.SerialNumber,
                EquipmentTypeId = dto.EquipmentTypeId
            };
            _context.Equipment.Add(equipment);
            await _context.SaveChangesAsync();

            CacheHelper.InvalidatePaginationCaches(_cache, _logger, "Equipment");

            _logger.LogInformation("Equipment created with ID {Id}", equipment.Id);

            return equipment.Id;
        }

        public async Task UpdateAsync(EquipmentUpdateDto dto)
        {
            _logger.LogDebug("Updating equipment {Id}", dto.Id);

            var equipment = await _context.Equipment.FindAsync(dto.Id);

            if (equipment == null)
            {
                _logger.LogWarning("Cannot update equipment {Id} - not found", dto.Id);
                throw new EntityNotFoundException("Equipment", dto.Id);
            }

            equipment.Manufacturer = dto.Manufacturer;
            equipment.Model = dto.Model;
            equipment.SerialNumber = dto.SerialNumber;
            equipment.EquipmentTypeId = dto.EquipmentTypeId;

            await _context.SaveChangesAsync();
            _cache.Remove($"Equipment_{dto.Id}");

            CacheHelper.InvalidatePaginationCaches(_cache, _logger, "Equipment");

            _logger.LogInformation("Equipment {Id} updated successfully", dto.Id);
        }

        private async Task<List<EquipmentType>> GetCachedEquipmentTypesAsync()
        {
            var cacheKey = "EquipmentTypes_All";

            if (!_cache.TryGetValue(cacheKey, out List<EquipmentType>? types) || types == null)
            {
                _logger.LogDebug("Loading equipment types from database");

                types = await _context.EquipmentTypes.ToListAsync();

                var options = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(4))
                    .SetPriority(CacheItemPriority.NeverRemove);

                _cache.Set(cacheKey, types, options);
            }

            return types;
        }

        private async Task<List<EquipmentCategory>> GetCachedEquipmentCategoriesAsync()
        {
            var cacheKey = "EquipmentCategories_All";

            if (!_cache.TryGetValue(cacheKey, out List<EquipmentCategory>? categories) || categories == null)
            {
                _logger.LogDebug("Loading equipment categories from database");

                categories = await _context.EquipmentCategories.ToListAsync();

                var options = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(4))
                    .SetPriority(CacheItemPriority.NeverRemove);

                _cache.Set(cacheKey, categories, options);
            }

            return categories;
        }
    }
}