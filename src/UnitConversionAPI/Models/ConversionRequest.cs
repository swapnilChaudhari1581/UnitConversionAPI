using System.ComponentModel.DataAnnotations;

namespace UnitConversionAPI.Models;

/// <summary>Request payload for a unit conversion operation.</summary>
public sealed class ConversionRequest
{
    /// <summary>The numeric value to convert.</summary>
    /// <example>100</example>
    [Required]
    public double Value { get; init; }

    /// <summary>The key of the source unit (e.g. "celsius", "meter", "kilogram").</summary>
    /// <example>celsius</example>
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public required string From { get; init; }

    /// <summary>The key of the target unit (e.g. "fahrenheit", "foot", "pound").</summary>
    /// <example>fahrenheit</example>
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public required string To { get; init; }
}
