using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UnitConversionAPI.Models;

namespace UnitConversionAPI.Tests.Integration;

/// <summary>
/// End-to-end integration tests that spin up the full ASP.NET Core pipeline in
/// memory using WebApplicationFactory.  These tests validate the HTTP contract
/// (status codes, JSON shape, error payloads) of the live application and
/// complement the faster unit tests in Services/ and Controllers/.
/// </summary>
public sealed class ConversionApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ConversionApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // ── GET /api/conversions/convert ─────────────────────────────────────────

    [Fact]
    public async Task Get_Convert_CelsiusToFahrenheit_Returns200AndCorrectBody()
    {
        var response = await _client.GetAsync(
            "/api/conversions/convert?value=100&from=celsius&to=fahrenheit");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ConversionResult>();
        result.Should().NotBeNull();
        result!.OutputValue.Should().BeApproximately(212.0, precision: 0.000_001);
        result.Category.Should().Be("Temperature");
        result.InputUnit.Should().Be("Celsius");
        result.OutputUnit.Should().Be("Fahrenheit");
    }

    [Fact]
    public async Task Get_Convert_MeterToFoot_Returns200AndCorrectBody()
    {
        var response = await _client.GetAsync(
            "/api/conversions/convert?value=1&from=meter&to=foot");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ConversionResult>();
        result!.OutputValue.Should().BeApproximately(3.280_839, precision: 0.000_01);
        result.Category.Should().Be("Length");
    }

    [Theory]
    [InlineData("kilogram", "pound",        1,   2.204_622_62)]
    [InlineData("kilometer", "mile",        1,   0.621_371_19)]
    [InlineData("liter",     "usgallon",    1,   0.264_172)]
    [InlineData("atmosphere","pascal",      1,   101_325.0)]
    public async Task Get_Convert_VariousCategories_ReturnCorrectValues(
        string from, string to, double input, double expected)
    {
        var response = await _client.GetAsync(
            $"/api/conversions/convert?value={input}&from={from}&to={to}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ConversionResult>();
        result!.OutputValue.Should().BeApproximately(expected, precision: 0.001);
    }

    // ── POST /api/conversions/convert ────────────────────────────────────────

    [Fact]
    public async Task Post_Convert_ValidBody_Returns200()
    {
        var payload = new { value = 0, from = "celsius", to = "kelvin" };

        var response = await _client.PostAsJsonAsync("/api/conversions/convert", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ConversionResult>();
        result!.OutputValue.Should().BeApproximately(273.15, precision: 0.000_001);
    }

    // ── Error cases ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Get_Convert_UnknownUnit_Returns422()
    {
        var response = await _client.GetAsync(
            "/api/conversions/convert?value=1&from=unknownunit&to=meter");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        response.Content.Headers.ContentType!.MediaType
            .Should().Be("application/problem+json");
    }

    [Fact]
    public async Task Get_Convert_IncompatibleUnits_Returns422()
    {
        var response = await _client.GetAsync(
            "/api/conversions/convert?value=1&from=meter&to=celsius");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // ── GET /api/conversions/units ───────────────────────────────────────────

    [Fact]
    public async Task Get_Units_NoFilter_Returns200AndNonEmptyList()
    {
        var response = await _client.GetAsync("/api/conversions/units");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var units = await response.Content.ReadFromJsonAsync<List<UnitInfo>>();
        units.Should().NotBeNullOrEmpty();
        units!.Should().OnlyContain(u => !string.IsNullOrWhiteSpace(u.Key));
    }

    [Fact]
    public async Task Get_Units_WithCategoryFilter_ReturnsOnlyThatCategory()
    {
        var response = await _client.GetAsync("/api/conversions/units?category=Temperature");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var units = await response.Content.ReadFromJsonAsync<List<UnitInfo>>();
        units.Should().NotBeNullOrEmpty();
        units!.Should().OnlyContain(u => u.Category == "Temperature");
    }

    [Fact]
    public async Task Get_Units_InvalidCategoryFilter_Returns422()
    {
        var response = await _client.GetAsync("/api/conversions/units?category=Nonsense");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // ── GET /api/conversions/categories ─────────────────────────────────────

    [Fact]
    public async Task Get_Categories_Returns200WithAllExpectedCategories()
    {
        var response = await _client.GetAsync("/api/conversions/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var categories = await response.Content.ReadFromJsonAsync<List<string>>();
        categories.Should().Contain(["Length", "Temperature", "Weight", "Area", "Volume", "Speed", "Pressure"]);
    }
}
