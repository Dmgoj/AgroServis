using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgroServis.DAL.Entities;

namespace AgroServis.DAL.Seeding
{
    public class EquipmentSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.EquipmentCategories.Any())
            {
                return;
            }
            var equipmentCategories = new List<EquipmentCategory>
            {
                new EquipmentCategory { Category = "Cultivation" },
                new EquipmentCategory { Category = "Planting" },
                new EquipmentCategory { Category = "Irrigation" },
                new EquipmentCategory { Category = "Protection" },
                new EquipmentCategory { Category = "Harvest" },
                new EquipmentCategory { Category = "Transport" },
                new EquipmentCategory { Category = "Precision Agriculture" },
            };
            var equipmentTypes = new List<EquipmentType>
            {
                new EquipmentType { Type = "Tractor", EquipmentCategory = equipmentCategories[0] },
                new EquipmentType { Type = "Plow", EquipmentCategory = equipmentCategories[0] },
                new EquipmentType { Type = "Seeder", EquipmentCategory = equipmentCategories[1] },
                new EquipmentType { Type = "Sprayer", EquipmentCategory = equipmentCategories[3] },
                new EquipmentType { Type = "Harvester", EquipmentCategory = equipmentCategories[4] },
                new EquipmentType { Type = "Sprinkler", EquipmentCategory = equipmentCategories[2] },
                new EquipmentType { Type = "Drone", EquipmentCategory = equipmentCategories[6] },
            };
            var equipment = new List<Equipment>
            {
                new Equipment {  Manufacturer = "John Deere", Model = "5055E", SerialNumber = "JD-TR-001", EquipmentType = equipmentTypes[0] },
                new Equipment {  Manufacturer = "Kverneland", Model = "150B", SerialNumber = "KV-PL-002", EquipmentType = equipmentTypes[1] },
                new Equipment {  Manufacturer = "Case IH", Model = "2100", SerialNumber = "CI-SE-003", EquipmentType = equipmentTypes[2] },
                new Equipment {  Manufacturer = "Hardi", Model = "Commander", SerialNumber = "HA-SP-004", EquipmentType = equipmentTypes[3] },
                new Equipment {  Manufacturer = "New Holland", Model = "CR10.90", SerialNumber = "NH-HA-005", EquipmentType = equipmentTypes[4] },
                new Equipment {  Manufacturer = "Rain Bird", Model = "5000", SerialNumber = "RB-SP-006", EquipmentType = equipmentTypes[5] },
                new Equipment {  Manufacturer = "DJI", Model = "Phantom 4 RTK", SerialNumber = "DJ-DR-007", EquipmentType = equipmentTypes[6] }
            };
            await context.EquipmentCategories.AddRangeAsync(equipmentCategories);
            await context.EquipmentTypes.AddRangeAsync(equipmentTypes);
            await context.Equipment.AddRangeAsync(equipment);
            await context.SaveChangesAsync();
        }
    }
}