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
    public class CreditReasonsController : ControllerBase
    {
        private readonly CCLogContext _context;

        public CreditReasonsController(CCLogContext context)
        {
            _context = context;
        }

        // GET: api/CreditReasons
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CreditReason>>> GetCreditReasons()
        {
            return await _context.CreditReasons.ToListAsync();
        }

        // GET: api/CreditReasons/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CreditReason>> GetCreditReason(int id)
        {
            var creditReason = await _context.CreditReasons.FindAsync(id);

            if (creditReason == null)
            {
                return NotFound();
            }

            return creditReason;
        }

        // PUT: api/CreditReasons/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCreditReason(int id, CreditReason creditReason)
        {
            if (id != creditReason.ID)
            {
                return BadRequest();
            }

            _context.Entry(creditReason).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CreditReasonExists(id))
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

        // POST: api/CreditReasons
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CreditReason>> PostCreditReason(CreditReason creditReason)
        {
            _context.CreditReasons.Add(creditReason);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCreditReason", new { id = creditReason.ID }, creditReason);
        }

        // DELETE: api/CreditReasons/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCreditReason(int id)
        {
            var creditReason = await _context.CreditReasons.FindAsync(id);
            if (creditReason == null)
            {
                return NotFound();
            }

            _context.CreditReasons.Remove(creditReason);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: CreditReasons/swapsort
        [HttpPost("swapsort")]
        public async Task<IActionResult> SwapSort(SwapSortRequest req)
        {
            var item1 = await _context.CreditReasons.FindAsync(req.Id1);
            var item2 = await _context.CreditReasons.FindAsync(req.Id2);

            if (item1 == null || item2 == null)
                return NotFound();

            (item1.OrderBy, item2.OrderBy) = (item2.OrderBy, item1.OrderBy);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CreditReasonExists(int id)
        {
            return _context.CreditReasons.Any(e => e.ID == id);
        }
    }
}
