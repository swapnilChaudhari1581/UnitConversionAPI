# Unit Conversion API

A production-ready ASP.NET Core Web API that converts numerical values between
units of measurement across **7 categories** and **60+ units**.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [API Reference](#api-reference)
- [Running the Tests](#running-the-tests)
- [Design Decisions](#design-decisions)
- [Extending the API](#extending-the-api)

---

## Features

| Category    | Example units                                              |
|-------------|------------------------------------------------------------|
| Length      | meter, kilometer, mile, foot, inch, nautical mile, …       |
| Temperature | Celsius, Fahrenheit, Kelvin, Rankine                       |
| Weight/Mass | kilogram, gram, pound, ounce, stone, tonne, …              |
| Area        | m², km², ft², acre, hectare, …                             |
| Volume      | litre, mL, m³, US gallon, fl oz, cup, tablespoon, …       |
| Speed       | m/s, km/h, mph, knot, Mach, …                             |
| Pressure    | pascal, bar, psi, atm, mmHg, …                             |

- Both `GET` (query-string) and `POST` (JSON body) conversion endpoints
- Discovery endpoints to list all units and categories
- Case-insensitive unit keys with common abbreviation aliases
- RFC 7807 `application/problem+json` error responses
- OpenAPI spec auto-generated at `/openapi/v1.json`
- Structured logging via Microsoft.Extensions.Logging

---

## Tech Stack

- **.NET 10** (LTS) — `net10.0`
- **ASP.NET Core** — controller-based Web API
- **Microsoft.AspNetCore.OpenApi** — OpenAPI spec generation
- **xUnit + FluentAssertions + NSubstitute** — test suite
- **Microsoft.AspNetCore.Mvc.Testing** — in-process integration tests

---

## Project Structure

```
UnitConversionAPI/
├── src/
│   └── UnitConversionAPI/
│       ├── Controllers/          # HTTP layer — thin, delegates to IConversionService
│       ├── Data/                 # UnitRegistry — all unit definitions (hardcoded)
│       ├── Exceptions/           # Domain exceptions (UnitNotFoundException, etc.)
│       ├── Middleware/           # ExceptionHandlingMiddleware → ProblemDetails
│       ├── Models/               # DTOs (ConversionRequest, ConversionResult, UnitInfo)
│       ├── Services/             # IConversionService + ConversionService
│       └── Program.cs            # Composition root
└── tests/
    └── UnitConversionAPI.Tests/
        ├── Controllers/          # Controller unit tests (service mocked)
        ├── Integration/          # End-to-end tests via WebApplicationFactory
        └── Services/             # Service unit tests (real UnitRegistry)
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later

### Run locally

```bash
# Clone the repository
git clone <your-repo-url>
cd UnitConversionAPI

# Restore dependencies
dotnet restore

# Start the API (HTTP on port 5176)
dotnet run --project src/UnitConversionAPI

# Or use HTTPS (port 7275)
dotnet run --project src/UnitConversionAPI --launch-profile https
```

The API starts on:
- HTTP:  `http://localhost:5176`
- HTTPS: `https://localhost:7275`

The OpenAPI specification is served at:  
`http://localhost:5176/openapi/v1.json`

You can import this JSON into **Postman**, **Insomnia**, or any OpenAPI-compatible tool.

---

## API Reference

### Convert a value — GET

```
GET /api/conversions/convert?value={value}&from={unit}&to={unit}
```

**Example:**

```bash
curl "http://localhost:5176/api/conversions/convert?value=100&from=celsius&to=fahrenheit"
```

**Response `200 OK`:**

```json
{
  "inputValue": 100,
  "inputUnit": "Celsius",
  "inputSymbol": "°C",
  "outputValue": 212,
  "outputUnit": "Fahrenheit",
  "outputSymbol": "°F",
  "category": "Temperature"
}
```

---

### Convert a value — POST

```
POST /api/conversions/convert
Content-Type: application/json
```

**Request body:**

```json
{
  "value": 1,
  "from": "kilometer",
  "to": "mile"
}
```

**Response `200 OK`:**

```json
{
  "inputValue": 1,
  "inputUnit": "Kilometer",
  "inputSymbol": "km",
  "outputValue": 0.6213711922,
  "outputUnit": "Mile",
  "outputSymbol": "mi",
  "category": "Length"
}
```

---

### List all units

```
GET /api/conversions/units
GET /api/conversions/units?category=Temperature
```

**Response `200 OK`:**

```json
[
  { "key": "celsius",    "displayName": "Celsius",    "symbol": "°C", "category": "Temperature" },
  { "key": "fahrenheit", "displayName": "Fahrenheit", "symbol": "°F", "category": "Temperature" },
  { "key": "kelvin",     "displayName": "Kelvin",     "symbol": "K",  "category": "Temperature" },
  { "key": "rankine",    "displayName": "Rankine",    "symbol": "°R", "category": "Temperature" }
]
```

---

### List categories

```
GET /api/conversions/categories
```

**Response `200 OK`:**

```json
["Length","Temperature","Weight","Area","Volume","Speed","Pressure"]
```

---

### Error responses

All errors follow RFC 7807 (`application/problem+json`):

```json
{
  "type": "https://httpstatuses.com/422",
  "title": "Unit Not Found",
  "status": 422,
  "detail": "Unit 'xyz' is not supported. Use GET /api/conversions/units to see all available units.",
  "traceId": "00-abc123..."
}
```

| HTTP Status | Scenario |
|---|---|
| `400 Bad Request` | Missing or malformed request parameters |
| `422 Unprocessable Entity` | Unknown unit key, or units from different categories |
| `500 Internal Server Error` | Unexpected server error |

---

### Common unit keys (aliases supported)

| Unit              | Primary key         | Common aliases          |
|-------------------|---------------------|-------------------------|
| Celsius           | `celsius`           | `c`, `centigrade`       |
| Fahrenheit        | `fahrenheit`        | `f`                     |
| Kelvin            | `kelvin`            | `k`                     |
| Meter             | `meter`             | `metre`, `m`            |
| Kilometer         | `kilometer`         | `kilometre`, `km`       |
| Foot              | `foot`              | `feet`, `ft`            |
| Mile              | `mile`              | `miles`                 |
| Kilogram          | `kilogram`          | `kg`, `kilo`            |
| Pound             | `pound`             | `pounds`, `lb`, `lbs`   |
| US Gallon         | `usgallon`          | `gallon`, `gal`         |

Run `GET /api/conversions/units` for the full list.

---

## Running the Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

The test suite includes:

| Layer | File | What it tests |
|---|---|---|
| Service unit tests | `Services/ConversionServiceTests.cs` | Conversion accuracy, error cases, aliases |
| Controller unit tests | `Controllers/ConversionControllerTests.cs` | HTTP routing, status codes (service mocked) |
| Integration tests | `Integration/ConversionApiIntegrationTests.cs` | Full HTTP pipeline, JSON contract |

---

## Design Decisions

### "Through base unit" conversion strategy

Every category has a designated **base unit** (metre, kelvin, kilogram, …).
Converting from unit A to unit B is always done in two steps:

```
value_A  →  [ToBaseUnit]  →  base_value  →  [FromBaseUnit]  →  value_B
```

This means adding a new unit requires only defining its relationship to the base
unit — no N×N conversion table is needed.  The approach scales to hundreds of
units without any algorithmic cost increase.

### Linear vs. non-linear conversions

Most conversions are **linear** (multiplication by a factor).
Temperature is **non-linear** (offset + scale).  Both are modelled uniformly
with `Func<double, double>` delegates on `UnitDefinition`, so the calling code
in `ConversionService` is identical for all categories.

### Internal vs. public types

`UnitDefinition` (which carries `Func<>` properties) and `IUnitRegistry` are
`internal`.  Only the service interface `IConversionService` and the DTO types
are `public`.  This prevents conversion implementation details from leaking into
the public API surface and makes the contracts for testing clean and stable.

### Registry as a singleton

`UnitRegistry` is registered as a `Singleton` because it is pure data with no
mutable state.  This avoids rebuilding the dictionary on every request.

### Aliases in the registry

The dictionary is keyed by both the primary key and common abbreviations/synonyms
(e.g. `"km"`, `"kilometre"` both resolve to the same `UnitDefinition`).
Collisions on aliases are silently ignored (`TryAdd`), with the primary key
always winning.

### Error handling

A `ExceptionHandlingMiddleware` converts domain exceptions into RFC 7807
`application/problem+json` responses.  This keeps controllers free of try/catch
blocks and guarantees a consistent error envelope regardless of where in the
pipeline the exception originated.

### Future scalability

To support hundreds or thousands of units from a database:
1. Implement a new `IUnitRegistry` that reads from a database / configuration file.
2. Register the new implementation in `Program.cs` — no other code changes needed.
3. Non-linear units can be stored as polynomial coefficients or expression strings
   and evaluated at runtime.

---

## Git instructions

### Push to GitHub (first time)

```bash
cd C:\Projects\UnitConversionAPI

# Initialise the repository
git init
git add .
git commit -m "feat: initial Unit Conversion API implementation"

# Create a repo on GitHub (github.com → New repository)
# Then add the remote and push:
git remote add origin https://github.com/<your-username>/<your-repo-name>.git
git branch -M main
git push -u origin main
```

### Subsequent pushes

```bash
git add .
git commit -m "your commit message"
git push
```
