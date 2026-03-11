using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroServis.DAL.Entities
{
    public class MaintenanceSchedule
    {
        public int Id { get; set; }

        public int EquipmentId { get; set; }
        public Equipment Equipment { get; set; } = null!;

        public string Title { get; set; } = "";
        public string? Description { get; set; }

        public int IntervalDays { get; set; }

        public DateTime? LastPerformedAt { get; set; }
        public DateTime NextDueDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}