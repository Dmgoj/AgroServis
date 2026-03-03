using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroServis.Services.DTO
{
    public class MaintenanceReportOptionsDto
    {
        public bool IncludeEquipmentName { get; set; } = true;

        public bool IncludeSerialNumber { get; set; } = true;
        public bool IncludeMaintenanceDate { get; set; } = true;
        public bool IncludeDescription { get; set; } = true;
        public bool IncludeType { get; set; } = true;
        public bool IncludeStatus { get; set; } = true;
        public bool IncludeCost { get; set; } = true;
        public bool IncludeNotes { get; set; } = true;
        public bool IncludePerformedBy { get; set; } = true;

        public string? Search { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? EquipmentId { get; set; }

        public List<SelectListItem> AvailableEquipment { get; set; } = new();
    }
}