/*
	Copyright (c) 2016 Denis Zykov
	
	This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 

	License: https://opensource.org/licenses/MIT
*/

using System;
using System.Linq;
using Xunit;

namespace TypeConvert.Tests
{
	public class Base64ConvertTest
	{
		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		[InlineData(4)]
		[InlineData(8)]
		[InlineData(9)]
		[InlineData(255)]
		[InlineData(512)]
		[InlineData(1024)]
		public void Base64ToBufferConvertTest(int count)
		{
			var expectedBuffer = new byte[count];
			var r = new Random(count);
			r.NextBytes(expectedBuffer);
			var expectedBase64String = Convert.ToBase64String(expectedBuffer);
			var expectedBase64Chars = expectedBase64String.ToCharArray();


			var inputOffset = r.Next(0, 100);
			var inputBuffer = new char[inputOffset + expectedBase64Chars.Length + inputOffset];
			expectedBase64Chars.CopyTo(inputBuffer, inputOffset);
			var inputString = new string('a', inputOffset) + expectedBase64String + new string('a', inputOffset);

			// base64 string -> buffer
			var outputBuffer = Base64Convert.ToBytes(inputString, inputOffset, expectedBase64String.Length);
			Assert.Equal(expectedBuffer, outputBuffer);

			// base64 buffer -> buffer
			outputBuffer = Base64Convert.ToBytes(inputBuffer, inputOffset, expectedBase64Chars.Length);
			Assert.Equal(expectedBuffer, outputBuffer);

			// base64 buffer -> buffer (copy)
			outputBuffer = new byte[expectedBuffer.Length];
			Base64Convert.Decode(new ArraySegment<char>(inputBuffer, inputOffset, expectedBase64Chars.Length), new ArraySegment<byte>(outputBuffer));
			Assert.Equal(expectedBuffer, outputBuffer);

			// base64 buffer -> buffer (copy)
			outputBuffer = new byte[expectedBuffer.Length];
			Base64Convert.Decode(inputString, inputOffset, expectedBase64Chars.Length, new ArraySegment<byte>(outputBuffer));
			Assert.Equal(expectedBuffer, outputBuffer);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(8)]
		[InlineData(9)]
		[InlineData(255)]
		public void BufferToBase64ConvertTest(int count)
		{
			var expectedBuffer = new byte[count];
			var r = new Random(count);
			r.NextBytes(expectedBuffer);
			var expectedBase64String = Convert.ToBase64String(expectedBuffer);
			var expectedBase64Chars = expectedBase64String.ToCharArray();

			var inputOffset = r.Next(0, 100);
			var inputBuffer = new byte[inputOffset + expectedBuffer.Length + inputOffset];
			Buffer.BlockCopy(expectedBuffer, 0, inputBuffer, inputOffset, expectedBuffer.Length);

			// buffer -> base64 string
			var base64String = Base64Convert.ToString(inputBuffer, inputOffset, expectedBuffer.Length);
			Assert.Equal(expectedBase64String, base64String);
			// buffer -> base64 buffer
			var base64Chars = Base64Convert.ToCharArray(inputBuffer, inputOffset, expectedBuffer.Length);
			Assert.Equal(expectedBase64Chars, base64Chars);

			// buffer -> base64 buffer (copy)
			base64Chars = new char[expectedBase64Chars.Length];
			Base64Convert.Encode(new ArraySegment<byte>(inputBuffer, inputOffset, expectedBuffer.Length), new ArraySegment<char>(base64Chars));
			Assert.Equal(expectedBase64Chars, base64Chars);
		}


		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		[InlineData(4)]
		[InlineData(8)]
		[InlineData(9)]
		[InlineData(255)]
		[InlineData(512)]
		[InlineData(1024)]
		public void Base64BytesDecodeTest(int count)
		{
			var outputBytes = new byte[count];
			var r = new Random(count);
			r.NextBytes(outputBytes);
			var base64String = Convert.ToBase64String(outputBytes);
			var base64Buffer = base64String.ToCharArray().Select(v => (byte)v).ToArray();

			// transform block
			var inputOffset = r.Next(0, 100);
			var inputBuffer = new byte[inputOffset + base64Buffer.Length + inputOffset];
			Buffer.BlockCopy(base64Buffer, 0, inputBuffer, inputOffset, base64Buffer.Length);
			var outputOffset = r.Next(0, 100);
			var outputBuffer = new byte[outputOffset + outputBytes.Length];
			var written = Base64Convert.Decode(new ArraySegment<byte>(inputBuffer, inputOffset, base64Buffer.Length), new ArraySegment<byte>(outputBuffer, outputOffset, outputBuffer.Length - outputOffset), out var inputUsed, out var outputUsed);
			var actualOutput = outputBuffer.Skip(outputOffset).ToArray();

			Assert.Equal(outputBytes.Length, Base64Convert.GetBytesCount(inputBuffer, inputOffset, base64Buffer.Length));
			Assert.Equal(outputBytes.Length, written);
			Assert.Equal(outputBytes.Length, outputUsed);
			Assert.Equal(base64Buffer.Length, inputUsed);
			Assert.Equal(outputBytes, actualOutput);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		[InlineData(4)]
		[InlineData(8)]
		[InlineData(9)]
		[InlineData(255)]
		[InlineData(512)]
		[InlineData(1024)]
		public void Base64BytesEncodeTest(int count)
		{
			var inputBytes = new byte[count];
			var r = new Random(count);
			r.NextBytes(inputBytes);
			var expectedBase64String = Convert.ToBase64String(inputBytes);
			var expectedBase64Buffer = expectedBase64String.ToCharArray().Select(v => (byte)v).ToArray();

			// transform block
			var inputOffset = r.Next(0, 100);
			var inputBuffer = new byte[inputOffset + inputBytes.Length + inputOffset];
			Buffer.BlockCopy(inputBytes, 0, inputBuffer, inputOffset, inputBytes.Length);
			var outputOffset = r.Next(0, 100);
			var outputBuffer = new byte[outputOffset + expectedBase64Buffer.Length];
			var written = Base64Convert.Encode(new ArraySegment<byte>(inputBuffer, inputOffset, inputBytes.Length), new ArraySegment<byte>(outputBuffer, outputOffset, outputBuffer.Length - outputOffset), out var inputUsed, out var outputUsed);
			var actualOutput = outputBuffer.Skip(outputOffset).ToArray();

			Assert.Equal(expectedBase64Buffer.Length, Base64Convert.GetBase64OutputLength(inputBytes.Length, true));
			Assert.Equal(expectedBase64Buffer.Length, written);
			Assert.Equal(expectedBase64Buffer.Length, outputUsed);
			Assert.Equal(inputBytes.Length, inputUsed);
			Assert.Equal(expectedBase64Buffer, actualOutput);
		}
	}
}
