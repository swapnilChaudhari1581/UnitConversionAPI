using UnitConversionAPI.Models;

namespace UnitConversionAPI.Services;

/// <summary>
/// Public contract for the unit conversion service.
/// Exposed as public so it can be injected into controllers and mocked in tests.
/// </summary>
public interface IConversionService
{
    /// <summary>Converts <paramref name="value"/> from <paramref name="fromUnit"/> to <paramref name="toUnit"/>.</summary>
    /// <exception cref="Exceptions.UnitNotFoundException">When either unit key is unknown.</exception>
    /// <exception cref="Exceptions.IncompatibleUnitsException">When the two units belong to different categories.</exception>
    ConversionResult Convert(double value, string fromUnit, string toUnit);

    /// <summary>Returns every unit supported by the API.</summary>
    IReadOnlyList<UnitInfo> GetAllUnits();

    /// <summary>Returns all units within a specific category name (case-insensitive).</summary>
    /// <exception cref="Exceptions.UnitNotFoundException">When the category name is unknown.</exception>
    IReadOnlyList<UnitInfo> GetUnitsByCategory(string category);

    /// <summary>Returns the list of supported category names.</summary>
    IReadOnlyList<string> GetCategories();
}
