![dotnet_build](https://github.com/deniszykov/TypeConversion/workflows/dotnet_build/badge.svg)

Introduction
============
For historical reasons, .NET has several approaches to value conversion:
- Explicit/Implicit operators
- `System.Convert` class
- `IConvertible` interface
- `System.ComponentModel.TypeConverter`
- `To`, `From`, `Parse`, `Create` methods (and their `TryParse`/`TryCreate` counterparts)
- Constructors (Uri, Guid)
- Meta types (Enums, Nullable types)

This package combines all these approaches under one API.

Supported target frameworks: `net45`, `net461`, `net48`, `netstandard1.6`, `netstandard2.1`, `netcoreapp3.1`, `net6.0`, `net10.0`.

Installation
============
```
Install-Package deniszykov.TypeConversion
```

Usage
============

#### `TypeConversionProvider` methods
```csharp
// generic
ToType Convert<FromType, ToType>(fromValue, [format], [formatProvider]);
bool TryConvert<FromType, ToType>(fromValue, out result, [format], [formatProvider]);
string ConvertToString<FromType>(fromValue, [format], [formatProvider]);

// non-generic
object Convert(fromType, toType, fromValue, [format], [formatProvider]);
bool TryConvert(fromType, toType, fromValue, out result, [format], [formatProvider]);
```

## Example
```csharp
using deniszykov.TypeConversion;

var conversionProvider = new TypeConversionProvider();
var timeSpanString = "00:00:01";

var timeSpan = conversionProvider.Convert<string, TimeSpan>(timeSpanString);

// with default settings TimeSpan.Parse(value, format, formatProvider) 
// is used for conversion inside Convert<string, TimeSpan>()
```

## Configuration
```csharp
using deniszykov.TypeConversion;

var configuration = new TypeConversionProviderOptions
{
    Options = ConversionOptions.UseDefaultFormatIfNotSpecified
};

#if NET45
var typeConversionProvider = new TypeConversionProvider(configuration);
#else
var typeConversionProvider = new TypeConversionProvider(Options.Create(configuration));
#endif
```
Or configure via DI
```csharp
using deniszykov.TypeConversion;
using Microsoft.Extensions.DependencyInjection;

Host.CreateDefaultBuilder().ConfigureServices((IServiceCollection services) => {

    // add configuration
    services.Configure<TypeConversionProviderOptions>(options =>
    {
        options.DefaultFormatProvider = CultureInfo.CurrentUICulture;
    });

    // register service
    services.AddSingleton<ITypeConversionProvider, TypeConversionProvider>();
});
```

### Which conversion method is used?
At the beginning, for each pair of types, all possible conversions are found. Then they are sorted by quality and the best one is selected.
In one group, the method with parameter type closest in inheritance to the required one is selected.
For debug purposes `TypeConversionProvider.DebugPrintConversions()` could be used to review which conversion methods are selected.

Quality of conversions from worst to best (`ConversionQuality` enum):

- `None` — no conversion is found (calling `Convert` throws)
- `Constructor` — single-parameter constructor accepting source type
- `TypeConverter` — `System.ComponentModel.TypeConverter` (where supported)
- `Method` — `Parse`, `TryParse`, `Create`, `TryCreate`, `From*`, `To*` methods
- `Explicit` — explicit conversion operator
- `Implicit` — implicit conversion operator
- `Native` — built-in (long → int, class → null, enum ↔ number, nullable transitions, etc.)
- `Custom` — user-provided conversion function (highest priority by default)

Within the same quality group, methods that accept `format` and/or `formatProvider` parameters are preferred. The selection strategy is configurable via `TypeConversionProviderOptions.ConversionMethodSelectionStrategy`:
- `MostFittingMethod` (default) — picks the overload that best matches the arguments actually passed (`format`/`formatProvider`).
- `MostSpecificMethod` — always picks the overload with the most format-related parameters.

### `TryParse` / `TryCreate` support
If a target type exposes a `TryParse` / `TryCreate` method, `IConverter.TryConvert` calls it directly without `try`/`catch`, which is faster than catching `FormatException` from `Parse`. The same `TryX` method is also used as a fast fallback path in `IConverter.Convert`.

### Providing custom conversion between types
```csharp
using deniszykov.TypeConversion;
using Microsoft.Extensions.DependencyInjection;

Host.CreateDefaultBuilder().ConfigureServices((IServiceCollection services) => {

    // register custom conversion from Uri to string
    services.AddSingleton<ICustomConversionRegistration>(
        new CustomConversion<Uri, string>((uri, _, _) => uri.OriginalString)
    );

    // register custom conversion from string to Uri
    services.AddSingleton<ICustomConversionRegistration>(
        new CustomConversion<string, Uri>((str, _, _) => new Uri(str))
    );

    // ...
});
```

A `CustomConversion<TFrom, TTo>` may also be supplied with a "safe" non-throwing function so `TryConvert` does not need exception handling:
```csharp
new CustomConversion<string, int>(
    conversionFunc:     (s, _, _) => int.Parse(s),
    safeConversionFunc: (s, _, _) => new KeyValuePair<int, bool>(
                                          int.TryParse(s, out var v) ? v : 0,
                                          int.TryParse(s, out _)),
    quality: ConversionQuality.Custom);
```

### Preparing for AOT runtime
On AOT runtimes (Unity IL2CPP, Mono `--aot`, NativeAOT, WebGL, PlayStation, etc.) dynamic code generation is not available. Disable expression-tree and generic-method optimizations:

```csharp
using deniszykov.TypeConversion;
using Microsoft.Extensions.DependencyInjection;

Host.CreateDefaultBuilder().ConfigureServices((IServiceCollection services) => {

    services.Configure<TypeConversionProviderOptions>(options =>
    {
        // disable optimizations which use dynamic code generation
        options.Options &= ~(ConversionOptions.OptimizeWithExpressions | ConversionOptions.OptimizeWithGenerics);
    });
});
```

For non-generic `ITypeConversionProvider.GetConverter(Type, Type)` lookups on AOT runtimes, pre-register the type pair at startup so the corresponding generic instantiation is rooted by the IL Linker:

```csharp
typeConversionProvider.PrepareGetConverter<MyEnum, string>();
typeConversionProvider.PrepareGetConverter<string, MyEnum>();
```

The library is annotated with `DynamicallyAccessedMembers` to keep the IL Linker / trimmer informed about reflection access, so trim-friendly publishing is supported.

## Key Abstractions

```csharp
interface ITypeConversionProvider;        // provides IConverter instances on demand
interface IConverter<FromType, ToType>;   // converts a value
interface IConverter;                     // non-generic counterpart
interface IConversionMetadataProvider;    // discovers conversion methods on types
interface ICustomConversionRegistry;      // accepts custom conversion registrations
interface ICustomConversionRegistration;  // describes a custom conversion to register
class    ConversionDescriptor;            // describes the chosen conversion method(s) and delegates
class    ConversionMethodInfo;            // metadata about a single conversion method
class    EnumConversionInfo<EnumT>;       // fast enum ↔ underlying-type / name conversion helper
```

## Format strings

The library reserves a few well-known `format` values:

| Constant                                              | Value         | Effect                                                                |
| ----------------------------------------------------- | ------------- | --------------------------------------------------------------------- |
| `TypeConversionProvider.IgnoreCaseFormat`             | `"ignoreCase"`| Case-insensitive parsing (e.g. `string` → `Enum`).                    |
| `TypeConversionProvider.CheckedConversionFormat`      | `"checked"`   | Use `checked` arithmetic for built-in numeric conversions.            |
| `TypeConversionProvider.UncheckedConversionFormat`    | `"unchecked"` | Use `unchecked` arithmetic for built-in numeric conversions.          |

## License

MIT — see [LICENSE](LICENSE).
