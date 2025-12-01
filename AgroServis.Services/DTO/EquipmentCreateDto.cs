namespace AgroServis.Services.DTO;

public record EquipmentCreateDto
{
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public int EquipmentTypeId { get; set; }
}