using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CompressorMonitoringAPI.Data;
using CompressorMonitoringAPI.Models;

namespace CompressorMonitoringAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Сводный отчёт по цеху
        [HttpGet("summary")]
        public async Task<ActionResult> GetCompressorShopSummary()
        {
            var equipmentSummary = await _context.Equipment
                .GroupBy(e => e.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var recentReports = await _context.MonitoringReports
                .Where(r => r.ReportDate >= DateTime.Now.AddDays(-1))
                .ToListAsync();

            return Ok(new
            {
                ReportDate = DateTime.Now,
                TotalEquipment = await _context.Equipment.CountAsync(),
                EquipmentInOperation = await _context.Equipment.CountAsync(e => e.Status == "Operational"),
                EquipmentUnderMaintenance = await _context.Equipment.CountAsync(e => e.Status == "Warning" || e.Status == "Critical"),
                CriticalEventsLast24h = recentReports.Count(r => r.HealthStatus == "Критическое"),
                EquipmentByStatus = equipmentSummary,
                AverageHealthScore = recentReports.Any() ? 
                    recentReports.Average(r => r.HealthScore) : 0
            });
        }

        // 2. Отчёт по техническому обслуживанию
        [HttpGet("maintenance")]
        public async Task<ActionResult> GetMaintenanceReport()
        {
            var equipment = await _context.Equipment
                .Include(e => e.Reports)
                .ToListAsync();

            var maintenanceNeeded = equipment
                .Where(e => e.RequiresMaintenance() || e.GetEquipmentAgeInYears() > 5)
                .Select(e => new
                {
                    e.Name,
                    e.Type,
                    e.Location,
                    Age = e.GetEquipmentAgeInYears(),
                    LastReport = e.Reports.OrderByDescending(r => r.ReportDate).FirstOrDefault(),
                    Reason = e.RequiresMaintenance() ? "Текущий статус" : "Возраст оборудования",
                    RecommendedAction = e.RequiresMaintenance() ? "Срочный ремонт" : "Плановое ТО",
                    InspectionInterval = e.GetRecommendedInspectionInterval()
                })
                .ToList();

            return Ok(new
            {
                ReportDate = DateTime.Now,
                TotalRequiringMaintenance = maintenanceNeeded.Count,
                UrgentRepairs = maintenanceNeeded.Count(m => m.RecommendedAction == "Срочный ремонт"),
                PlannedMaintenance = maintenanceNeeded.Count(m => m.RecommendedAction == "Плановое ТО"),
                Equipment = maintenanceNeeded
            });
        }

        // 3. Анализ трендов по параметрам
        [HttpGet("trends/{days}")]
        public async Task<ActionResult> GetParameterTrends(int days)
        {
            var startDate = DateTime.Now.AddDays(-days);
            
            var recentReports = await _context.MonitoringReports
                .Include(r => r.Parameters)
                .Where(r => r.ReportDate >= startDate)
                .ToListAsync();

            var allParameters = recentReports
                .SelectMany(r => r.Parameters)
                .ToList();

            var parameterTrends = allParameters
                .GroupBy(p => p.Name)
                .Select(g => new
                {
                    Parameter = g.Key,
                    Average = g.Average(p => p.Value),
                    Max = g.Max(p => p.Value),
                    Min = g.Min(p => p.Value),
                    Count = g.Count(),
                    Trend = GetTrendDirection(g.Select(p => p.Value).ToList())
                })
                .ToList();

            return Ok(new
            {
                AnalysisPeriod = $"{days} дней",
                StartDate = startDate,
                EndDate = DateTime.Now,
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
        public async Task<ActionResult> GetShiftReport(string shiftName)
        {
            DateTime startDate, endDate;
    
            if (shiftName == "Ночная")
            {
                startDate = DateTime.Today.AddDays(-1).AddHours(22);
                endDate = DateTime.Today.AddHours(6);
            }
            else if (shiftName == "Дневная")
            {
                startDate = DateTime.Today.AddHours(8);
                endDate = DateTime.Today.AddHours(16);
            }
            else if (shiftName == "Вечерняя")
            {
                startDate = DateTime.Today.AddHours(16);
                endDate = DateTime.Today.AddHours(22);
            }
            else
            {
                startDate = DateTime.Today;
                endDate = DateTime.Today.AddDays(1);
            }
    
            var shiftReports = await _context.MonitoringReports
                .Include(r => r.Parameters)
                .Include(r => r.Equipment)
                .Where(r => r.Shift == shiftName && 
                            r.ReportDate >= startDate && 
                            r.ReportDate < endDate)
                .ToListAsync();
    
            var equipmentGroups = shiftReports
                .GroupBy(r => r.Equipment)
                .Select(g => new
                {
                    Equipment = g.Key,
                    Reports = g.OrderByDescending(r => r.ReportDate).ToList()
                })
                .ToList();
    
            return Ok(new
            {
                Shift = shiftName,
                Date = DateTime.Today.ToString("yyyy-MM-dd"),
                ReportsCount = shiftReports.Count,
                EquipmentMonitored = equipmentGroups.Count,
                CriticalEvents = shiftReports.Count(r => r.HealthStatus == "Критическое"),
                Details = equipmentGroups.Select(x => new
                {
                    x.Equipment.Name,
                    x.Equipment.Location,
                    ReportsCount = x.Reports.Count,
                    LastReportTime = x.Reports.FirstOrDefault()?.ReportDate,
                    HealthStatus = x.Reports.FirstOrDefault()?.HealthStatus,
                    HealthScore = x.Reports.FirstOrDefault()?.HealthScore
                })
            });
        }

        // 5. Отчёт по критическим параметрам
        [HttpGet("critical-parameters")]
        public async Task<ActionResult> GetCriticalParametersReport()
        {
            var criticalReports = await _context.MonitoringReports
                .Include(r => r.Parameters)
                .Include(r => r.Equipment)
                .Where(r => r.HealthStatus == "Критическое")
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();

            var result = criticalReports.Select(r => new
            {
                ReportDate = r.ReportDate,
                Equipment = new { r.Equipment.Name, r.Equipment.Location },
                Operator = r.OperatorName,
                CriticalParameters = r.Parameters
                    .Where(p => p.IsCritical)
                    .Select(p => new { p.Name, p.Value, p.Unit, p.CriticalLimit }),
                Conclusions = r.Conclusions
            }).ToList();

            return Ok(new
            {
                TotalCriticalReports = result.Count,
                TimePeriod = "Последние 7 дней",
                Reports = result
            });
        }
    }
}