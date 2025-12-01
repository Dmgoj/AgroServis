using AgroServis.DAL.Entities;
using AgroServis.Services.DTO;

namespace AgroServis.Services
{
    public interface IEquipmentService
    {
        Task<PagedResult<EquipmentDto>> GetAllAsync(int page, int pageSize);
        Task<EquipmentDto> GetByIdAsync(int id);
        Task<EquipmentEditDto> GetByIdForEditAsync(int id);
        Task<EquipmentEditDto> GetForCreateAsync();
        Task<int> CreateAsync(EquipmentCreateDto dto);
        Task UpdateAsync(EquipmentUpdateDto dto);
        Task DeleteAsync(int id);
    }
}