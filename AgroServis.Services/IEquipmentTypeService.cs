using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgroServis.Services.DTO;

namespace AgroServis.Services
{
    public interface IEquipmentTypeService
    {
        Task<PagedResult<EquipmentTypeDto>> GetAllAsync(int page, int pageSize);
        Task<EquipmentTypeDto> GetByIdAsync(int id);
        Task CreateAsync(EquipmentTypeDto dto);
        Task UpdateAsync(EquipmentTypeDto dto);
        Task DeleteAsync(int id);
    }
}