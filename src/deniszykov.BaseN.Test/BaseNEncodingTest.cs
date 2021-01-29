using Xunit;

namespace deniszykov.BaseN.Tests
{
	public class BaseNEncodingTest
	{
		[Fact]
		public void Base16UpperCaseTest()
		{
			Assert.NotNull(BaseNEncoding.Base16UpperCase);
			Assert.Equal(BaseNAlphabet.Base16UpperCaseAlphabet, BaseNEncoding.Base16UpperCase.Alphabet);
		}

		[Fact]
		public void Base16LowerCaseTest()
		{
			Assert.NotNull(BaseNEncoding.Base16LowerCase);
			Assert.Equal(BaseNAlphabet.Base16LowerCaseAlphabet, BaseNEncoding.Base16LowerCase.Alphabet);
		}

		[Fact]
		public void Base32Test()
		{
			Assert.NotNull(BaseNEncoding.Base32);
			Assert.Equal(BaseNAlphabet.Base32Alphabet, BaseNEncoding.Base32.Alphabet);
		}

		[Fact]
		public void ZBase32Test()
		{
			Assert.NotNull(BaseNEncoding.ZBase32);
			Assert.Equal(BaseNAlphabet.ZBase32Alphabet, BaseNEncoding.ZBase32.Alphabet);
		}

		[Fact]
		public void Base64Test()
		{
			Assert.NotNull(BaseNEncoding.Base64);
			Assert.Equal(BaseNAlphabet.Base64Alphabet, BaseNEncoding.Base64.Alphabet);
		}

		[Fact]
		public void Base64UrlTest()
		{
			Assert.NotNull(BaseNEncoding.Base64Url);
			Assert.Equal(BaseNAlphabet.Base64UrlAlphabet, BaseNEncoding.Base64Url.Alphabet);
		}

		[Fact]
		public void ConstructorTest()
		{
			var expectedAlphabet = BaseNAlphabet.Base64UrlAlphabet;
			var expectedEncodingName = "base64";
			var baseNEncoding = new BaseNEncoding(expectedAlphabet, expectedEncodingName);

			Assert.Equal(expectedAlphabet, baseNEncoding.Alphabet);
			Assert.Equal(expectedEncodingName, baseNEncoding.EncodingName);
			Assert.Equal(expectedAlphabet.EncodingBlockSize == 1, baseNEncoding.IsSingleByte);
		}

		[Fact]
		public void GetByteCountCharArray()
		{
			var base64String = "80_poA1Ahss=";
			var base64Bytes = new byte[] { 243, 79, 233, 160, 13, 64, 134, 203 };
			var baseNEncoding = new BaseNEncoding(BaseNAlphabet.Base64UrlAlphabet, "name");

			var actual = baseNEncoding.GetByteCount(base64String.ToCharArray(), 0, base64String.Length);
			Assert.Equal(base64Bytes.Length, actual);
		}

#if NETCOREAPP
		[Fact]
		public void GetByteCountString()
		{
			var base64String = "80_poA1Ahss=";
			var base64Bytes = new byte[] { 243, 79, 233, 160, 13, 64, 134, 203 };
			var baseNEncoding = new BaseNEncoding(BaseNAlphabet.Base64UrlAlphabet, "name");

			var actual = baseNEncoding.GetByteCount(base64String, 0, base64String.Length);
			Assert.Equal(base64Bytes.Length, actual);
		}
#endif

		[Fact]
		public unsafe void GetByteCountPtrTest()
		{
			var base64String = "80_poA1Ahss=";
			var base64Bytes = new byte[] { 243, 79, 233, 160, 13, 64, 134, 203 };
			var baseNEncoding = new BaseNEncoding(BaseNAlphabet.Base64UrlAlphabet, "name");

			fixed (char* charsPtr = base64String)
			{
				var actual = baseNEncoding.GetByteCount(charsPtr, base64String.Length);
				Assert.Equal(base64Bytes.Length, actual);
			}
		}

		[Fact]
		public void GetBytesCharArrayTest()
		{
			var base64String = "80_poA1Ahss=";
			var base64Bytes = new byte[] { 243, 79, 233, 160, 13, 64, 134, 203 };
			var baseNEncoding = new BaseNEncoding(BaseNAlphabet.Base64UrlAlphabet, "name");

			var actual = baseNEncoding.GetBytes(base64String.ToCharArray(), 0, base64String.Length);
			Assert.Equal(base64Bytes, actual);
		}

		[Fact]
		public void GetBytesStringTest()
		{
			var base64String = "80_poA1Ahss=";
			var base64Bytes = new byte[] { 243, 79, 233, 160, 13, 64, 134, 203 };
			var baseNEncoding = new BaseNEncoding(BaseNAlphabet.Base64UrlAlphabet, "name");

			var actual = baseNEncoding.GetBytes(base64String, 0, base64String.Length);
			Assert.Equal(base64Bytes, actual);
		}

