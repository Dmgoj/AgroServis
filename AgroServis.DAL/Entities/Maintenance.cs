using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgroServis.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace AgroServis.DAL.Entities
{
    public class Maintenance
    {
        public int Id { get; set; }

        public int EquipmentId { get; set; }
        public Equipment Equipment { get; set; }

        public DateTime MaintenanceDate { get; set; }

        public string Description { get; set; }

        public MaintenanceType Type { get; set; }

        public MaintenanceStatus Status { get; set; }

        [Precision(18, 2)]
        public decimal? Cost { get; set; }

        public string? PerformedBy { get; set; }

        [ForeignKey(nameof(PerformedBy))]
        public ApplicationUser? PerformedByUser { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}