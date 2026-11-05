using MiniDpp.Api.Validators;

namespace MiniDpp.Tests;

public class GtinValidatorTests
{
    [Theory]
    [InlineData("08690000000012")]
    [InlineData("08690000000029")]
    [InlineData("00000000000000")]
    public void AcceptsValidGtinsIncludingLeadingZeros(string gtin)
    {
        Assert.True(GtinValidator.IsValid(gtin));
        Assert.Empty(GtinValidator.Validate(gtin));
    }

    [Fact]
    public void RejectsEmptyGtin()
    {
        var errors = GtinValidator.Validate("  ");
        Assert.Contains("GTIN cannot be empty.", errors);
    }

    [Theory]
    [InlineData("8690000000012")]
    [InlineData("086900000000120")]
    public void RejectsWrongLength(string gtin)
    {
        var errors = GtinValidator.Validate(gtin);
        Assert.Contains("GTIN must contain exactly 14 numeric digits.", errors);
    }

    [Fact]
    public void RejectsNonNumericGtin()
    {
        var errors = GtinValidator.Validate("0869000000001A");
        Assert.Contains("GTIN must contain exactly 14 numeric digits.", errors);
    }

    [Fact]
    public void RejectsInvalidCheckDigit()
    {
        var errors = GtinValidator.Validate("08690000000013");
        Assert.Contains("GTIN check digit is invalid.", errors);
    }
}
