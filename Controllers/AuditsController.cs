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
    public class AuditsController : ControllerBase
    {
        private readonly CCLogContext _context;

        public AuditsController(CCLogContext context)
        {
            _context = context;
        }

        // GET: api/Audits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Audit>>> GetAudits()
        {
            return await _context.Audits.ToListAsync();
        }

        // GET: api/Audits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Audit>> GetAudit(long id)
        {
            var audit = await _context.Audits.FindAsync(id);

            if (audit == null)
            {
                return NotFound();
            }

            return audit;
        }

        // GET: api/Audits/by-log/5
        [HttpGet("by-log/{logId}")]
        public async Task<ActionResult<IEnumerable<Audit>>> GetAuditsByLogId(long logId)
        {
            var audits = await _context.Audits
                .Where(a => a.LogID == logId)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();

            return audits;
        }

        // GET: api/Audits/by-user/{username}
        [HttpGet("by-user/{username}")]
        public async Task<ActionResult<IEnumerable<Audit>>> GetAuditsByUser(string username)
        {
            var audits = await _context.Audits
                .Where(a => a.UserName == username)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();

            return audits;
        }

        // PUT: api/Audits/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAudit(long id, Audit audit)
        {
            if (id != audit.ID)
            {
                return BadRequest();
            }

            _context.Entry(audit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AuditExists(id))
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

        // POST: api/Audits
        [HttpPost]
        public async Task<ActionResult<Audit>> PostAudit(Audit audit)
        {
            _context.Audits.Add(audit);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAudit", new { id = audit.ID }, audit);
        }

        // DELETE: api/Audits/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAudit(long id)
        {
            var audit = await _context.Audits.FindAsync(id);
            if (audit == null)
            {
                return NotFound();
            }

            _context.Audits.Remove(audit);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AuditExists(long id)
        {
            return _context.Audits.Any(e => e.ID == id);
        }
    }
}
