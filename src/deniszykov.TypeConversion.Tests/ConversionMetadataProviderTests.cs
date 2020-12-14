using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit;

namespace deniszykov.TypeConversion.Tests
{
	public class ConversionMetadataProviderTests
	{
		private  const string DEFAULT_FORMAT = "defaultFormat";
		private static long IntToLong(int value, string format = DEFAULT_FORMAT) => value;

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
					return ((MyConvertibleType?)value)?.ToString(culture);
				}
				return base.ConvertTo(context, culture, value, destinationType);
			}
			/// <inheritdoc />
			public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				if (value is string stringValue)
				{
					return MyConvertibleType.Parse(stringValue);
				}
				return base.ConvertFrom(context, culture, value);
			}
		}

		[System.ComponentModel.TypeConverter(typeof(MyConvertibleTypeConverter))]
#endif
		public struct MyConvertibleType : IComparable<MyConvertibleType>
		{
			public int Value;

			public MyConvertibleType(int value)
			{
				this.Value = value;
			}

			public static explicit operator int(MyConvertibleType value)
			{
				return value.Value;
			}
			public static implicit operator sbyte(MyConvertibleType value)
			{
				return (sbyte)value.Value;
			}
			public static explicit operator MyConvertibleType(int value)
			{
				return new MyConvertibleType { Value = value };
			}
			public static implicit operator MyConvertibleType(sbyte value)
			{
				return new MyConvertibleType { Value = value };
			}

			public static MyConvertibleType Parse(string value)
			{
				return (MyConvertibleType)int.Parse(value);
			}
			public static MyConvertibleType Parse(string value, string format)
			{
				return (MyConvertibleType)int.Parse(value);
			}
			public static MyConvertibleType Parse(string value, string format, IFormatProvider formatProvider)
			{
				return (MyConvertibleType)int.Parse(value, formatProvider);
			}
			public static MyConvertibleType Parse(string value, IFormatProvider formatProvider, string format)
			{
				return (MyConvertibleType)int.Parse(value, formatProvider);
			}
			public static MyConvertibleType Parse(string value, IFormatProvider formatProvider)
			{
				return (MyConvertibleType)int.Parse(value, formatProvider);
			}

			public static MyConvertibleType Create(int value)
			{
				return (MyConvertibleType)value;
			}
			public static MyConvertibleType CreateFromInt32(int value)
			{
				return (MyConvertibleType)value;
			}
			public static MyConvertibleType FromInt32(int value)
			{
				return (MyConvertibleType)value;
			}
			public static MyConvertibleType From(int value)
			{
				return (MyConvertibleType)value;
			}

			/// <inheritdoc />
			public override string ToString() => this.Value.ToString();
			public string ToString(string format) => this.Value.ToString(format);
			public string ToString(string format, IFormatProvider formatProvider) => this.Value.ToString(format, formatProvider);
			public string ToString(IFormatProvider formatProvider) => this.Value.ToString(formatProvider);
			/// <inheritdoc />
			public int CompareTo(MyConvertibleType other)
			{
				return this.Value.CompareTo(other.Value);
			}
		}

		[Fact]
		public void GetConvertFromConstructorTest()
		{
			var metadataProvider = new ConversionMetadataProvider();
			var fromMethods = metadataProvider.GetConvertFromMethods(typeof(MyConvertibleType));

			Assert.Contains(fromMethods, method => method.Method is ConstructorInfo);
		}

		[Fact]
		public void GetConvertFromExplicitOperatorTest()
		{
			var metadataProvider = new ConversionMetadataProvider();
			var fromMethods = metadataProvider.GetConvertFromMethods(typeof(MyConvertibleType));

			Assert.Contains(fromMethods, method => method.Method.Name == "op_Explicit" && method.FromType == typeof(int));
		}

		[Fact]
		public void GetConvertFromImplicitOperatorTest()
		{
			var metadataProvider = new ConversionMetadataProvider();
			var fromMethods = metadataProvider.GetConvertFromMethods(typeof(MyConvertibleType));

			Assert.Contains(fromMethods, method => method.Method.Name == "op_Implicit" && method.FromType == typeof(sbyte));
		}

		[Fact]
		public void GetConvertFromParseTest()
		{
			var metadataProvider = new ConversionMetadataProvider();
			var fromMethods = metadataProvider.GetConvertFromMethods(typeof(MyConvertibleType));

			Assert.Equal(5, fromMethods.Count(method => method.Method.Name == "Parse" && method.FromType == typeof(string)));
		}

		[Fact]
		public void GetConvertFromCreateTest()
		{
			var metadataProvider = new ConversionMetadataProvider();
			var fromMethods = metadataProvider.GetConvertFromMethods(typeof(MyConvertibleType));

			Assert.Contains(fromMethods, method => method.Method.Name == "Create" && method.FromType == typeof(int));
		}

		[Fact]
		public void GetConvertFromCreateFromInt32Test()
		{
			var metadataProvider = new ConversionMetadataProvider();
			var fromMethods = metadataProvider.GetConvertFromMethods(typeof(MyConvertibleType));

			Assert.Contains(fromMethods, method => method.Method.Name == "CreateFromInt32" && method.FromType == typeof(int));
		}

		[Fact]
		public void GetConvertFromTest()
		{
			var metadataProvider = new ConversionMetadataProvider();
			var fromMethods = metadataProvider.GetConvertFromMethods(typeof(MyConvertibleType));

			Assert.Contains(fromMethods, method => method.Method.Name == "From" && method.FromType == typeof(int));
		}

		[Fact]
		public void GetConvertFromInt32Test()
		{
			var metadataProvider = new ConversionMetadataProvider();
			var fromMethods = metadataProvider.GetConvertFromMethods(typeof(MyConvertibleType));

			Assert.Contains(fromMethods, method => method.Method.Name == "FromInt32" && method.FromType == typeof(int));
		}

		[Fact]
		public void GetConvertToStringTest()
		{
			var metadataProvider = new ConversionMetadataProvider();
			var toMethods = metadataProvider.GetConvertToMethods(typeof(MyConvertibleType));

			Assert.Equal(4, toMethods.Count(method => method.Method.Name == "ToString" && method.ToType == typeof(string)));
		}

		[Fact]
		public void GetConvertToExplicitOperatorTest()
		{
			var metadataProvider = new ConversionMetadataProvider();
			var toMethods = metadataProvider.GetConvertToMethods(typeof(MyConvertibleType));

			Assert.Contains(toMethods, method => method.Method.Name == "op_Explicit" && method.ToType == typeof(int));
		}

		[Fact]
		public void GetConvertToImplicitOperatorTest()
		{
			var metadataProvider = new ConversionMetadataProvider();
			var toMethods = metadataProvider.GetConvertToMethods(typeof(MyConvertibleType));

			Assert.Contains(toMethods, method => method.Method.Name == "op_Implicit" && method.ToType == typeof(sbyte));
		}

