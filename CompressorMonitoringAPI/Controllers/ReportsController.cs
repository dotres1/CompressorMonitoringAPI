using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using CompressorMonitoringAPI.Models;

namespace CompressorMonitoringAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private static List<Equipment> _equipment = DataContext.Equipment;
        private static List<MonitoringReport> _reports = DataContext.Reports;

        // 1. Сводный отчёт по цеху
        [HttpGet("summary")]
        public ActionResult GetCompressorShopSummary()
        {
            var equipmentSummary = _equipment
                .GroupBy(e => e.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            var recentReports = _reports
                .Where(r => r.ReportDate >= System.DateTime.Now.AddDays(-1))
                .ToList();

            return Ok(new
            {
                ReportDate = System.DateTime.Now,
                TotalEquipment = _equipment.Count,
                EquipmentInOperation = _equipment.Count(e => e.Status == "Operational"),
                EquipmentUnderMaintenance = _equipment.Count(e => e.RequiresMaintenance()),
                CriticalEventsLast24h = recentReports.Count(r => r.HasCriticalParameters()),
                EquipmentByStatus = equipmentSummary,
                AverageHealthScore = recentReports.Any() ? 
                    recentReports.Average(r => r.CalculateHealthScore()) : 0
            });
        }

        // 2. Отчёт по техническому обслуживанию
        [HttpGet("maintenance")]
        public ActionResult GetMaintenanceReport()
        {
            var maintenanceNeeded = _equipment
                .Where(e => e.RequiresMaintenance() || e.GetEquipmentAgeInYears() > 5)
                .Select(e => new
                {
                    e.Name,
                    e.Type,
                    e.Location,
                    Age = e.GetEquipmentAgeInYears(),
                    Reason = e.RequiresMaintenance() ? "Текущий статус" : "Возраст оборудования",
                    RecommendedAction = e.RequiresMaintenance() ? "Срочный ремонт" : "Плановое ТО",
                    InspectionInterval = e.GetRecommendedInspectionInterval()
                })
                .ToList();

            return Ok(new
            {
                ReportDate = System.DateTime.Now,
                TotalRequiringMaintenance = maintenanceNeeded.Count,
                UrgentRepairs = maintenanceNeeded.Count(m => m.RecommendedAction == "Срочный ремонт"),
                PlannedMaintenance = maintenanceNeeded.Count(m => m.RecommendedAction == "Плановое ТО"),
                Equipment = maintenanceNeeded
            });
        }

        // 3. Анализ трендов по параметрам
        [HttpGet("trends/{days}")]
        public ActionResult GetParameterTrends(int days)
        {
            var startDate = System.DateTime.Now.AddDays(-days);
            var recentReports = _reports
                .Where(r => r.ReportDate >= startDate)
                .ToList();

            var parameterTrends = recentReports
                .SelectMany(r => r.Parameters)
                .GroupBy(p => p.Key)
                .Select(g => new
                {
                    Parameter = g.Key,
                    Average = g.Average(p => p.Value),
                    Max = g.Max(p => p.Value),
                    Min = g.Min(p => p.Value),
                    Trend = GetTrendDirection(g.Select(p => p.Value).ToList())
                })
                .ToList();

            return Ok(new
            {
                AnalysisPeriod = $"{days} дней",
                StartDate = startDate,
                EndDate = System.DateTime.Now,
                TotalReportsAnalyzed = recentReports.Count,
                ParameterTrends = parameterTrends
            });
        }

        private string GetTrendDirection(List<double> values)
        {
            if (values.Count < 2) return "Стабильный";
            
            var firstHalf = values.Take(values.Count / 2).Average();
            var secondHalf = values.Skip(values.Count / 2).Average();
            
            return secondHalf > firstHalf ? "Возрастающий" : 
                   secondHalf < firstHalf ? "Убывающий" : "Стабильный";
        }

        // 4. Отчёт по сменам
        [HttpGet("shift/{shiftName}")]
        public ActionResult GetShiftReport(string shiftName)
        {
            var shiftReports = _reports
                .Where(r => r.Shift == shiftName && r.ReportDate.Date == System.DateTime.Today)
                .ToList();

            var equipmentWithReports = _equipment
                .Select(e => new
                {
                    Equipment = e,
                    Reports = shiftReports.Where(r => r.EquipmentId == e.Id).ToList(),
                    LastReport = shiftReports.Where(r => r.EquipmentId == e.Id)
                                           .OrderByDescending(r => r.ReportDate)
                                           .FirstOrDefault()
                })
                .Where(x => x.Reports.Any())
                .ToList();

            return Ok(new
            {
                Shift = shiftName,
                Date = System.DateTime.Today,
                ReportsCount = shiftReports.Count,
                EquipmentMonitored = equipmentWithReports.Count,
                CriticalEvents = shiftReports.Count(r => r.HasCriticalParameters()),
                Details = equipmentWithReports.Select(x => new
                {
                    x.Equipment.Name,
                    x.Equipment.Location,
                    ReportsCount = x.Reports.Count,
                    LastReportTime = x.LastReport?.ReportDate,
                    HealthStatus = x.LastReport?.GetEquipmentHealthStatus(),
                    HealthScore = x.LastReport?.CalculateHealthScore()
                })
            });
        }
    }
}