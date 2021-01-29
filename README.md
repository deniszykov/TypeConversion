Introduction
============
For historical reasons, .NET has several approaches to value conversion:
- Explicit/Implicit operators
- System.Convert class 
- IConvertible interface
- System.ComponentModel.TypeConverter
- To, From, Parse, Create methods
- Constructors (Uri, Guid)
- Meta types(Enums, Nullable Types).

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
ToType ITypeConversionProvider.Convert<FromType, ToType>(fromValue, [format], [formatProvider]);
bool ITypeConversionProvider.TryConvert<FromType, ToType>(fromValue, out result, [format], [formatProvider])
string ITypeConversionProvider.ConvertToString<FromType>(fromValue, [format], [formatProvider]);
// non-generic
object ITypeConversionProvider.Convert(fromValue, toType, fromValue, [format], [formatProvider]);
bool ITypeConversionProvider.TryConvert(fromValue, toType, fromValue, out result, [format], [formatProvider]);
```

## Example
```csharp
  var conversionProvider = new TypeConversionProvider();
  var timeSpanString = "00:00:01";
  var timeSpan = conversionProvider.Convert<object, TimeSpan>(timeSpanString);
```

## Configuration
```csharp
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
.ConfigureServices(IServiceCollection services) => {
  services.Configure<TypeConversionProviderConfiguration>(options =>
  {
    options.DefaultFormatProvider = CultureInfo.CurrentUICulture;
  });
  // services.AddSingleton<IConversionMetadataProvider, MyCustomConversionMetadataProvider>();
  services.AddSingleton<ITypeConversionProvider, TypeConversionProvider>();
}
```

### Providing custom conversion between types
```csharp
.ConfigureServices(IServiceCollection services) => {
  services.Configure<TypeConversionProviderConfiguration>(options =>
  {
    options.RegisterConversion<Uri, string>((uri, format, formatProvider) => uri.OriginalString);
  });
}
```

### Preparing for AOT runtime
```csharp
.ConfigureServices(IServiceCollection services) => {
  services.Configure<TypeConversionProviderConfiguration>(options =>
  {
	// disable optimizations which use dynamic code generation
    options.Options &= ~(ConversionOptions.OptimizeWithExpressions | ConversionOptions.OptimizeWithGenerics);
  });
}
```

## Key Abstractions

```csharp
interface ITypeConversionProvider // provides methods to get IConverter
interface IConverter<FromType, ToType> // converts values
interface IConversionMetadataProvider // provides metadata for ITypeConversionProvider
```


