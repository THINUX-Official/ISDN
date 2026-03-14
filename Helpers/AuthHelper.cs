using System;

namespace ISDN.Helpers
{
    public static class AuthHelper
    {
        /// <summary>
        /// Splits the stored string to retrieve the registration code and hashed password.
        /// Expected format: "UNIQUECODE|HASHEDPASSWORD"
        /// </summary>
        public static (string? Code, string Hash) ParseTempPasswordHash(string? combined)
        {
            if (string.IsNullOrEmpty(combined))
                return (null, string.Empty);

            var parts = combined.Split('|');

            // If the pipe separator exists, we have both
            if (parts.Length >= 2)
                return (parts[0], parts[1]);

            // Fallback: if no separator, assume it's just the old hash format
            return (null, parts[0]);
        }

        /// <summary>
        /// Combines a registration code and a password hash into a single string for storage.
        /// </summary>
        public static string CreateTempPasswordHash(string password, string? uniqueCode)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(password);
            return uniqueCode != null ? $"{uniqueCode}|{hash}" : hash;
        }


        /// <summary>
        /// Safely retrieves data from the formatted string.
        /// Format: "|[Type]|[UserType]|[BusinessName]|[BranchName]"
        /// </summary>
        public static string GetValue(string formattedString, int index = 3)
        {
            if (string.IsNullOrEmpty(formattedString)) return "N/A";

            var parts = formattedString.Split('|');

            return (parts.Length > index && !string.IsNullOrWhiteSpace(parts[index]))
                ? parts[index]
                : "N/A";
        }

        public static string FormatBusinessName(string businessType, string userType, string businessName, string branchName)
        {
            // Maintains the structure: |Type|UserType|BusinessName|BranchName
            return $"|{businessType}|{userType}|{businessName}|{branchName}";
        }
    }
}