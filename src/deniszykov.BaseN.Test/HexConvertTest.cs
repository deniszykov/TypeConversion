using System.Linq;
using Xunit;

namespace deniszykov.BaseN.Tests
{
	public class HexConvertTest
	{
		[Fact]
		public void ToStringTest()
		{
			var expected = "9c";
			var data = new byte[] {156};

			var actual = HexConvert.ToString(data, lowerCase: true);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToStringPartialTest()
		{
			var expected = "9c";
			var data = new byte[] {255, 156, 255};

			var actual = HexConvert.ToString(data, 1, 1, lowerCase: true);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToCharArrayTest()
		{
			var expected = "9c".ToCharArray();
			var data = new byte[] {156};

			var actual = HexConvert.ToCharArray(data, lowerCase: true);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToCharArrayPartialTest()
		{
			var expected = "9c".ToCharArray();
			var data = new byte[] {255, 156, 255};

			var actual = HexConvert.ToCharArray(data, 1, 1, lowerCase: true);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToBytesCharArrayTest()
		{
			var baseNChars = "9c".ToCharArray();
			var expected = new byte[] {156};

			var actual = HexConvert.ToBytes(baseNChars);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToBytesCharArrayPartialTest()
		{
			var baseNChars = "999c99".ToCharArray();
			var expected = new byte[] {156};

			var actual = HexConvert.ToBytes(baseNChars, 2, baseNChars.Length - 4);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToBytesByteArrayTest()
		{
			var baseNCharBytes = "9c".ToCharArray().Select(ch => (byte)ch).ToArray();
			var expected = new byte[] {156};

			var actual = HexConvert.ToBytes(baseNCharBytes);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToBytesByteArrayPartialTest()
		{
			var baseNCharBytes = "999c99".ToCharArray().Select(ch => (byte)ch).ToArray();
			var expected = new byte[] {156};

			var actual = HexConvert.ToBytes(baseNCharBytes, 2, baseNCharBytes.Length - 4);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToByteStringTest()
		{
			var baseNString = "9c";
			var expected = new byte[] {156};

			var actual = HexConvert.ToBytes(baseNString);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToBytesStringPartialTest()
		{
			var baseNString = "999c99";
			var expected = new byte[] {156};

			var actual = HexConvert.ToBytes(baseNString, 2, baseNString.Length - 4);

			Assert.Equal(expected, actual);
		}
	}
}
