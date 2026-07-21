using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CCLogAPI.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace CCLogAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly CCLogContext _context;
        private readonly ILogger<UsersController> _logger;
        private readonly Microsoft.AspNetCore.Identity.UserManager<User> _userManager;

        public UsersController(CCLogContext context, ILogger<UsersController> logger, Microsoft.AspNetCore.Identity.UserManager<User> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    Name = u.Name,
                    Role = u.Role,
                    Team = u.Team,
                    PhoneNumber = u.PhoneNumber,
                    Assignable = u.Assignable,
                    EmailConfirmed = u.EmailConfirmed
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    Name = u.Name,
                    Role = u.Role,
                    Team = u.Team,
                    PhoneNumber = u.PhoneNumber,
                    Assignable = u.Assignable,
                    EmailConfirmed = u.EmailConfirmed
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            _logger.LogInformation($"PutUser called for ID: {id}");

            if (id != user.Id)
            {
                _logger.LogWarning($"ID mismatch: URL ID {id} != User ID {user.Id}");
                return BadRequest("ID mismatch");
            }

            try
            {
                // Get the existing user from database WITH tracking
                var existingUser = await _context.Users.FindAsync(id);
                if (existingUser == null)
                {
                    _logger.LogWarning($"User {id} not found");
                    return NotFound();
                }

                // Update only the fields we want to allow changing
                // Don't touch PasswordHash, SecurityStamp, ConcurrencyStamp, etc.
                existingUser.UserName = user.UserName;
                existingUser.NormalizedUserName = user.UserName?.ToUpperInvariant();
                existingUser.Email = user.Email;
                existingUser.NormalizedEmail = user.Email?.ToUpperInvariant();
                existingUser.Name = user.Name;
                existingUser.Role = user.Role;
                existingUser.Team = user.Team;
                existingUser.PhoneNumber = user.PhoneNumber;
                // Include Assignable flag
                _logger.LogInformation($"Assignable incoming: {user.Assignable}, existing: {existingUser.Assignable}");
                existingUser.Assignable = user.Assignable;
                // Ensure EF marks Assignable as modified in case its value type doesn't trigger change detection
                _context.Entry(existingUser).Property(u => u.Assignable).IsModified = true;

                // SaveChanges will automatically update ConcurrencyStamp
                await _context.SaveChangesAsync();

                // Verify Assignable persisted; if not, perform a direct SQL update as a fallback
                if (existingUser.Assignable != user.Assignable)
                {
                    _logger.LogWarning($"Assignable not persisted via EF for user {id}, performing fallback SQL update to {user.Assignable}");
                    try
                    {
                        await _context.Database.ExecuteSqlInterpolatedAsync($"UPDATE [User] SET Assignable = {user.Assignable} WHERE ID = {id}");
                        _logger.LogInformation($"Fallback SQL update succeeded for user {id}");
                    }
                    catch (Exception sqlEx)
                    {
                        _logger.LogError(sqlEx, $"Fallback SQL update failed for user {id}: {sqlEx.Message}");
                        return StatusCode(500, new { error = "Error updating user Assignable flag", details = sqlEx.Message });
                    }
                }

                _logger.LogInformation($"User {id} updated successfully");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"Concurrency error updating user {id}");
                if (!UserExists(id))
                {
                    return NotFound();
                }
                return StatusCode(409, new { error = "The user was modified by another process. Please refresh and try again." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user {id}: {ex.Message}");
                return StatusCode(500, new { error = "Error updating user", details = ex.Message });
            }
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(CreateUserDto dto)
        {
            // Create user via UserManager so password is hashed and stamps are generated
            var newUser = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                Name = dto.Name,
                Role = dto.Role,
                Team = dto.Team,
                PhoneNumber = dto.PhoneNumber,
                Assignable = dto.Assignable,
                EmailConfirmed = dto.EmailConfirmed
            };

            IdentityResult result;
            _logger.LogInformation($"CreateUser: password provided: {!string.IsNullOrEmpty(dto.Password)}");
            if (!string.IsNullOrEmpty(dto.Password))
            {
                result = await _userManager.CreateAsync(newUser, dto.Password);
            }
            else
            {
                result = await _userManager.CreateAsync(newUser);
            }

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { message = "Failed to create user", errors });
            }

            // Log whether password hash was generated
            _logger.LogInformation($"CreateUser result.Succeeded={result.Succeeded}. PasswordHash present after create: {!string.IsNullOrEmpty(newUser.PasswordHash)}");

            // If password was provided but no PasswordHash present, try to add the password explicitly
            if (!string.IsNullOrEmpty(dto.Password))
            {
                var fresh = await _context.Users.FindAsync(newUser.Id);
                if (fresh != null && string.IsNullOrEmpty(fresh.PasswordHash))
                {
                    _logger.LogWarning($"PasswordHash missing after CreateAsync for user {fresh.Id}, attempting AddPasswordAsync fallback");
                    var addPwResult = await _userManager.AddPasswordAsync(fresh, dto.Password);
                    if (!addPwResult.Succeeded)
                    {
                        var addErrors = addPwResult.Errors.Select(e => e.Description);
                        _logger.LogError($"AddPasswordAsync failed for user {fresh.Id}: {string.Join(';', addErrors)}");
                        return BadRequest(new { message = "Failed to set user password", errors = addErrors });
                    }
                }
            }

            // Return created user (sanitized DTO)
            var createdDto = new UserDto
            {
                Id = newUser.Id,
                UserName = newUser.UserName,
                Email = newUser.Email,
                Name = newUser.Name,
                Role = newUser.Role,
                Team = newUser.Team,
                PhoneNumber = newUser.PhoneNumber,
                Assignable = newUser.Assignable,
                EmailConfirmed = newUser.EmailConfirmed
            };

            return CreatedAtAction("GetUser", new { id = createdDto.Id }, createdDto);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
