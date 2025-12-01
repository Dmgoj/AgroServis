using AgroServis.DAL.Entities;

namespace AgroServis.Services.DTO
{
    public record EquipmentUpdateDto
    {
        public int Id { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }

        public int EquipmentTypeId { get; set; }
    }
}