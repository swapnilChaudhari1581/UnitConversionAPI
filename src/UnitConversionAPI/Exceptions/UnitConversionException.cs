namespace UnitConversionAPI.Exceptions;

/// <summary>Thrown when a requested unit key does not exist in the registry.</summary>
public sealed class UnitNotFoundException : Exception
{
    public string UnitKey { get; }

    public UnitNotFoundException(string unitKey)
        : base($"Unit '{unitKey}' is not supported. Use GET /api/conversions/units to see all available units.")
    {
        UnitKey = unitKey;
    }
}

/// <summary>
/// Thrown when the caller attempts to convert between units that belong to
/// different categories (e.g. meters to kilograms).
/// </summary>
public sealed class IncompatibleUnitsException : Exception
{
    public string FromUnit { get; }
    public string ToUnit { get; }
    public string FromCategory { get; }
    public string ToCategory { get; }

    public IncompatibleUnitsException(
        string fromUnit, string toUnit,
        string fromCategory, string toCategory)
        : base(
            $"Cannot convert '{fromUnit}' ({fromCategory}) to '{toUnit}' ({toCategory}). " +
            $"Units must belong to the same category.")
    {
        FromUnit = fromUnit;
        ToUnit = toUnit;
        FromCategory = fromCategory;
        ToCategory = toCategory;
    }
}
