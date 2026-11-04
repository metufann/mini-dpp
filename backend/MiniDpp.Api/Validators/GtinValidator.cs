using System.Text.RegularExpressions;

namespace MiniDpp.Api.Validators;

public static class GtinValidator
{
    private static readonly Regex FourteenDigits = new("^[0-9]{14}$", RegexOptions.Compiled);

    public static List<string> Validate(string? gtin)
    {
        var errors = new List<string>();
        var value = gtin?.Trim() ?? string.Empty;

        if (value.Length == 0)
        {
            errors.Add("GTIN cannot be empty.");
            return errors;
        }

        if (!FourteenDigits.IsMatch(value))
        {
            errors.Add("GTIN must contain exactly 14 numeric digits.");
            return errors;
        }

        if (!HasValidCheckDigit(value))
        {
            errors.Add("GTIN check digit is invalid.");
        }

        return errors;
    }

    public static bool IsValid(string? gtin) => Validate(gtin).Count == 0;

    public static bool HasValidCheckDigit(string gtin)
    {
        var sum = 0;
        var multiplier = 3;

        for (var i = gtin.Length - 2; i >= 0; i--)
        {
            sum += (gtin[i] - '0') * multiplier;
            multiplier = multiplier == 3 ? 1 : 3;
        }

        var expectedCheckDigit = (10 - (sum % 10)) % 10;
        return gtin[^1] - '0' == expectedCheckDigit;
    }
}
