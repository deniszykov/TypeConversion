using System.Linq;
using Xunit;

namespace deniszykov.BaseN.Tests
{
	public class ZBase32ConvertTest
	{
		[Fact]
		public void ToStringTest()
		{
			var expected = "qy";
			var data = new byte[] {112};

			var actual = ZBase32Convert.ToString(data);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToStringPartialTest()
		{
			var expected = "qy";
			var data = new byte[] {255, 112, 255};

			var actual = ZBase32Convert.ToString(data, 1, 1);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToCharArrayTest()
		{
			var expected = "qy".ToCharArray();
			var data = new byte[] {112};

			var actual = ZBase32Convert.ToCharArray(data);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToCharArrayPartialTest()
		{
			var expected = "qy".ToCharArray();
			var data = new byte[] {255, 112, 255};

			var actual = ZBase32Convert.ToCharArray(data, 1, 1);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToBytesCharArrayTest()
		{
			var baseNChars = "qy".ToCharArray();
			var expected = new byte[] {112};

			var actual = ZBase32Convert.ToBytes(baseNChars);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToBytesCharArrayPartialTest()
		{
			var baseNChars = "99qy99".ToCharArray();
			var expected = new byte[] {112};

			var actual = ZBase32Convert.ToBytes(baseNChars, 2, baseNChars.Length - 4);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToBytesByteArrayTest()
		{
			var baseNCharBytes = "qy".ToCharArray().Select(ch => (byte)ch).ToArray();
			var expected = new byte[] {112};

			var actual = ZBase32Convert.ToBytes(baseNCharBytes);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToBytesByteArrayPartialTest()
		{
			var baseNCharBytes = "99qy99".ToCharArray().Select(ch => (byte)ch).ToArray();
			var expected = new byte[] {112};

			var actual = ZBase32Convert.ToBytes(baseNCharBytes, 2, baseNCharBytes.Length - 4);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToByteStringTest()
		{
			var baseNString = "qy";
			var expected = new byte[] {112};

			var actual = ZBase32Convert.ToBytes(baseNString);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToBytesStringPartialTest()
		{
			var baseNString = "99qy99";
			var expected = new byte[] {112};

			var actual = ZBase32Convert.ToBytes(baseNString, 2, baseNString.Length - 4);

			Assert.Equal(expected, actual);
		}
	}
}
