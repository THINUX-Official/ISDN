using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ISDN.Models;
using ISDN.Services;

namespace ISDN.Middleware
{
    /// <summary>
    /// Middleware to validate JWT tokens from cookies and set user claims
    /// </summary>
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtSettings _jwtSettings;

        public JwtMiddleware(RequestDelegate next, JwtSettings jwtSettings)
        {
            _next = next;
            _jwtSettings = jwtSettings;
        }

        public async Task Invoke(HttpContext context, IAuthenticationService authService)
        {
            // Try to get token from cookie
            var token = context.Request.Cookies["AuthToken"];

            if (!string.IsNullOrEmpty(token))
            {
                await AttachUserToContext(context, authService, token);
            }

            await _next(context);
        }

        private async Task AttachUserToContext(HttpContext context, IAuthenticationService authService, string token)
        {
            try
            {
                // Validate token
                var isValid = await authService.ValidateTokenAsync(token);
                if (!isValid)
                {
                    return;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

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

                // Extract claims and set them in HttpContext
                var claims = jwtToken.Claims.ToList();
                var identity = new ClaimsIdentity(claims, "jwt");
                context.User = new ClaimsPrincipal(identity);
            }
            catch
            {
                // Token validation failed - do nothing
            }
        }
    }
}
