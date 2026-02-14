namespace ISDN.Models
{
    /// <summary>
    /// JWT configuration settings mapped from appsettings.json
    /// </summary>
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpirationInMinutes { get; set; } = 120; // Default 2 hours
    }
}
