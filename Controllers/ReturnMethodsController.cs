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
    public class ReturnMethodsController : ControllerBase
    {
        private readonly CCLogContext _context;

        public ReturnMethodsController(CCLogContext context)
        {
            _context = context;
        }

        // GET: ReturnMethods
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReturnMethod>>> GetReturnMethods()
        {
            return await _context.ReturnMethods.ToListAsync();
        }

        // GET: ReturnMethods/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReturnMethod>> GetReturnMethod(int id)
        {
            var returnMethod = await _context.ReturnMethods.FindAsync(id);
            if (returnMethod == null)
                return NotFound();
            return returnMethod;
        }

        // PUT: ReturnMethods/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReturnMethod(int id, ReturnMethod returnMethod)
        {
            if (id != returnMethod.ID)
                return BadRequest();

            _context.Entry(returnMethod).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReturnMethodExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: ReturnMethods
        [HttpPost]
        public async Task<ActionResult<ReturnMethod>> PostReturnMethod(ReturnMethod returnMethod)
        {
            _context.ReturnMethods.Add(returnMethod);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetReturnMethod", new { id = returnMethod.ID }, returnMethod);
        }

        // DELETE: ReturnMethods/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReturnMethod(int id)
        {
            var returnMethod = await _context.ReturnMethods.FindAsync(id);
            if (returnMethod == null)
                return NotFound();

            _context.ReturnMethods.Remove(returnMethod);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool ReturnMethodExists(int id)
        {
            return _context.ReturnMethods.Any(e => e.ID == id);
        }
    }
}
