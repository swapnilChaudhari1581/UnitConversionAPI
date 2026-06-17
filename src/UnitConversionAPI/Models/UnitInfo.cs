namespace UnitConversionAPI.Models;

/// <summary>Public DTO representing a supported unit of measurement.</summary>
public sealed record UnitInfo(
    string Key,
    string DisplayName,
    string Symbol,
    string Category
);
