using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using CCLogAPI.Models;

namespace CCLogAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SageCcLookupController : ControllerBase
    {
        private readonly SageContext _context;

        public SageCcLookupController(SageContext context)
        {
            _context = context;
        }

        // GET: SageCcLookup
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SageBrisCcLu>>> GetSageCcLookup()
        {
            return await _context.SageBrisCcLus.ToListAsync();
        }

        // GET: SageCcLookup/search?gdnNumber=123&stockCode=ABC&accno=12345678
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<SageBrisCcLu>>> SearchSageCcLookup(
            [FromQuery] string? gdnNumber,
            [FromQuery] string? stockCode,
            [FromQuery] string? orderNumber,
            [FromQuery] string? accno,
            [FromQuery] string? accname,
            [FromQuery] string? custorder,
            [FromQuery] string? weborder)
        {
            var q = _context.SageBrisCcLus.AsQueryable();

            if (!string.IsNullOrWhiteSpace(gdnNumber))
            {
                var gdn = gdnNumber.Trim();
                q = q.Where(x => EF.Functions.Like(x.GDN_NUMBER, $"%{gdn}%"));
            }

            if (!string.IsNullOrWhiteSpace(stockCode))
            {
                var stock = stockCode.Trim();
                q = q.Where(x => EF.Functions.Like(x.STOCK_CODE, $"%{stock}%"));
            }

            if (!string.IsNullOrWhiteSpace(orderNumber))
            {
                var order = orderNumber.Trim();
                q = q.Where(x => EF.Functions.Like(x.ORDER_NUMBER, $"%{order}%"));
            }

            if (!string.IsNullOrWhiteSpace(accno))
            {
                var acc = accno.Trim();
                q = q.Where(x => EF.Functions.Like(x.accno, $"%{acc}%"));
            }

            if (!string.IsNullOrWhiteSpace(accname))
            {
                var name = accname.Trim();
                q = q.Where(x => EF.Functions.Like(x.accname, $"%{name}%"));
            }

            if (!string.IsNullOrWhiteSpace(custorder))
            {
                var co = custorder.Trim();
                q = q.Where(x => EF.Functions.Like(x.custorder, $"%{co}%"));
            }

            if (!string.IsNullOrWhiteSpace(weborder))
            {
                var wo = weborder.Trim();
                q = q.Where(x => EF.Functions.Like(x.weborder, $"%{wo}%"));
            }

            return await q.ToListAsync();
        }

        // GET: SageCcLookup/by-account/12345678
        [HttpGet("by-account/{accno}")]
        public async Task<ActionResult<IEnumerable<SageBrisCcLu>>> GetByAccount(string accno)
        {
            var results = await _context.SageBrisCcLus
                .Where(x => x.accno == accno)
                .ToListAsync();

            return results;
        }

        // GET: SageCcLookup/by-gdn/GDN123
        [HttpGet("by-gdn/{gdnNumber}")]
        public async Task<ActionResult<IEnumerable<SageBrisCcLu>>> GetByGdn(string gdnNumber)
        {
            var results = await _context.SageBrisCcLus
                .Where(x => x.GDN_NUMBER == gdnNumber)
                .ToListAsync();

            return results;
        }

        // GET: SageCcLookup/by-order/ORD123
        [HttpGet("by-order/{orderNumber}")]
        public async Task<ActionResult<IEnumerable<SageBrisCcLu>>> GetByOrder(string orderNumber)
        {
            var results = await _context.SageBrisCcLus
                .Where(s => s.ORDER_NUMBER.EndsWith(orderNumber) || s.ORDER_NUMBER == orderNumber)
                .ToListAsync();

            return results;
        }

        // GET: SageCcLookup/by-invoice/INV123
        [HttpGet("by-invoice/{invoiceNo}")]
        public async Task<ActionResult<IEnumerable<SageBrisCcLu>>> GetByInvoice(string invoiceNo)
        {
            var results = await _context.SageBrisCcLus
                .Where(s => s.InvoiceCreditNo.EndsWith(invoiceNo) || s.InvoiceCreditNo == invoiceNo)
                .ToListAsync();

            return results;
        }

        // GET: SageCcLookup/orders-by-account/12345678
        // Returns distinct order numbers for autocomplete
        [HttpGet("orders-by-account/{accno}")]
        public async Task<ActionResult<IEnumerable<string>>> GetOrdersByAccount(string accno)
        {
            var orderNumbers = await _context.SageBrisCcLus
                .Where(x => x.accno == accno)
                .Select(x => x.ORDER_NUMBER)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            return orderNumbers;
        }

        // GET: SageCcLookup/invoices-by-account/12345678
        // Returns distinct invoice numbers for autocomplete
        [HttpGet("invoices-by-account/{accno}")]
        public async Task<ActionResult<IEnumerable<string>>> GetInvoicesByAccount(string accno)
        {
            var invoiceNumbers = await _context.SageBrisCcLus
                .Where(x => x.accno == accno)
                .Select(x => x.InvoiceCreditNo)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            return invoiceNumbers;
        }

        // GET: SageCcLookup/delivery-address/1064028
        // Returns delivery address for a given order number
        [HttpGet("delivery-address/{orderNumber}")]
        public async Task<ActionResult<SageDeliveryAddress>> GetDeliveryAddress(string orderNumber)
        {
            var sql = @"
                SELECT 
                    SDDA.PostalName,
                    SDDA.AddressLine1, 
                    SDDA.AddressLine2, 
                    SDDA.AddressLine3, 
                    SDDA.AddressLine4, 
                    SDDA.City, 
                    SDDA.County, 
                    SDDA.Country, 
                    SDDA.Postcode, 
                    SDDA.Contact, 
                    SDDA.TelephoneNo, 
                    SDDA.EmailAddress 
                FROM [SOPOrderReturn] AS SOR with (NOLOCK)
                INNER JOIN [SOPDocDelAddress] AS SDDA with (NOLOCK) 
                    ON SOR.SOPOrderReturnID = SDDA.SOPOrderReturnID
                WHERE TRY_CAST(SOR.documentNo AS INT) = TRY_CAST(@orderNumber AS INT)";

            var connection = _context.Database.GetDbConnection();
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.Add(new SqlParameter("@orderNumber", orderNumber));

            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var address = new SageDeliveryAddress
                {
                    PostalName = reader["PostalName"] as string,
                    AddressLine1 = reader["AddressLine1"] as string,
                    AddressLine2 = reader["AddressLine2"] as string,
                    AddressLine3 = reader["AddressLine3"] as string,
                    AddressLine4 = reader["AddressLine4"] as string,
                    City = reader["City"] as string,
                    County = reader["County"] as string,
                    Country = reader["Country"] as string,
                    Postcode = reader["Postcode"] as string,
                    Contact = reader["Contact"] as string,
                    TelephoneNo = reader["TelephoneNo"] as string,
                    EmailAddress = reader["EmailAddress"] as string
                };

                return address;
            }

            return NotFound(new { message = "Delivery address not found for the specified order number." });
        }
    }
}
