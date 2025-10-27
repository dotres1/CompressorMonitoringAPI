using System;
using System.Collections.Generic;
using System.Linq;

namespace CompressorMonitoringAPI.Models
{
    public class MonitoringReport
    {
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public DateTime ReportDate { get; set; } = DateTime.Now;
        public string OperatorName { get; set; } = string.Empty;
        public string Shift { get; set; } = "Дневная";
        public Dictionary<string, double> Parameters { get; set; } = new();
        public string Conclusions { get; set; } = string.Empty;
        public string ReportType { get; set; } = "Оперативный";

        // Обновлённая бизнес-логика
        public bool IsReportValid()
        {
            return Parameters.Count > 0 && !string.IsNullOrEmpty(Conclusions);
        }
        
        public bool HasCriticalParameters()
        {
            return Parameters.Any(p => IsParameterCritical(p.Key, p.Value));
        }
        
        // Реальные критические значения для нефтегазового оборудования
        private bool IsParameterCritical(string parameterName, double value)
        {
            var criticalLimits = new Dictionary<string, double>
            {
                { "ТемператураМасла", 70.0 },
                { "ТемператураПодшипников", 90.0 },
                { "ВибрацияОсевая", 4.0 },
                { "ВибрацияРадиальная", 3.5 },
                { "ТемператураГазов", 100.0 },
                { "ДавлениеНагнетания", 7.8 },
                { "ОборотыРотора", 15500 }
            };
            
            return criticalLimits.ContainsKey(parameterName) && value > criticalLimits[parameterName];
        }
        
        // Новая бизнес-логика - оценка состояния оборудования
        public string GetEquipmentHealthStatus()
        {
            if (HasCriticalParameters()) return "Критическое";
            var warningCount = Parameters.Count(p => p.Value > GetWarningLimit(p.Key));
            return warningCount > 0 ? "Требует внимания" : "Нормальное";
        }
        
        private double GetWarningLimit(string parameterName)
        {
            var warningLimits = new Dictionary<string, double>
            {
                { "ТемператураМасла", 65.0 },
                { "ТемператураПодшипников", 80.0 },
                { "ВибрацияОсевая", 3.0 },
                { "ВибрацияРадиальная", 2.5 },
                { "ТемператураГазов", 90.0 }
            };
            
            return warningLimits.GetValueOrDefault(parameterName, 80.0);
        }
        
        // Расчёт общего "здоровья" оборудования по параметрам
        public double CalculateHealthScore()
        {
            if (Parameters.Count == 0) return 0;
            
            var maxScore = Parameters.Count * 100;
            var currentScore = Parameters.Sum(p => 
                Math.Max(0, 100 - (p.Value / GetWarningLimit(p.Key) * 100 - 100) * 10));
            
            return Math.Round(currentScore / maxScore * 100, 1);
        }
    }
}