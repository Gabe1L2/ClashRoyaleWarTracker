using System.Text.RegularExpressions;

namespace ClashRoyaleWarTracker.Application.Helpers
{
    public static class ClanTagValidator
    {
        public static (bool isValid, string sanitizedTag, string errorMessage) ValidateAndSanitizeClanTag(string clanTag)
        {
            if (string.IsNullOrWhiteSpace(clanTag))
            {
                return (false, string.Empty, "Clan tag cannot be empty");
            }

            // Remove all non-alphanumeric characters (including spaces, special chars, etc.)
            var sanitized = Regex.Replace(clanTag.Trim(), @"[^a-zA-Z0-9]", "");

            if (string.IsNullOrEmpty(sanitized))
            {
                return (false, string.Empty, "Clan tag must contain at least one letter or number");
            }

            if (sanitized.Length > 25)
            {
                return (false, string.Empty, "Clan tag cannot exceed 25 characters");
            }

            // Clash Royale clan tags are typically 8-9 characters, but let's be flexible
            // Most real clan tags are between 3-15 characters after removing the #
            if (sanitized.Length < 3)
            {
                return (false, string.Empty, "Clan tag must be at least 3 characters long");
            }

            // Add # prefix if not already present (Clash Royale format)

            return (true, sanitized, string.Empty);
        }
    }
}
