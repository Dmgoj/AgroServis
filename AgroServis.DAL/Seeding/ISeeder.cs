using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroServis.DAL.Seeding
{
    public interface ISeeder
    {
        Task SeedAsync(ApplicationDbContext context);
    }
}
