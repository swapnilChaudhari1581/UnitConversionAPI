using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using UnitConversionAPI.Controllers;
using UnitConversionAPI.Exceptions;
using UnitConversionAPI.Models;
using UnitConversionAPI.Services;

namespace UnitConversionAPI.Tests.Controllers;

/// <summary>
/// Unit tests for ConversionController.
/// IConversionService is mocked so these tests are purely about HTTP routing,
/// request binding, and response shaping — not about conversion accuracy
/// (that belongs in ConversionServiceTests).
/// </summary>
public sealed class ConversionControllerTests
{
    private readonly IConversionService _serviceMock;
    private readonly ConversionController _sut;

    private static readonly ConversionResult SampleResult = new()
    {
        InputValue   = 100,
        InputUnit    = "Celsius",
        InputSymbol  = "°C",
        OutputValue  = 212,
        OutputUnit   = "Fahrenheit",
        OutputSymbol = "°F",
        Category     = "Temperature"
    };

    public ConversionControllerTests()
    {
        _serviceMock = Substitute.For<IConversionService>();
        _sut = new ConversionController(_serviceMock);
    }

    // ── GET convert ──────────────────────────────────────────────────────────

    [Fact]
    public void ConvertGet_ValidRequest_Returns200WithResult()
    {
        _serviceMock.Convert(100, "celsius", "fahrenheit").Returns(SampleResult);

        var actionResult = _sut.ConvertGet(100, "celsius", "fahrenheit");

        var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(SampleResult);
    }

    [Fact]
    public void ConvertGet_CallsServiceWithCorrectParameters()
    {
        _serviceMock.Convert(Arg.Any<double>(), Arg.Any<string>(), Arg.Any<string>())
                    .Returns(SampleResult);

        _sut.ConvertGet(42.5, "meter", "foot");

        _serviceMock.Received(1).Convert(42.5, "meter", "foot");
    }

    // ── POST convert ─────────────────────────────────────────────────────────

    [Fact]
    public void ConvertPost_ValidRequest_Returns200WithResult()
    {
        var request = new ConversionRequest { Value = 100, From = "celsius", To = "fahrenheit" };
        _serviceMock.Convert(100, "celsius", "fahrenheit").Returns(SampleResult);

        var actionResult = _sut.ConvertPost(request);

        var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(SampleResult);
    }

    // ── GET units ────────────────────────────────────────────────────────────

    [Fact]
    public void GetUnits_NoFilter_ReturnsAllUnits()
    {
        var units = new List<UnitInfo>
        {
            new("celsius", "Celsius", "°C", "Temperature"),
            new("meter",   "Meter",   "m",  "Length")
        };
        _serviceMock.GetAllUnits().Returns(units);

        var actionResult = _sut.GetUnits();

        var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(units);
    }

    [Fact]
    public void GetUnits_WithCategoryFilter_CallsGetUnitsByCategory()
    {
        var units = new List<UnitInfo>
        {
            new("celsius",    "Celsius",    "°C", "Temperature"),
            new("fahrenheit", "Fahrenheit", "°F", "Temperature")
        };
        _serviceMock.GetUnitsByCategory("Temperature").Returns(units);

        var actionResult = _sut.GetUnits("Temperature");

        _serviceMock.Received(1).GetUnitsByCategory("Temperature");
        _serviceMock.DidNotReceive().GetAllUnits();
        var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(units);
    }

    // ── GET categories ───────────────────────────────────────────────────────

    [Fact]
    public void GetCategories_ReturnsServiceResult()
    {
        var categories = new[] { "Length", "Temperature", "Weight" };
        _serviceMock.GetCategories().Returns(categories);

        var actionResult = _sut.GetCategories();

        var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(categories);
    }
}
