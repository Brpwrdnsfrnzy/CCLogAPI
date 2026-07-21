using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CCLogAPI.Models;
using Microsoft.Extensions.Logging;

namespace CCLogAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DiaryEntriesController : ControllerBase
    {
        private readonly CCLogContext _context;
        private readonly ILogger<DiaryEntriesController> _logger;

        public DiaryEntriesController(CCLogContext context, ILogger<DiaryEntriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/DiaryEntries
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DiaryEntryDto>>> GetDiaryEntries()
        {
            var entries = await _context.DiaryEntries
                .Select(d => new DiaryEntryDto
                {
                    ID = d.ID,
                    CreatedBy = d.CreatedBy,
                    CreatedDate = d.CreatedDate,
                    LogId = d.LogId,
                    ParentId = d.ParentId,
                    Text = d.Text
                })
                .ToListAsync();

            return Ok(entries);
        }

        // GET: api/DiaryEntries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DiaryEntryDto>> GetDiaryEntry(int id)
        {
            var diaryEntry = await _context.DiaryEntries
                .Where(d => d.ID == id)
                .Select(d => new DiaryEntryDto
                {
                    ID = d.ID,
                    CreatedBy = d.CreatedBy,
                    CreatedDate = d.CreatedDate,
                    LogId = d.LogId,
                    ParentId = d.ParentId,
                    Text = d.Text
                })
                .FirstOrDefaultAsync();

            if (diaryEntry == null)
            {
                return NotFound();
            }

            return diaryEntry;
        }

        // PUT: api/DiaryEntries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDiaryEntry(int id, DiaryEntry diaryEntry)
        {
            if (id != diaryEntry.ID)
            {
                return BadRequest();
            }

            _context.Entry(diaryEntry).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DiaryEntryExists(id))
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

        // POST: api/DiaryEntries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<DiaryEntryDto>> PostDiaryEntry(CreateDiaryEntryDto dto)
        {
            try
            {
                _logger.LogInformation($"Creating diary entry for Log {dto.LogId}, ParentId: {dto.ParentId}");

                // Convert 0 to null for ParentId
                int? parentId = (dto.ParentId.HasValue && dto.ParentId.Value == 0) ? null : dto.ParentId;

                var diaryEntry = new DiaryEntry
                {
                    CreatedBy = dto.CreatedBy,
                    CreatedDate = dto.CreatedDate,
                    Text = dto.Text,
                    LogId = dto.LogId,
                    ParentId = parentId
                };

                _logger.LogInformation($"Saving with ParentId: {diaryEntry.ParentId}");

                _context.DiaryEntries.Add(diaryEntry);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Diary entry created with ID {diaryEntry.ID}");

                // Return DTO instead of entity to avoid circular reference
                var resultDto = new DiaryEntryDto
                {
                    ID = diaryEntry.ID,
                    CreatedBy = diaryEntry.CreatedBy,
                    CreatedDate = diaryEntry.CreatedDate,
                    LogId = diaryEntry.LogId,
                    ParentId = diaryEntry.ParentId,
                    Text = diaryEntry.Text
                };

                return CreatedAtAction("GetDiaryEntry", new { id = diaryEntry.ID }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating diary entry: {ex.Message}");
                return StatusCode(500, new { 
                    error = "Error creating diary entry", 
                    details = ex.Message, 
                    innerException = ex.InnerException?.Message 
                });
            }
        }

        // DELETE: api/DiaryEntries/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiaryEntry(int id)
        {
            var diaryEntry = await _context.DiaryEntries.FindAsync(id);
            if (diaryEntry == null)
            {
                return NotFound();
            }

            _context.DiaryEntries.Remove(diaryEntry);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DiaryEntryExists(int id)
        {
            return _context.DiaryEntries.Any(e => e.ID == id);
        }
    }
}
