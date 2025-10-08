using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace CompressorMonitoringAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentController : ControllerBase
    {
        private static List<Equipment> _equipment = new List<Equipment>
        {
            new Equipment { Id = 1, Name = "Компрессор 1", Type = "Поршневой", Location = "Цех 1", InstallationDate = new DateTime(2020, 1, 15), Status = "Operational" },
            new Equipment { Id = 2, Name = "Компрессор 2", Type = "Винтовой", Location = "Цех 2", InstallationDate = new DateTime(2019, 5, 20), Status = "Warning" }
        };

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
            equipment.InstallationDate = updatedEquipment.InstallationDate;
            equipment.Status = updatedEquipment.Status;

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
        public ActionResult<IEnumerable<Equipment>> GetEquipmentRequiringMaintenance()
        {
            var result = _equipment
                .Where(e => e.RequiresMaintenance())
                .Select(e => new { e.Name, e.Location, e.Status })
                .ToList();
            return Ok(result);
        }
    }
}