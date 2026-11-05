using MiniDpp.Api.DTOs;
using MiniDpp.Api.Validators;

namespace MiniDpp.Tests;

public class ProductRowValidatorTests
{
    private readonly ProductRowValidator _validator = new();

    [Fact]
    public void AcceptsCorrectPercentageTotalAndDecimalCarbon()
    {
        var errors = _validator.Validate(ValidRow(), out var validated);

        Assert.Empty(errors);
        Assert.NotNull(validated);
        Assert.Equal(98m, validated!.CottonPercentage);
        Assert.Equal(2m, validated.ElastanePercentage);
        Assert.Equal(12.4m, validated.CarbonKgCO2e);
        Assert.Equal("08690000000012", validated.Gtin);
    }

    [Fact]
    public void RejectsIncorrectPercentageTotal()
    {
        var row = ValidRow();
        row.CottonPercentage = "95";
        row.ElastanePercentage = "3";

        var errors = _validator.Validate(row, out var validated);

        Assert.Null(validated);
        Assert.Contains("Cotton and elastane percentages must total 100.", errors);
    }

    [Fact]
    public void RejectsNegativeCarbon()
    {
        var row = ValidRow();
        row.CarbonKgCO2e = "-1";

        var errors = _validator.Validate(row, out var validated);

        Assert.Null(validated);
        Assert.Contains("Carbon value must be nonnegative.", errors);
    }

    [Fact]
    public void RejectsEmptyRequiredFields()
    {
        var row = new CsvProductRow();

        var errors = _validator.Validate(row, out var validated);

        Assert.Null(validated);
        Assert.Contains("GTIN cannot be empty.", errors);
        Assert.Contains("Model name cannot be empty.", errors);
        Assert.Contains("Batch number cannot be empty.", errors);
        Assert.Contains("Production place cannot be empty.", errors);
        Assert.Contains("Cotton percentage cannot be empty.", errors);
        Assert.Contains("Elastane percentage cannot be empty.", errors);
        Assert.Contains("Carbon value cannot be empty.", errors);
    }

    [Fact]
    public void RejectsNonNumericDecimalValues()
    {
        var row = ValidRow();
        row.CottonPercentage = "98%";
        row.CarbonKgCO2e = "twelve";

        var errors = _validator.Validate(row, out var validated);

        Assert.Null(validated);
        Assert.Contains("Cotton percentage must be numeric.", errors);
        Assert.Contains("Carbon value must be numeric.", errors);
    }

    [Fact]
    public void CollectsMultipleValidationErrors()
    {
        var row = ValidRow();
        row.Gtin = "";
        row.ModelName = "";
        row.CottonPercentage = "150";
        row.ElastanePercentage = "-5";
        row.CarbonKgCO2e = "-2";

        var errors = _validator.Validate(row, out var validated);

        Assert.Null(validated);
        Assert.True(errors.Count >= 5);
        Assert.Contains("GTIN cannot be empty.", errors);
        Assert.Contains("Model name cannot be empty.", errors);
        Assert.Contains("Cotton percentage must be between 0 and 100.", errors);
        Assert.Contains("Elastane percentage must be between 0 and 100.", errors);
        Assert.Contains("Carbon value must be nonnegative.", errors);
    }

    private static CsvProductRow ValidRow() => new()
    {
        Gtin = "08690000000012",
        ModelName = "Slim Denim 501",
        BatchNumber = "B2027-01",
        CottonPercentage = "98",
        ElastanePercentage = "2",
        ProductionPlace = "Denizli",
        CarbonKgCO2e = "12.4"
    };
}
