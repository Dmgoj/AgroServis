using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgroServis.Services.DTO;
using AgroServis.Services.DTOs;

namespace AgroServis.Services
{
    public interface IMaintenanceService
    {
        Task<PagedResult<MaintenanceDto>> GetAllAsync(
            int page,
            int pageSize,
            string? sortBy = null,
            string? sortDir = null
        );
        Task<MaintenanceDto> GetByIdAsync(int id);
        Task<MaintenanceEditDto> GetByIdForEditAsync(int id);
        Task<MaintenanceCreateDto> GetForCreateAsync();
        Task<int> CreateAsync(MaintenanceCreateDto dto);
        Task UpdateAsync(MaintenanceUpdateDto dto);
        Task DeleteAsync(int id);
    }
}