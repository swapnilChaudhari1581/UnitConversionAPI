using UnitConversionAPI.Models;
using UnitConversionAPI.Services;

namespace UnitConversionAPI.Data;

/// <summary>
/// In-memory registry of all supported units and their conversion logic.
///
/// Design notes:
/// - Every category has a designated "base unit" (see comments below).
/// - Linear units are defined by a single factor: value_in_base = value * factor.
/// - Non-linear units (temperature) are defined by explicit lambda functions.
/// - Aliases are flattened into the lookup dictionary so callers can use common
///   abbreviations and synonyms (e.g. "km" resolves to the same entry as "kilometer").
/// - The registry is registered as a singleton — it holds no mutable state.
///
/// Adding a new unit:
///   Add a yield return in the appropriate "Build*" method below and (optionally)
///   add aliases. No other code changes are required.
/// </summary>
internal sealed class UnitRegistry : IUnitRegistry
{
    // -----------------------------------------------------------------------
    // Base units (the unit each category normalises through):
    //   Length      → metre
    //   Temperature → kelvin
    //   Weight      → kilogram
    //   Area        → square metre
    //   Volume      → litre
    //   Speed       → metre per second
    //   Pressure    → pascal
    // -----------------------------------------------------------------------

    private readonly Dictionary<string, UnitDefinition> _units;

    public UnitRegistry()
    {
        _units = new Dictionary<string, UnitDefinition>(StringComparer.OrdinalIgnoreCase);

        foreach (var unit in BuildAllUnits())
        {
            // Primary key
            _units[unit.Key] = unit;

            // Aliases — allow both abbreviated and alternative spellings
            foreach (var alias in unit.Aliases)
                _units.TryAdd(alias, unit); // first definition wins on collision
        }
    }

    public UnitDefinition? GetUnit(string key) =>
        _units.TryGetValue(key, out var unit) ? unit : null;

    public IReadOnlyList<UnitDefinition> GetAllUnits() =>
        _units.Values
              .DistinctBy(u => u.Key)          // de-duplicate aliases
              .OrderBy(u => u.Category)
              .ThenBy(u => u.DisplayName)
              .ToList();

    public IReadOnlyList<UnitDefinition> GetUnitsByCategory(ConversionCategory category) =>
        _units.Values
              .DistinctBy(u => u.Key)
              .Where(u => u.Category == category)
              .OrderBy(u => u.DisplayName)
              .ToList();

    // =========================================================================
    // Unit definitions
    // =========================================================================

    private static IEnumerable<UnitDefinition> BuildAllUnits()
    {
        foreach (var u in BuildLengthUnits())      yield return u;
        foreach (var u in BuildTemperatureUnits()) yield return u;
        foreach (var u in BuildWeightUnits())      yield return u;
        foreach (var u in BuildAreaUnits())        yield return u;
        foreach (var u in BuildVolumeUnits())      yield return u;
        foreach (var u in BuildSpeedUnits())       yield return u;
        foreach (var u in BuildPressureUnits())    yield return u;
    }

    // -------------------------------------------------------------------------
    // LENGTH  (base: metre = 1.0)
    // -------------------------------------------------------------------------
    private static IEnumerable<UnitDefinition> BuildLengthUnits()
    {
        const ConversionCategory cat = ConversionCategory.Length;

        yield return Linear("meter",          "Meter",          "m",   cat, 1.0,             ["metre", "m"]);
        yield return Linear("kilometer",      "Kilometer",      "km",  cat, 1_000.0,         ["kilometre", "km"]);
        yield return Linear("centimeter",     "Centimeter",     "cm",  cat, 0.01,            ["centimetre", "cm"]);
        yield return Linear("millimeter",     "Millimeter",     "mm",  cat, 0.001,           ["millimetre", "mm"]);
        yield return Linear("micrometer",     "Micrometer",     "μm",  cat, 0.000_001,       ["micrometre", "micron", "um"]);
        yield return Linear("nanometer",      "Nanometer",      "nm",  cat, 0.000_000_001,   ["nanometre", "nm"]);
        yield return Linear("mile",           "Mile",           "mi",  cat, 1_609.344,       ["miles"]);
        yield return Linear("yard",           "Yard",           "yd",  cat, 0.9144,          ["yards", "yd"]);
        yield return Linear("foot",           "Foot",           "ft",  cat, 0.3048,          ["feet", "ft"]);
        yield return Linear("inch",           "Inch",           "in",  cat, 0.0254,          ["inches", "in"]);
        yield return Linear("nauticalmile",   "Nautical Mile",  "nmi", cat, 1_852.0,         ["nautical-mile", "nmi"]);
        yield return Linear("lightyear",      "Light Year",     "ly",  cat, 9.461e15,        ["light-year", "ly"]);
    }

