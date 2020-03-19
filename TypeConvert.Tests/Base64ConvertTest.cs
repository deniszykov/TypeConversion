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
			var random = new Random(count);
			var expectedBuffer = new byte[count];
			random.NextBytes(expectedBuffer);

			var offset = random.Next(0, 10);
			var extra = random.Next(0, 10);
			var expectedBase64String = Convert.ToBase64String(expectedBuffer);
			var base64String = new string('X', offset) + expectedBase64String  + new string('Y', extra);
			var base64Chars = base64String.ToCharArray();

			// base64 string -> buffer
			var outputBuffer = Base64Convert.ToBytes(base64String, offset, expectedBase64String.Length);
			Assert.Equal(expectedBuffer, outputBuffer);

			// base64 buffer -> buffer
			outputBuffer = Base64Convert.ToBytes(base64Chars, offset, expectedBase64String.Length);
			Assert.Equal(expectedBuffer, outputBuffer);

			// base64 buffer -> buffer (copy)
			outputBuffer = new byte[expectedBuffer.Length];
			Base64Convert.Decode(new ArraySegment<char>(base64Chars, offset, expectedBase64String.Length), new ArraySegment<byte>(outputBuffer));
			Assert.Equal(expectedBuffer, outputBuffer);

			// base64 buffer -> buffer (copy)
			outputBuffer = new byte[expectedBuffer.Length];
			Base64Convert.Decode(base64String, offset, expectedBase64String.Length, new ArraySegment<byte>(outputBuffer));
			Assert.Equal(expectedBuffer, outputBuffer);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(8)]
		[InlineData(9)]
		[InlineData(255)]
		public void BufferToBase64ConvertTest(int count)
		{
			var random = new Random(count);
			var offset = random.Next(0, 10);
			var extra = random.Next(0, 10);
			var expectedBuffer = new byte[offset + count + extra];
			random.NextBytes(expectedBuffer);
			var expectedBase64String = Convert.ToBase64String(expectedBuffer, offset, count);
			var expectedBase64Chars = expectedBase64String.ToCharArray();

			// buffer -> base64 string
			var base64String = Base64Convert.ToString(expectedBuffer, offset, count);
			Assert.Equal(expectedBase64String, base64String);
			// buffer -> base64 buffer
			var base64Chars = Base64Convert.ToCharArray(expectedBuffer, offset, count);
			Assert.Equal(expectedBase64Chars, base64Chars);

			// buffer -> base64 buffer (copy)
			base64Chars = new char[expectedBase64Chars.Length];
			Base64Convert.Encode(new ArraySegment<byte>(expectedBuffer, offset, count), new ArraySegment<char>(base64Chars));
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
