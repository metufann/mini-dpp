using System.Globalization;
using MiniDpp.Api.DTOs;

namespace MiniDpp.Api.Validators;

public class ProductRowValidator
{
    public IReadOnlyList<string> Validate(CsvProductRow row, out ValidatedProductRow? validated)
    {
        var errors = new List<string>();
        errors.AddRange(GtinValidator.Validate(row.Gtin));

        var modelName = NormalizeRequired(row.ModelName);
        if (modelName is null)
        {
            errors.Add("Model name cannot be empty.");
        }

        var batchNumber = NormalizeRequired(row.BatchNumber);
        if (batchNumber is null)
        {
            errors.Add("Batch number cannot be empty.");
        }

        var productionPlace = NormalizeRequired(row.ProductionPlace);
        if (productionPlace is null)
        {
            errors.Add("Production place cannot be empty.");
        }

        var cotton = ParseDecimal(row.CottonPercentage, "Cotton percentage", errors, requireNonNegativeRange: true);
        var elastane = ParseDecimal(row.ElastanePercentage, "Elastane percentage", errors, requireNonNegativeRange: true);
        var carbon = ParseCarbon(row.CarbonKgCO2e, errors);

        if (cotton is not null && elastane is not null && cotton.Value + elastane.Value != 100m)
        {
            errors.Add("Cotton and elastane percentages must total 100.");
        }

        if (errors.Count > 0)
        {
            validated = null;
            return errors;
        }

        validated = new ValidatedProductRow
        {
            Gtin = row.Gtin!.Trim(),
            ModelName = modelName!,
            BatchNumber = batchNumber!,
            ProductionPlace = productionPlace!,
            CottonPercentage = cotton!.Value,
            ElastanePercentage = elastane!.Value,
            CarbonKgCO2e = carbon!.Value
        };

        return errors;
    }

    private static string? NormalizeRequired(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static decimal? ParseDecimal(string? raw, string fieldName, List<string> errors, bool requireNonNegativeRange)
    {
        var trimmed = raw?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            errors.Add($"{fieldName} cannot be empty.");
            return null;
        }

        if (!decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            errors.Add($"{fieldName} must be numeric.");
            return null;
        }

        if (requireNonNegativeRange && (value < 0 || value > 100))
        {
            errors.Add($"{fieldName} must be between 0 and 100.");
        }

        return value;
    }

    private static decimal? ParseCarbon(string? raw, List<string> errors)
    {
        var trimmed = raw?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            errors.Add("Carbon value cannot be empty.");
            return null;
        }

        if (!decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            errors.Add("Carbon value must be numeric.");
            return null;
        }

        if (value < 0)
        {
            errors.Add("Carbon value must be nonnegative.");
        }

        return value;
    }
}
