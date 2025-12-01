using AgroServis.DAL.Entities;

public record CategoryDto
{
    public int Id { get; set; }
    public string Category { get; set; }
    public ICollection<EquipmentType>? EquipmentTypes { get; set; }
}