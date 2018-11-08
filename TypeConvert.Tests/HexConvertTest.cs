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
	public class HexConvertTest
	{
		[Theory]
		[InlineData(1)]
		[InlineData(8)]
		[InlineData(9)]
		[InlineData(255)]
		public void HexToBufferConvertTest(int count)
		{
			var expectedBuffer = new byte[count];
			new Random(count).NextBytes(expectedBuffer);
			var expectedHexString = BitConverter.ToString(expectedBuffer).Replace("-", "").ToLowerInvariant();
			var expectedHexBuffer = expectedHexString.ToCharArray();

			// hex string -> buffer
			var buffer = HexConvert.ToBytes(expectedHexString, 0, expectedHexString.Length);
			Assert.Equal(expectedBuffer, buffer);
			// hex buffer -> buffer
			buffer = HexConvert.ToBytes(expectedHexBuffer, 0, expectedHexBuffer.Length);
			Assert.Equal(expectedBuffer, buffer);

			// hex buffer -> buffer (copy)
			buffer = new byte[expectedBuffer.Length];
			HexConvert.Decode(new ArraySegment<char>(expectedHexBuffer, 0, expectedHexBuffer.Length), new ArraySegment<byte>(buffer));
			Assert.Equal(expectedBuffer, buffer);

			// hex buffer -> buffer (copy)
			buffer = new byte[expectedBuffer.Length];
			HexConvert.Decode(expectedHexString, 0, expectedHexBuffer.Length, new ArraySegment<byte>(buffer));
			Assert.Equal(expectedBuffer, buffer);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(8)]
		[InlineData(9)]
		[InlineData(255)]
		public void BufferToHexConvertTest(int count)
		{
			var expectedBuffer = new byte[count];
			new Random(count).NextBytes(expectedBuffer);
			var expectedHexString = BitConverter.ToString(expectedBuffer).Replace("-", "").ToLowerInvariant();
			var expectedHexBuffer = expectedHexString.ToCharArray();

			// buffer -> hex string
			var hexString = HexConvert.ToString(expectedBuffer, 0, expectedBuffer.Length);
			Assert.Equal(expectedHexString, hexString);
			// buffer -> hex buffer
			var hexBuffer = HexConvert.ToCharArray(expectedBuffer, 0, expectedBuffer.Length);
			Assert.Equal(expectedHexBuffer, hexBuffer);

			// buffer -> hex buffer (copy)
			hexBuffer = new char[expectedHexBuffer.Length];
			HexConvert.Encode(new ArraySegment<byte>(expectedBuffer, 0, expectedBuffer.Length), new ArraySegment<char>(hexBuffer));
			Assert.Equal(expectedHexBuffer, hexBuffer);
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
		public void HexBytesDecodeTest(int count)
		{
			var outputBytes = new byte[count];
			var r = new Random(count);
			r.NextBytes(outputBytes);
			var base64String = BitConverter.ToString(outputBytes).Replace("-", "").ToLowerInvariant();
			var base64Buffer = base64String.ToCharArray().Select(v => (byte)v).ToArray();

			// transform block
			var inputOffset = r.Next(0, 100);
			var inputBuffer = new byte[inputOffset + base64Buffer.Length + inputOffset];
			Buffer.BlockCopy(base64Buffer, 0, inputBuffer, inputOffset, base64Buffer.Length);
			var outputOffset = r.Next(0, 100);
			var outputBuffer = new byte[outputOffset + outputBytes.Length];
			var written = HexConvert.Decode(new ArraySegment<byte>(inputBuffer, inputOffset, base64Buffer.Length), new ArraySegment<byte>(outputBuffer, outputOffset, outputBuffer.Length - outputOffset), out var inputUsed, out var outputUsed);
			var actualOutput = outputBuffer.Skip(outputOffset).ToArray();

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
		public void HexBytesEncodeTest(int count)
		{
			var inputBytes = new byte[count];
			var r = new Random(count);
			r.NextBytes(inputBytes);
			var expectedBase64String = BitConverter.ToString(inputBytes).Replace("-", "").ToLowerInvariant();
			var expectedBase64Buffer = expectedBase64String.ToCharArray().Select(v => (byte)v).ToArray();

			// transform block
			var inputOffset = r.Next(0, 100);
			var inputBuffer = new byte[inputOffset + inputBytes.Length + inputOffset];
			Buffer.BlockCopy(inputBytes, 0, inputBuffer, inputOffset, inputBytes.Length);
			var outputOffset = r.Next(0, 100);
			var outputBuffer = new byte[outputOffset + expectedBase64Buffer.Length];
			var written = HexConvert.Encode(new ArraySegment<byte>(inputBuffer, inputOffset, inputBytes.Length), new ArraySegment<byte>(outputBuffer, outputOffset, outputBuffer.Length - outputOffset), out var inputUsed, out var outputUsed);
			var actualOutput = outputBuffer.Skip(outputOffset).ToArray();

			Assert.Equal(expectedBase64Buffer.Length, written);
			Assert.Equal(expectedBase64Buffer.Length, outputUsed);
			Assert.Equal(inputBytes.Length, inputUsed);
			Assert.Equal(expectedBase64Buffer, actualOutput);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(100)]
		[InlineData(500)]
		[InlineData(100000)]
		public void NumbersToHexConvertTest(int seed)
		{
			var random = new Random(seed);
			var uint8 = unchecked((byte)(random.Next() % byte.MaxValue));
			var uint16 = unchecked((ushort)(random.Next() % ushort.MaxValue));
			var uint32 = unchecked((uint)random.Next());
			var uint64 = unchecked((ulong)random.NextDouble() * long.MaxValue);
			var uint8Hex = BitConverter.ToString(new byte[] { uint8 }).Replace("-", "").ToLowerInvariant();
			var uint16Hex = BitConverter.ToString(BitConverter.GetBytes(uint16)).Replace("-", "").ToLowerInvariant();
			var uint32Hex = BitConverter.ToString(BitConverter.GetBytes(uint32)).Replace("-", "").ToLowerInvariant();
			var uint64Hex = BitConverter.ToString(BitConverter.GetBytes(uint64)).Replace("-", "").ToLowerInvariant();

			var hexBuffer = new char[256];
			// uint8 -> hex
			var count = HexConvert.WriteTo(uint8, hexBuffer, 0);
			var actualHex = new string(hexBuffer, 0, count);
			Assert.Equal(uint8Hex, actualHex);
			Array.Clear(hexBuffer, 0, hexBuffer.Length);

			// uint16 -> hex
			count = HexConvert.WriteTo(uint16, hexBuffer, 0);
			actualHex = new string(hexBuffer, 0, count);
			Assert.Equal(uint16Hex, actualHex);
			Array.Clear(hexBuffer, 0, hexBuffer.Length);

			// uint32 -> hex
			count = HexConvert.WriteTo(uint32, hexBuffer, 0);
			actualHex = new string(hexBuffer, 0, count);
			Assert.Equal(uint32Hex, actualHex);
			Array.Clear(hexBuffer, 0, hexBuffer.Length);

			// uint64 -> hex
			count = HexConvert.WriteTo(uint64, hexBuffer, 0);
			actualHex = new string(hexBuffer, 0, count);
			Assert.Equal(uint64Hex, actualHex);
			Array.Clear(hexBuffer, 0, hexBuffer.Length);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(100)]
		[InlineData(500)]
		[InlineData(100000)]
		public void HexToNumbersConvertTest(int seed)
		{
			var random = new Random(seed);
			var uint8 = unchecked((byte)(random.Next() % byte.MaxValue));
			var uint16 = unchecked((ushort)(random.Next() % ushort.MaxValue));
			var uint32 = unchecked((uint)random.Next());
			var uint64 = unchecked((ulong)random.NextDouble() * long.MaxValue);
			var uint8Hex = BitConverter.ToString(new byte[] { uint8 }).Replace("-", "").ToLowerInvariant();
			var uint16Hex = BitConverter.ToString(BitConverter.GetBytes(uint16)).Replace("-", "").ToLowerInvariant();
			var uint32Hex = BitConverter.ToString(BitConverter.GetBytes(uint32)).Replace("-", "").ToLowerInvariant();
			var uint64Hex = BitConverter.ToString(BitConverter.GetBytes(uint64)).Replace("-", "").ToLowerInvariant();

			var hexBuffer = new char[256];
			// hex -> uint8
			Array.Copy(uint8Hex.ToCharArray(), hexBuffer, uint8Hex.Length);

			var actualUInt8 = HexConvert.ToUInt8(hexBuffer, 0);
			Assert.Equal(uint8, actualUInt8);

			actualUInt8 = HexConvert.ToUInt8(uint8Hex, 0);
			Assert.Equal(uint8, actualUInt8);

			Array.Clear(hexBuffer, 0, hexBuffer.Length);

			// hex -> uint16
			Array.Copy(uint16Hex.ToCharArray(), hexBuffer, uint16Hex.Length);

			var actualUInt16 = HexConvert.ToUInt16(hexBuffer, 0);
			Assert.Equal(uint16, actualUInt16);

			actualUInt16 = HexConvert.ToUInt16(uint16Hex, 0);
			Assert.Equal(uint16, actualUInt16);

			Array.Clear(hexBuffer, 0, hexBuffer.Length);

			// hex -> uint32
			Array.Copy(uint32Hex.ToCharArray(), hexBuffer, uint32Hex.Length);

			var actualUInt32 = HexConvert.ToUInt32(hexBuffer, 0);
			Assert.Equal(uint32, actualUInt32);

			actualUInt32 = HexConvert.ToUInt32(uint32Hex, 0);
			Assert.Equal(uint32, actualUInt32);

			Array.Clear(hexBuffer, 0, hexBuffer.Length);

			// hex -> uint64
			Array.Copy(uint64Hex.ToCharArray(), hexBuffer, uint64Hex.Length);

			var actualUInt64 = HexConvert.ToUInt64(hexBuffer, 0);
			Assert.Equal(uint64, actualUInt64);

			actualUInt64 = HexConvert.ToUInt64(uint64Hex, 0);
			Assert.Equal(uint64, actualUInt64);

			Array.Clear(hexBuffer, 0, hexBuffer.Length);
		}
	}
}
