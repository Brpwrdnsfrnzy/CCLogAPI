using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using CCLogAPI.Models;
using CCLogAPI.Services;

namespace CCLogAPI.Controllers;

[Route("[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IConfiguration configuration,
        IEmailService emailService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new User
        {
            UserName = dto.Username ?? dto.Email,
            Email = dto.Email,
            Name = dto.Name,
            Role = dto.Role,
            Team = dto.Team
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByNameAsync(dto.Username);
        if (user == null)
        {
            // Try finding by email if username lookup failed
            user = await _userManager.FindByEmailAsync(dto.Username);
        }
        if (user == null)
            return Unauthorized();

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
            return Unauthorized();

        // create claims
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("Team", user.Team.ToString()),
            new Claim("Name", user.Name ?? string.Empty)
        };

        var jwtKey = _configuration["Jwt:Key"] ?? "ReplaceThisWithAStrongSecretInUserSecrets";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            // Don't reveal that the user doesn't exist
            return Ok(new { message = "If the email exists, a password reset link has been sent." });
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Get frontend URL from configuration
        var frontendUrl = _configuration["WebAppBaseUrl"] ?? _configuration["FrontendUrl"] ?? "http://localhost:3000";

        // URL encode the token and email for safe transmission in URL
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(user.Email);

        // Create the password reset link
        var resetLink = $"{frontendUrl}/reset-password?token={encodedToken}&email={encodedEmail}";

        // Create email HTML body
        var emailBody = $@"
            <html>
            <body>
                <h2>Password Reset Request</h2>
                <p>Hello {user.Name},</p>
                <p>You have requested to reset your password for the CCLog system.</p>
                <p>Please click the link below to reset your password:</p>
                <p><a href=""{resetLink}"">Reset Password</a></p>
                <p>If the link doesn't work, copy and paste this URL into your browser:</p>
                <p>{resetLink}</p>
                <p>This link will expire in 24 hours.</p>
                <p>If you did not request a password reset, please ignore this email.</p>
                <br/>
                <p>Best regards,<br/>CCLog System</p>
            </body>
            </html>
        ";

        try
        {
            await _emailService.SendAsync(user.Email, "CCLog - Password Reset Request", emailBody);
            _logger.LogInformation("Password reset email sent to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
            return StatusCode(500, new { message = "Failed to send password reset email. Please try again later." });
        }

        return Ok(new { message = "If the email exists, a password reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return BadRequest(new { message = "Invalid request." });

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = "Password has been reset successfully." });
    }

    [HttpPost("admin/reset-password")]
    // TODO: Add [Authorize(Roles = "Admin")] when role-based auth is configured
    public async Task<IActionResult> AdminResetPassword(AdminResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { message = "Validation failed", errors = errors });
        }

        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
        if (user == null)
            return NotFound(new { message = "User not found." });

        // Trim potentially dirty values from imported or pasted emails/usernames
        var trimmedEmail = user.Email?.Trim();
        var trimmedUserName = user.UserName?.Trim();
        var needUpdate = false;
        if (trimmedEmail != user.Email)
        {
            user.Email = trimmedEmail;
            user.NormalizedEmail = trimmedEmail?.ToUpperInvariant();
            needUpdate = true;
        }
        if (trimmedUserName != user.UserName)
        {
            user.UserName = trimmedUserName;
            user.NormalizedUserName = trimmedUserName?.ToUpperInvariant();
            needUpdate = true;
        }

        if (needUpdate)
        {
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description);
                return BadRequest(new { message = "Failed to normalize user data.", errors = errors });
            }
        }

        // Remove existing password if one exists
        if (await _userManager.HasPasswordAsync(user))
        {
            await _userManager.RemovePasswordAsync(user);
        }

        // Add the new password
        var result = await _userManager.AddPasswordAsync(user, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { message = "Password validation failed", errors = errors });
        }

        // Optionally invalidate existing sessions
        await _userManager.UpdateSecurityStampAsync(user);

        return Ok(new { 
            message = $"Password reset successfully for user {user.Name} ({user.Email}).",
            requirePasswordChange = dto.RequirePasswordChange
        });
    }
}