/*
	Copyright (c) 2016 Denis Zykov
	
	This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 

	License: https://opensource.org/licenses/MIT
*/

using System.Text;

// ReSharper disable once CheckNamespace
namespace System
{
	public class HexConvert
	{
		private static readonly char[] HexChar = "0123456789ABCDEF".ToCharArray();

		public static string BufferToHexString(byte[] buffer)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");

			return BufferToHexString(buffer, 0, buffer.Length);
		}
		public static string BufferToHexString(byte[] buffer, int offset, int count)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			if (offset + count > buffer.Length) throw new ArgumentOutOfRangeException("count");

			if (count == 0) return string.Empty;

			var hexString = new StringBuilder(buffer.Length * 2);
			hexString.Append('0', hexString.Capacity);

			var end = offset + count;
			var hexStringIndex = 0;
			for (var index = offset; index < end; index++)
			{
				var value = buffer[index];
				hexString[hexStringIndex] = HexChar[(value >> 4) & 15u];
				hexString[hexStringIndex + 1] = HexChar[value & 15u];
				hexStringIndex += 2;
			}

			return hexString.ToString();
		}
		public static char[] BufferToHexBuffer(byte[] buffer)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");

			return BufferToHexBuffer(buffer, 0, buffer.Length);
		}
		public static char[] BufferToHexBuffer(byte[] buffer, int offset, int count)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			if (offset + count > buffer.Length) throw new ArgumentOutOfRangeException("count");

			if (count == 0) return new char[0];

			var hexBuffer = new char[count * 2];
			CopyBufferToHexBuffer(buffer, offset, count, hexBuffer, 0);
			return hexBuffer;
		}
		public static byte[] HexBufferToBuffer(char[] hexBuffer)
		{
			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");

			return HexBufferToBuffer(hexBuffer, 0, hexBuffer.Length);
		}
		public static byte[] HexBufferToBuffer(char[] hexBuffer, int offset, int count)
		{
			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			if (offset + count > hexBuffer.Length) throw new ArgumentOutOfRangeException("count");

			var buffer = new byte[(hexBuffer.Length + 1) / 2];
			CopyHexBufferToBuffer(hexBuffer, offset, count, buffer, 0);
			return buffer;
		}
		public static byte[] HexStringToBuffer(string hexString)
		{
			if (hexString == null) throw new ArgumentNullException("hexString");

			return HexStringToBuffer(hexString, 0, hexString.Length);
		}
		public static byte[] HexStringToBuffer(string hexString, int offset, int count)
		{
			if (hexString == null) throw new ArgumentNullException("hexString");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			if (offset + count > hexString.Length) throw new ArgumentOutOfRangeException("count");

			var buffer = new byte[(hexString.Length + 1) / 2];
			var bufferOffset = 0;
			var end = offset + count;
			for (; offset < end; offset += 2, bufferOffset++)
				buffer[bufferOffset] = ToUInt8(hexString, offset);
			return buffer;
		}

		public static void CopyBufferToHexBuffer(byte[] buffer, int offset, int count, char[] hexBuffer, int hexBufferOffset)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			if (offset + count > buffer.Length) throw new ArgumentOutOfRangeException("count");
			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (hexBufferOffset < 0) throw new ArgumentOutOfRangeException("hexBufferOffset");
			if (hexBufferOffset + count * 2 > hexBuffer.Length) throw new ArgumentOutOfRangeException("hexBufferOffset");

			if (count == 0)
				return;

			var end = offset + count;
			for (var index = offset; index < end; index++)
			{
				var value = buffer[index];
				hexBuffer[hexBufferOffset] = HexChar[(value >> 4) & 15u];
				hexBuffer[hexBufferOffset + 1] = HexChar[value & 15u];
				hexBufferOffset += 2;
			}
		}
		public static void CopyHexBufferToBuffer(char[] hexBuffer, int offset, int count, byte[] buffer, int bufferOffset)
		{
			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + count > hexBuffer.Length) throw new ArgumentOutOfRangeException("offset");
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (bufferOffset < 0) throw new ArgumentOutOfRangeException("bufferOffset");
			if (bufferOffset + (count + 1) / 2 > buffer.Length) throw new ArgumentOutOfRangeException("count");

			if (count == 0)
				return;

			var end = offset + count;
			for (; offset < end; offset += 2, bufferOffset++)
				buffer[bufferOffset] = ToUInt8(hexBuffer, offset);
		}

		public static ulong ToUInt64(char[] hexBuffer, int offset)
		{
			const int maxLength = 16;

			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + 1 > hexBuffer.Length) throw new ArgumentOutOfRangeException("offset");

			var end = Math.Min(hexBuffer.Length, offset + maxLength);
			var result = 0ul;
			for (var i = 0; offset < end; offset++, i++)
			{
				var hexChar = hexBuffer[offset];
				var hexNum = ToHexNum(hexChar);

				if (i % 2 == 1)
					result |= hexNum << (i - 1) * 4;
				else
					result |= hexNum << (i + 1) * 4;
			}

			return result;
		}
		public static ulong ToUInt64(string hexString, int offset)
		{
			const int maxLength = 16;

			if (hexString == null) throw new ArgumentNullException("hexString");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + 1 > hexString.Length) throw new ArgumentOutOfRangeException("offset");

			var end = Math.Min(hexString.Length, offset + maxLength);
			var result = 0ul;
			for (var i = 0; offset < end; offset++, i++)
			{
				var hexChar = hexString[offset];
				var hexNum = ToHexNum(hexChar);

				if (i % 2 == 1)
					result |= hexNum << (i - 1) * 4;
				else
					result |= hexNum << (i + 1) * 4;
			}

			return result;
		}
		public static uint ToUInt32(char[] hexBuffer, int offset)
		{
			const int maxLength = 8;

			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + 1 > hexBuffer.Length) throw new ArgumentOutOfRangeException("offset");

			var end = Math.Min(hexBuffer.Length, offset + maxLength);
			var result = 0u;
			for (var i = 0; offset < end; offset++, i++)
			{
				var hexChar = hexBuffer[offset];
				var hexNum = ToHexNum(hexChar);

				if (i % 2 == 1)
					result |= hexNum << (i - 1) * 4;
				else
					result |= hexNum << (i + 1) * 4;
			}

			return result;
		}
		public static uint ToUInt32(string hexString, int offset)
		{
			const int maxLength = 8;

			if (hexString == null) throw new ArgumentNullException("hexString");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + 1 > hexString.Length) throw new ArgumentOutOfRangeException("offset");

			var end = Math.Min(hexString.Length, offset + maxLength);
			var result = 0u;
			for (var i = 0; offset < end; offset++, i++)
			{
				var hexChar = hexString[offset];
				var hexNum = ToHexNum(hexChar);

				if (i % 2 == 1)
					result |= hexNum << (i - 1) * 4;
				else
					result |= hexNum << (i + 1) * 4;
			}


			return result;
		}
		public static ushort ToUInt16(char[] hexBuffer, int offset)
		{
			const int maxLength = 4;

			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + 1 > hexBuffer.Length) throw new ArgumentOutOfRangeException("offset");

			var end = Math.Min(hexBuffer.Length, offset + maxLength);
			var result = 0u;
			for (var i = 0; offset < end; offset++, i++)
			{
				var hexChar = hexBuffer[offset];
				var hexNum = ToHexNum(hexChar);

				if (i % 2 == 1)
					result |= hexNum << (i - 1) * 4;
				else
					result |= hexNum << (i + 1) * 4;
			}

			return checked((ushort)result);
		}
		public static ushort ToUInt16(string hexString, int offset)
		{
			const int maxLength = 4;

			if (hexString == null) throw new ArgumentNullException("hexString");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + 1 > hexString.Length) throw new ArgumentOutOfRangeException("offset");

			var end = Math.Min(hexString.Length, offset + maxLength);
			var result = 0u;
			for (var i = 0; offset < end; offset++, i++)
			{
				var hexChar = hexString[offset];
				var hexNum = ToHexNum(hexChar);

				if (i % 2 == 1)
					result |= hexNum << (i - 1) * 4;
				else
					result |= hexNum << (i + 1) * 4;
			}

			return checked((ushort)result);
		}
		public static byte ToUInt8(char[] hexBuffer, int offset)
		{
			const int maxLength = 2;

			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + 1 > hexBuffer.Length) throw new ArgumentOutOfRangeException("offset");

			var end = Math.Min(hexBuffer.Length, offset + maxLength);
			var result = 0u;
			for (var i = 0; offset < end; offset++, i++)
			{
				var hexChar = hexBuffer[offset];
				var hexNum = ToHexNum(hexChar);

				if (i % 2 == 1)
					result |= hexNum << (i - 1) * 4;
				else
					result |= hexNum << (i + 1) * 4;
			}

			return checked((byte)result);
		}
		public static byte ToUInt8(string hexString, int offset)
		{
			const int maxLength = 2;

			if (hexString == null) throw new ArgumentNullException("hexString");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + 1 > hexString.Length) throw new ArgumentOutOfRangeException("offset");

			var end = Math.Min(hexString.Length, offset + maxLength);
			var result = 0u;
			for (var i = 0; offset < end; offset++, i++)
			{
				var hexChar = hexString[offset];
				var hexNum = ToHexNum(hexChar);

				if (i % 2 == 1)
					result |= hexNum << (i - 1) * 4;
				else
					result |= hexNum << (i + 1) * 4;
			}

			return checked((byte)result);
		}

		public static int ToHex(ulong value, char[] hexBuffer, int offset)
		{
			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");

			const int maxLength = 16;

			if (value == 0)
			{
				for (var i = 0; i < maxLength; i++)
					hexBuffer[i] = '0';
			}
			else
			{
				hexBuffer[offset + 0] = HexChar[(value >> 4) & 15u];
				hexBuffer[offset + 1] = HexChar[value & 15u];
				hexBuffer[offset + 2] = HexChar[(value >> 4 * 3) & 15u];
				hexBuffer[offset + 3] = HexChar[(value >> 4 * 2) & 15u];
				hexBuffer[offset + 4] = HexChar[(value >> 4 * 5) & 15u];
				hexBuffer[offset + 5] = HexChar[(value >> 4 * 4) & 15u];
				hexBuffer[offset + 6] = HexChar[(value >> 4 * 7) & 15u];
				hexBuffer[offset + 7] = HexChar[(value >> 4 * 6) & 15u];
				hexBuffer[offset + 8] = HexChar[(value >> 4 * 9) & 15u];
				hexBuffer[offset + 9] = HexChar[(value >> 4 * 8) & 15u];
				hexBuffer[offset + 10] = HexChar[(value >> 4 * 11) & 15u];
				hexBuffer[offset + 11] = HexChar[(value >> 4 * 10) & 15u];
				hexBuffer[offset + 12] = HexChar[(value >> 4 * 13) & 15u];
				hexBuffer[offset + 13] = HexChar[(value >> 4 * 12) & 15u];
				hexBuffer[offset + 14] = HexChar[(value >> 4 * 15) & 15u];
				hexBuffer[offset + 15] = HexChar[(value >> 4 * 14) & 15u];
			}

			return maxLength;
		}
		public static int ToHex(uint value, char[] hexBuffer, int offset)
		{
			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");

			const int maxLength = 8;

			if (value == 0)
			{
				for (var i = 0; i < maxLength; i++)
					hexBuffer[i] = '0';
			}
			else
			{
				hexBuffer[offset + 0] = HexChar[(value >> 4) & 15u];
				hexBuffer[offset + 1] = HexChar[value & 15u];
				hexBuffer[offset + 2] = HexChar[(value >> 4 * 3) & 15u];
				hexBuffer[offset + 3] = HexChar[(value >> 4 * 2) & 15u];
				hexBuffer[offset + 4] = HexChar[(value >> 4 * 5) & 15u];
				hexBuffer[offset + 5] = HexChar[(value >> 4 * 4) & 15u];
				hexBuffer[offset + 6] = HexChar[(value >> 4 * 7) & 15u];
				hexBuffer[offset + 7] = HexChar[(value >> 4 * 6) & 15u];
			}

			return maxLength;
		}
		public static int ToHex(ushort value, char[] hexBuffer, int offset)
		{
			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");

			const int maxLength = 4;

			if (value == 0)
			{
				hexBuffer[offset + 0] = '0';
				hexBuffer[offset + 1] = '0';
				hexBuffer[offset + 2] = '0';
				hexBuffer[offset + 3] = '0';
			}
			else
			{
				hexBuffer[offset + 0] = HexChar[(value >> 4) & 15u];
				hexBuffer[offset + 1] = HexChar[value & 15u];
				hexBuffer[offset + 2] = HexChar[(value >> 4 * 3) & 15u];
				hexBuffer[offset + 3] = HexChar[(value >> 4 * 2) & 15u];
			}

			return maxLength;
		}
		public static int ToHex(byte value, char[] hexBuffer, int offset)
		{
			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");

			const int maxLength = 2;

			if (value == 0)
			{
				hexBuffer[offset] = '0';
				hexBuffer[offset + 1] = '0';
			}
			else
			{
				hexBuffer[offset] = HexChar[(value >> 4) & 15u];
				hexBuffer[offset + 1] = HexChar[value & 15u];
			}

			return maxLength;
		}

		private static uint ToHexNum(char hexChar)
		{
			const uint ZERO = '0';
			const uint a = 'a';
			const uint A = 'A';

			var hexNum = 0u;
			if (hexChar >= '0' && hexChar <= '9')
				hexNum = hexChar - ZERO;
			else if (hexChar >= 'a' && hexChar <= 'f')
				hexNum = 10u + (hexChar - a);
			else if (hexChar >= 'A' && hexChar <= 'F')
				hexNum = 10u + (hexChar - A);
			else
				throw new FormatException();
			return hexNum;
		}
	}
}
