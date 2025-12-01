using System.ComponentModel.DataAnnotations;

namespace CompressorMonitoringAPI.Models
{
    public class EquipmentSpecification
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int EquipmentId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string ParameterName { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string ParameterValue { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string ParameterUnit { get; set; } = string.Empty;
        
        // Навигационное свойство
        public virtual Equipment Equipment { get; set; } = null!;
    }
}