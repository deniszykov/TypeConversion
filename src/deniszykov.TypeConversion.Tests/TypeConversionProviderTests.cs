
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
			public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
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
		public void DefaultConstructorTest()
		{
			var typeConversionProvider = new TypeConversionProvider(null, null);
			Assert.NotNull(typeConversionProvider);
		}

		[Fact]
		public void ConfiguredConstructorTest()
		{
			var configuration =
#if NETFRAMEWORK
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
			var converter = typeConversionProvider.GetConverter<int, int>();

			Assert.NotNull(converter);
			Assert.NotNull(converter.Descriptor);
			Assert.Equal(ConversionQuality.Native, converter.Descriptor.Methods[0].Quality);
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
	}
}
