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
    public class SageCustomersController : ControllerBase
    {
        private readonly SageContext _context;

        public SageCustomersController(SageContext context)
        {
            _context = context;
        }

        // GET: SageCustomers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SageBrisCustomer>>> GetSageCustomers()
        {
            return await _context.SageBrisCustomers.ToListAsync();
        }

        // GET: SageCustomers/search?code=ABC
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<SageBrisCustomer>>> SearchSageCustomers(
            [FromQuery] string? code,
            [FromQuery] string? name)
        {
            var q = _context.SageBrisCustomers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(code))
            {
                var c = code.Trim();
                q = q.Where(x => EF.Functions.Like(x.Code, $"%{c}%"));
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                var n = name.Trim();
                q = q.Where(x => EF.Functions.Like(x.Name, $"%{n}%"));
            }

            return await q.ToListAsync();
        }

        // GET: SageCustomers/bycode/ABC123
        [HttpGet("bycode/{code}")]
        public async Task<ActionResult<SageBrisCustomer>> GetSageCustomerByCode(string code)
        {
            var customer = await _context.SageBrisCustomers
                .FirstOrDefaultAsync(x => x.Code == code);

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }
    }
}
