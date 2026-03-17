using AgroServis.Services.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroServis.Services
{
    public interface IMaintenanceScheduleService
    {
        Task<int> CreateAsync(MaintenanceScheduleCreateDto dto);

        Task<List<MaintenanceScheduleDto>> GetAllAsync();

        Task<MaintenanceScheduleDto?> GetByIdAsync(int id);

        Task UpdateAsync(MaintenanceScheduleUpdateDto dto);

        Task DeleteAsync(int id);
        Task<MaintenanceScheduleCreateDto> GetForCreateAsync();
        Task<MaintenanceScheduleUpdateDto?> GetForEditAsync(int id);
    }
}