using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CCLogAPI.Models;

namespace CCLogAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LogStatusesController : ControllerBase
    {
        private readonly CCLogContext _context;

        public LogStatusesController(CCLogContext context)
        {
            _context = context;
        }

        // GET: api/LogStatuses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LogStatus>>> GetLogStatuses()
        {
            return await _context.LogStatuses.ToListAsync();
        }

        // GET: api/LogStatuses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LogStatus>> GetLogStatus(int id)
        {
            var logStatus = await _context.LogStatuses.FindAsync(id);
            if (logStatus == null)
                return NotFound();
            return logStatus;
        }

        // PUT: api/LogStatuses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLogStatus(int id, LogStatus logStatus)
        {
            if (id != logStatus.ID)
                return BadRequest();
            _context.Entry(logStatus).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LogStatusExists(id))
                    return NotFound();
                else
                    throw;
            }
            return NoContent();
        }

        // POST: api/LogStatuses
        [HttpPost]
        public async Task<ActionResult<LogStatus>> PostLogStatus(LogStatus logStatus)
        {
            _context.LogStatuses.Add(logStatus);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetLogStatus", new { id = logStatus.ID }, logStatus);
        }

        // DELETE: api/LogStatuses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLogStatus(int id)
        {
            var logStatus = await _context.LogStatuses.FindAsync(id);
            if (logStatus == null)
                return NotFound();
            _context.LogStatuses.Remove(logStatus);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: LogStatuses/swapsort
        [HttpPost("swapsort")]
        public async Task<IActionResult> SwapSort(SwapSortRequest req)
        {
            var item1 = await _context.LogStatuses.FindAsync(req.Id1);
            var item2 = await _context.LogStatuses.FindAsync(req.Id2);
            if (item1 == null || item2 == null)
                return NotFound();
            (item1.OrderBy, item2.OrderBy) = (item2.OrderBy, item1.OrderBy);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool LogStatusExists(int id)
        {
            return _context.LogStatuses.Any(e => e.ID == id);
        }
    }
}
