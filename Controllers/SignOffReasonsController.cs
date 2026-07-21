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
    public class SignOffReasonsController : ControllerBase
    {
        private readonly CCLogContext _context;

        public SignOffReasonsController(CCLogContext context)
        {
            _context = context;
        }

        // GET: api/SignOffReasons
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SignOffReason>>> GetSignOffReasons()
        {
            return await _context.SignOffReasons.ToListAsync();
        }

        // GET: api/SignOffReasons/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SignOffReason>> GetSignOffReason(int id)
        {
            var signOffReason = await _context.SignOffReasons.FindAsync(id);

            if (signOffReason == null)
            {
                return NotFound();
            }

            return signOffReason;
        }

        // PUT: api/SignOffReasons/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSignOffReason(int id, SignOffReason signOffReason)
        {
            if (id != signOffReason.ID)
            {
                return BadRequest();
            }

            _context.Entry(signOffReason).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SignOffReasonExists(id))
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

        // POST: api/SignOffReasons
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SignOffReason>> PostSignOffReason(SignOffReason signOffReason)
        {
            _context.SignOffReasons.Add(signOffReason);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSignOffReason", new { id = signOffReason.ID }, signOffReason);
        }

        // DELETE: api/SignOffReasons/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSignOffReason(int id)
        {
            var signOffReason = await _context.SignOffReasons.FindAsync(id);
            if (signOffReason == null)
            {
                return NotFound();
            }

            _context.SignOffReasons.Remove(signOffReason);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: SignOffReasons/swapsort
        [HttpPost("swapsort")]
        public async Task<IActionResult> SwapSort(SwapSortRequest req)
        {
            var item1 = await _context.SignOffReasons.FindAsync(req.Id1);
            var item2 = await _context.SignOffReasons.FindAsync(req.Id2);

            if (item1 == null || item2 == null)
                return NotFound();

            (item1.OrderBy, item2.OrderBy) = (item2.OrderBy, item1.OrderBy);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SignOffReasonExists(int id)
        {
            return _context.SignOffReasons.Any(e => e.ID == id);
        }
    }
}
