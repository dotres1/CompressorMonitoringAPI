using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using CompressorMonitoringAPI.Models;

namespace CompressorMonitoringAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentController : ControllerBase
    {
        private static List<Equipment> _equipment = DataContext.Equipment;
        private static List<MonitoringReport> _reports = DataContext.Reports;

        // GET: api/equipment
        [HttpGet]
        public ActionResult<IEnumerable<Equipment>> Get()
        {
            return _equipment;
        }

        // GET api/equipment/5
        [HttpGet("{id}")]
        public ActionResult<Equipment> Get(int id)
        {
            var equipment = _equipment.FirstOrDefault(e => e.Id == id);
            if (equipment == null)
            {
                return NotFound();
            }
            return equipment;
        }

        // POST api/equipment
        [HttpPost]
        public ActionResult<Equipment> Post([FromBody] Equipment newEquipment)
        {
            newEquipment.Id = _equipment.Max(e => e.Id) + 1;
            _equipment.Add(newEquipment);
            return CreatedAtAction(nameof(Get), new { id = newEquipment.Id }, newEquipment);
        }

        // PUT api/equipment/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Equipment updatedEquipment)
        {
            var equipment = _equipment.FirstOrDefault(e => e.Id == id);
            if (equipment == null)
            {
                return NotFound();
            }

            equipment.Name = updatedEquipment.Name;
            equipment.Type = updatedEquipment.Type;
            equipment.Location = updatedEquipment.Location;
            equipment.ProcessStage = updatedEquipment.ProcessStage;
            equipment.InstallationDate = updatedEquipment.InstallationDate;
            equipment.Status = updatedEquipment.Status;
            equipment.Specifications = updatedEquipment.Specifications;

            return NoContent();
        }

        // DELETE api/equipment/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var equipment = _equipment.FirstOrDefault(e => e.Id == id);
            if (equipment == null)
            {
                return NotFound();
            }

            _equipment.Remove(equipment);
            return NoContent();
        }
        
        // Специальные LINQ-запросы
        [HttpGet("status/{status}")]
        public ActionResult<IEnumerable<Equipment>> GetByStatus(string status)
        {
            var result = _equipment
                .Where(e => e.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                .Select(e => e)
                .ToList();
            return Ok(result);
        }

        [HttpGet("requires-maintenance")]
        public ActionResult<IEnumerable<object>> GetEquipmentRequiringMaintenance()
        {
            var result = _equipment
                .Where(e => e.RequiresMaintenance())
                .Select(e => new { e.Name, e.Location, e.Status })
                .ToList();
            return Ok(result);
        }

        // Новые методы для объединения данных
        [HttpGet("with-reports")]
        public ActionResult<IEnumerable<object>> GetEquipmentWithReports()
        {
            var result = _equipment
                .Select(e => new
                {
                    Equipment = e,
                    ReportsCount = _reports.Count(r => r.EquipmentId == e.Id),
                    LastReport = _reports.Where(r => r.EquipmentId == e.Id)
                                       .OrderByDescending(r => r.ReportDate)
                                       .FirstOrDefault(),
                    HealthScore = _reports.Where(r => r.EquipmentId == e.Id)
                                        .OrderByDescending(r => r.ReportDate)
                                        .FirstOrDefault()?.CalculateHealthScore() ?? 0
                })
                .ToList();
            
            return Ok(result);
        }

        [HttpGet("critical-process")]
        public ActionResult<IEnumerable<object>> GetCriticalProcessEquipment()
        {
            var result = _equipment
                .Where(e => e.IsCriticalForProcess())
                .Select(e => new
                {
                    e.Name,
                    e.ProcessStage,
                    e.Location,
                    InspectionInterval = e.GetRecommendedInspectionInterval(),
                    EquipmentCategory = e.GetEquipmentCategory()
                })
                .ToList();
            
            return Ok(result);
        }

        // Статический метод для доступа из ReportsController
        public static List<Equipment> GetEquipmentList()
        {
            return _equipment;
        }
    }
}