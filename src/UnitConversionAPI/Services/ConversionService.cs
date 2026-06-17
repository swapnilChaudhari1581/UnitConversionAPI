using UnitConversionAPI.Exceptions;
using UnitConversionAPI.Models;

namespace UnitConversionAPI.Services;

/// <summary>
/// Performs unit conversions using a two-step "through base unit" strategy:
///   1. Convert the source value to the category's base unit.
///   2. Convert from the base unit to the target unit.
///
/// This keeps the conversion logic O(1) per pair regardless of how many units
/// are supported, and allows both linear and non-linear conversions (e.g. temperature)
/// through the same Func&lt;double,double&gt; interface on UnitDefinition.
/// </summary>
internal sealed class ConversionService : IConversionService
{
    private readonly IUnitRegistry _registry;

    public ConversionService(IUnitRegistry registry)
    {
        _registry = registry;
    }

    /// <inheritdoc/>
    public ConversionResult Convert(double value, string fromUnit, string toUnit)
    {
        var fromKey = fromUnit.Trim();
        var toKey = toUnit.Trim();

        var from = _registry.GetUnit(fromKey)
            ?? throw new UnitNotFoundException(fromUnit);

        var to = _registry.GetUnit(toKey)
            ?? throw new UnitNotFoundException(toUnit);

        if (from.Category != to.Category)
            throw new IncompatibleUnitsException(
                fromUnit, toUnit,
                from.Category.ToString(), to.Category.ToString());

        var baseValue = from.ToBaseUnit(value);
        var outputValue = to.FromBaseUnit(baseValue);

        // Round to 10 decimal places to suppress floating-point noise while
        // retaining precision well beyond any practical measurement requirement.
        outputValue = Math.Round(outputValue, 10);

        return new ConversionResult
        {
            InputValue = value,
            InputUnit = from.DisplayName,
            InputSymbol = from.Symbol,
            OutputValue = outputValue,
            OutputUnit = to.DisplayName,
            OutputSymbol = to.Symbol,
            Category = from.Category.ToString()
        };
    }

    /// <inheritdoc/>
    public IReadOnlyList<UnitInfo> GetAllUnits() =>
        _registry.GetAllUnits().Select(u => u.ToUnitInfo()).ToList();

    /// <inheritdoc/>
    public IReadOnlyList<UnitInfo> GetUnitsByCategory(string category)
    {
        if (!Enum.TryParse<ConversionCategory>(category, ignoreCase: true, out var parsed))
            throw new UnitNotFoundException(category);

        return _registry.GetUnitsByCategory(parsed)
            .Select(u => u.ToUnitInfo())
            .ToList();
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetCategories() =>
        Enum.GetNames<ConversionCategory>();
}
