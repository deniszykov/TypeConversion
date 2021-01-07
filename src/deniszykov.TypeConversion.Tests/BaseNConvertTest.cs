/*
	Copyright (c) 2020 Denis Zykov
	
	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 

	License: https://opensource.org/licenses/MIT
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using deniszykov.DataTransformation;
using Xunit;

namespace deniszykov.TypeConversion.Tests
{
	public class BaseNConvertTest
	{
		public static IEnumerable<object[]> Base32TestData()
		{
			return new object[][] {
				// bytes, base 32 encoded bytes
				new object[] { BaseNConvert.Base32Alphabet, new byte[] { 244 }, "6Q======" },
				new object[] { BaseNConvert.Base32Alphabet, new byte[] { 48, 235 }, "GDVQ====" },
				new object[] { BaseNConvert.Base32Alphabet, new byte[] { 214, 190, 16 }, "227BA===" },
				new object[] { BaseNConvert.Base32Alphabet, new byte[] { 88, 13, 19, 170 }, "LAGRHKQ=" },
				new object[] { BaseNConvert.Base32Alphabet, new byte[] { 52, 42, 221, 47, 173, 60, 156 }, "GQVN2L5NHSOA====" },
				new object[] { BaseNConvert.Base32Alphabet, new byte[] { 204, 219, 194, 32, 11, 120, 172, 225 }, "ZTN4EIALPCWOC===" },
				new object[] { BaseNConvert.Base32Alphabet, new byte[] { 2, 0, 160, 56, 46, 190, 0, 49, 15, 149, 189, 191, 96, 16, 193, 245, 93, 228, 25, 53, 47, 232, 176, 236, 163, 64, 234, 76, 93, 192, 233, 183 }, "AIAKAOBOXYADCD4VXW7WAEGB6VO6IGJVF7ULB3FDIDVEYXOA5G3Q====" },
				new object[] { BaseNConvert.Base32Alphabet, new byte[] { 63, 244, 188, 16, 183, 212, 229, 236, 51, 249, 206, 153, 240, 124, 60, 187, 25, 204, 195, 38, 95, 118, 178, 25, 79, 180, 148, 237, 212, 180, 211, 78, 171, 5, 128, 108, 84, 177, 62, 137, 254, 45, 1, 53, 25, 22, 252, 38, 4, 46, 50, 145, 253, 195, 87, 200, 65, 162, 192, 2, 63, 247, 23, 127 }, "H72LYEFX2TS6YM7ZZ2M7A7B4XMM4ZQZGL53LEGKPWSKO3VFU2NHKWBMANRKLCPUJ7YWQCNIZC36CMBBOGKI73Q2XZBA2FQACH73RO7Y=" },

			};
		}

		public static IEnumerable<object[]> Base64TestData()
		{
			return new object[][] {
				// bytes, base 64 encoded bytes
				new object[] { BaseNConvert.Base64Alphabet, new byte[] { 122 }, "eg==" },
				new object[] { BaseNConvert.Base64Alphabet, new byte[] { 108, 65 }, "bEE=" },
				new object[] { BaseNConvert.Base64Alphabet, new byte[] { 251, 238, 210 }, "++7S" },
				new object[] { BaseNConvert.Base64Alphabet, new byte[] { 93, 38, 202, 100 }, "XSbKZA==" },
				new object[] { BaseNConvert.Base64Alphabet, new byte[] { 141, 189, 212, 127, 3, 16, 209 }, "jb3UfwMQ0Q==" },
				new object[] { BaseNConvert.Base64Alphabet, new byte[] { 243, 79, 233, 160, 13, 64, 134, 203 }, "80/poA1Ahss=" },
				new object[] { BaseNConvert.Base64Alphabet, new byte[] { 107, 32, 5, 80, 107, 70, 81, 88, 81, 142, 244, 20, 65, 135, 208, 48, 49, 202, 224, 24, 226, 227, 53, 220, 46, 212, 25, 16, 143, 135, 108, 43 }, "ayAFUGtGUVhRjvQUQYfQMDHK4Bji4zXcLtQZEI+HbCs=" },
				new object[] { BaseNConvert.Base64Alphabet, new byte[] { 187, 47, 130, 177, 88, 214, 94, 30, 59, 100, 131, 240, 15, 251, 144, 108, 126, 185, 111, 67, 96, 109, 232, 136, 235, 41, 34, 150, 44, 64, 200, 201, 34, 200, 89, 17, 5, 30, 215, 91, 107, 193, 220, 126, 96, 68, 205, 51, 68, 173, 146, 214, 24, 40, 203, 77, 71, 250, 198, 47, 179, 199, 241, 250 }, "uy+CsVjWXh47ZIPwD/uQbH65b0NgbeiI6ykilixAyMkiyFkRBR7XW2vB3H5gRM0zRK2S1hgoy01H+sYvs8fx+g==" },
			};
		}

		public static IEnumerable<object[]> Base64UrlTestData()
		{
			return new object[][] {
				// bytes, base 64 url encoded bytes
				new object[] { BaseNConvert.Base64UrlAlphabet, new byte[] { 122 }, "eg==" },
				new object[] { BaseNConvert.Base64UrlAlphabet, new byte[] { 108, 65 }, "bEE=" },
				new object[] { BaseNConvert.Base64UrlAlphabet, new byte[] { 251, 238, 210 }, "--7S" },
				new object[] { BaseNConvert.Base64UrlAlphabet, new byte[] { 93, 38, 202, 100 }, "XSbKZA==" },
				new object[] { BaseNConvert.Base64UrlAlphabet, new byte[] { 141, 189, 212, 127, 3, 16, 209 }, "jb3UfwMQ0Q==" },
				new object[] { BaseNConvert.Base64UrlAlphabet, new byte[] { 243, 79, 233, 160, 13, 64, 134, 203 }, "80_poA1Ahss=" },
				new object[] { BaseNConvert.Base64UrlAlphabet, new byte[] { 107, 32, 5, 80, 107, 70, 81, 88, 81, 142, 244, 20, 65, 135, 208, 48, 49, 202, 224, 24, 226, 227, 53, 220, 46, 212, 25, 16, 143, 135, 108, 43 }, "ayAFUGtGUVhRjvQUQYfQMDHK4Bji4zXcLtQZEI-HbCs=" },
				new object[] { BaseNConvert.Base64UrlAlphabet, new byte[] { 187, 47, 130, 177, 88, 214, 94, 30, 59, 100, 131, 240, 15, 251, 144, 108, 126, 185, 111, 67, 96, 109, 232, 136, 235, 41, 34, 150, 44, 64, 200, 201, 34, 200, 89, 17, 5, 30, 215, 91, 107, 193, 220, 126, 96, 68, 205, 51, 68, 173, 146, 214, 24, 40, 203, 77, 71, 250, 198, 47, 179, 199, 241, 250 }, "uy-CsVjWXh47ZIPwD_uQbH65b0NgbeiI6ykilixAyMkiyFkRBR7XW2vB3H5gRM0zRK2S1hgoy01H-sYvs8fx-g==" },
			};
		}

		public static IEnumerable<object[]> Base16UpperTestData()
		{
			return new object[][] {
				// bytes, hex encoded bytes
				new object[] { BaseNConvert.Base16LowerCaseAlphabet, new byte[] { 156 }, "9c" },
				new object[] { BaseNConvert.Base16LowerCaseAlphabet, new byte[] { 185, 206 }, "b9ce" },
				new object[] { BaseNConvert.Base16LowerCaseAlphabet, new byte[] { 70, 161, 126 }, "46a17e" },
				new object[] { BaseNConvert.Base16LowerCaseAlphabet, new byte[] { 102, 95, 225, 20 }, "665fe114" },
				new object[] { BaseNConvert.Base16LowerCaseAlphabet, new byte[] { 215, 158, 144, 106, 40, 167, 91 }, "d79e906a28a75b" },
				new object[] { BaseNConvert.Base16LowerCaseAlphabet, new byte[] { 239, 82, 127, 174, 241, 177, 249, 253 }, "ef527faef1b1f9fd" },
				new object[] { BaseNConvert.Base16LowerCaseAlphabet, new byte[] { 11, 202, 10, 88, 168, 167, 163, 208, 226, 226, 166, 118, 134, 145, 198, 70, 50, 245, 3, 80, 44, 188, 221, 210, 31, 211, 196, 117, 114, 75, 170, 7 }, "0bca0a58a8a7a3d0e2e2a6768691c64632f503502cbcddd21fd3c475724baa07" },
				new object[] { BaseNConvert.Base16LowerCaseAlphabet, new byte[] { 212, 73, 150, 179, 92, 7, 56, 108, 52, 206, 174, 136, 129, 48, 213, 94, 139, 57, 124, 252, 173, 168, 208, 78, 237, 56, 56, 213, 226, 45, 93, 150, 55, 159, 161, 61, 250, 19, 233, 43, 188, 151, 28, 94, 14, 85, 81, 238, 253, 102, 234, 56, 207, 173, 89, 43, 121, 71, 198, 36, 206, 99, 137, 6 }, "d44996b35c07386c34ceae888130d55e8b397cfcada8d04eed3838d5e22d5d96379fa13dfa13e92bbc971c5e0e5551eefd66ea38cfad592b7947c624ce638906" },
			};
		}

		public static IEnumerable<object[]> Base16LowerTestData()
		{
			return new object[][] {
				// bytes, hex encoded bytes
				new object[] { BaseNConvert.Base16UpperCaseAlphabet, new byte[] { 156 }, "9C" },
				new object[] { BaseNConvert.Base16UpperCaseAlphabet, new byte[] { 185, 206 }, "B9CE" },
				new object[] { BaseNConvert.Base16UpperCaseAlphabet, new byte[] { 70, 161, 126 }, "46A17E" },
				new object[] { BaseNConvert.Base16UpperCaseAlphabet, new byte[] { 102, 95, 225, 20 }, "665FE114" },
				new object[] { BaseNConvert.Base16UpperCaseAlphabet, new byte[] { 215, 158, 144, 106, 40, 167, 91 }, "D79E906A28A75B" },
				new object[] { BaseNConvert.Base16UpperCaseAlphabet, new byte[] { 239, 82, 127, 174, 241, 177, 249, 253 }, "EF527FAEF1B1F9FD" },
				new object[] { BaseNConvert.Base16UpperCaseAlphabet, new byte[] { 11, 202, 10, 88, 168, 167, 163, 208, 226, 226, 166, 118, 134, 145, 198, 70, 50, 245, 3, 80, 44, 188, 221, 210, 31, 211, 196, 117, 114, 75, 170, 7 }, "0BCA0A58A8A7A3D0E2E2A6768691C64632F503502CBCDDD21FD3C475724BAA07" },
				new object[] { BaseNConvert.Base16UpperCaseAlphabet, new byte[] { 212, 73, 150, 179, 92, 7, 56, 108, 52, 206, 174, 136, 129, 48, 213, 94, 139, 57, 124, 252, 173, 168, 208, 78, 237, 56, 56, 213, 226, 45, 93, 150, 55, 159, 161, 61, 250, 19, 233, 43, 188, 151, 28, 94, 14, 85, 81, 238, 253, 102, 234, 56, 207, 173, 89, 43, 121, 71, 198, 36, 206, 99, 137, 6 }, "D44996B35C07386C34CEAE888130D55E8B397CFCADA8D04EED3838D5E22D5D96379FA13DFA13E92BBC971C5E0E5551EEFD66EA38CFAD592B7947C624CE638906" },
			};
		}

		public static IEnumerable<object[]> ZBase32TestData()
		{
			return new object[][] {
				// bytes, z-base 32 encoded bytes
				new object[] { BaseNConvert.ZBase32Alphabet, new byte[] { 112 }, "qy" },
				new object[] { BaseNConvert.ZBase32Alphabet, new byte[] { 23, 80 }, "n7ey" },
				new object[] { BaseNConvert.ZBase32Alphabet, new byte[] { 225, 143, 152 }, "hg83o" },
				new object[] { BaseNConvert.ZBase32Alphabet, new byte[] { 99, 120, 79, 81 }, "cphr6we" },
				new object[] { BaseNConvert.ZBase32Alphabet, new byte[] { 63, 22, 140, 113, 63, 152, 230 }, "8hmeahj9uduy" },
				new object[] { BaseNConvert.ZBase32Alphabet, new byte[] { 74, 215, 124, 67, 37, 199, 137, 69 }, "jmmzao3fa6rwk" },
				new object[] { BaseNConvert.ZBase32Alphabet, new byte[] { 117, 196, 178, 76, 197, 92, 89, 13, 22, 237, 247, 4, 111, 95, 101, 3, 113, 232, 126, 110, 66, 159, 7, 8, 46, 61, 132, 139, 13, 194, 75, 79 }, "qznmrugfmtco4fzp6hng6z5fypa6o9uqekxoqnbq8snesdqnjp8o" },
				new object[] { BaseNConvert.ZBase32Alphabet, new byte[] { 198, 155, 25, 211, 176, 43, 137, 244, 230, 9, 118, 131, 72, 147, 119, 234, 113, 121, 210, 60, 72, 27, 2, 213, 189, 170, 30, 136, 216, 206, 255, 83, 162, 167, 61, 211, 69, 146, 82, 69, 95, 137, 135, 57, 41, 131, 191, 154, 198, 153, 25, 148, 240, 14, 6, 171, 153, 67, 22, 5, 13, 1, 28, 24 }, "a4ptuw7ofqr9j3ojq4bwtr5z7jazuwthjypofip7iexetsgq97j4fj374pn3rw1fm6raqqjjoq93itw3dgkxydogiqcwgfofbwytagy" },
			};
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		public void EncodeBytesToCharsTest(BaseNAlphabet alphabet, byte[] plainTextData, string encodedData)
		{
			var input = new ArraySegment<byte>(plainTextData, 0, plainTextData.Length);
			var outputBuffer = new char[encodedData.Length];
			var output = new ArraySegment<char>(outputBuffer, 0, outputBuffer.Length);

			BaseNConvert.Encode(input, output, out var inputUsed, out var outputUsed, alphabet);

			Assert.Equal(inputUsed, input.Count);
			Assert.Equal(outputUsed, output.Count);
			Assert.Equal(encodedData, new string(outputBuffer, 0, output.Count));
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		public void EncodeBytesToBytesTest(BaseNAlphabet alphabet, byte[] plainTextData, string encodedData)
		{
			var input = new ArraySegment<byte>(plainTextData, 0, plainTextData.Length);
			var outputBuffer = new byte[encodedData.Length];
			var output = new ArraySegment<byte>(outputBuffer, 0, outputBuffer.Length);

			BaseNConvert.Encode(input, output, out var inputUsed, out var outputUsed, alphabet);

			Assert.Equal(inputUsed, input.Count);
			Assert.Equal(outputUsed, output.Count);
			Assert.Equal(encodedData, new string(outputBuffer.Select(b => (char)b).ToArray(), 0, output.Count));
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		public unsafe void EncodePtrToPtrTest(BaseNAlphabet alphabet, byte[] plainTextData, string encodedData)
		{
			var outputBuffer = new byte[encodedData.Length];
			var inputUsed = 0;
			var outputUsed = 0;
			fixed (byte* inputPtr = plainTextData)
			fixed (byte* outputPtr = outputBuffer)
				BaseNConvert.Encode(inputPtr, plainTextData.Length, outputPtr, outputBuffer.Length, out inputUsed, out outputUsed, alphabet);

			Assert.Equal(inputUsed, plainTextData.Length);
			Assert.Equal(outputUsed, outputBuffer.Length);
			Assert.Equal(encodedData, new string(outputBuffer.Select(b => (char)b).ToArray(), 0, outputBuffer.Length));
		}

#if NETCOREAPP
		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		public void EncodeSpanToSpanTest(BaseNAlphabet alphabet, byte[] plainTextData, string encodedData)
		{
			var outputBuffer = new byte[encodedData.Length];
			var input = (Span<byte>)plainTextData;
			var output = (Span<byte>)outputBuffer;

			BaseNConvert.Encode(input, output, out var inputUsed, out var outputUsed, alphabet);

			Assert.Equal(inputUsed, plainTextData.Length);
			Assert.Equal(outputUsed, outputBuffer.Length);
			Assert.Equal(encodedData, new string(outputBuffer.Select(b => (char)b).ToArray(), 0, outputBuffer.Length));
		}
#endif

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		public void DecodeCharsToBytesTest(BaseNAlphabet alphabet, byte[] plainTextData, string encodedData)
		{
			var input = new ArraySegment<char>(encodedData.ToCharArray(), 0, encodedData.Length);
			var outputBuffer = new byte[plainTextData.Length];
			var output = new ArraySegment<byte>(outputBuffer, 0, outputBuffer.Length);

			BaseNConvert.Decode(input, output, out var inputUsed, out var outputUsed, alphabet);

			Assert.Equal(inputUsed, input.Count);
			Assert.Equal(outputUsed, output.Count);
			Assert.Equal(plainTextData, outputBuffer);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		public void DecodeBytesToBytesTest(BaseNAlphabet alphabet, byte[] plainTextData, string encodedData)
		{
			var inputBuffer = new byte[encodedData.Length];
			for (var i = 0; i < encodedData.Length; i++)
			{
				inputBuffer[i] = (byte)encodedData[i];
			}
			var input = new ArraySegment<byte>(inputBuffer, 0, inputBuffer.Length);
			var outputBuffer = new byte[plainTextData.Length];
			var output = new ArraySegment<byte>(outputBuffer, 0, outputBuffer.Length);

			BaseNConvert.Decode(input, output, out var inputUsed, out var outputUsed, alphabet);

			Assert.Equal(inputUsed, input.Count);
			Assert.Equal(outputUsed, output.Count);
			Assert.Equal(plainTextData, outputBuffer);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		public unsafe void DecodePtrToPtrTest(BaseNAlphabet alphabet, byte[] plainTextData, string encodedData)
		{
			var inputBuffer = new byte[encodedData.Length];
			for (var i = 0; i < encodedData.Length; i++)
			{
				inputBuffer[i] = (byte)encodedData[i];
			}
			var input = new ArraySegment<byte>(inputBuffer, 0, inputBuffer.Length);
			var outputBuffer = new byte[plainTextData.Length];
			var output = new ArraySegment<byte>(outputBuffer, 0, outputBuffer.Length);
			var inputUsed = 0;
			var outputUsed = 0;

			fixed (byte* inputPtr = inputBuffer)
			fixed (byte* outputPtr = outputBuffer)
				BaseNConvert.Decode(inputPtr, inputBuffer.Length, outputPtr, outputBuffer.Length, out inputUsed, out outputUsed, alphabet);

			Assert.Equal(inputUsed, input.Count);
			Assert.Equal(outputUsed, output.Count);
			Assert.Equal(plainTextData, outputBuffer);
		}

#if NETCOREAPP
		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		public void DecodeSpanToSpanTest(BaseNAlphabet alphabet, byte[] plainTextData, string encodedData)
		{
			var inputBuffer = new byte[encodedData.Length];
			for (var i = 0; i < encodedData.Length; i++)
			{
				inputBuffer[i] = (byte)encodedData[i];
			}
			var input = (Span<byte>)inputBuffer;
			var outputBuffer = new byte[plainTextData.Length];
			var output = (Span<byte>)outputBuffer;

			BaseNConvert.Decode(input, output, out var inputUsed, out var outputUsed, alphabet);

			Assert.Equal(inputUsed, inputBuffer.Length);
			Assert.Equal(outputUsed, outputBuffer.Length);
			Assert.Equal(plainTextData, outputBuffer);
		}
#endif

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		public void ToStringTest(BaseNAlphabet alphabet, byte[] plainTextData, string encodedData)
		{
			var actual = BaseNConvert.ToString(plainTextData, alphabet);

			Assert.Equal(encodedData, actual);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		public void ToStringPartTest(BaseNAlphabet alphabet, byte[] plainTextData, string encodedData)
		{
			var random = new Random(9375220);
			var input = PadData(plainTextData, out var offset, out var extra, random);
			var actual = BaseNConvert.ToString(input, offset, plainTextData.Length, alphabet);

			Assert.Equal(encodedData, actual);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		public void ToCharArrayTest(BaseNAlphabet alphabet, byte[] plainTextData, string encodedData)
		{
			var actual = BaseNConvert.ToCharArray(plainTextData, alphabet);

			Assert.Equal(encodedData, actual);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		public void ToCharArrayPartTest(BaseNAlphabet alphabet, byte[] plainTextData, string encodedData)
		{
			var random = new Random(9375220);
			var input = PadData(plainTextData, out var offset, out var extra, random);
			var actual = BaseNConvert.ToCharArray(input, offset, plainTextData.Length, alphabet);

			Assert.Equal(encodedData, actual);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		public void ToBytesFromStringTest(BaseNAlphabet alphabet, byte[] plainTextData, string encodedData)
		{
			var actual = BaseNConvert.ToBytes(encodedData, alphabet);

			Assert.Equal(plainTextData, actual);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		public void ToBytesFromStringPartTest(BaseNAlphabet alphabet, byte[] plainTextData, string encodedData)
		{
			var random = new Random(9375220);
			var input = PadData(encodedData, out var offset, out var extra, random);
			var actual = BaseNConvert.ToBytes(input, offset, encodedData.Length, alphabet);

			Assert.Equal(plainTextData, actual);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		public void ToBytesFromCharArrayTest(BaseNAlphabet alphabet, byte[] plainTextData, string encodedData)
		{
			var actual = BaseNConvert.ToBytes(encodedData.ToCharArray(), alphabet);

			Assert.Equal(plainTextData, actual);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		public void ToBytesFromCharArrayPartTest(BaseNAlphabet alphabet, byte[] plainTextData, string encodedData)
		{
			var random = new Random(9375220);
			var input = PadData(encodedData, out var offset, out var extra, random);
			var actual = BaseNConvert.ToBytes(input.ToCharArray(), offset, encodedData.Length, alphabet);

			Assert.Equal(plainTextData, actual);
		}

		private static byte[] PadData(byte[] data, out int offset, out int extra, Random random)
		{
			offset = random.Next(1, 100);
			extra = random.Next(1, 100);
			var newData = new byte[data.Length + offset + extra];

			random.NextBytes(newData);

			data.CopyTo(newData, offset);

			return newData;
		}
		private static string PadData(string data, out int offset, out int extra, Random random)
		{
			offset = random.Next(1, 100);
			extra = random.Next(1, 100);
			var newData = new char[data.Length + offset + extra];

			for (var i = 0; i < newData.Length; i++)
			{
				newData[i] = (char)random.Next(1, 64);
			}

			data.CopyTo(0, newData, offset, data.Length);

			return new string(newData, 0, newData.Length);
		}
	}
}
