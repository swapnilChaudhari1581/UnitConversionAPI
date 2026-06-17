using Microsoft.AspNetCore.Mvc;
using UnitConversionAPI.Models;
using UnitConversionAPI.Services;

namespace UnitConversionAPI.Controllers;

/// <summary>
/// Exposes endpoints for converting numerical values between units of measurement
/// and for discovering the units and categories that the API supports.
/// </summary>
[ApiController]
[Route("api/conversions")]
[Produces("application/json")]
public sealed class ConversionController : ControllerBase
{
    private readonly IConversionService _conversionService;

    public ConversionController(IConversionService conversionService)
    {
        _conversionService = conversionService;
    }

    // -------------------------------------------------------------------------
    // Conversion endpoints
    // -------------------------------------------------------------------------

    /// <summary>
    /// Convert a value from one unit to another via query-string parameters.
    /// </summary>
    /// <param name="value">The numeric value to convert.</param>
    /// <param name="from">Source unit key (e.g. "celsius", "meter", "kilogram").</param>
    /// <param name="to">Target unit key (e.g. "fahrenheit", "foot", "pound").</param>
    /// <returns>The conversion result including both the input and output values.</returns>
    /// <response code="200">Conversion succeeded.</response>
    /// <response code="400">Missing or malformed query parameters.</response>
    /// <response code="422">Unknown unit key or incompatible unit categories.</response>
    [HttpGet("convert")]
    [ProducesResponseType(typeof(ConversionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult ConvertGet(
        [FromQuery] double value,
        [FromQuery] string from,
        [FromQuery] string to)
    {
        var result = _conversionService.Convert(value, from, to);
        return Ok(result);
    }

    /// <summary>
    /// Convert a value from one unit to another via a JSON request body.
    /// Useful for complex clients or when the value contains characters
    /// that are awkward to encode in a URL.
    /// </summary>
    /// <param name="request">Conversion request payload.</param>
    /// <returns>The conversion result.</returns>
    /// <response code="200">Conversion succeeded.</response>
    /// <response code="400">Invalid request body.</response>
    /// <response code="422">Unknown unit key or incompatible unit categories.</response>
    [HttpPost("convert")]
    [ProducesResponseType(typeof(ConversionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult ConvertPost([FromBody] ConversionRequest request)
    {
        var result = _conversionService.Convert(request.Value, request.From, request.To);
        return Ok(result);
    }

    // -------------------------------------------------------------------------
    // Discovery endpoints
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns all supported units, optionally filtered by category.
    /// </summary>
    /// <param name="category">
    /// Optional category name filter (e.g. "Length", "Temperature").
    /// Case-insensitive. If omitted, all units are returned.
    /// </param>
    /// <returns>List of supported units.</returns>
    /// <response code="200">Units returned successfully.</response>
    /// <response code="422">Unknown category name.</response>
    [HttpGet("units")]
    [ProducesResponseType(typeof(IReadOnlyList<UnitInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetUnits([FromQuery] string? category = null)
    {
        var units = category is null
            ? _conversionService.GetAllUnits()
            : _conversionService.GetUnitsByCategory(category);

        return Ok(units);
    }

    /// <summary>
    /// Returns the list of all supported conversion categories.
    /// </summary>
    /// <returns>List of category names.</returns>
    /// <response code="200">Categories returned successfully.</response>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public IActionResult GetCategories()
    {
        return Ok(_conversionService.GetCategories());
    }
}
