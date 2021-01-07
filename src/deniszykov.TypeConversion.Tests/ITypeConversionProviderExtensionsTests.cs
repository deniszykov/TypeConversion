

using System;
using Xunit;

namespace deniszykov.TypeConversion.Tests
{
	public class ITypeConversionProviderExtensionsTests
	{

		[Fact]
		public void ConvertTest()
		{
			var typeConversionProvider = new TypeConversionProvider();
			var expected = (byte)1;
			var actual = typeConversionProvider.Convert(typeof(int), typeof(byte), 1);

			Assert.Equal(expected, actual);

			actual = typeConversionProvider.Convert(typeof(int), typeof(byte), 1, default(string));

			Assert.Equal(expected, actual);

			actual = typeConversionProvider.Convert(typeof(int), typeof(byte), 1, default(IFormatProvider));

			Assert.Equal(expected, actual);

			actual = typeConversionProvider.Convert(typeof(int), typeof(byte), 1, default(string), default(IFormatProvider));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ConvertGenericTest()
		{
			var typeConversionProvider = new TypeConversionProvider();
			var expected = (byte)1;
			var actual = typeConversionProvider.Convert<int, byte>(1);

			Assert.Equal(expected, actual);

			actual = typeConversionProvider.Convert<int, byte>(1, default(string));

			Assert.Equal(expected, actual);

			actual = typeConversionProvider.Convert<int, byte>(1, default(IFormatProvider));

			Assert.Equal(expected, actual);

			actual = typeConversionProvider.Convert<int, byte>(1, default(string), default(IFormatProvider));

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void TryConvertTest()
		{
			var typeConversionProvider = new TypeConversionProvider();
			var expected = (byte)1;
			var success = typeConversionProvider.TryConvert(typeof(int), typeof(byte), 1, out var actual);

			Assert.True(success);
			Assert.Equal(expected, actual);

			success = typeConversionProvider.TryConvert(typeof(int), typeof(byte), 1, out actual, default(string));

			Assert.True(success);
			Assert.Equal(expected, actual);

			success = typeConversionProvider.TryConvert(typeof(int), typeof(byte), 1, out actual, default(IFormatProvider));

			Assert.True(success);
			Assert.Equal(expected, actual);

			success = typeConversionProvider.TryConvert(typeof(int), typeof(byte), 1, out actual, default(string), default(IFormatProvider));

			Assert.True(success);
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void TryConvertGenericTest()
		{
			var typeConversionProvider = new TypeConversionProvider();
			var expected = (byte)1;
			var success = typeConversionProvider.TryConvert<int, byte>(expected, out var actual);

			Assert.True(success);
			Assert.Equal(expected, actual);

			success = typeConversionProvider.TryConvert<int, byte>(expected, out actual, default(string));

			Assert.True(success);
			Assert.Equal(expected, actual);

			success = typeConversionProvider.TryConvert<int, byte>(expected, out actual, default(IFormatProvider));

			Assert.True(success);
			Assert.Equal(expected, actual);

			success = typeConversionProvider.TryConvert<int, byte>(expected, out actual, default(string), default(IFormatProvider));

			Assert.True(success);
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ConvertToStringGenericTest()
		{
			var typeConversionProvider = new TypeConversionProvider();
			var expected = "1";
			var actual = typeConversionProvider.ConvertToString(1);

			Assert.Equal(expected, actual);

			actual = typeConversionProvider.ConvertToString(1, default(string));

			Assert.Equal(expected, actual);

			actual = typeConversionProvider.ConvertToString(1, default(IFormatProvider));

			Assert.Equal(expected, actual);

			actual = typeConversionProvider.ConvertToString(1, default(string), default(IFormatProvider));

			Assert.Equal(expected, actual);
		}
	}
}
