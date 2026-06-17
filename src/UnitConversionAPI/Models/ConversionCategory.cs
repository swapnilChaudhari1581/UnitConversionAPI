namespace UnitConversionAPI.Models;

/// <summary>
/// Supported categories of unit conversion.
/// Only units within the same category can be converted between each other.
/// </summary>
public enum ConversionCategory
{
    Length,
    Temperature,
    Weight,
    Area,
    Volume,
    Speed,
    Pressure
}
