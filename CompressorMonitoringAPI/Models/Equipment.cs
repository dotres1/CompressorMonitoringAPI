using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CompressorMonitoringAPI.Models
{
    public class Equipment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Location { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string ProcessStage { get; set; } = "Сжатие";
        
        public DateTime InstallationDate { get; set; }
        
        [MaxLength(20)]
        public string Status { get; set; } = "Operational";
        
        // Навигационные свойства
        public virtual ICollection<EquipmentSpecification> Specifications { get; set; } = new List<EquipmentSpecification>();
        public virtual ICollection<MonitoringReport> Reports { get; set; } = new List<MonitoringReport>();
        
        // Бизнес-логика
        public bool RequiresMaintenance() => Status == "Warning" || Status == "Critical";
        public int GetEquipmentAgeInYears() => DateTime.Now.Year - InstallationDate.Year;
        public bool IsCriticalForProcess() => ProcessStage == "Сжатие" || ProcessStage == "Бустерирование";
        public string GetRecommendedInspectionInterval() => IsCriticalForProcess() ? "2 часа" : "8 часов";
    }
}