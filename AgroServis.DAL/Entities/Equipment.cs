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
    }
}