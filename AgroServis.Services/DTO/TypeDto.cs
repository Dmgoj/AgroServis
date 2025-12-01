using AgroServis.DAL.Entities;

public record TypeDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int EquipmentCategoryId { get; set; }
    public EquipmentCategory EquipmentCategory { get; set; } = new EquipmentCategory();
}