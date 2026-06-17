using FluentAssertions;
using UnitConversionAPI.Data;
using UnitConversionAPI.Exceptions;
using UnitConversionAPI.Services;

namespace UnitConversionAPI.Tests.Services;

/// <summary>
/// Unit tests for ConversionService.
/// Uses the real UnitRegistry (pure data, no I/O) to keep tests fast and
/// validate the full conversion pipeline without mocking data.
/// </summary>
public sealed class ConversionServiceTests
{
    private readonly ConversionService _sut;

    public ConversionServiceTests()
    {
        var registry = new UnitRegistry();
        _sut = new ConversionService(registry);
    }

    // ── Temperature ──────────────────────────────────────────────────────────

    [Fact]
    public void Convert_CelsiusToFahrenheit_ReturnsCorrectValue()
    {
        var result = _sut.Convert(100, "celsius", "fahrenheit");

        result.OutputValue.Should().BeApproximately(212.0, precision: 0.000_001);
        result.Category.Should().Be("Temperature");
    }

    [Fact]
    public void Convert_FahrenheitToCelsius_FreezingPoint()
    {
        var result = _sut.Convert(32, "fahrenheit", "celsius");

        result.OutputValue.Should().BeApproximately(0.0, precision: 0.000_001);
    }

    [Fact]
    public void Convert_CelsiusToKelvin_AbsoluteZero()
    {
        var result = _sut.Convert(-273.15, "celsius", "kelvin");

        result.OutputValue.Should().BeApproximately(0.0, precision: 0.000_001);
    }

    [Fact]
    public void Convert_KelvinToCelsius_BoilingPoint()
    {
        var result = _sut.Convert(373.15, "kelvin", "celsius");

        result.OutputValue.Should().BeApproximately(100.0, precision: 0.000_001);
    }

    [Fact]
    public void Convert_FahrenheitToKelvin_BodyTemperature()
    {
        // 98.6 °F = 310.15 K
        var result = _sut.Convert(98.6, "fahrenheit", "kelvin");

        result.OutputValue.Should().BeApproximately(310.15, precision: 0.001);
    }

    // ── Length ───────────────────────────────────────────────────────────────

    [Fact]
    public void Convert_MeterToFoot_OneMetreIsCorrect()
    {
        var result = _sut.Convert(1, "meter", "foot");

        result.OutputValue.Should().BeApproximately(3.280_839_895, precision: 0.000_001);
    }

    [Fact]
    public void Convert_KilometerToMile_OneKmIsCorrect()
    {
        var result = _sut.Convert(1, "kilometer", "mile");

        result.OutputValue.Should().BeApproximately(0.621_371_192, precision: 0.000_001);
    }

    [Fact]
    public void Convert_InchToCentimeter_OneInchIsCorrect()
    {
        var result = _sut.Convert(1, "inch", "centimeter");

        result.OutputValue.Should().BeApproximately(2.54, precision: 0.000_001);
    }

    [Theory]
    [InlineData("meter", "km", 1000, 1)]
    [InlineData("foot", "inch", 1, 12)]
    [InlineData("mile", "meter", 1, 1609.344)]
    public void Convert_CommonLengthPairs_ReturnsCorrectValue(
        string from, string to, double input, double expected)
    {
        var result = _sut.Convert(input, from, to);

        result.OutputValue.Should().BeApproximately(expected, precision: 0.001);
    }

    // ── Weight ───────────────────────────────────────────────────────────────

    [Fact]
    public void Convert_KilogramToPound_OneKgIsCorrect()
    {
        var result = _sut.Convert(1, "kilogram", "pound");

        result.OutputValue.Should().BeApproximately(2.204_622_62, precision: 0.000_001);
    }

    [Fact]
    public void Convert_GramToOunce_OneGramIsCorrect()
    {
        var result = _sut.Convert(1, "gram", "ounce");

        result.OutputValue.Should().BeApproximately(0.035_274, precision: 0.000_01);
    }

    // ── Area ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Convert_HectareToAcre_OneHectareIsCorrect()
    {
        var result = _sut.Convert(1, "hectare", "acre");

        result.OutputValue.Should().BeApproximately(2.471_053_814, precision: 0.000_001);
    }

    // ── Volume ───────────────────────────────────────────────────────────────

