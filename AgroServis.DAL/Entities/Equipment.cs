namespace AgroServis.DAL.Entities
{
    public class Equipment
    {
        public int Id { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }

        public int EquipmentTypeId { get; set; }
        public EquipmentType EquipmentType { get; set; }

        public ICollection<Maintenance>? MaintenanceRecords { get; set; } = new List<Maintenance>();

        public bool IsMaintenanceDue
        {
            get
            {
                var lastMaintenance = MaintenanceRecords
                    .OrderByDescending(m => m.MaintenanceDate)
                    .FirstOrDefault();

                if (lastMaintenance == null)
                    return true;

                return lastMaintenance.MaintenanceDate.AddYears(1) <= DateTime.Now;
            }
        }
    }
}