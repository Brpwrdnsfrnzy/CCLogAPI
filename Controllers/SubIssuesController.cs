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
    public class SubIssuesController : ControllerBase
    {
        private readonly CCLogContext _context;

        public SubIssuesController(CCLogContext context)
        {
            _context = context;
        }

        // GET: api/SubIssues
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubIssue>>> GetSubIssues()
        {
            return await _context.SubIssues.ToListAsync();
        }

        // GET: api/SubIssues/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SubIssue>> GetSubIssue(int id)
        {
            var subIssue = await _context.SubIssues.FindAsync(id);

            if (subIssue == null)
            {
                return NotFound();
            }

            return subIssue;
        }

        // PUT: api/SubIssues/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSubIssue(int id, SubIssue subIssue)
        {
            if (id != subIssue.ID)
            {
                return BadRequest();
            }

            _context.Entry(subIssue).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SubIssueExists(id))
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

        // POST: api/SubIssues
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SubIssue>> PostSubIssue(SubIssue subIssue)
        {
            _context.SubIssues.Add(subIssue);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSubIssue", new { id = subIssue.ID }, subIssue);
        }

        // DELETE: api/SubIssues/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubIssue(int id)
        {
            var subIssue = await _context.SubIssues.FindAsync(id);
            if (subIssue == null)
            {
                return NotFound();
            }

            _context.SubIssues.Remove(subIssue);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: SubIssues/swapsort
        [HttpPost("swapsort")]
        public async Task<IActionResult> SwapSort(SwapSortRequest req)
        {
            var item1 = await _context.SubIssues.FindAsync(req.Id1);
            var item2 = await _context.SubIssues.FindAsync(req.Id2);

            if (item1 == null || item2 == null)
                return NotFound();

            (item1.OrderBy, item2.OrderBy) = (item2.OrderBy, item1.OrderBy);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: SubIssues/mark-deleted/5
        [HttpPost("mark-deleted/{id}")]
        public async Task<IActionResult> MarkAsDeleted(int id)
        {
            var subIssue = await _context.SubIssues.FindAsync(id);

            if (subIssue == null)
            {
                return NotFound();
            }

            subIssue.Deleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SubIssueExists(int id)
        {
            return _context.SubIssues.Any(e => e.ID == id);
        }
    }
}