    [Fact]
    public void Convert_LiterToUsGallon_OneLiterIsCorrect()
    {
        var result = _sut.Convert(1, "liter", "usgallon");

        result.OutputValue.Should().BeApproximately(0.264_172, precision: 0.000_01);
    }

    // ── Pressure ─────────────────────────────────────────────────────────────

    [Fact]
    public void Convert_AtmosphereToPascal_OneAtmIsCorrect()
    {
        var result = _sut.Convert(1, "atmosphere", "pascal");

        result.OutputValue.Should().BeApproximately(101_325.0, precision: 0.001);
    }

    // ── Speed ────────────────────────────────────────────────────────────────

    [Fact]
    public void Convert_KphToMph_OneHundredKphIsCorrect()
    {
        var result = _sut.Convert(100, "kilometerperhour", "mileperhour");

        result.OutputValue.Should().BeApproximately(62.137_119_2, precision: 0.0001);
    }

    // ── Alias support ────────────────────────────────────────────────────────

    [Theory]
    [InlineData("c", "f", 100, 212)]
    [InlineData("km", "m", 1, 1000)]
    [InlineData("kg", "lb", 1, 2.204_622_62)]
    public void Convert_UsingAliasKeys_ReturnsCorrectValue(
        string from, string to, double input, double expected)
    {
        var result = _sut.Convert(input, from, to);

        result.OutputValue.Should().BeApproximately(expected, precision: 0.001);
    }

    // ── Same-unit identity ───────────────────────────────────────────────────

    [Theory]
    [InlineData("meter", 42.5)]
    [InlineData("celsius", -40)]
    [InlineData("kilogram", 100)]
    public void Convert_SameUnit_ReturnsIdenticalValue(string unit, double value)
    {
        var result = _sut.Convert(value, unit, unit);

        result.OutputValue.Should().BeApproximately(value, precision: 0.000_001);
    }

    // ── Error cases ──────────────────────────────────────────────────────────

    [Fact]
    public void Convert_UnknownFromUnit_ThrowsUnitNotFoundException()
    {
        var act = () => _sut.Convert(100, "flibbertigibbet", "meter");

        act.Should().Throw<UnitNotFoundException>()
            .Which.UnitKey.Should().Be("flibbertigibbet");
    }

    [Fact]
    public void Convert_UnknownToUnit_ThrowsUnitNotFoundException()
    {
        var act = () => _sut.Convert(100, "meter", "unknownunit");

        act.Should().Throw<UnitNotFoundException>();
    }

    [Fact]
    public void Convert_IncompatibleCategories_ThrowsIncompatibleUnitsException()
    {
        // Length ↔ Temperature — must be rejected
        var act = () => _sut.Convert(100, "meter", "celsius");

        act.Should().Throw<IncompatibleUnitsException>()
            .WithMessage("*Length*Temperature*");
    }

    // ── Discovery endpoints ──────────────────────────────────────────────────

    [Fact]
    public void GetAllUnits_ReturnsNonEmptyList()
    {
        var units = _sut.GetAllUnits();

        units.Should().NotBeEmpty();
        units.Should().OnlyContain(u =>
            !string.IsNullOrWhiteSpace(u.Key) &&
            !string.IsNullOrWhiteSpace(u.DisplayName) &&
            !string.IsNullOrWhiteSpace(u.Symbol) &&
            !string.IsNullOrWhiteSpace(u.Category));
    }

    [Fact]
    public void GetUnitsByCategory_Temperature_ReturnsOnlyTemperatureUnits()
    {
        var units = _sut.GetUnitsByCategory("Temperature");

        units.Should().NotBeEmpty();
        units.Should().OnlyContain(u => u.Category == "Temperature");
        units.Should().Contain(u => u.Key == "celsius");
        units.Should().Contain(u => u.Key == "fahrenheit");
        units.Should().Contain(u => u.Key == "kelvin");
    }

    [Fact]
    public void GetUnitsByCategory_InvalidCategory_ThrowsUnitNotFoundException()
    {
        var act = () => _sut.GetUnitsByCategory("Nonsense");

        act.Should().Throw<UnitNotFoundException>();
    }

    [Fact]
    public void GetCategories_ReturnsAllExpectedCategories()
    {
        var categories = _sut.GetCategories();

        categories.Should().Contain(["Length", "Temperature", "Weight", "Area", "Volume", "Speed", "Pressure"]);
    }
}
