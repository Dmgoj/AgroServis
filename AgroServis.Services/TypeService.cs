using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroServis.Services
{
    internal class TypeService : ITypeService
    {
        public Task<TypeDto> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<TypeDto> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task CreateAsync(TypeDto category)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(TypeDto category)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}