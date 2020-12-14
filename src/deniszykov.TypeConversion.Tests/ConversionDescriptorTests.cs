using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit;

namespace deniszykov.TypeConversion.Tests
{
	public class ConversionDescriptorTests
	{
		public static IEnumerable<object[]> ConstructorTestData()
		{
			var defaultFormats = new string[] { null, "unchecked", "" };
			var defaultFormatProviders = new IFormatProvider[] { null, CultureInfo.CurrentCulture, CultureInfo.InvariantCulture, CultureInfo.InvariantCulture.NumberFormat };
			var withSafeConversions = new[] { true, false };
			return
			(
				from defaultFormat in defaultFormats
				from defaultFormatProvider in defaultFormatProviders
				from withSafeConversion in withSafeConversions
				select new object[] { defaultFormat, defaultFormatProvider, withSafeConversion }
			);
		}

		private static long IntToLong(int value) => value;

		[Theory]
		[MemberData(nameof(ConstructorTestData))]
		public void ConstructorTest(string defaultFormat, IFormatProvider defaultFormatProvider, bool withSafeConversion)
		{

			var conversionMethodInfo = new ConversionMethodInfo(new Func<int, long>(IntToLong).GetMethodInfo(), 0);
			var conversionFn = new Func<int, string, IFormatProvider, long>((value, format, formatProvider) => value);
			var safeConversionFn = withSafeConversion ? new Func<int, string, IFormatProvider, KeyValuePair<long, bool>>((value, format, formatProvider) => new KeyValuePair<long, bool>(value, true)) : null;
			var conversionInfo = new ConversionDescriptor(new ReadOnlyCollection<ConversionMethodInfo>(new[]{  conversionMethodInfo }), defaultFormat, defaultFormatProvider, conversionFn, safeConversionFn);

			Assert.NotNull(conversionInfo);
			Assert.NotNull(conversionInfo.Methods);
			Assert.Same(safeConversionFn, conversionInfo.SafeConversion);
			Assert.Same(conversionFn, conversionInfo.Conversion);
			Assert.NotNull(conversionInfo.FromType);
			Assert.NotNull(conversionInfo.ToType);
		}

		[Fact]
		public void ConstructorConverterFnCheckTest()
		{
			var conversionMethodInfo = new ConversionMethodInfo(new Func<int, long>(IntToLong).GetMethodInfo(), 0);
			var conversionFn = new Func<long, string, IFormatProvider, long>((value, format, formatProvider) => value);

			Assert.ThrowsAny<ArgumentException>(() =>
			{
				var conversionInfo = new ConversionDescriptor(new ReadOnlyCollection<ConversionMethodInfo>(new[]{  conversionMethodInfo }), null, null, conversionFn, null);
				Assert.NotNull(conversionInfo);
			});
		}

		[Fact]
		public void ConstructorSafeConverterFnCheckTest()
		{
			var conversionMethodInfo = new ConversionMethodInfo(new Func<int, long>(IntToLong).GetMethodInfo(), 0);
			var conversionFn = new Func<int, string, IFormatProvider, long>((value, format, formatProvider) => value);
			var safeConversionFn = new Func<long, string, IFormatProvider, KeyValuePair<long, bool>>((value, format, formatProvider) => new KeyValuePair<long, bool>(value, true));

			Assert.ThrowsAny<ArgumentException>(() =>
			{
				var conversionInfo = new ConversionDescriptor(new ReadOnlyCollection<ConversionMethodInfo>(new[]{  conversionMethodInfo }), null, null, conversionFn, safeConversionFn);
				Assert.NotNull(conversionInfo);
			});
		}
	}
}
