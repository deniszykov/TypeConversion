![dotnet_build](https://github.com/deniszykov/TypeConversion/workflows/dotnet_build/badge.svg)

Introduction
============
For historical reasons, .NET has several approaches to value conversion:
- Explicit/Implicit operators
- System.Convert class 
- IConvertible interface
- System.ComponentModel.TypeConverter
- To, From, Parse, Create methods
- Constructors (Uri, Guid)
- Meta types (Enums, Nullable Types).

This package combines all these approaches under one API. 

Installation
============
```
Install-Package deniszykov.TypeConversion 
```

Usage
============

#### TypeConversionProvider methods
```csharp
// generic
ToType Convert<FromType, ToType>(fromValue, [format], [formatProvider]);
bool TryConvert<FromType, ToType>(fromValue, out result, [format], [formatProvider]);
string ConvertToString<FromType>(fromValue, [format], [formatProvider]);

// non-generic
object Convert(fromValue, toType, fromValue, [format], [formatProvider]);
bool TryConvert(fromValue, toType, fromValue, out result, [format], [formatProvider]);
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

var configuration = new TypeConversionProviderConfiguration
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

Host.CreateDefaultBuilder().ConfigureServices(IServiceCollection services) => {

  // add configuration
  services.Configure<TypeConversionProviderConfiguration>(options =>
  {
    options.DefaultFormatProvider = CultureInfo.CurrentUICulture;
  });

  // register service
  services.AddSingleton<ITypeConversionProvider, TypeConversionProvider>();
}
```

### Which conversion method is used?
At the beginning, for each pair of types, all possible conversions are found. Then they are sorted by quality and the best one is selected.
In one group, method with parameter type closest in inheritance to the required one is selected. 
For debug purposes `TypeConversionProvider.DebugPrintConversions` could be used to review which conversion methods are selected.

Quality of conversions form worst to best: 

- Constructor (worst)
- System.ComponentModel.TypeConverter
- Methods like Parse, From, To
- Explicit conversion operator
- Implicit conversion operator
- Build-in (long -> int, class -> null)
- Custom

### Providing custom conversion between types
```csharp
using deniszykov.TypeConversion;
using Microsoft.Extensions.DependencyInjection;

Host.CreateDefaultBuilder().ConfigureServices(IServiceCollection services) => {

    // register custom conversion from Uri to string
    serviceCollection.AddSingleton<ICustomConversionRegistration>(
        new CustomConversion<Uri, string>((uri, _, _) => uri.OriginalString)
    );

    // register custom conversion from string to Uri
    serviceCollection.AddSingleton<ICustomConversionRegistration>(
        new CustomConversion<string, Uri>((str, _, _) => new Uri(str))
    );
    
    // ...  
}
```

### Preparing for AOT runtime
```csharp
using deniszykov.TypeConversion;
using Microsoft.Extensions.DependencyInjection;

Host.CreateDefaultBuilder().ConfigureServices(IServiceCollection services) => {

  services.Configure<TypeConversionProviderConfiguration>(options =>
  {
    // disable optimizations which use dynamic code generation
    options.Options &= ~(ConversionOptions.OptimizeWithExpressions | ConversionOptions.OptimizeWithGenerics);
  });
  
}
```

## Key Abstractions

```csharp
interface ITypeConversionProvider; // provides methods to get IConverter
interface IConverter<FromType, ToType>; // converts values
interface IConversionMetadataProvider; // provides metadata for ITypeConversionProvider
```


