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
    public class GoodsReceivedStatusController : ControllerBase
    {
        private readonly CCLogContext _context;

        public GoodsReceivedStatusController(CCLogContext context)
        {
            _context = context;
        }

        // GET: api/GoodsReceivedStatus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GoodsReceivedStatus>>> GetGoodsReceivedStatuses()
        {
            return await _context.GoodsReceivedStatuses.ToListAsync();
        }

        // GET: api/GoodsReceivedStatus/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GoodsReceivedStatus>> GetGoodsReceivedStatus(int id)
        {
            var goodsReceivedStatus = await _context.GoodsReceivedStatuses.FindAsync(id);

            if (goodsReceivedStatus == null)
            {
                return NotFound();
            }

            return goodsReceivedStatus;
        }

        // PUT: api/GoodsReceivedStatus/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGoodsReceivedStatus(int id, GoodsReceivedStatus goodsReceivedStatus)
        {
            if (id != goodsReceivedStatus.ID)
            {
                return BadRequest();
            }

            _context.Entry(goodsReceivedStatus).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GoodsReceivedStatusExists(id))
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

        // POST: api/GoodsReceivedStatus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<GoodsReceivedStatus>> PostGoodsReceivedStatus(GoodsReceivedStatus goodsReceivedStatus)
        {
            _context.GoodsReceivedStatuses.Add(goodsReceivedStatus);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGoodsReceivedStatus", new { id = goodsReceivedStatus.ID }, goodsReceivedStatus);
        }

        // DELETE: api/GoodsReceivedStatus/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGoodsReceivedStatus(int id)
        {
            var goodsReceivedStatus = await _context.GoodsReceivedStatuses.FindAsync(id);
            if (goodsReceivedStatus == null)
            {
                return NotFound();
            }

            _context.GoodsReceivedStatuses.Remove(goodsReceivedStatus);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GoodsReceivedStatusExists(int id)
        {
            return _context.GoodsReceivedStatuses.Any(e => e.ID == id);
        }
    }
}
