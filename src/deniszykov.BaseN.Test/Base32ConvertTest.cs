using System.Linq;
using Xunit;

namespace deniszykov.BaseN.Tests
{
	public class Base32ConvertTest
	{
		[Fact]
		public void ToStringTest()
		{
			var expected = "6Q======";
			var data = new byte[] {244};

			var actual = Base32Convert.ToString(data);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToStringPartialTest()
		{
			var expected = "6Q======";
			var data = new byte[] {255, 244, 255};

			var actual = Base32Convert.ToString(data, 1, 1);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToCharArrayTest()
		{
			var expected = "6Q======".ToCharArray();
			var data = new byte[] {244};

			var actual = Base32Convert.ToCharArray(data);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToCharArrayPartialTest()
		{
			var expected = "6Q======".ToCharArray();
			var data = new byte[] {255, 244, 255};

			var actual = Base32Convert.ToCharArray(data, 1, 1);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToBytesCharArrayTest()
		{
			var baseNChars = "6Q======".ToCharArray();
			var expected = new byte[] {244};

			var actual = Base32Convert.ToBytes(baseNChars);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToBytesCharArrayPartialTest()
		{
			var baseNChars = "996Q======99".ToCharArray();
			var expected = new byte[] {244};

			var actual = Base32Convert.ToBytes(baseNChars, 2, baseNChars.Length - 4);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToBytesByteArrayTest()
		{
			var baseNCharBytes = "6Q======".ToCharArray().Select(ch => (byte)ch).ToArray();
			var expected = new byte[] {244};

			var actual = Base32Convert.ToBytes(baseNCharBytes);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToBytesByteArrayPartialTest()
		{
			var baseNCharBytes = "996Q======99".ToCharArray().Select(ch => (byte)ch).ToArray();
			var expected = new byte[] {244};

			var actual = Base32Convert.ToBytes(baseNCharBytes, 2, baseNCharBytes.Length - 4);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToByteStringTest()
		{
			var baseNString = "6Q======";
			var expected = new byte[] {244};

			var actual = Base32Convert.ToBytes(baseNString);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToBytesStringPartialTest()
		{
			var baseNString = "996Q======99";
			var expected = new byte[] {244};

			var actual = Base32Convert.ToBytes(baseNString, 2, baseNString.Length - 4);

			Assert.Equal(expected, actual);
		}
	}
}
