using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ISDN.Data;
using ISDN.Models;
using BCrypt.Net;

namespace ISDN.Services
{
    /// <summary>
    /// Authentication service implementing JWT-based authentication with BCrypt password hashing
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IsdnDbContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly IAuditLogService _auditLogService;

        public AuthenticationService(
            IsdnDbContext context,
            IOptions<JwtSettings> jwtSettings,
            IAuditLogService auditLogService)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// Authenticate user and generate JWT token
        /// </summary>
        public async Task<(bool Success, string Token, User? User, string Message)> LoginAsync(
            string email, string password, string ipAddress)
        {
            try
            {
                // Normalize email input: trim whitespace and convert to lowercase
                var normalizedEmail = email?.Trim().ToLower();
                
                if (string.IsNullOrWhiteSpace(normalizedEmail))
                {
                    await _auditLogService.LogActionAsync(0, "LOGIN_FAILED", "User", null, 
                        "Failed login attempt: empty email", ipAddress);
                    return (false, string.Empty, null, "Invalid email or password.");
                }

                // Find user by email (case-insensitive comparison) and check if active
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail && u.IsActive);

                if (user == null)
                {
                    await _auditLogService.LogActionAsync(0, "LOGIN_FAILED", "User", null, 
                        $"Failed login attempt for email: {normalizedEmail}", ipAddress);
                    return (false, string.Empty, null, "Invalid email or password.");
                }

                // Verify password using BCrypt - DO NOT hash the input password again
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                if (!isPasswordValid)
                {
                    await _auditLogService.LogActionAsync(user.UserId, "LOGIN_FAILED", "User", user.UserId, 
                        "Invalid password attempt", ipAddress);
                    return (false, string.Empty, null, "Invalid email or password.");
                }

                // Generate JWT token
                var token = GenerateJwtToken(user);

                // Store token in database
                var jwtToken = new JwtToken
                {
                    UserId = user.UserId,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                    CreatedAt = DateTime.UtcNow,
                    IsRevoked = false
                };
                _context.JwtTokens.Add(jwtToken);

                // Update last login
                user.LastLogin = DateTime.UtcNow;
                _context.Users.Update(user);

                await _context.SaveChangesAsync();

                // Log successful login
                await _auditLogService.LogActionAsync(user.UserId, "LOGIN_SUCCESS", "User", user.UserId, 
                    $"User logged in successfully", ipAddress);

                return (true, token, user, "Login successful.");
            }
            catch (Exception ex)
            {
                return (false, string.Empty, null, $"Login failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Register a new user with BCrypt password hashing
        /// </summary>
        public async Task<(bool Success, string Message)> RegisterAsync(
            string fullName, string email, string password, string roleName = "CUSTOMER")
        {
            try
            {
                // Normalize email input: trim whitespace and convert to lowercase
                var normalizedEmail = email?.Trim().ToLower();
                
                if (string.IsNullOrWhiteSpace(normalizedEmail))
                {
                    return (false, "Invalid email address.");
                }

                // Check if email already exists (case-insensitive)
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
                if (existingUser != null)
                {
                    return (false, "Email already registered.");
                }

                // Get role
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
                if (role == null)
                {
                    return (false, "Invalid role specified.");
                }

                // Hash password using BCrypt
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                // Create new user with normalized email
                var user = new User
                {
                    FullName = fullName?.Trim(),
                    Email = normalizedEmail,
                    PasswordHash = passwordHash,
                    RoleId = role.RoleId,
                    IsActive = true,
                    TwoFactorEnabled = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Log registration
                await _auditLogService.LogActionAsync(user.UserId, "USER_REGISTERED", "User", user.UserId, 
                    $"New user registered: {email}", "System");

                return (true, "Registration successful.");
            }
            catch (Exception ex)
            {
                return (false, $"Registration failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Validate if JWT token is valid and not revoked
        /// </summary>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            var jwtToken = await _context.JwtTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);

            return jwtToken != null;
        }

        /// <summary>
        /// Revoke a JWT token (logout)
        /// </summary>
        public async Task RevokeTokenAsync(string token)
        {
            var jwtToken = await _context.JwtTokens.FirstOrDefaultAsync(t => t.Token == token);
            if (jwtToken != null)
            {
                jwtToken.IsRevoked = true;
                _context.JwtTokens.Update(jwtToken);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Get user from JWT token
        /// </summary>
        public async Task<User?> GetUserFromTokenAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "user_id").Value);

                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                return user;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Generate JWT token for authenticated user
        /// </summary>
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

            var claims = new List<Claim>
            {
                new Claim("user_id", user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "CUSTOMER"),
                new Claim("role_name", user.Role?.RoleName ?? "CUSTOMER")
            };

            // Add RDC ID claim if user is assigned to an RDC
            if (user.RdcId.HasValue && user.RdcId.Value > 0)
            {
                claims.Add(new Claim("rdc_id", user.RdcId.Value.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
