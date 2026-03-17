using AgroServis.DAL;
using AgroServis.DAL.Entities;
using AgroServis.Services.DTO;
using AgroServis.Services.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AgroServis.Services
{
    public class MaintenanceScheduleService : IMaintenanceScheduleService
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceScheduleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAsync(MaintenanceScheduleCreateDto dto)
        {
            var equipmentExists = await _context.Equipment
                .AnyAsync(e => e.Id == dto.EquipmentId);

            if (!equipmentExists)
                throw new EntityNotFoundException("Equipment", dto.EquipmentId);

            var baseDate = dto.LastPerformedAt ?? DateTime.UtcNow;

            var schedule = new MaintenanceSchedule
            {
                EquipmentId = dto.EquipmentId,
                Title = dto.Title,
                Description = dto.Description,
                IntervalDays = dto.IntervalDays,
                LastPerformedAt = dto.LastPerformedAt,
                NextDueDate = baseDate.AddDays(dto.IntervalDays),
                CreatedAt = DateTime.UtcNow
            };

            _context.MaintenanceSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            return schedule.Id;
        }

        public async Task<List<MaintenanceScheduleDto>> GetAllAsync()
        {
            return await _context.MaintenanceSchedules
                .Include(s => s.Equipment)
                .Select(s => new MaintenanceScheduleDto
                {
                    Id = s.Id,
                    EquipmentId = s.EquipmentId,
                    EquipmentName = s.Equipment.Manufacturer + " " + s.Equipment.Model,
                    Title = s.Title,
                    Description = s.Description,
                    IntervalDays = s.IntervalDays,
                    LastPerformedAt = s.LastPerformedAt,
                    NextDueDate = s.NextDueDate,
                    IsActive = s.IsActive
                })
                .ToListAsync();
        }

        public async Task<MaintenanceScheduleDto?> GetByIdAsync(int id)
        {
            return await _context.MaintenanceSchedules
                .Include(s => s.Equipment)
                .Where(s => s.Id == id)
                .Select(s => new MaintenanceScheduleDto
                {
                    Id = s.Id,
                    EquipmentId = s.EquipmentId,
                    EquipmentName = s.Equipment.Manufacturer + " " + s.Equipment.Model,
                    Title = s.Title,
                    Description = s.Description,
                    IntervalDays = s.IntervalDays,
                    LastPerformedAt = s.LastPerformedAt,
                    NextDueDate = s.NextDueDate,
                    IsActive = s.IsActive
                })
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(MaintenanceScheduleUpdateDto dto)
        {
            var schedule = await _context.MaintenanceSchedules
                .FirstOrDefaultAsync(s => s.Id == dto.Id);

            if (schedule == null)
                throw new EntityNotFoundException("MaintenanceSchedule", dto.Id);

            schedule.EquipmentId = dto.EquipmentId;
            schedule.Title = dto.Title;
            schedule.Description = dto.Description;
            schedule.IntervalDays = dto.IntervalDays;
            schedule.LastPerformedAt = dto.LastPerformedAt;
            schedule.IsActive = dto.IsActive;
            schedule.UpdatedAt = DateTime.UtcNow;

            var baseDate = dto.LastPerformedAt ?? DateTime.UtcNow;
            schedule.NextDueDate = baseDate.AddDays(dto.IntervalDays);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var schedule = await _context.MaintenanceSchedules
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
                throw new EntityNotFoundException("MaintenanceSchedule", id);

            _context.MaintenanceSchedules.Remove(schedule);

            await _context.SaveChangesAsync();
        }

        public async Task<MaintenanceScheduleCreateDto> GetForCreateAsync()
        {
            var availableEquipment = await _context.Equipment
                .AsNoTracking()
                .OrderBy(e => e.Manufacturer)
                .ThenBy(e => e.Model)
                .Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.Manufacturer} {e.Model} (SN: {e.SerialNumber})"
                })
                .ToListAsync();

            return new MaintenanceScheduleCreateDto
            {
                AvailableEquipment = availableEquipment
            };
        }

        public async Task<MaintenanceScheduleUpdateDto?> GetForEditAsync(int id)
        {
            var schedule = await _context.MaintenanceSchedules
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
                return null;

            var availableEquipment = await _context.Equipment
                .AsNoTracking()
                .OrderBy(e => e.Manufacturer)
                .ThenBy(e => e.Model)
                .Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.Manufacturer} {e.Model} (SN: {e.SerialNumber})",
                    Selected = e.Id == schedule.EquipmentId
                })
                .ToListAsync();

            return new MaintenanceScheduleUpdateDto
            {
                Id = schedule.Id,
                EquipmentId = schedule.EquipmentId,
                Title = schedule.Title,
                Description = schedule.Description,
                IntervalDays = schedule.IntervalDays,
                LastPerformedAt = schedule.LastPerformedAt,
                IsActive = schedule.IsActive,
                AvailableEquipment = availableEquipment
            };
        }
    }
}