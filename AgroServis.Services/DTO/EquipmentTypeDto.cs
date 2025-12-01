using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgroServis.DAL.Entities;

namespace AgroServis.Services.DTO
{
    public record EquipmentTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int EquipmentCategoryId { get; set; }
    }
}