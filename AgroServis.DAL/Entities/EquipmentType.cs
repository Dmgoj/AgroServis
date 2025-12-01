namespace AgroServis.DAL.Entities
{
    public class EquipmentType
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int EquipmentCategoryId { get; set; }
        public EquipmentCategory EquipmentCategory { get; set; }
        public ICollection<Equipment>? Equipment { get; set; }
    }
}