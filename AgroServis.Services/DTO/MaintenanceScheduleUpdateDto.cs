using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgroServis.Services.DTO
{
    public record MaintenanceScheduleUpdateDto
    {
        public int Id { get; init; }
        public int EquipmentId { get; init; }
        public string Title { get; init; } = "";
        public string? Description { get; init; }
        public int IntervalDays { get; init; }
        public DateTime? LastPerformedAt { get; init; }
        public bool IsActive { get; init; }
        public List<SelectListItem> AvailableEquipment { get; init; } = new();
    }
}