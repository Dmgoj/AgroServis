using AgroServis.Services.DTO;
using AgroServis.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroServis.Services
{
    public interface IMaintenanceService
    {
        Task<PagedResult<MaintenanceDto>> GetAllAsync(int page, int pageSize);
        Task<MaintenanceDto> GetByIdAsync(int id);
        Task<MaintenanceEditDto> GetByIdForEditAsync(int id);
        Task<MaintenanceCreateDto> GetForCreateAsync();
        Task<int> CreateAsync(MaintenanceCreateDto dto);
        Task UpdateAsync(MaintenanceUpdateDto dto);
        Task DeleteAsync(int id);
    }
}