    // -------------------------------------------------------------------------
    // TEMPERATURE  (base: kelvin)
    // Non-linear — explicit lambdas are used instead of a multiplier.
    // -------------------------------------------------------------------------
    private static IEnumerable<UnitDefinition> BuildTemperatureUnits()
    {
        const ConversionCategory cat = ConversionCategory.Temperature;

        yield return new UnitDefinition
        {
            Key = "kelvin", DisplayName = "Kelvin", Symbol = "K", Category = cat,
            Aliases = ["k"],
            ToBaseUnit   = v => v,
            FromBaseUnit = v => v
        };

        yield return new UnitDefinition
        {
            Key = "celsius", DisplayName = "Celsius", Symbol = "°C", Category = cat,
            Aliases = ["c", "centigrade"],
            ToBaseUnit   = v => v + 273.15,
            FromBaseUnit = v => v - 273.15
        };

        yield return new UnitDefinition
        {
            Key = "fahrenheit", DisplayName = "Fahrenheit", Symbol = "°F", Category = cat,
            Aliases = ["f"],
            ToBaseUnit   = v => (v + 459.67) * (5.0 / 9.0),
            FromBaseUnit = v => v * (9.0 / 5.0) - 459.67
        };

        yield return new UnitDefinition
        {
            Key = "rankine", DisplayName = "Rankine", Symbol = "°R", Category = cat,
            Aliases = ["r"],
            ToBaseUnit   = v => v * (5.0 / 9.0),
            FromBaseUnit = v => v * (9.0 / 5.0)
        };
    }

    // -------------------------------------------------------------------------
    // WEIGHT / MASS  (base: kilogram = 1.0)
    // -------------------------------------------------------------------------
    private static IEnumerable<UnitDefinition> BuildWeightUnits()
    {
        const ConversionCategory cat = ConversionCategory.Weight;

        yield return Linear("kilogram",   "Kilogram",    "kg",  cat, 1.0,               ["kg", "kilo"]);
        yield return Linear("gram",       "Gram",        "g",   cat, 0.001,             ["g"]);
        yield return Linear("milligram",  "Milligram",   "mg",  cat, 0.000_001,         ["mg"]);
        yield return Linear("microgram",  "Microgram",   "μg",  cat, 0.000_000_001,     ["ug"]);
        yield return Linear("tonne",      "Metric Tonne","t",   cat, 1_000.0,           ["metric-ton", "metricton", "t"]);
        yield return Linear("pound",      "Pound",       "lb",  cat, 0.453_592_37,      ["pounds", "lb", "lbs"]);
        yield return Linear("ounce",      "Ounce",       "oz",  cat, 0.028_349_523_125, ["ounces", "oz"]);
        yield return Linear("stone",      "Stone",       "st",  cat, 6.350_293_18,      ["stones", "st"]);
        yield return Linear("shortton",   "Short Ton",   "ton", cat, 907.184_74,        ["short-ton", "uston", "ton"]);
        yield return Linear("longton",    "Long Ton",    "LT",  cat, 1_016.046_908_8,   ["long-ton", "imperialton"]);
    }

    // -------------------------------------------------------------------------
    // AREA  (base: square metre = 1.0)
    // -------------------------------------------------------------------------
    private static IEnumerable<UnitDefinition> BuildAreaUnits()
    {
        const ConversionCategory cat = ConversionCategory.Area;

        yield return Linear("squaremeter",      "Square Meter",      "m²",  cat, 1.0,                ["sqm", "m2", "square-meter", "squaremetre"]);
        yield return Linear("squarekilometer",  "Square Kilometer",  "km²", cat, 1_000_000.0,        ["sqkm", "km2", "square-kilometer"]);
        yield return Linear("squarecentimeter", "Square Centimeter", "cm²", cat, 0.000_1,            ["sqcm", "cm2"]);
        yield return Linear("squaremillimeter", "Square Millimeter", "mm²", cat, 0.000_001,          ["sqmm", "mm2"]);
        yield return Linear("squaremile",       "Square Mile",       "mi²", cat, 2_589_988.110_336,  ["sqmi", "mi2", "square-mile"]);
        yield return Linear("squareyard",       "Square Yard",       "yd²", cat, 0.836_127_36,       ["sqyd", "yd2"]);
        yield return Linear("squarefoot",       "Square Foot",       "ft²", cat, 0.092_903_04,       ["sqft", "ft2", "squarefeet"]);
        yield return Linear("squareinch",       "Square Inch",       "in²", cat, 0.000_645_16,       ["sqin", "in2"]);
        yield return Linear("acre",             "Acre",              "ac",  cat, 4_046.856_422_4,    ["acres", "ac"]);
        yield return Linear("hectare",          "Hectare",           "ha",  cat, 10_000.0,           ["hectares", "ha"]);
    }

