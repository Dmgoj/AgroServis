using AgroServis.DAL.Entities;
using AgroServis.Services.DTO;

namespace AgroServis.Services
{
    public interface IWorkerService
    {
        Task<PagedResult<WorkerDto>> GetAllAsync(
            int page,
            int pageSize,
            string? sortBy = null,
            string? sortDir = null,
            string? search = null
        );
        Task<Worker?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateWorkerDto dto);
        Task DeleteAsync(int id);
        Task<Worker?> GetByUserIdAsync(string userId);
        Task<(bool Success, string Message, string? FirstName)> ApproveRegistrationByTokenAsync(string token);
        Task<(bool Success, string Message, string? FirstName)> RejectRegistrationByTokenAsync(string token);
        Task<(bool Success, string Message, string? FirstName)> ApproveRegistrationByIdAsync(int id);
        Task<(bool Success, string Message, string? FirstName)> RejectRegistrationByIdAsync(int id);
        Task<List<PendingRegistration>> GetPendingRegistrationsAsync();
    }
}