using System;
using System.Text.RegularExpressions;

namespace PMS.Application.Validation
{
    /// <summary>
    /// Utility methods for validating hex color strings in the format #RRGGBB.
    /// </summary>
    public static class HexColorValidator
    {
        private static readonly Regex HexColorRegex = new Regex("^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

        public static bool IsValid(string? color)
        {
            if (string.IsNullOrWhiteSpace(color))
            {
                return false;
            }

            return HexColorRegex.IsMatch(color);
        }
    }
}

