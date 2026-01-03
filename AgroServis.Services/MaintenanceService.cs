using AgroServis.DAL;
using AgroServis.Services.DTO;
using AgroServis.Services.DTOs;
using AgroServis.Services.Utilities;
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

        public Task<int> CreateAsync(MaintenanceCreateDto dto)
        {
            throw new NotImplementedException();
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

        public Task<MaintenanceEditDto> GetForCreateAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(MaintenanceUpdateDto dto)
        {
            throw new NotImplementedException();
        }
    }
}