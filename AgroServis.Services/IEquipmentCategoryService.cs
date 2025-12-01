using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgroServis.Services.DTO;

namespace AgroServis.Services
{
    public interface IEquipmentCategoryService
    {
        Task<PagedResult<CategoryDto>> GetAllAsync(int page, int pageSize);
        Task<CategoryDto> GetByIdAsync(int id);
        Task CreateAsync(CategoryDto dto);
        Task UpdateAsync(CategoryDto dto);
        Task DeleteAsync(int id);
    }
}