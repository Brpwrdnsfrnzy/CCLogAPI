using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CCLogAPI.Models;

namespace CCLogAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LogTypesController : ControllerBase
    {
        private readonly CCLogContext _context;
        private readonly ILogger<LogTypesController> _logger;

        public LogTypesController(CCLogContext context, ILogger<LogTypesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/LogTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LogType>>> GetLogTypes()
        {
            try
            {
                return await _context.LogTypes
                    .AsNoTracking()
                    .OrderBy(lt => lt.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load LogTypes.");
                return StatusCode(500, new { error = "Failed to load log types.", details = ex.GetBaseException().Message });
            }
        }

        // GET: api/LogTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LogType>> GetLogType(int id)
        {
            var logType = await _context.LogTypes.FindAsync(id);

            if (logType == null)
            {
                return NotFound();
            }

            return logType;
        }

        // PUT: api/LogTypes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLogType(int id, LogType logType)
        {
            if (id != logType.ID)
            {
                return BadRequest();
            }

            _context.Entry(logType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LogTypeExists(id))
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

        // POST: api/LogTypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<LogType>> PostLogType(LogType logType)
        {
            _context.LogTypes.Add(logType);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLogType", new { id = logType.ID }, logType);
        }

        // DELETE: api/LogTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLogType(int id)
        {
            var logType = await _context.LogTypes.FindAsync(id);
            if (logType == null)
            {
                return NotFound();
            }

            _context.LogTypes.Remove(logType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LogTypeExists(int id)
        {
            return _context.LogTypes.Any(e => e.ID == id);
        }
    }
}
