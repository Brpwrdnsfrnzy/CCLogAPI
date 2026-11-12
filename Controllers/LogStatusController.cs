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
    public class LogStatusController : ControllerBase
    {
        private readonly CCLogContext _context;
        private readonly ILogger<LogStatusController> _logger;

        public LogStatusController(CCLogContext context, ILogger<LogStatusController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/LogStatus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LogStatus>>> GetLogStatuses()
        {
            try
            {
                return await _context.LogStatuses
                    .AsNoTracking()
                    .OrderBy(ls => ls.OrderBy)
                    .ThenBy(ls => ls.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load LogStatus list.");
                return StatusCode(500, new { error = "Failed to load log statuses.", details = ex.GetBaseException().Message });
            }
        }

        // GET: api/LogStatus/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LogStatus>> GetLogStatus(int id)
        {
            var logStatus = await _context.LogStatuses.FindAsync(id);

            if (logStatus == null)
            {
                return NotFound();
            }

            return logStatus;
        }

        // PUT: api/LogStatus/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLogStatus(int id, LogStatus logStatus)
        {
            if (id != logStatus.ID)
            {
                return BadRequest();
            }

            _context.Entry(logStatus).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LogStatusExists(id))
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

        // POST: api/LogStatus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<LogStatus>> PostLogStatus(LogStatus logStatus)
        {
            _context.LogStatuses.Add(logStatus);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLogStatus", new { id = logStatus.ID }, logStatus);
        }

        // DELETE: api/LogStatus/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLogStatus(int id)
        {
            var logStatus = await _context.LogStatuses.FindAsync(id);
            if (logStatus == null)
            {
                return NotFound();
            }

            _context.LogStatuses.Remove(logStatus);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LogStatusExists(int id)
        {
            return _context.LogStatuses.Any(e => e.ID == id);
        }
    }
}
