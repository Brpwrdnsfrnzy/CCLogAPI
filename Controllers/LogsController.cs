using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CCLogAPI.Models;
using AutoMapper;

namespace CCLogAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly CCLogContext _context;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Logging.ILogger<LogsController> _logger;

        public LogsController(CCLogContext context, IMapper mapper, Microsoft.Extensions.Logging.ILogger<LogsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

    // POST: api/Logs/search
    [HttpPost("search")]
    public async Task<ActionResult> SearchLogs(SearchLogsRequest req)
    {
        // Basic server-side search implementation. All filters are optional.
        _logger?.LogInformation($"SearchLogs request: Id={req.Id}, UserId={req.UserId}, AssignedUserId={req.AssignedUserId}, CustomerAccount={req.CustomerAccount}, Text={req.Text}, StatusId={req.StatusId}, StatusIdsCount={(req.StatusIds==null?0:req.StatusIds.Count)}, LogTypeId={req.LogTypeId}, AssignedTeamId={req.AssignedTeamId}, DateFrom={req.DateFrom}, DateTo={req.DateTo}, Completed={req.Completed}, ReportedIssueId={req.ReportedIssueId}, SubIssue={req.SubIssue}, Page={req.Page}, PageSize={req.PageSize}");

        var q = _context.Logs.AsQueryable();
        // Normalize date range: if both provided but DateTo is earlier than DateFrom, swap them
        if (req.DateFrom.HasValue && req.DateTo.HasValue && req.DateTo.Value < req.DateFrom.Value)
        {
            var tmp = req.DateFrom;
            req.DateFrom = req.DateTo;
            req.DateTo = tmp;
        }


        // Search by exact log ID
        if (req.Id.HasValue)
        {
            q = q.Where(l => l.ID == req.Id.Value);
        }

        // Search by userId against LoggedBy or AssignedTo
        if (req.UserId.HasValue)
        {
            var uid = req.UserId.Value;
            q = q.Where(l => l.LoggedBy == uid || l.AssignedTo == uid);
        }

        if (!string.IsNullOrWhiteSpace(req.CustomerAccount))
        {
            var acct = req.CustomerAccount.Trim();
            // case-insensitive contains via EF.Functions.Like
            q = q.Where(l => EF.Functions.Like(l.CustomerAccount, $"%{acct}%"));
        }

        if (!string.IsNullOrWhiteSpace(req.CustomerCompany))
        {
            var company = req.CustomerCompany.Trim();
            q = q.Where(l => EF.Functions.Like(l.CustomerCompany, $"%{company}%"));
        }

        // Date range filtering (inclusive). DateTo is treated as end-of-day.
        if (req.DateFrom.HasValue)
        {
            var from = req.DateFrom.Value.Date;
            q = q.Where(l => l.DateLogged >= from);
        }

        if (req.DateTo.HasValue)
        {
            var toEnd = req.DateTo.Value.Date.AddDays(1).AddTicks(-1);
            q = q.Where(l => l.DateLogged <= toEnd);
        }

        // Completed filter: if true => SignOffDate IS NOT NULL; if false => SignOffDate IS NULL
        if (req.Completed.HasValue)
        {
            if (req.Completed.Value)
                q = q.Where(l => l.SignOffDate.HasValue);
            else
                q = q.Where(l => !l.SignOffDate.HasValue);
        }

        // Support multiple status filters (client may send statusIds array)
        if (req.StatusIds != null && req.StatusIds.Any())
        {
            var sids = req.StatusIds;
            q = q.Where(l => sids.Contains(l.Status));
        }
        else if (req.StatusId.HasValue)
        {
            var sid = req.StatusId.Value;
            q = q.Where(l => l.Status == sid);
        }

        if (req.LogTypeId.HasValue)
        {
            var lt = req.LogTypeId.Value;
            q = q.Where(l => l.LogType == lt);
        }

        // Filter by assigned user or assigned user's team
        if (req.AssignedUserId.HasValue)
        {
            var assignedUser = req.AssignedUserId.Value;
            q = q.Where(l => l.AssignedTo == assignedUser);
        }
        else if (req.AssignedTeamId.HasValue)
        {
            var teamId = req.AssignedTeamId.Value;
            // Filter logs where the AssignedTo user's Team matches the requested team
            q = q.Where(l => _context.Users.Any(u => u.Id == l.AssignedTo && u.Team == teamId));
        }

        if (!string.IsNullOrWhiteSpace(req.Text))
        {
            var lowered = req.Text.Trim().ToLower();
            var pattern = $"%{lowered}%";
            // Case-insensitive search across LogText, customer phone, email, contact/name, company and any related diary entries
            q = q.Where(l =>
                (l.LogText != null && EF.Functions.Like(l.LogText.ToLower(), pattern))
                || (l.CustomerPhone != null && EF.Functions.Like(l.CustomerPhone.ToLower(), pattern))
                || (l.CustomerEmail != null && EF.Functions.Like(l.CustomerEmail.ToLower(), pattern))
                || (l.CustomerContact != null && EF.Functions.Like(l.CustomerContact.ToLower(), pattern))
                || (l.CustomerCompany != null && EF.Functions.Like(l.CustomerCompany.ToLower(), pattern))
                || l.DiaryEntries.Any(de => de.Text != null && EF.Functions.Like(de.Text.ToLower(), pattern))
            );
        }

        if (req.ReportedIssueId.HasValue)
        {
            var ri = req.ReportedIssueId.Value;
            q = q.Where(l => l.ReportedIssue == ri);
        }

        if (!string.IsNullOrWhiteSpace(req.SubIssue))
        {
            var si = req.SubIssue.Trim();
            q = q.Where(l => EF.Functions.Like(l.SubIssue, $"%{si}%"));
        }

        if (req.ReturnMethodId.HasValue)
        {
            var rm = req.ReturnMethodId.Value;
            q = q.Where(l => l.ReturnMethod == rm);
        }

        // Order by DateLogged desc by default
        q = q.OrderByDescending(l => l.DateLogged);

        // Simple paging with total count in the response so clients can page correctly
        var page = Math.Max(1, req.Page);
        var pageSize = Math.Clamp(req.PageSize, 1, 1000);

        var totalCount = await q.CountAsync();
        var results = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new
        {
            totalCount,
            page,
            pageSize,
            results
        });
    }

        // GET: api/Logs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Log>>> GetLogs()
        {
            return await _context.Logs.ToListAsync();
        }

        // GET: Logs/by-order/{orderNo}
        // Normalises by stripping leading zeros so that padded (e.g. 0000450321)
        // and unpadded (e.g. 450321) values match. Uses a candidate IN list so
        // the query stays SARGable and EF can translate it to SQL.
        [HttpGet("by-order/{orderNo}")]
        public async Task<ActionResult<IEnumerable<Log>>> GetLogsByOrder(string orderNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderNo))
                {
                    return BadRequest("orderNo is required.");
                }

                var candidates = BuildZeroPaddedCandidates(orderNo);
                if (candidates.Count == 0)
                {
                    return Ok(Array.Empty<Log>());
                }

                var results = await _context.Logs
                    .Where(l => l.OrderNo != null && candidates.Contains(l.OrderNo))
                    .OrderByDescending(l => l.DateLogged)
                    .ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error searching for order {OrderNo}", orderNo);
                return StatusCode(500, new { error = "Error searching for order", details = ex.Message });
            }
        }

        // GET: Logs/by-invoice/{invoiceNo}
        // Normalises by stripping leading zeros so that padded (e.g. 0000450321)
        // and unpadded (e.g. 450321) values match. Uses a candidate IN list so
        // the query stays SARGable and EF can translate it to SQL.
        [HttpGet("by-invoice/{invoiceNo}")]
        public async Task<ActionResult<IEnumerable<Log>>> GetLogsByInvoice(string invoiceNo)
        {
            if (string.IsNullOrWhiteSpace(invoiceNo))
            {
                return BadRequest("invoiceNo is required.");
            }

            var candidates = BuildZeroPaddedCandidates(invoiceNo);
            if (candidates.Count == 0)
            {
                return Ok(Array.Empty<Log>());
            }

            var results = await _context.Logs
                .Where(l => l.InvoiceNo != null && candidates.Contains(l.InvoiceNo))
                .OrderByDescending(l => l.DateLogged)
                .ToListAsync();

            return Ok(results);
        }

        // Builds a list of zero-padded variants of the input, e.g. for "450321"
        // => ["450321", "0450321", "00450321", ..., up to MaxPaddedLength chars].
        // This lets us do an indexed IN comparison instead of a non-SARGable
        // TRIM on the column.
        private static List<string> BuildZeroPaddedCandidates(string input)
        {
            const int MaxPaddedLength = 20;
            var trimmed = input.Trim().TrimStart('0');
            var list = new List<string>();

            if (trimmed.Length == 0)
            {
                // Input was all zeros (or empty) - match stored all-zero or empty values.
                list.Add(string.Empty);
                for (var i = 1; i <= MaxPaddedLength; i++)
                {
                    list.Add(new string('0', i));
                }
                return list;
            }

            list.Add(trimmed);
            for (var len = trimmed.Length + 1; len <= MaxPaddedLength; len++)
            {
                list.Add(trimmed.PadLeft(len, '0'));
            }
            return list;
        }

        // GET: api/Logs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Log>> GetLog(int id)
        {
            var log = await _context.Logs.FindAsync(id);

            if (log == null)
            {
                return NotFound();
            }

            return log;
        }

        // PUT: api/Logs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLog(int id, Log log)
        {
            if (id != log.ID)
            {
                return BadRequest();
            }

            _context.Entry(log).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LogExists(id))
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

        // POST: api/Logs
        // Accept DTO, validate FKs, map with AutoMapper, then save
        [HttpPost]
        public async Task<ActionResult<Log>> PostLog(CreateLogDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Server-side foreign key validation
            if (!await _context.Users.AnyAsync(u => u.Id == dto.LoggedBy))
                ModelState.AddModelError(nameof(dto.LoggedBy), "LoggedBy user does not exist.");

            if (!await _context.Users.AnyAsync(u => u.Id == dto.LastUpdatedBy))
                ModelState.AddModelError(nameof(dto.LastUpdatedBy), "LastUpdatedBy user does not exist.");

            if (dto.SignedOffBy.HasValue && !await _context.Users.AnyAsync(u => u.Id == dto.SignedOffBy.Value))
                ModelState.AddModelError(nameof(dto.SignedOffBy), "SignedOffBy user does not exist.");

            if (!await _context.LogTypes.AnyAsync(lt => lt.ID == dto.LogType))
                ModelState.AddModelError(nameof(dto.LogType), "LogType does not exist.");

            if (!await _context.LogStatuses.AnyAsync(ls => ls.ID == dto.Status))
                ModelState.AddModelError(nameof(dto.Status), "Status does not exist.");

            if (dto.GoodsReceived.HasValue)
            {
                if (dto.GoodsReceived.Value == 0)
                    ModelState.AddModelError(nameof(dto.GoodsReceived), "GoodsReceived must be a valid status id.");
                else if (!await _context.GoodsReceivedStatuses.AnyAsync(g => g.ID == dto.GoodsReceived.Value))
                    ModelState.AddModelError(nameof(dto.GoodsReceived), "GoodsReceived status does not exist.");
            }

            if (dto.SignOffReason.HasValue && !await _context.SignOffReasons.AnyAsync(s => s.ID == dto.SignOffReason.Value))
                ModelState.AddModelError(nameof(dto.SignOffReason), "SignOffReason does not exist.");

            if (dto.ReturnMethod.HasValue && !await _context.ReturnMethods.AnyAsync(r => r.ID == dto.ReturnMethod.Value))
                ModelState.AddModelError(nameof(dto.ReturnMethod), "ReturnMethod does not exist.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var log = _mapper.Map<Log>(dto);
            log.CustomerAccount = log.CustomerAccount ?? string.Empty;
            log.SubIssue = log.SubIssue ?? string.Empty;
            log.ReturnMethod ??= 1;

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLog", new { id = log.ID }, log);
        }

        // DELETE: api/Logs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLog(int id)
        {
            var log = await _context.Logs.FindAsync(id);
            if (log == null)
            {
                return NotFound();
            }

            _context.Logs.Remove(log);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LogExists(int id)
        {
            return _context.Logs.Any(e => e.ID == id);
        }
    }
}
