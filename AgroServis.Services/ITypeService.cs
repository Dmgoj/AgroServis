using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroServis.Services
{
    public interface ITypeService
    {
        public Task<TypeDto> GetAllAsync();
        public Task<TypeDto> GetByIdAsync(int id);
        public Task CreateAsync(TypeDto category);
        public Task UpdateAsync(TypeDto category);
        public Task DeleteAsync(int id);
    }
}