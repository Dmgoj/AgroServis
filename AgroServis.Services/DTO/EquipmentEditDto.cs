using AgroServis.DAL.Entities;

namespace AgroServis.Services.DTO;

public record EquipmentEditDto
{
    public int Id { get; set; }
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public string SerialNumber { get; set; }
    public int EquipmentTypeId { get; set; }
    public EquipmentType EquipmentType { get; set; }
    public EquipmentCategory EquipmentCategory { get; set; }
    public ICollection<EquipmentType> EquipmentTypes { get; set; }
    public ICollection<EquipmentCategory> EquipmentCategories { get; set; }
}