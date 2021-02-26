
using System;
using System.Globalization;
using System.Net;
using Xunit;

namespace deniszykov.TypeConversion.Tests
{
	public class TypeConversionProviderTests
	{
#if NETFRAMEWORK
		private class MyConvertibleTypeConverter : System.ComponentModel.TypeConverter
		{
			/// <inheritdoc />
			public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, Type sourceType)
			{
				return sourceType == typeof(string);
			}
			/// <inheritdoc />
			public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, Type destinationType)
			{
				return destinationType == typeof(string);
			}
			/// <inheritdoc />
			public override object? ConvertTo(System.ComponentModel.ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			{
				if (destinationType == typeof(string))
				{
					return ((MyConvertibleType?)value)?.ToString();
				}
				return base.ConvertTo(context, culture, value, destinationType);
			}
			/// <inheritdoc />
			public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				if (value is string stringValue)
				{
					return new MyConvertibleType { Value = int.Parse(stringValue) };
				}
				return base.ConvertFrom(context, culture, value);
			}
		}

		[System.ComponentModel.TypeConverter(typeof(MyConvertibleTypeConverter))]
		public struct MyConvertibleType
		{
			public int Value;
		}
#endif

		[Fact]
		public void ConstructorTest()
		{
			var typeConversionProvider = new TypeConversionProvider(null, null);

			Assert.NotNull(typeConversionProvider);
			Assert.NotNull(typeConversionProvider.ToString());
		}

		[Fact]
		public void ConfiguredConstructorTest()
		{
			var configuration =
#if NET45
				new TypeConversionProviderConfiguration();
#else
				Microsoft.Extensions.Options.Options.Create(new TypeConversionProviderConfiguration());
#endif
			var typeConversionProvider = new TypeConversionProvider(configuration, null);
			Assert.NotNull(typeConversionProvider);
		}


		[Fact]
		public void WithMetadataProviderConstructorTest()
		{
			var metadataProvider = new ConversionMetadataProvider();
			var typeConversionProvider = new TypeConversionProvider(null, metadataProvider);
			Assert.NotNull(typeConversionProvider);
		}

		[Fact]
		public void GetConverterGenericTest()
		{
			var typeConversionProvider = new TypeConversionProvider();
			var converter = typeConversionProvider.GetConverter<int, string>();

			Assert.NotNull(converter);
			Assert.NotNull(converter.Descriptor);
			Assert.Equal(typeof(int), converter.FromType);
			Assert.Equal(typeof(string), converter.ToType);
			Assert.NotNull(converter.ToString());
			Assert.Equal(ConversionQuality.Method, converter.Descriptor.Methods[0].Quality);
		}

		[Theory]
		[InlineData(typeof(int), typeof(int), ConversionQuality.Native)]
		[InlineData(typeof(int), typeof(long), ConversionQuality.Native)]
		[InlineData(typeof(decimal), typeof(double), ConversionQuality.Explicit)]
		[InlineData(typeof(int), typeof(decimal), ConversionQuality.Implicit)]
		[InlineData(typeof(Uri), typeof(string), ConversionQuality.Custom)]
		[InlineData(typeof(string), typeof(IPAddress), ConversionQuality.Method)]
		[InlineData(typeof(byte[]), typeof(IPAddress), ConversionQuality.Constructor)]
		[InlineData(typeof(Guid), typeof(IPAddress), ConversionQuality.None)]
#if NETFRAMEWORK
		[InlineData(typeof(string), typeof(MyConvertibleType), ConversionQuality.TypeConverter)]
