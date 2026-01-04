using AgroServis.DAL;
using AgroServis.DAL.Entities;
using AgroServis.Services.DTO;
using AgroServis.Services.DTOs;
using AgroServis.Services.Exceptions;
using AgroServis.Services.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            ILogger<MaintenanceService> logger)
        {
            _context = context;
            _paginationService = paginationService;
            _cache = cache;
            _logger = logger;
        }


        public async Task<int> CreateAsync(MaintenanceCreateDto dto)
        {
            _logger.LogInformation("Creating maintenance for equipment {EquipmentId}", dto.EquipmentId);

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
                CreatedAt = DateTime.Now
            };

            _context.MaintenanceRecords.Add(maintenance);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Maintenance created with ID {Id}", maintenance.Id);

            return maintenance.Id;
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<MaintenanceDto>> GetAllAsync(int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<MaintenanceDto> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<MaintenanceEditDto> GetByIdForEditAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<MaintenanceCreateDto> GetForCreateAsync()
        {
            var availableEquipment = await _context.Equipment
        .Select(e => new EquipmentDto
        {
            Id = e.Id,
            Manufacturer = e.Manufacturer,
            Model = e.Model,
            SerialNumber = e.SerialNumber,
            EquipmentTypeId = e.EquipmentTypeId,
            EquipmentType = e.EquipmentType.Type,
            EquipmentCategory = e.EquipmentType.EquipmentCategory.Category
        })
        .ToListAsync();

            return new MaintenanceCreateDto
            {
                AvailableEquipment = availableEquipment
            };
        }

        public Task UpdateAsync(MaintenanceUpdateDto dto)
        {
            throw new NotImplementedException();
        }
    }
}