    // -------------------------------------------------------------------------
    // VOLUME  (base: litre = 1.0)
    // -------------------------------------------------------------------------
    private static IEnumerable<UnitDefinition> BuildVolumeUnits()
    {
        const ConversionCategory cat = ConversionCategory.Volume;

        yield return Linear("liter",           "Liter",              "L",    cat, 1.0,               ["litre", "l"]);
        yield return Linear("milliliter",      "Milliliter",         "mL",   cat, 0.001,             ["millilitre", "ml"]);
        yield return Linear("centiliter",      "Centiliter",         "cL",   cat, 0.01,              ["centilitre", "cl"]);
        yield return Linear("cubicmeter",      "Cubic Meter",        "m³",   cat, 1_000.0,           ["m3", "cubic-meter"]);
        yield return Linear("cubiccentimeter", "Cubic Centimeter",   "cm³",  cat, 0.001,             ["cc", "cm3"]);
        yield return Linear("cubicfoot",       "Cubic Foot",         "ft³",  cat, 28.316_846_592,    ["ft3", "cubic-foot"]);
        yield return Linear("cubicinch",       "Cubic Inch",         "in³",  cat, 0.016_387_064,     ["in3", "cubic-inch"]);
        yield return Linear("usgallon",        "US Gallon",          "gal",  cat, 3.785_411_784,     ["gallon", "gal"]);
        yield return Linear("ukgallon",        "UK Gallon",          "UKgal",cat, 4.546_09,          ["imperialgallon"]);
        yield return Linear("usquart",         "US Quart",           "qt",   cat, 0.946_352_946,     ["quart", "qt"]);
        yield return Linear("uspint",          "US Pint",            "pt",   cat, 0.473_176_473,     ["pint", "pt"]);
        yield return Linear("uscup",           "US Cup",             "cup",  cat, 0.236_588_236_5,   ["cup"]);
        yield return Linear("usfluidounce",    "US Fluid Ounce",     "fl oz",cat, 0.029_573_529_6,   ["floz", "fl-oz"]);
        yield return Linear("tablespoon",      "Tablespoon",         "tbsp", cat, 0.014_786_764_8,   ["tbsp"]);
        yield return Linear("teaspoon",        "Teaspoon",           "tsp",  cat, 0.004_928_921_6,   ["tsp"]);
    }

    // -------------------------------------------------------------------------
    // SPEED  (base: metre per second = 1.0)
    // -------------------------------------------------------------------------
    private static IEnumerable<UnitDefinition> BuildSpeedUnits()
    {
        const ConversionCategory cat = ConversionCategory.Speed;

        yield return Linear("meterpersecond",     "Meter per Second",     "m/s",  cat, 1.0,           ["mps", "m/s"]);
        yield return Linear("kilometerperhour",   "Kilometer per Hour",   "km/h", cat, 1.0 / 3.6,     ["kph", "km/h", "kmh"]);
        yield return Linear("mileperhour",        "Mile per Hour",        "mph",  cat, 0.447_04,       ["mph"]);
        yield return Linear("knot",               "Knot",                 "kn",   cat, 0.514_444_444,  ["knots", "kn", "kt"]);
        yield return Linear("footpersecond",      "Foot per Second",      "ft/s", cat, 0.3048,         ["fps", "ft/s"]);
        yield return Linear("mach",               "Mach",                 "Ma",   cat, 340.29,         ["ma"]);
    }

    // -------------------------------------------------------------------------
    // PRESSURE  (base: pascal = 1.0)
    // -------------------------------------------------------------------------
    private static IEnumerable<UnitDefinition> BuildPressureUnits()
    {
        const ConversionCategory cat = ConversionCategory.Pressure;

        yield return Linear("pascal",      "Pascal",               "Pa",   cat, 1.0,           ["pa"]);
        yield return Linear("kilopascal",  "Kilopascal",           "kPa",  cat, 1_000.0,       ["kpa"]);
        yield return Linear("megapascal",  "Megapascal",           "MPa",  cat, 1_000_000.0,   ["mpa"]);
        yield return Linear("bar",         "Bar",                  "bar",  cat, 100_000.0,     []);
        yield return Linear("millibar",    "Millibar",             "mbar", cat, 100.0,         ["mbar"]);
        yield return Linear("atmosphere",  "Atmosphere",           "atm",  cat, 101_325.0,     ["atm"]);
        yield return Linear("psi",         "Pound per Square Inch","psi",  cat, 6_894.757_293, []);
        yield return Linear("mmhg",        "Millimeter of Mercury","mmHg", cat, 133.322_387,   ["torr"]);
        yield return Linear("inhg",        "Inch of Mercury",      "inHg", cat, 3_386.388_64,  ["inhg"]);
    }

    // =========================================================================
    // Factory helpers
    // =========================================================================

    /// <summary>
    /// Creates a unit whose conversion is linear: <c>baseValue = value * factor</c>.
    /// </summary>
    private static UnitDefinition Linear(
        string key, string displayName, string symbol,
        ConversionCategory category, double factor,
        IReadOnlyList<string> aliases)
        => new()
        {
            Key          = key,
            DisplayName  = displayName,
            Symbol       = symbol,
            Category     = category,
            Aliases      = aliases,
            ToBaseUnit   = v => v * factor,
            FromBaseUnit = v => v / factor
        };
}
