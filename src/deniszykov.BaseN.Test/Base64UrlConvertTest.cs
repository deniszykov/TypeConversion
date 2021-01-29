using System.Linq;
using Xunit;

namespace deniszykov.BaseN.Tests
{
	public class Base64UrlConvertTest
	{
		[Fact]
		public void ToStringTest()
		{
			var expected = "eg==";
			var data = new byte[] { 122 };

			var actual = Base64UrlConvert.ToString(data);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToStringPartialTest()
		{
			var expected = "eg==";
			var data = new byte[] {255, 122, 255};

			var actual = Base64UrlConvert.ToString(data, 1, 1);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToCharArrayTest()
		{
			var expected = "eg==".ToCharArray();
			var data = new byte[] { 122 };

			var actual = Base64UrlConvert.ToCharArray(data);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToCharArrayPartialTest()
		{
			var expected = "eg==".ToCharArray();
			var data = new byte[] {255, 122, 255};

			var actual = Base64UrlConvert.ToCharArray(data, 1, 1);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToBytesCharArrayTest()
		{
			var baseNChars = "eg==".ToCharArray();
			var expected = new byte[] { 122 };

			var actual = Base64UrlConvert.ToBytes(baseNChars);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToBytesCharArrayPartialTest()
		{
			var baseNChars = "99eg==99".ToCharArray();
			var expected = new byte[] { 122 };

			var actual = Base64UrlConvert.ToBytes(baseNChars, 2, baseNChars.Length - 4);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToBytesByteArrayTest()
		{
			var baseNCharBytes = "eg==".ToCharArray().Select(ch => (byte)ch).ToArray();
			var expected = new byte[] { 122 };

			var actual = Base64UrlConvert.ToBytes(baseNCharBytes);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToBytesByteArrayPartialTest()
		{
			var baseNCharBytes = "99eg==99".ToCharArray().Select(ch => (byte)ch).ToArray();
			var expected = new byte[] { 122 };

			var actual = Base64UrlConvert.ToBytes(baseNCharBytes, 2, baseNCharBytes.Length - 4);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToByteStringTest()
		{
			var baseNString = "eg==";
			var expected = new byte[] { 122 };

			var actual = Base64UrlConvert.ToBytes(baseNString);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void ToBytesStringPartialTest()
		{
			var baseNString = "99eg==99";
			var expected = new byte[] { 122 };

			var actual = Base64UrlConvert.ToBytes(baseNString, 2, baseNString.Length - 4);

			Assert.Equal(expected, actual);
		}
	}
}
