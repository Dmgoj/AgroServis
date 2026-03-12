namespace AgroServis.Services.DTO
{
    public record MaintenanceScheduleDto
    {
        public int Id { get; init; }
        public int EquipmentId { get; init; }
        public string EquipmentName { get; init; } = "";
        public string Title { get; init; } = "";
        public string? Description { get; init; }
        public int IntervalDays { get; init; }
        public DateTime? LastPerformedAt { get; init; }
        public DateTime NextDueDate { get; init; }
        public bool IsActive { get; init; }

        public bool IsOverdue => IsActive && NextDueDate.Date < DateTime.UtcNow.Date;
        public int DaysUntilDue => (NextDueDate.Date - DateTime.UtcNow.Date).Days;
    }
}