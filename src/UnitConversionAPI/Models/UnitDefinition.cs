namespace UnitConversionAPI.Models;

/// <summary>
/// Internal model representing a unit of measurement with its conversion logic.
/// The ToBaseUnit and FromBaseUnit functions allow both linear (factor-based)
/// and non-linear (e.g., temperature) conversions through a common interface.
/// </summary>
internal sealed class UnitDefinition
{
    public required string Key { get; init; }
    public required string DisplayName { get; init; }
    public required string Symbol { get; init; }
    public required ConversionCategory Category { get; init; }

    /// <summary>Additional lookup keys (abbreviations, synonyms).</summary>
    public IReadOnlyList<string> Aliases { get; init; } = [];

    /// <summary>Converts a value expressed in this unit to the category's base unit.</summary>
    internal required Func<double, double> ToBaseUnit { get; init; }

    /// <summary>Converts a value expressed in the category's base unit to this unit.</summary>
    internal required Func<double, double> FromBaseUnit { get; init; }

    /// <summary>Projects to the public API DTO, hiding conversion internals.</summary>
    public UnitInfo ToUnitInfo() => new(Key, DisplayName, Symbol, Category.ToString());
}
