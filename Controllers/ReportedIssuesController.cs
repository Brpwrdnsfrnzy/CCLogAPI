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
    public class ReportedIssuesController : ControllerBase
    {
        private readonly CCLogContext _context;

        public ReportedIssuesController(CCLogContext context)
        {
            _context = context;
        }

        // GET: api/ReportedIssues
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReportedIssue>>> GetReportedIssues()
        {
            return await _context.ReportedIssues.ToListAsync();
        }

        // GET: api/ReportedIssues/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReportedIssue>> GetReportedIssue(int id)
        {
            var reportedIssue = await _context.ReportedIssues.FindAsync(id);

            if (reportedIssue == null)
            {
                return NotFound();
            }

            return reportedIssue;
        }

        // PUT: api/ReportedIssues/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReportedIssue(int id, ReportedIssue reportedIssue)
        {
            if (id != reportedIssue.ID)
            {
                return BadRequest();
            }

            _context.Entry(reportedIssue).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReportedIssueExists(id))
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

        // POST: api/ReportedIssues
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ReportedIssue>> PostReportedIssue(ReportedIssue reportedIssue)
        {
            _context.ReportedIssues.Add(reportedIssue);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetReportedIssue", new { id = reportedIssue.ID }, reportedIssue);
        }

        // DELETE: api/ReportedIssues/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReportedIssue(int id)
        {
            var reportedIssue = await _context.ReportedIssues.FindAsync(id);
            if (reportedIssue == null)
            {
                return NotFound();
            }

            _context.ReportedIssues.Remove(reportedIssue);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: ReportedIssues/swapsort
        [HttpPost("swapsort")]
        public async Task<IActionResult> SwapSort(SwapSortRequest req)
        {
            var item1 = await _context.ReportedIssues.FindAsync(req.Id1);
            var item2 = await _context.ReportedIssues.FindAsync(req.Id2);

            if (item1 == null || item2 == null)
                return NotFound();

            (item1.OrderBy, item2.OrderBy) = (item2.OrderBy, item1.OrderBy);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReportedIssueExists(int id)
        {
            return _context.ReportedIssues.Any(e => e.ID == id);
        }
    }
}
