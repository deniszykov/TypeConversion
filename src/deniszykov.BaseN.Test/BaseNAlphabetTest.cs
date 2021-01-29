using Xunit;

namespace deniszykov.BaseN.Tests
{
	public class BaseNAlphabetTest
	{
		[Fact]
		public void Base16UpperCaseAlphabetTest()
		{
			Assert.NotNull(BaseNAlphabet.Base16UpperCaseAlphabet);
		}

		[Fact]
		public void Base16LowerCaseAlphabetTest()
		{
			Assert.NotNull(BaseNAlphabet.Base16LowerCaseAlphabet);
		}

		[Fact]
		public void Base32AlphabetTest()
		{
			Assert.NotNull(BaseNAlphabet.Base32Alphabet);
		}

		[Fact]
		public void ZBase32AlphabetTest()
		{
			Assert.NotNull(BaseNAlphabet.ZBase32Alphabet);
		}

		[Fact]
		public void Base64AlphabetTest()
		{
			Assert.NotNull(BaseNAlphabet.Base64Alphabet);
		}

		[Fact]
		public void Base64UrlAlphabetTest()
		{
			Assert.NotNull(BaseNAlphabet.Base64UrlAlphabet);
		}

		[Theory]
		[InlineData(16)]
		[InlineData(32)]
		[InlineData(64)]
		public void ConstructorTest(int size)
		{
			var alphabetString = new string('a', size);
			var padding = '_';
			var alphabet = new BaseNAlphabet(alphabetString.ToCharArray(), padding);

			Assert.NotEqual(0, alphabet.DecodingBlockSize);
			Assert.NotEqual(0, alphabet.EncodingBlockSize);
			Assert.NotEqual(0, alphabet.EncodingBits);
			Assert.NotNull(alphabet.Alphabet);
			Assert.NotNull(alphabet.AlphabetInverse);
			Assert.True(alphabet.HasPadding);
			Assert.Equal(padding, alphabet.Padding);
			Assert.True(alphabet.HasPadding);
		}

		[Fact]
		public void ToStringTest()
		{
			Assert.NotNull(BaseNAlphabet.Base64UrlAlphabet.ToString());
		}
	}
}