#endif
		public void GetConverterTest(Type fromType, Type toType, ConversionQuality quality)
		{
			var typeConversionProvider = new TypeConversionProvider();
			var converter = typeConversionProvider.GetConverter(fromType, toType);

			Assert.NotNull(converter);
			Assert.NotNull(converter.Descriptor);
			Assert.Equal(quality, converter.Descriptor.Methods[0].Quality);
		}

		[Fact]
		public void ConfigurationCustomConversionTest()
		{
			var url = new Uri("http://example.com/");
			var expected = "expected";
			var configuration = new TypeConversionProviderConfiguration();
			configuration.RegisterConversion(new Func<Uri, string, IFormatProvider, string>((uri, _, __) => expected));
#if NET45
			var typeConversionProvider = new TypeConversionProvider(configuration);
#else
			var typeConversionProvider = new TypeConversionProvider(Microsoft.Extensions.Options.Options.Create(configuration));
#endif
			var actual = typeConversionProvider.Convert<Uri, string>(url);
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ConfigurationDefaultFormatProviderTest()
		{
			var value = new Uri("http://example.com/");
			var defaultFormatProvider = CultureInfo.CurrentUICulture;
			var expected = defaultFormatProvider.Name;
			var configuration = new TypeConversionProviderConfiguration
			{
				DefaultFormatProvider = defaultFormatProvider,
				Options = ConversionOptions.UseDefaultFormatIfNotSpecified
			};
			configuration.RegisterConversion(new Func<Uri, string, IFormatProvider, string>((_, __, formatProvider) => ((CultureInfo)formatProvider).Name));
#if NET45
			var typeConversionProvider = new TypeConversionProvider(configuration);
#else
			var typeConversionProvider = new TypeConversionProvider(Microsoft.Extensions.Options.Options.Create(configuration));
#endif

			var actual = typeConversionProvider.Convert<Uri, string>(value);
			Assert.Equal(expected, actual);

			var success = typeConversionProvider.TryConvert<Uri, string>(value, out actual);

			Assert.True(success);
			Assert.NotNull(actual);
		}

		[Fact]
		public void ConfigurationDefaultFormatProviderCultureNameTest()
		{
			var value = new Uri("http://example.com/");
			var defaultFormatProvider = CultureInfo.CurrentUICulture;
			var expected = defaultFormatProvider.Name;
			var configuration = new TypeConversionProviderConfiguration
			{
				DefaultFormatProviderCultureName = defaultFormatProvider.Name,
				Options = ConversionOptions.UseDefaultFormatIfNotSpecified
			};
			configuration.RegisterConversion(new Func<Uri, string, IFormatProvider, string>((_, __, formatProvider) => ((CultureInfo)formatProvider).Name));
#if NET45
			var typeConversionProvider = new TypeConversionProvider(configuration);
#else
			var typeConversionProvider = new TypeConversionProvider(Microsoft.Extensions.Options.Options.Create(configuration));
#endif

			var actual = typeConversionProvider.Convert<Uri, string>(value);
			Assert.Equal(expected, actual);

			var success = typeConversionProvider.TryConvert<Uri, string>(value, out actual);

			Assert.True(success);
			Assert.NotNull(actual);
		}

		[Fact]
		public void ConfigurationConversionMethodSelectionStrategyTest()
		{
			var value = DateTime.UtcNow;
			// ReSharper disable once SpecifyACultureInStringConversionExplicitly
			var expected = value.ToString(null, CultureInfo.InvariantCulture);
			var configuration = new TypeConversionProviderConfiguration
			{
				ConversionMethodSelectionStrategy = ConversionMethodSelectionStrategy.MostSpecificMethod,
				Options = ConversionOptions.None
			};
#if NET45
			var typeConversionProvider = new TypeConversionProvider(configuration);
#else
			var typeConversionProvider = new TypeConversionProvider(Microsoft.Extensions.Options.Options.Create(configuration));
#endif

			var actual = typeConversionProvider.Convert<DateTime, string>(value, null, CultureInfo.InvariantCulture);
			Assert.Equal(expected, actual);

			var success = typeConversionProvider.TryConvert<DateTime, string>(value, out actual);

			Assert.True(success);
			Assert.NotNull(actual);
		}

		[Fact]
		public void ConfigurationNoOptimizationsTest()
		{
			var value = DateTime.UtcNow;
			// ReSharper disable once SpecifyACultureInStringConversionExplicitly
			var configuration = new TypeConversionProviderConfiguration
			{
				Options = ConversionOptions.None
			};
#if NET45
			var typeConversionProvider = new TypeConversionProvider(configuration);
#else
			var typeConversionProvider = new TypeConversionProvider(Microsoft.Extensions.Options.Options.Create(configuration));
#endif

			var actual = typeConversionProvider.Convert<DateTime, string>(value);
			Assert.NotNull(actual);
		}

		[Fact]
		public void ConfigurationFastCallOptimizationTest()
		{
			// ReSharper disable once SpecifyACultureInStringConversionExplicitly
			var configuration = new TypeConversionProviderConfiguration
			{
				Options = ConversionOptions.FastCast
			};
			configuration.RegisterConversion(new Func<TypeConversionProvider, string, IFormatProvider, ITypeConversionProvider>((___, _, __) => throw new Exception("Shouldn't be executed.")));
#if NET45
			var typeConversionProvider = new TypeConversionProvider(configuration);
#else
			var typeConversionProvider = new TypeConversionProvider(Microsoft.Extensions.Options.Options.Create(configuration));
#endif

			var actual = typeConversionProvider.Convert<TypeConversionProvider, ITypeConversionProvider>(typeConversionProvider);

			Assert.NotNull(actual);

			var success = typeConversionProvider.TryConvert<TypeConversionProvider, ITypeConversionProvider>(typeConversionProvider, out actual);

			Assert.True(success);
			Assert.NotNull(actual);
		}

		[Fact]
		public void ConfigurationUseDefaultFormatIfNotSpecifiedTest()
		{
			var value = DateTime.UtcNow;
			// ReSharper disable once SpecifyACultureInStringConversionExplicitly
			var expected = value.ToString("o", CultureInfo.InvariantCulture);
			var configuration = new TypeConversionProviderConfiguration
			{
				Options = ConversionOptions.UseDefaultFormatIfNotSpecified
			};
#if NET45
			var typeConversionProvider = new TypeConversionProvider(configuration);
#else
			var typeConversionProvider = new TypeConversionProvider(Microsoft.Extensions.Options.Options.Create(configuration));
#endif

			var actual = typeConversionProvider.Convert<DateTime, string>(value);
			Assert.Equal(expected, actual);

			var success = typeConversionProvider.TryConvert<DateTime, string>(value, out actual);

			Assert.True(success);
			Assert.NotNull(actual);
		}
	}
}
