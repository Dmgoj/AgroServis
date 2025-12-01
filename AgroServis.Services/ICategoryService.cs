using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroServis.Services
{
    public interface ICategoryService
    {
        public Task<CategoryDto> GetAllAsync();
        public Task<CategoryDto> GetByIdAsync(int id);
        public Task CreateAsync(CategoryDto category);
        public Task UpdateAsync(CategoryDto category);
        public Task DeleteAsync(int id);
    }
}