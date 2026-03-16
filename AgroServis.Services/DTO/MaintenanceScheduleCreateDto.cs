using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgroServis.Services.DTO
{
    public record MaintenanceScheduleCreateDto
    {
        public int EquipmentId { get; init; }
        public string Title { get; init; } = "";
        public string? Description { get; init; }
        public int IntervalDays { get; init; }
        public DateTime? LastPerformedAt { get; init; }
        public List<SelectListItem> AvailableEquipment { get; init; } = new();
    }
}