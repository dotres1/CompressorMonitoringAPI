using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace CompressorMonitoringAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonitoringReportController : ControllerBase
    {
        private static List<MonitoringReport> _reports = new List<MonitoringReport>
        {
            new MonitoringReport 
            { 
                Id = 1, 
                EquipmentId = 1, 
                ReportDate = DateTime.Now, 
                OperatorName = "Иванов", 
                Parameters = new Dictionary<string, double> 
                { 
                    { "Давление", 10.5 }, 
                    { "Температура", 75.2 } 
                }, 
                Conclusions = "Норма" 
            },
            new MonitoringReport 
            { 
                Id = 2, 
                EquipmentId = 2, 
                ReportDate = DateTime.Now, 
                OperatorName = "Петров", 
                Parameters = new Dictionary<string, double> 
                { 
                    { "Давление", 12.8 }, 
                    { "Температура", 80.1 } 
                }, 
                Conclusions = "Повышенная температура" 
            }
        };

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
            report.Parameters = updatedReport.Parameters;
            report.Conclusions = updatedReport.Conclusions;

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
        public ActionResult<IEnumerable<MonitoringReport>> GetReportsByOperator(string operatorName)
        {
            var result = _reports
                .Where(r => r.OperatorName.Contains(operatorName, StringComparison.OrdinalIgnoreCase))
                .Select(r => new { r.Id, r.ReportDate, r.Conclusions })
                .ToList();
            return Ok(result);
        }
    }
}