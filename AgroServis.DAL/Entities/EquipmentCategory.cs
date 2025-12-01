namespace AgroServis.DAL.Entities
{
    public class EquipmentCategory
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public ICollection<EquipmentType>? EquipmentTypes { get; set; }
    }
}
