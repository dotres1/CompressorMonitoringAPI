using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using CompressorMonitoringAPI.Models;

namespace CompressorMonitoringAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonitoringReportController : ControllerBase
    {
        private static List<MonitoringReport> _reports = DataContext.Reports;
        private static List<Equipment> _equipment = DataContext.Equipment;

        // GET: api/monitoringreport
        [HttpGet]
        public ActionResult<IEnumerable<MonitoringReport>> Get()
        {
            return _reports;
        }

        // GET api/monitoringreport/5
        [HttpGet("{id}")]
        public ActionResult<MonitoringReport> Get(int id)
        {
            var report = _reports.FirstOrDefault(r => r.Id == id);
            if (report == null)
            {
                return NotFound();
            }
            return report;
        }

        // POST api/monitoringreport
        [HttpPost]
        public ActionResult<MonitoringReport> Post([FromBody] MonitoringReport newReport)
        {
            newReport.Id = _reports.Max(r => r.Id) + 1;
            _reports.Add(newReport);
            return CreatedAtAction(nameof(Get), new { id = newReport.Id }, newReport);
        }

        // PUT api/monitoringreport/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] MonitoringReport updatedReport)
        {
            var report = _reports.FirstOrDefault(r => r.Id == id);
            if (report == null)
            {
                return NotFound();
            }

            report.EquipmentId = updatedReport.EquipmentId;
            report.ReportDate = updatedReport.ReportDate;
            report.OperatorName = updatedReport.OperatorName;
            report.Shift = updatedReport.Shift;
            report.Parameters = updatedReport.Parameters;
            report.Conclusions = updatedReport.Conclusions;
            report.ReportType = updatedReport.ReportType;

            return NoContent();
        }

        // DELETE api/monitoringreport/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var report = _reports.FirstOrDefault(r => r.Id == id);
            if (report == null)
            {
                return NotFound();
            }

            _reports.Remove(report);
            return NoContent();
        }
        
        // Специальные LINQ-запросы
        [HttpGet("critical")]
        public ActionResult<IEnumerable<MonitoringReport>> GetCriticalReports()
        {
            var result = _reports
                .Where(r => r.HasCriticalParameters())
                .Select(r => r)
                .ToList();
            return Ok(result);
        }

        [HttpGet("operator/{operatorName}")]
        public ActionResult<IEnumerable<object>> GetReportsByOperator(string operatorName)
        {
            var result = _reports
                .Where(r => r.OperatorName.Contains(operatorName, StringComparison.OrdinalIgnoreCase))
                .Select(r => new { r.Id, r.ReportDate, r.Conclusions })
                .ToList();
            return Ok(result);
        }

        // Новые методы для объединения данных
        [HttpGet("with-equipment")]
        public ActionResult<IEnumerable<object>> GetReportsWithEquipment()
        {
            var result = _reports
                .Select(r => new
                {
                    Report = r,
                    Equipment = _equipment.FirstOrDefault(e => e.Id == r.EquipmentId)
                })
                .Where(x => x.Equipment != null)
                .Select(x => new
                {
                    x.Report.Id,
                    x.Report.ReportDate,
                    x.Report.OperatorName,
                    x.Report.Shift,
                    EquipmentName = x.Equipment.Name,
                    EquipmentType = x.Equipment.Type,
                    EquipmentLocation = x.Equipment.Location,
                    HealthStatus = x.Report.GetEquipmentHealthStatus(),
                    HealthScore = x.Report.CalculateHealthScore(),
                    x.Report.Parameters,
                    x.Report.Conclusions
                })
                .OrderByDescending(x => x.ReportDate)
                .ToList();
            
            return Ok(result);
        }

        // Статический метод для доступа из ReportsController
        public static List<MonitoringReport> GetReportsList()
        {
            return _reports;
        }
    }
}