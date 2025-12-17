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
    public class EquipmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EquipmentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/equipment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Equipment>>> Get()
        {
            return await _context.Equipment
                .Include(e => e.Specifications)
                .Include(e => e.Reports)
                .ToListAsync();
        }

        // GET api/equipment/id
        [HttpGet("{id}")]
        public async Task<ActionResult<Equipment>> Get(int id)
        {
            var equipment = await _context.Equipment
                .Include(e => e.Specifications)
                .Include(e => e.Reports)
                    .ThenInclude(r => r.Parameters)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipment == null)
            {
                return NotFound();
            }

            return equipment;
        }

        // POST api/equipment
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Equipment>> Post([FromBody] Equipment newEquipment)
        {
            _context.Equipment.Add(newEquipment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = newEquipment.Id }, newEquipment);
        }

        // PUT api/equipment/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Put(int id, [FromBody] Equipment updatedEquipment)
        {
            if (id != updatedEquipment.Id)
            {
                return BadRequest();
            }

            _context.Entry(updatedEquipment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EquipmentExists(id))
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

        // DELETE api/equipment/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var equipment = await _context.Equipment.FindAsync(id);
            if (equipment == null)
            {
                return NotFound();
            }

            _context.Equipment.Remove(equipment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<Equipment>>> GetByStatus(string status)
        {
            var result = await _context.Equipment
                .Where(e => e.Status == status)
                .Include(e => e.Specifications)
                .ToListAsync();
            
            return Ok(result);
        }

        [HttpGet("requires-maintenance")]
        public async Task<ActionResult<IEnumerable<object>>> GetEquipmentRequiringMaintenance()
        {
            var result = await _context.Equipment
                .Where(e => e.Status == "Warning" || e.Status == "Critical")
                .Select(e => new 
                { 
                    e.Name, 
                    e.Location, 
                    e.Status,
                    Age = e.GetEquipmentAgeInYears(),
                    RequiresMaintenance = e.RequiresMaintenance()
                })
                .ToListAsync();
            
            return Ok(result);
        }

        // методы для объединения данных
        [HttpGet("with-reports")]
        public async Task<ActionResult<IEnumerable<object>>> GetEquipmentWithReports()
        {
            var result = await _context.Equipment
                .Include(e => e.Reports)
                    .ThenInclude(r => r.Parameters)
                .Select(e => new
                {
                    Equipment = new
                    {
                        e.Id,
                        e.Name,
                        e.Type,
                        e.Location,
                        e.Status
                    },
                    ReportsCount = e.Reports.Count,
                    LastReport = e.Reports
                        .OrderByDescending(r => r.ReportDate)
                        .Select(r => new { r.Id, r.ReportDate, r.HealthStatus })
                        .FirstOrDefault(),
                    CriticalReports = e.Reports.Count(r => r.HealthStatus == "Критическое")
                })
                .ToListAsync();
            
            return Ok(result);
        }

        [HttpGet("critical-process")]
        public async Task<ActionResult<IEnumerable<object>>> GetCriticalProcessEquipment()
        {
            var result = await _context.Equipment
                .Where(e => e.ProcessStage == "Сжатие" || e.ProcessStage == "Бустерирование")
                .Select(e => new
                {
                    e.Name,
                    e.ProcessStage,
                    e.Location,
                    InspectionInterval = e.GetRecommendedInspectionInterval(),
                    Status = e.Status
                })
                .ToListAsync();
            
            return Ok(result);
        }

        private bool EquipmentExists(int id)
        {
            return _context.Equipment.Any(e => e.Id == id);
        }
    }
}