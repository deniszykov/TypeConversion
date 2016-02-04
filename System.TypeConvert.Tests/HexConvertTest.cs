/*
	Copyright (c) 2016 Denis Zykov
	
	This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 

	License: https://opensource.org/licenses/MIT
*/

using System;
using Xunit;

namespace TypeUtils.Tests
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
			var expectedHexString = BitConverter.ToString(expectedBuffer).Replace("-", "");
			var expectedHexBuffer = expectedHexString.ToCharArray();

			// hex string -> buffer
			var buffer = HexConvert.ToBuffer(expectedHexString, 0, expectedHexString.Length);
			Assert.Equal(expectedBuffer, buffer);
			// hex buffer -> buffer
			buffer = HexConvert.ToBuffer(expectedHexBuffer, 0, expectedHexBuffer.Length);
			Assert.Equal(expectedBuffer, buffer);

			// hex buffer -> buffer (copy)
			buffer = new byte[expectedBuffer.Length];
			HexConvert.CopyHexBufferToBuffer(expectedHexBuffer, 0, expectedHexBuffer.Length, buffer, 0);
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
			var expectedHexString = BitConverter.ToString(expectedBuffer).Replace("-", "");
			var expectedHexBuffer = expectedHexString.ToCharArray();

			// buffer -> hex string
			var hexString = HexConvert.ToHex(expectedBuffer, 0, expectedBuffer.Length);
			Assert.Equal(expectedHexString, hexString);
			// buffer -> hex buffer
			var hexBuffer = HexConvert.ToHexBuffer(expectedBuffer, 0, expectedBuffer.Length);
			Assert.Equal(expectedHexBuffer, hexBuffer);

			// buffer -> hex buffer (copy)
			hexBuffer = new char[expectedHexBuffer.Length];
			HexConvert.CopyBufferToHexBuffer(expectedBuffer, 0, expectedBuffer.Length, hexBuffer, 0);
			Assert.Equal(expectedHexBuffer, hexBuffer);
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
			var uint8Hex = BitConverter.ToString(new byte[] { uint8 }).Replace("-", "");
			var uint16Hex = BitConverter.ToString(BitConverter.GetBytes(uint16)).Replace("-", "");
			var uint32Hex = BitConverter.ToString(BitConverter.GetBytes(uint32)).Replace("-", "");
			var uint64Hex = BitConverter.ToString(BitConverter.GetBytes(uint64)).Replace("-", "");

			var hexBuffer = new char[256];
			// uint8 -> hex
			var count = HexConvert.ToHex(uint8, hexBuffer, 0);
			var actualHex = new string(hexBuffer, 0, count);
			Assert.Equal(uint8Hex, actualHex);
			Array.Clear(hexBuffer, 0, hexBuffer.Length);

			// uint16 -> hex
			count = HexConvert.ToHex(uint16, hexBuffer, 0);
			actualHex = new string(hexBuffer, 0, count);
			Assert.Equal(uint16Hex, actualHex);
			Array.Clear(hexBuffer, 0, hexBuffer.Length);

			// uint32 -> hex
			count = HexConvert.ToHex(uint32, hexBuffer, 0);
			actualHex = new string(hexBuffer, 0, count);
			Assert.Equal(uint32Hex, actualHex);
			Array.Clear(hexBuffer, 0, hexBuffer.Length);

			// uint64 -> hex
			count = HexConvert.ToHex(uint64, hexBuffer, 0);
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
			var uint8Hex = BitConverter.ToString(new byte[] { uint8 }).Replace("-", "");
			var uint16Hex = BitConverter.ToString(BitConverter.GetBytes(uint16)).Replace("-", "");
			var uint32Hex = BitConverter.ToString(BitConverter.GetBytes(uint32)).Replace("-", "");
			var uint64Hex = BitConverter.ToString(BitConverter.GetBytes(uint64)).Replace("-", "");

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
