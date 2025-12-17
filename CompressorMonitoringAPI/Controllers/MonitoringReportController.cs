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
    public class MonitoringReportController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MonitoringReportController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/monitoringreport
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MonitoringReport>>> Get()
        {
            return await _context.MonitoringReports
                //.Include(r => r.Parameters)
               // .Include(r => r.Equipment)
               // .OrderByDescending(r => r.ReportDate)
                  .ToListAsync();
        }

        // GET api/monitoringreport/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MonitoringReport>> Get(int id)
        {
            var report = await _context.MonitoringReports
                .Include(r => r.Parameters)
                .Include(r => r.Equipment)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
            {
                return NotFound();
            }

            return report;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> Post([FromBody] SimpleReportDto dto)
        {
            var report = new MonitoringReport
            {
                EquipmentId = dto.EquipmentId,
                OperatorName = dto.OperatorName,
                Shift = dto.Shift,
                ReportType = dto.ReportType,
                Conclusions = dto.Conclusions,
                HealthScore = dto.HealthScore,
                HealthStatus = dto.HealthStatus
            };
    
            _context.MonitoringReports.Add(report);
            await _context.SaveChangesAsync();
    
            return Ok(new { id = report.Id });
        }

        public class SimpleReportDto
        {
            public int EquipmentId { get; set; }
            public string OperatorName { get; set; } = string.Empty;
            public string Shift { get; set; } = "Дневная";
            public string ReportType { get; set; } = "Оперативный";
            public string Conclusions { get; set; } = string.Empty;
            public double HealthScore { get; set; }
            public string HealthStatus { get; set; } = "Нормальное";
        }

        // PUT api/monitoringreport/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Put(int id, [FromBody] MonitoringReport updatedReport)
        {
            if (id != updatedReport.Id)
            {
                return BadRequest();
            }

            _context.Entry(updatedReport).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReportExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE api/monitoringreport/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var report = await _context.MonitoringReports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            _context.MonitoringReports.Remove(report);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        
        // Специальные LINQ-запросы
        [HttpGet("critical")]
        public async Task<ActionResult<IEnumerable<MonitoringReport>>> GetCriticalReports()
        {
            var result = await _context.MonitoringReports
                .Include(r => r.Parameters)
                .Include(r => r.Equipment)
                .Where(r => r.HealthStatus == "Критическое")
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
            
            return Ok(result);
        }

        [HttpGet("operator/{operatorName}")]
        public async Task<ActionResult<IEnumerable<object>>> GetReportsByOperator(string operatorName)
        {
            var result = await _context.MonitoringReports
                .Include(r => r.Equipment)
                .Where(r => r.OperatorName == operatorName)
                .Select(r => new 
                { 
                    r.Id, 
                    r.ReportDate, 
                    r.OperatorName,
                    EquipmentName = r.Equipment.Name,
                    r.HealthStatus,
                    r.Conclusions 
                })
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
            
            return Ok(result);
        }

        // методы для объединения данных
        [HttpGet("with-equipment")]
        public async Task<ActionResult<IEnumerable<object>>> GetReportsWithEquipment()
        {
            var result = await _context.MonitoringReports
                .Include(r => r.Parameters)
                .Include(r => r.Equipment)
                .Select(r => new
                {
                    Report = new
                    {
                        r.Id,
                        r.ReportDate,
                        r.OperatorName,
                        r.Shift,
                        r.HealthScore,
                        r.HealthStatus,
                        r.Conclusions
                    },
                    Equipment = new
                    {
                        r.Equipment.Id,
                        r.Equipment.Name,
                        r.Equipment.Type,
                        r.Equipment.Location
                    },
                    Parameters = r.Parameters.Select(p => new
                    {
                        p.Name,
                        p.Value,
                        p.Unit,
                        p.IsWarning,
                        p.IsCritical
                    })
                })
                .OrderByDescending(x => x.Report.ReportDate)
                .ToListAsync();
            
            return Ok(result);
        }

        [HttpGet("equipment/{equipmentId}")]
        public async Task<ActionResult<IEnumerable<MonitoringReport>>> GetReportsByEquipment(int equipmentId)
        {
            var reports = await _context.MonitoringReports
                .Include(r => r.Parameters)
                .Where(r => r.EquipmentId == equipmentId)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();
            
            return Ok(reports);
        }

        private bool ReportExists(int id)
        {
            return _context.MonitoringReports.Any(e => e.Id == id);
        }
    }
}