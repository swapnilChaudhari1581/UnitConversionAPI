using UnitConversionAPI.Models;

namespace UnitConversionAPI.Services;

/// <summary>
/// Internal contract for the unit definition store.
/// Kept internal because UnitDefinition exposes Func&lt;double,double&gt; conversion logic
/// that is an implementation detail and must not leak into the public API surface.
/// </summary>
internal interface IUnitRegistry
{
    UnitDefinition? GetUnit(string key);
    IReadOnlyList<UnitDefinition> GetAllUnits();
    IReadOnlyList<UnitDefinition> GetUnitsByCategory(ConversionCategory category);
}
