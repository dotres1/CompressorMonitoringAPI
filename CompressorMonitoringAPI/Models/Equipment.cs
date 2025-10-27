using System;
using System.Collections.Generic;
using System.Linq;

namespace CompressorMonitoringAPI.Models
{
    public class Equipment
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string ProcessStage { get; set; } = "Сжатие";
        public DateTime InstallationDate { get; set; }
        public string Status { get; set; } = "Operational";
        public Dictionary<string, string> Specifications { get; set; } = new();
        
        // Бизнес-логика
        public bool RequiresMaintenance()
        {
            return Status == "Warning" || Status == "Critical";
        }
        
        public int GetEquipmentAgeInYears()
        {
            return DateTime.Now.Year - InstallationDate.Year;
        }
        
        // Новая бизнес-логика для реального оборудования
        public bool IsCriticalForProcess()
        {
            return ProcessStage == "Сжатие" || ProcessStage == "Бустерирование";
        }
        
        public string GetRecommendedInspectionInterval()
        {
            return IsCriticalForProcess() ? "2 часа" : "8 часов";
        }
        
        // Определяем тип оборудования для специфических проверок
        public string GetEquipmentCategory()
        {
            if (Type.Contains("поршнев", StringComparison.OrdinalIgnoreCase)) return "Поршневой";
            if (Type.Contains("винтов", StringComparison.OrdinalIgnoreCase)) return "Винтовой";
            if (Type.Contains("турбо", StringComparison.OrdinalIgnoreCase)) return "Турбокомпрессор";
            if (Type.Contains("бустер", StringComparison.OrdinalIgnoreCase)) return "Бустерный";
            return "Другое";
        }
    }
}