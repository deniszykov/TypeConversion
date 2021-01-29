/*
	Copyright (c) 2020 Denis Zykov
	
	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 

	License: https://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace deniszykov.BaseN.Tests
{
	public class BaseNEncodingDecodingTest
	{
		public static IEnumerable<object[]> Base32TestData()
		{
			return new object[][] {
				// bytes, base 32 encoded bytes
				new object[] { BaseNEncoding.Base32, new byte[] { 244 }, "6Q======" },
				new object[] { BaseNEncoding.Base32, new byte[] { 48, 235 }, "GDVQ====" },
				new object[] { BaseNEncoding.Base32, new byte[] { 214, 190, 16 }, "227BA===" },
				new object[] { BaseNEncoding.Base32, new byte[] { 88, 13, 19, 170 }, "LAGRHKQ=" },
				new object[] { BaseNEncoding.Base32, new byte[] { 52, 42, 221, 47, 173, 60, 156 }, "GQVN2L5NHSOA====" },
				new object[] { BaseNEncoding.Base32, new byte[] { 204, 219, 194, 32, 11, 120, 172, 225 }, "ZTN4EIALPCWOC===" },
				new object[] { BaseNEncoding.Base32, new byte[] { 2, 0, 160, 56, 46, 190, 0, 49, 15, 149, 189, 191, 96, 16, 193, 245, 93, 228, 25, 53, 47, 232, 176, 236, 163, 64, 234, 76, 93, 192, 233, 183 }, "AIAKAOBOXYADCD4VXW7WAEGB6VO6IGJVF7ULB3FDIDVEYXOA5G3Q====" },
				new object[] { BaseNEncoding.Base32, new byte[] { 63, 244, 188, 16, 183, 212, 229, 236, 51, 249, 206, 153, 240, 124, 60, 187, 25, 204, 195, 38, 95, 118, 178, 25, 79, 180, 148, 237, 212, 180, 211, 78, 171, 5, 128, 108, 84, 177, 62, 137, 254, 45, 1, 53, 25, 22, 252, 38, 4, 46, 50, 145, 253, 195, 87, 200, 65, 162, 192, 2, 63, 247, 23, 127 }, "H72LYEFX2TS6YM7ZZ2M7A7B4XMM4ZQZGL53LEGKPWSKO3VFU2NHKWBMANRKLCPUJ7YWQCNIZC36CMBBOGKI73Q2XZBA2FQACH73RO7Y=" },

			};
		}

		public static IEnumerable<object[]> Base64TestData()
		{
			return new object[][] {
				// bytes, base 64 encoded bytes
				new object[] { BaseNEncoding.Base64, new byte[] { 122 }, "eg==" },
				new object[] { BaseNEncoding.Base64, new byte[] { 108, 65 }, "bEE=" },
				new object[] { BaseNEncoding.Base64, new byte[] { 251, 238, 210 }, "++7S" },
				new object[] { BaseNEncoding.Base64, new byte[] { 93, 38, 202, 100 }, "XSbKZA==" },
				new object[] { BaseNEncoding.Base64, new byte[] { 141, 189, 212, 127, 3, 16, 209 }, "jb3UfwMQ0Q==" },
				new object[] { BaseNEncoding.Base64, new byte[] { 243, 79, 233, 160, 13, 64, 134, 203 }, "80/poA1Ahss=" },
				new object[] { BaseNEncoding.Base64, new byte[] { 107, 32, 5, 80, 107, 70, 81, 88, 81, 142, 244, 20, 65, 135, 208, 48, 49, 202, 224, 24, 226, 227, 53, 220, 46, 212, 25, 16, 143, 135, 108, 43 }, "ayAFUGtGUVhRjvQUQYfQMDHK4Bji4zXcLtQZEI+HbCs=" },
				new object[] { BaseNEncoding.Base64, new byte[] { 187, 47, 130, 177, 88, 214, 94, 30, 59, 100, 131, 240, 15, 251, 144, 108, 126, 185, 111, 67, 96, 109, 232, 136, 235, 41, 34, 150, 44, 64, 200, 201, 34, 200, 89, 17, 5, 30, 215, 91, 107, 193, 220, 126, 96, 68, 205, 51, 68, 173, 146, 214, 24, 40, 203, 77, 71, 250, 198, 47, 179, 199, 241, 250 }, "uy+CsVjWXh47ZIPwD/uQbH65b0NgbeiI6ykilixAyMkiyFkRBR7XW2vB3H5gRM0zRK2S1hgoy01H+sYvs8fx+g==" },
			};
		}

		public static IEnumerable<object[]> Base64UrlTestData()
		{
			return new object[][] {
				// bytes, base 64 url encoded bytes
				new object[] { BaseNEncoding.Base64Url, new byte[] { 122 }, "eg==" },
				new object[] { BaseNEncoding.Base64Url, new byte[] { 108, 65 }, "bEE=" },
				new object[] { BaseNEncoding.Base64Url, new byte[] { 251, 238, 210 }, "--7S" },
				new object[] { BaseNEncoding.Base64Url, new byte[] { 93, 38, 202, 100 }, "XSbKZA==" },
				new object[] { BaseNEncoding.Base64Url, new byte[] { 141, 189, 212, 127, 3, 16, 209 }, "jb3UfwMQ0Q==" },
				new object[] { BaseNEncoding.Base64Url, new byte[] { 243, 79, 233, 160, 13, 64, 134, 203 }, "80_poA1Ahss=" },
				new object[] { BaseNEncoding.Base64Url, new byte[] { 107, 32, 5, 80, 107, 70, 81, 88, 81, 142, 244, 20, 65, 135, 208, 48, 49, 202, 224, 24, 226, 227, 53, 220, 46, 212, 25, 16, 143, 135, 108, 43 }, "ayAFUGtGUVhRjvQUQYfQMDHK4Bji4zXcLtQZEI-HbCs=" },
				new object[] { BaseNEncoding.Base64Url, new byte[] { 187, 47, 130, 177, 88, 214, 94, 30, 59, 100, 131, 240, 15, 251, 144, 108, 126, 185, 111, 67, 96, 109, 232, 136, 235, 41, 34, 150, 44, 64, 200, 201, 34, 200, 89, 17, 5, 30, 215, 91, 107, 193, 220, 126, 96, 68, 205, 51, 68, 173, 146, 214, 24, 40, 203, 77, 71, 250, 198, 47, 179, 199, 241, 250 }, "uy-CsVjWXh47ZIPwD_uQbH65b0NgbeiI6ykilixAyMkiyFkRBR7XW2vB3H5gRM0zRK2S1hgoy01H-sYvs8fx-g==" },
			};
		}

		public static IEnumerable<object[]> Base16UpperTestData()
		{
			return new object[][] {
				// bytes, hex encoded bytes
				new object[] { BaseNEncoding.Base16LowerCase, new byte[] { 156 }, "9c" },
				new object[] { BaseNEncoding.Base16LowerCase, new byte[] { 185, 206 }, "b9ce" },
				new object[] { BaseNEncoding.Base16LowerCase, new byte[] { 70, 161, 126 }, "46a17e" },
				new object[] { BaseNEncoding.Base16LowerCase, new byte[] { 102, 95, 225, 20 }, "665fe114" },
				new object[] { BaseNEncoding.Base16LowerCase, new byte[] { 215, 158, 144, 106, 40, 167, 91 }, "d79e906a28a75b" },
				new object[] { BaseNEncoding.Base16LowerCase, new byte[] { 239, 82, 127, 174, 241, 177, 249, 253 }, "ef527faef1b1f9fd" },
				new object[] { BaseNEncoding.Base16LowerCase, new byte[] { 11, 202, 10, 88, 168, 167, 163, 208, 226, 226, 166, 118, 134, 145, 198, 70, 50, 245, 3, 80, 44, 188, 221, 210, 31, 211, 196, 117, 114, 75, 170, 7 }, "0bca0a58a8a7a3d0e2e2a6768691c64632f503502cbcddd21fd3c475724baa07" },
				new object[] { BaseNEncoding.Base16LowerCase, new byte[] { 212, 73, 150, 179, 92, 7, 56, 108, 52, 206, 174, 136, 129, 48, 213, 94, 139, 57, 124, 252, 173, 168, 208, 78, 237, 56, 56, 213, 226, 45, 93, 150, 55, 159, 161, 61, 250, 19, 233, 43, 188, 151, 28, 94, 14, 85, 81, 238, 253, 102, 234, 56, 207, 173, 89, 43, 121, 71, 198, 36, 206, 99, 137, 6 }, "d44996b35c07386c34ceae888130d55e8b397cfcada8d04eed3838d5e22d5d96379fa13dfa13e92bbc971c5e0e5551eefd66ea38cfad592b7947c624ce638906" },
			};
		}

		public static IEnumerable<object[]> Base16LowerTestData()
		{
			return new object[][] {
				// bytes, hex encoded bytes
				new object[] { BaseNEncoding.Base16UpperCase, new byte[] { 156 }, "9C" },
				new object[] { BaseNEncoding.Base16UpperCase, new byte[] { 185, 206 }, "B9CE" },
				new object[] { BaseNEncoding.Base16UpperCase, new byte[] { 70, 161, 126 }, "46A17E" },
				new object[] { BaseNEncoding.Base16UpperCase, new byte[] { 102, 95, 225, 20 }, "665FE114" },
				new object[] { BaseNEncoding.Base16UpperCase, new byte[] { 215, 158, 144, 106, 40, 167, 91 }, "D79E906A28A75B" },
				new object[] { BaseNEncoding.Base16UpperCase, new byte[] { 239, 82, 127, 174, 241, 177, 249, 253 }, "EF527FAEF1B1F9FD" },
				new object[] { BaseNEncoding.Base16UpperCase, new byte[] { 11, 202, 10, 88, 168, 167, 163, 208, 226, 226, 166, 118, 134, 145, 198, 70, 50, 245, 3, 80, 44, 188, 221, 210, 31, 211, 196, 117, 114, 75, 170, 7 }, "0BCA0A58A8A7A3D0E2E2A6768691C64632F503502CBCDDD21FD3C475724BAA07" },
				new object[] { BaseNEncoding.Base16UpperCase, new byte[] { 212, 73, 150, 179, 92, 7, 56, 108, 52, 206, 174, 136, 129, 48, 213, 94, 139, 57, 124, 252, 173, 168, 208, 78, 237, 56, 56, 213, 226, 45, 93, 150, 55, 159, 161, 61, 250, 19, 233, 43, 188, 151, 28, 94, 14, 85, 81, 238, 253, 102, 234, 56, 207, 173, 89, 43, 121, 71, 198, 36, 206, 99, 137, 6 }, "D44996B35C07386C34CEAE888130D55E8B397CFCADA8D04EED3838D5E22D5D96379FA13DFA13E92BBC971C5E0E5551EEFD66EA38CFAD592B7947C624CE638906" },
			};
		}

		public static IEnumerable<object[]> ZBase32TestData()
		{
			return new object[][] {
				// bytes, z-base 32 encoded bytes
				new object[] { BaseNEncoding.ZBase32, new byte[] { 112 }, "qy" },
				new object[] { BaseNEncoding.ZBase32, new byte[] { 23, 80 }, "n7ey" },
				new object[] { BaseNEncoding.ZBase32, new byte[] { 225, 143, 152 }, "hg83o" },
				new object[] { BaseNEncoding.ZBase32, new byte[] { 99, 120, 79, 81 }, "cphr6we" },
				new object[] { BaseNEncoding.ZBase32, new byte[] { 63, 22, 140, 113, 63, 152, 230 }, "8hmeahj9uduy" },
				new object[] { BaseNEncoding.ZBase32, new byte[] { 74, 215, 124, 67, 37, 199, 137, 69 }, "jmmzao3fa6rwk" },
				new object[] { BaseNEncoding.ZBase32, new byte[] { 117, 196, 178, 76, 197, 92, 89, 13, 22, 237, 247, 4, 111, 95, 101, 3, 113, 232, 126, 110, 66, 159, 7, 8, 46, 61, 132, 139, 13, 194, 75, 79 }, "qznmrugfmtco4fzp6hng6z5fypa6o9uqekxoqnbq8snesdqnjp8o" },
				new object[] { BaseNEncoding.ZBase32, new byte[] { 198, 155, 25, 211, 176, 43, 137, 244, 230, 9, 118, 131, 72, 147, 119, 234, 113, 121, 210, 60, 72, 27, 2, 213, 189, 170, 30, 136, 216, 206, 255, 83, 162, 167, 61, 211, 69, 146, 82, 69, 95, 137, 135, 57, 41, 131, 191, 154, 198, 153, 25, 148, 240, 14, 6, 171, 153, 67, 22, 5, 13, 1, 28, 24 }, "a4ptuw7ofqr9j3ojq4bwtr5z7jazuwthjypofip7iexetsgq97j4fj374pn3rw1fm6raqqjjoq93itw3dgkxydogiqcwgfofbwytagy" },
			};
		}

		public static IEnumerable<object[]> DifferentSizeTestData()
		{
			var random = new Random(1121384721);
			var getRandomData = new Func<int, byte[]>(size =>
			{
				var data = new byte[size];
				random.NextBytes(data);
				return data;
			});
			var sizes = Enumerable.Range(0, 16);
			var encodings = new[] { BaseNEncoding.Base32, BaseNEncoding.Base16LowerCase, BaseNEncoding.Base64 };
			return (
				from size in sizes
				from encoding in encodings
				let plainTextData = getRandomData(size)
				select new object[] { encoding, plainTextData, encoding.GetString(plainTextData) }
			);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		[MemberData(nameof(DifferentSizeTestData))]
		public void EncodeBytesToCharsTest(BaseNEncoding encoding, byte[] plainTextData, string encodedData)
		{
			var input = new ArraySegment<byte>(plainTextData, 0, plainTextData.Length);
			var outputBuffer = new char[encodedData.Length];
			var output = new ArraySegment<char>(outputBuffer, 0, outputBuffer.Length);

			var decoder = (BaseNDecoder)encoding.GetDecoder();
			decoder.Convert(input.Array, input.Offset, input.Count, output.Array, output.Offset, output.Count, true, out var inputUsed, out var outputUsed, out var completed);

			Assert.Equal(input.Count, inputUsed);
			Assert.Equal(output.Count, outputUsed);
			Assert.Equal(encodedData, new string(outputBuffer, 0, output.Count));
			Assert.True(completed);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		[MemberData(nameof(DifferentSizeTestData))]
		public void EncodeBytesToBytesTest(BaseNEncoding encoding, byte[] plainTextData, string encodedData)
		{
			var input = new ArraySegment<byte>(plainTextData, 0, plainTextData.Length);
			var outputBuffer = new byte[encodedData.Length];
			var output = new ArraySegment<byte>(outputBuffer, 0, outputBuffer.Length);

			var decoder = (BaseNDecoder)encoding.GetDecoder();
			decoder.Convert(input.Array, input.Offset, input.Count, output.Array, output.Offset, output.Count, true, out var inputUsed, out var outputUsed, out var completed);

			Assert.Equal(input.Count, inputUsed);
			Assert.Equal(output.Count, outputUsed);
			Assert.Equal(encodedData, new string(outputBuffer.Select(b => (char)b).ToArray(), 0, output.Count));
			Assert.True(completed);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		public unsafe void EncodePtrToPtrTest(BaseNEncoding encoding, byte[] plainTextData, string encodedData)
		{
			var decoder = (BaseNDecoder)encoding.GetDecoder();
			var outputBuffer = new byte[encodedData.Length];
			var inputUsed = 0;
			var outputUsed = 0;
			fixed (byte* inputPtr = plainTextData)
			fixed (byte* outputPtr = outputBuffer)
			{
				decoder.Convert(inputPtr, plainTextData.Length, outputPtr, outputBuffer.Length, true, out inputUsed, out outputUsed, out var completed);
				Assert.True(completed);
			}

			Assert.Equal(plainTextData.Length, inputUsed);
			Assert.Equal(outputBuffer.Length, outputUsed);
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
		[MemberData(nameof(DifferentSizeTestData))]
		public void EncodeSpanToSpanTest(BaseNEncoding encoding, byte[] plainTextData, string encodedData)
		{
			var decoder = (BaseNDecoder)encoding.GetDecoder();
			var outputBuffer = new byte[encodedData.Length];
			var input = (Span<byte>)plainTextData;
			var output = (Span<byte>)outputBuffer;

			decoder.Convert(input, output, true, out var inputUsed, out var outputUsed, out var completed);

			Assert.Equal(plainTextData.Length, inputUsed);
			Assert.Equal(outputBuffer.Length, outputUsed);
			Assert.Equal(encodedData, new string(outputBuffer.Select(b => (char)b).ToArray(), 0, outputBuffer.Length));
			Assert.True(completed);
		}
#endif

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		[MemberData(nameof(DifferentSizeTestData))]
		public void DecodeCharsToBytesTest(BaseNEncoding encoding, byte[] plainTextData, string encodedData)
		{
			var input = new ArraySegment<char>(encodedData.ToCharArray(), 0, encodedData.Length);
			var outputBuffer = new byte[plainTextData.Length];
			var output = new ArraySegment<byte>(outputBuffer, 0, outputBuffer.Length);

			var encoder = (BaseNEncoder)encoding.GetEncoder();
			encoder.Convert(input.Array, input.Offset, input.Count, output.Array, output.Offset, output.Count, true, out var inputUsed, out var outputUsed, out var completed);

			Assert.Equal(input.Count, inputUsed);
			Assert.Equal(output.Count, outputUsed);
			Assert.Equal(plainTextData, outputBuffer);
			Assert.True(completed);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		[MemberData(nameof(DifferentSizeTestData))]
		public void DecodeBytesToBytesTest(BaseNEncoding encoding, byte[] plainTextData, string encodedData)
		{
			var inputBuffer = new byte[encodedData.Length];
			for (var i = 0; i < encodedData.Length; i++)
			{
				inputBuffer[i] = (byte)encodedData[i];
			}
			var input = new ArraySegment<byte>(inputBuffer, 0, inputBuffer.Length);
			var outputBuffer = new byte[plainTextData.Length];
			var output = new ArraySegment<byte>(outputBuffer, 0, outputBuffer.Length);

			var encoder = (BaseNEncoder)encoding.GetEncoder();
			encoder.Convert(input.Array, input.Offset, input.Count, output.Array, output.Offset, output.Count, true, out var inputUsed, out var outputUsed, out var completed);

			Assert.Equal(inputUsed, input.Count);
			Assert.Equal(outputUsed, output.Count);
			Assert.Equal(plainTextData, outputBuffer);
			Assert.True(completed);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		public unsafe void DecodePtrToPtrTest(BaseNEncoding encoding, byte[] plainTextData, string encodedData)
		{
			var encoder = (BaseNEncoder)encoding.GetEncoder();
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
			{
				encoder.Convert(inputPtr, inputBuffer.Length, outputPtr, outputBuffer.Length, true, out inputUsed, out outputUsed, out var completed);
				Assert.True(completed);
			}

			Assert.Equal(input.Count, inputUsed);
			Assert.Equal(output.Count, outputUsed);
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
		[MemberData(nameof(DifferentSizeTestData))]
		public void DecodeSpanToSpanByteTest(BaseNEncoding encoding, byte[] plainTextData, string encodedData)
		{
			var encoder = (BaseNEncoder)encoding.GetEncoder();
			var inputBuffer = new byte[encodedData.Length];
			for (var i = 0; i < encodedData.Length; i++)
			{
				inputBuffer[i] = (byte)encodedData[i];
			}
			var input = (Span<byte>)inputBuffer;
			var outputBuffer = new byte[plainTextData.Length];
			var output = (Span<byte>)outputBuffer;

			encoder.Convert(input, output, true, out var inputUsed, out var outputUsed, out var completed);

			Assert.Equal(inputBuffer.Length, inputUsed);
			Assert.Equal(outputBuffer.Length, outputUsed);
			Assert.Equal(plainTextData, outputBuffer);
			Assert.True(completed);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		[MemberData(nameof(DifferentSizeTestData))]
		public void DecodeSpanToSpanCharTest(BaseNEncoding encoding, byte[] plainTextData, string encodedData)
		{
			var encoder = (BaseNEncoder)encoding.GetEncoder();
			var inputBuffer = new char[encodedData.Length];
			for (var i = 0; i < encodedData.Length; i++)
			{
				inputBuffer[i] = encodedData[i];
			}
			var input = (Span<char>)inputBuffer;
			var outputBuffer = new byte[plainTextData.Length];
			var output = (Span<byte>)outputBuffer;

			encoder.Convert(input, output, true, out var inputUsed, out var outputUsed, out var completed);

			Assert.Equal(inputBuffer.Length, inputUsed);
			Assert.Equal(outputBuffer.Length, outputUsed);
			Assert.Equal(plainTextData, outputBuffer);
			Assert.True(completed);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		[MemberData(nameof(DifferentSizeTestData))]
		public void GetBytesSpanCharsTest(BaseNEncoding encoding, byte[] plainTextData, string encodedData)
		{
			var encoder = (BaseNEncoder)encoding.GetEncoder();
			var inputBuffer = new char[encodedData.Length];
			for (var i = 0; i < encodedData.Length; i++)
			{
				inputBuffer[i] = encodedData[i];
			}
			var input = (Span<char>)inputBuffer;
			var outputBuffer = new byte[plainTextData.Length];
			var output = (Span<byte>)outputBuffer;

			encoder.GetBytes(input, output, true);
			var byteCount = encoder.GetByteCount(input, true);

			Assert.Equal(plainTextData, outputBuffer);
			Assert.Equal(outputBuffer.Length, byteCount);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		[MemberData(nameof(DifferentSizeTestData))]
		public void GetCharsSpanBytesTest(BaseNEncoding encoding, byte[] plainTextData, string encodedData)
		{
			var decoder = (BaseNDecoder)encoding.GetDecoder();
			var outputBuffer = new char[encodedData.Length];
			var input = (Span<byte>)plainTextData;
			var output = (Span<char>)outputBuffer;

			decoder.GetChars(input, output, true);
			var charCount = decoder.GetCharCount(input, true);

			Assert.Equal(encodedData, new string(output));
			Assert.Equal(outputBuffer.Length, charCount);
		}
#endif

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		[MemberData(nameof(DifferentSizeTestData))]
		public void GetStringTest(BaseNEncoding encoding, byte[] plainTextData, string encodedData)
		{
			var actual = encoding.GetString(plainTextData);
			var charCount = encoding.GetCharCount(plainTextData);

			Assert.Equal(encodedData, actual);
			Assert.Equal(charCount, actual.Length);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		[MemberData(nameof(DifferentSizeTestData))]
		public void GetStringPartTest(BaseNEncoding encoding, byte[] plainTextData, string encodedData)
		{
			var random = new Random(9375220);
			var input = PadData(plainTextData, out var offset, out _, random);
			var actual = encoding.GetString(input, offset, plainTextData.Length);
			var charCount = encoding.GetCharCount(input, offset, plainTextData.Length);

			Assert.Equal(encodedData, actual);
			Assert.Equal(charCount, actual.Length);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		[MemberData(nameof(DifferentSizeTestData))]
		public void GetCharsTest(BaseNEncoding encoding, byte[] plainTextData, string encodedData)
		{
			var actual = encoding.GetChars(plainTextData);
			var charCount = encoding.GetCharCount(plainTextData);

			Assert.Equal(encodedData, actual);
			Assert.Equal(charCount, actual.Length);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		[MemberData(nameof(DifferentSizeTestData))]
		public void GetCharsPartTest(BaseNEncoding encoding, byte[] plainTextData, string encodedData)
		{
			var random = new Random(9375220);
			var input = PadData(plainTextData, out var offset, out var extra, random);
			var actual = encoding.GetChars(input, offset, plainTextData.Length);
			var charCount = encoding.GetCharCount(input, offset, plainTextData.Length);

			Assert.Equal(encodedData, actual);
			Assert.Equal(charCount, actual.Length);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		[MemberData(nameof(DifferentSizeTestData))]
		public void GetCharsToBufferTest(BaseNEncoding encoding, byte[] plainTextData, string encodedData)
		{
			var decoder = (BaseNDecoder)encoding.GetDecoder();
			var random = new Random(9375220);
			var input = PadData(plainTextData, out var offset, out var extra, random);
			var output = new char[encodedData.Length + 2];
			var encodedChars = decoder.GetChars(input, offset, plainTextData.Length, output, 1);
			var charCount = decoder.GetCharCount(input, offset, plainTextData.Length, true);

			Assert.Equal(encodedData, new string(output, 1, output.Length - 2));
			Assert.Equal(charCount, encodedChars);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		[MemberData(nameof(DifferentSizeTestData))]
		public void ToBytesFromStringTest(BaseNEncoding encoding, byte[] plainTextData, string encodedData)
		{
			var actual = encoding.GetBytes(encodedData);
#if NETCOREAPP
			var bytesCount = encoding.GetByteCount(encodedData, 0, encodedData.Length);
#else
			var bytesCount = encoding.GetByteCount(encodedData.ToCharArray(), 0, encodedData.Length);
#endif
			Assert.Equal(plainTextData, actual);
			Assert.Equal(bytesCount, actual.Length);
		}

#if !NETFRAMEWORK
		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		[MemberData(nameof(DifferentSizeTestData))]
		public void ToBytesFromStringPartTest(BaseNEncoding encoding, byte[] plainTextData, string encodedData)
		{
			var random = new Random(9375220);
			var input = PadData(encodedData, out var offset, out var extra, random);
			var actual = encoding.GetBytes(input, offset, encodedData.Length);
			var bytesCount = encoding.GetByteCount(input, offset, encodedData.Length);

			Assert.Equal(plainTextData, actual);
			Assert.Equal(bytesCount, actual.Length);
		}
#endif
		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		[MemberData(nameof(DifferentSizeTestData))]
		public void ToBytesFromCharArrayTest(BaseNEncoding encoding, byte[] plainTextData, string encodedData)
		{
			var actual = encoding.GetBytes(encodedData.ToCharArray());
			var bytesCount = encoding.GetByteCount(encodedData.ToCharArray(), 0, encodedData.Length);

			Assert.Equal(plainTextData, actual);
			Assert.Equal(bytesCount, actual.Length);
		}

		[Theory]
		[MemberData(nameof(Base32TestData))]
		[MemberData(nameof(Base64TestData))]
		[MemberData(nameof(Base64UrlTestData))]
		[MemberData(nameof(Base16UpperTestData))]
		[MemberData(nameof(Base16LowerTestData))]
		[MemberData(nameof(ZBase32TestData))]
		[MemberData(nameof(DifferentSizeTestData))]
		public void ToBytesFromCharArrayPartTest(BaseNEncoding encoding, byte[] plainTextData, string encodedData)
		{
			var random = new Random(9375220);
			var input = PadData(encodedData, out var offset, out var extra, random);
			var actual = encoding.GetBytes(input.ToCharArray(), offset, encodedData.Length);
			var bytesCount = encoding.GetByteCount(input.ToCharArray(), offset, encodedData.Length);

			Assert.Equal(plainTextData, actual);
			Assert.Equal(bytesCount, actual.Length);

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
