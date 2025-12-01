using System.ComponentModel.DataAnnotations;

namespace CompressorMonitoringAPI.Models
{
    public class ReportParameter
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ReportId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public double Value { get; set; }
        
        [MaxLength(20)]
        public string Unit { get; set; } = string.Empty;
        
        public double WarningLimit { get; set; }
        public double CriticalLimit { get; set; }
        
        // Вычисляемые свойства
        public bool IsWarning => Value > WarningLimit;
        public bool IsCritical => Value > CriticalLimit;
        
        // Навигационное свойство
        public virtual MonitoringReport Report { get; set; } = null!;
    }
}