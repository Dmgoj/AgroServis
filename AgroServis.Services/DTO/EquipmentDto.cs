namespace AgroServis.Services.DTO;
public record EquipmentDto
{
    public int Id { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public int EquipmentTypeId { get; set; }
    public string EquipmentType { get; set; } = string.Empty;
    public string EquipmentCategory { get; set; } = string.Empty;
}