namespace UnitConversionAPI.Models;

/// <summary>Result of a successful unit conversion operation.</summary>
public sealed record ConversionResult
{
    /// <summary>The original numeric value that was provided.</summary>
    public double InputValue { get; init; }

    /// <summary>Human-readable name of the source unit.</summary>
    public string InputUnit { get; init; } = string.Empty;

    /// <summary>Symbol of the source unit (e.g. "°C", "m", "kg").</summary>
    public string InputSymbol { get; init; } = string.Empty;

    /// <summary>The converted numeric value.</summary>
    public double OutputValue { get; init; }

    /// <summary>Human-readable name of the target unit.</summary>
    public string OutputUnit { get; init; } = string.Empty;

    /// <summary>Symbol of the target unit.</summary>
    public string OutputSymbol { get; init; } = string.Empty;

    /// <summary>The conversion category (e.g. "Temperature", "Length").</summary>
    public string Category { get; init; } = string.Empty;
}