#if NETFRAMEWORK
		[Fact]
		public void GetTypeConverterTest()
		{
			var metadataProvider = new ConversionMetadataProvider();
			var typeConverter = metadataProvider.GetTypeConverter(typeof(MyConvertibleType));
			Assert.NotNull(typeConverter);
			Assert.IsType<MyConvertibleTypeConverter>(typeConverter);
		}
#endif
		[Theory]
		[InlineData(typeof(object), true)]
		[InlineData(typeof(ValueType), true)]
		[InlineData(typeof(MyConvertibleType), true)]
		[InlineData(typeof(IComparable<MyConvertibleType>), true)]
		[InlineData(typeof(IComparable<int>), false)]
		[InlineData(typeof(int), false)]
		public void IsAssignableFromTests(Type fromType, bool isAssignable)
		{
			var metadataProvider = new ConversionMetadataProvider();
			var actual = metadataProvider.IsAssignableFrom(typeof(MyConvertibleType), fromType);

			Assert.Equal(isAssignable, actual);
		}


		[Theory]
		[InlineData(0, false)]
		[InlineData(1, true)]
		[InlineData(2, false)]
		public void IsFormatParameterTest(int parameterIndex, bool isFormatParameter)
		{
			var metadataProvider = new ConversionMetadataProvider();
			var conversionMethod = new Func<long, string, IFormatProvider, long>((value, format, formatProvider) => value).GetMethodInfo();
			var parameters = conversionMethod.GetParameters();

			var actual = metadataProvider.IsFormatParameter(parameters[parameterIndex]);

			Assert.Equal(isFormatParameter, actual);
		}

		[Theory]
		[InlineData(0, false)]
		[InlineData(1, false)]
		[InlineData(2, true)]
		public void IsFormatProviderParameterTest(int parameterIndex, bool isFormatProviderParameter)
		{
			var metadataProvider = new ConversionMetadataProvider();
			var conversionMethod = new Func<long, string, IFormatProvider, long>((value, format, formatProvider) => value).GetMethodInfo();
			var parameters = conversionMethod.GetParameters();

			var actual = metadataProvider.IsFormatProviderParameter(parameters[parameterIndex]);

			Assert.Equal(isFormatProviderParameter, actual);
		}

		[Fact]
		public void GetDefaultFormatTest()
		{

			var metadataProvider = new ConversionMetadataProvider();
			var conversionMethodInfo = new ConversionMethodInfo(new Func<int, string, long>(IntToLong).GetMethodInfo(), 0);

			var actual = metadataProvider.GetDefaultFormat(conversionMethodInfo);

			Assert.Equal(DEFAULT_FORMAT, actual);
		}
	}
}
