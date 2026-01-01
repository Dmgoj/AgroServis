using AgroServis.DAL.Enums;
using System;

namespace AgroServis.Services.DTOs
{
    public record MaintenanceDto
    {
        public int Id { get; init; }

        public int EquipmentId { get; init; }

        public string EquipmentName { get; init; } = string.Empty;
        public string EquipmentSerialNumber { get; init; } = string.Empty;

        public DateTime MaintenanceDate { get; init; }
        public string Description { get; init; } = string.Empty;

        public MaintenanceType Type { get; init; }
        public MaintenanceStatus Status { get; init; }

        public decimal? Cost { get; init; }
        public string? Notes { get; init; }
        public string? PerformedBy { get; init; }

        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }

        public string FormattedCost => Cost.HasValue ? $"{Cost:C2}" : "N/A";
        public string FormattedDate => MaintenanceDate.ToString("yyyy-MM-dd");
    }
}