		[Fact]
		public unsafe void GetBytesPtrTest()
		{
			var base64String = "80_poA1Ahss=";
			var base64Bytes = new byte[] { 243, 79, 233, 160, 13, 64, 134, 203 };
			var baseNEncoding = new BaseNEncoding(BaseNAlphabet.Base64UrlAlphabet, "name");
			var actualBytes = new byte[base64Bytes.Length];

			fixed (byte* bytesPtr = actualBytes)
			fixed (char* charsPtr = base64String)
			{
				baseNEncoding.GetBytes(charsPtr, base64String.Length, bytesPtr, actualBytes.Length);

				Assert.Equal(base64Bytes, actualBytes);
			}
		}

		[Fact]
		public void GetCharCountByteArrayTest()
		{
			var base64String = "80_poA1Ahss=";
			var base64Bytes = new byte[] { 243, 79, 233, 160, 13, 64, 134, 203 };
			var baseNEncoding = new BaseNEncoding(BaseNAlphabet.Base64UrlAlphabet, "name");

			var actual = baseNEncoding.GetCharCount(base64Bytes, 0, base64Bytes.Length);
			Assert.Equal(base64String.Length, actual);

		}

		[Fact]
		public unsafe void GetCharCountPtrTest()
		{
			var base64String = "80_poA1Ahss=";
			var base64Bytes = new byte[] { 243, 79, 233, 160, 13, 64, 134, 203 };
			var baseNEncoding = new BaseNEncoding(BaseNAlphabet.Base64UrlAlphabet, "name");

			fixed (byte* bytesPtr = base64Bytes)
			{
				var actual = baseNEncoding.GetCharCount(bytesPtr, base64Bytes.Length);
				Assert.Equal(base64String.Length, actual);
			}
		}

		[Fact]
		public void GetCharsByteArrayTest()
		{
			var base64String = "80_poA1Ahss=";
			var base64Bytes = new byte[] { 243, 79, 233, 160, 13, 64, 134, 203 };
			var baseNEncoding = new BaseNEncoding(BaseNAlphabet.Base64UrlAlphabet, "name");
			var actualChars = new char[base64String.Length];

			baseNEncoding.GetChars(base64Bytes, 0, base64Bytes.Length, actualChars, 0);

			Assert.Equal(base64String.ToCharArray(), actualChars);
		}

		[Fact]
		public unsafe void GetCharsPtrTest()
		{
			var base64String = "80_poA1Ahss=";
			var base64Bytes = new byte[] { 243, 79, 233, 160, 13, 64, 134, 203 };
			var baseNEncoding = new BaseNEncoding(BaseNAlphabet.Base64UrlAlphabet, "name");
			var actualChars = new char[base64String.Length];

			fixed (char* charsPtr = actualChars)
			fixed (byte* bytesPtr = base64Bytes)
			{
				baseNEncoding.GetChars(bytesPtr, base64Bytes.Length, charsPtr, actualChars.Length);
			}

			Assert.Equal(base64String.ToCharArray(), actualChars);
		}

		[Fact]
		public void GetMaxByteCountTest()
		{
			var alphabet = BaseNAlphabet.Base64UrlAlphabet;
			var baseNEncoding = new BaseNEncoding(alphabet, "name");
			var expected = alphabet.EncodingBlockSize * 5;
			var actual = baseNEncoding.GetMaxByteCount(alphabet.DecodingBlockSize * 5);
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void GetMaxCharCountTest()
		{
			var alphabet = BaseNAlphabet.Base64UrlAlphabet;
			var baseNEncoding = new BaseNEncoding(alphabet, "name");
			var expected = alphabet.DecodingBlockSize * 5;
			var actual = baseNEncoding.GetMaxCharCount(alphabet.EncodingBlockSize * 5);
			Assert.Equal(expected, actual);
		}


		[Fact]
		public void GetDecoderTest()
		{
			var baseNEncoding = new BaseNEncoding(BaseNAlphabet.Base64UrlAlphabet, "name");
			Assert.NotNull(baseNEncoding.GetDecoder());
		}

		[Fact]
		public void GetEncoderTest()
		{
			var baseNEncoding = new BaseNEncoding(BaseNAlphabet.Base64UrlAlphabet, "name");
			Assert.NotNull(baseNEncoding.GetEncoder());
		}
		[Fact]
		public void ToStringTest()
		{
			var baseNEncoding = new BaseNEncoding(BaseNAlphabet.Base64UrlAlphabet, "name");
			Assert.NotNull(baseNEncoding.ToString());
		}
	}
}
