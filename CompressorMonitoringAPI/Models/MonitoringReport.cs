using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CompressorMonitoringAPI.Models
{
    public class MonitoringReport
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int EquipmentId { get; set; }
        
        [Required]
        public DateTime ReportDate { get; set; } = DateTime.Now;
        
        [Required]
        [MaxLength(100)]
        public string OperatorName { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string Shift { get; set; } = "Дневная";
        
        [MaxLength(50)]
        public string ReportType { get; set; } = "Оперативный";
        
        public string Conclusions { get; set; } = string.Empty;
        
        public double HealthScore { get; set; }
        
        [MaxLength(20)]
        public string HealthStatus { get; set; } = "Нормальное";
        
        // Навигационные свойства
        public virtual Equipment Equipment { get; set; } = null!;
        public virtual ICollection<ReportParameter> Parameters { get; set; } = new List<ReportParameter>();
        
        // Бизнес-логика
        public bool IsReportValid() => Parameters.Any() && !string.IsNullOrEmpty(Conclusions);
        public bool HasCriticalParameters() => Parameters.Any(p => p.IsCritical);
    }
}