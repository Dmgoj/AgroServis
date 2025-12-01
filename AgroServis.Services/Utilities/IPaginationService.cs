using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgroServis.Services.DTO;

namespace AgroServis.Services.Utilities
{
    public interface IPaginationService
    {
        Task<PagedResult<T>> GetPagedAsync<T>(
      IQueryable<T> query,
      int pageNumber,
      int pageSize,
      CancellationToken cancellationToken = default);
    }
}
