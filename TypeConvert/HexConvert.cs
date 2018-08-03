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
	/// <summary>
	/// Utility class for Number/Bytes to Hex transformation.
	/// </summary>
	public static class HexConvert
	{
		private static readonly char[] HexChar = "0123456789abcdef".ToCharArray();

		/// <summary>
		/// Set this property to true to use upper case letters in hex encoding methods. Default case is lower.
		/// </summary>
		public static bool UseUppercaseHex { set { for (var i = 0; i < HexChar.Length; i++) HexChar[i] = value ? char.ToLower(HexChar[i]) : char.ToUpper(HexChar[i]); } }

		/// <summary>
		/// Encode byte array to hex string.
		/// </summary>
		/// <param name="buffer">Byte array to encode.</param>
		/// <returns>Hex-encoded string.</returns>
		public static string ToString(byte[] buffer)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");

			return ToString(buffer, 0, buffer.Length);
		}
		/// <summary>
		/// Encode part of byte array to hex string.
		/// </summary>
		/// <param name="buffer">Byte array to encode.</param>
		/// <param name="offset">Encode start index in <paramref name="buffer"/>.</param>
		/// <param name="count">Number of bytes to encode in <paramref name="buffer"/>.</param>
		/// <returns>Hex-encoded string.</returns>
		public static string ToString(byte[] buffer, int offset, int count)
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
		/// <summary>
		/// Encode byte array to hex char array.
		/// </summary>
		/// <param name="buffer">Byte array to encode.</param>
		/// <returns>Hex-encoded char array.</returns>
		public static char[] ToCharArray(byte[] buffer)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");

			return ToCharArray(buffer, 0, buffer.Length);
		}
		/// <summary>
		/// Encode part of byte array to hex char array.
		/// </summary>
		/// <param name="buffer">Byte array to encode.</param>
		/// <param name="offset">Encode start index in <paramref name="buffer"/>.</param>
		/// <param name="count">Number of bytes to encode in <paramref name="buffer"/>.</param>
		/// <returns>Hex-encoded char array.</returns>
		public static char[] ToCharArray(byte[] buffer, int offset, int count)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			if (offset + count > buffer.Length) throw new ArgumentOutOfRangeException("count");

			if (count == 0) return new char[0];

			var hexBuffer = new char[count * 2];
			Encode(buffer, offset, count, hexBuffer, 0);
			return hexBuffer;
		}

		/// <summary>
		/// Decode hex char array into byte array.
		/// </summary>
		/// <param name="hexBuffer">Char array contains hex encoded bytes.</param>
		/// <returns>Decoded bytes.</returns>
		public static byte[] ToBytes(char[] hexBuffer)
		{
			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");

			return ToBytes(hexBuffer, 0, hexBuffer.Length);
		}
		/// <summary>
		/// Decode part of hex char array into byte array.
		/// </summary>
		/// <param name="hexBuffer">Char array contains hex encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="hexBuffer"/>.</param>
		/// <param name="count">Number of chars to decode in <paramref name="hexBuffer"/>.</param>
		/// <returns>Decoded bytes.</returns>
		public static byte[] ToBytes(char[] hexBuffer, int offset, int count)
		{
			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			if (offset + count > hexBuffer.Length) throw new ArgumentOutOfRangeException("count");

			var buffer = new byte[(hexBuffer.Length + 1) / 2];
			Decode(hexBuffer, offset, count, buffer, 0);
			return buffer;
		}
		/// <summary>
		/// Decode hex string into byte array.
		/// </summary>
		/// <param name="hexString">Hex string contains hex encoded bytes.</param>
		/// <returns>Decoded bytes.</returns>
		public static byte[] ToBytes(string hexString)
		{
			if (hexString == null) throw new ArgumentNullException("hexString");

			return ToBytes(hexString, 0, hexString.Length);
		}
		/// <summary>
		/// Decode part of hex string into byte array.
		/// </summary>
		/// <param name="hexString">Hex string contains hex encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="hexString"/>.</param>
		/// <param name="count">Number of chars to decode in <paramref name="hexString"/>.</param>
		/// <returns>Decoded bytes.</returns>
		public static byte[] ToBytes(string hexString, int offset, int count)
		{
			if (hexString == null) throw new ArgumentNullException("hexString");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			if (offset + count > hexString.Length) throw new ArgumentOutOfRangeException("count");

			var buffer = new byte[(hexString.Length + 1) / 2];
			Decode(hexString, offset, count, buffer, 0);
			return buffer;
		}

		/// <summary>
		/// Encode part of <paramref name="buffer"/> and store encoded bytes into specified part of <paramref name="hexBuffer"/>.
		/// </summary>
		/// <param name="buffer">Bytes to encode.</param>
		/// <param name="offset">Encode start index in <paramref name="buffer"/>.</param>
		/// <param name="count">Number of bytes to encode in <paramref name="buffer"/>.</param>
		/// <param name="hexBuffer">Char array to store hex encoded bytes from <paramref name="buffer"/>. Array should fit encoded bytes or exception will be thrown.</param>
		/// <param name="hexBufferOffset">Storage offset in <paramref name="hexBuffer"/>.</param>
		/// <returns>Number of chars written to <paramref name="hexBuffer"/></returns>
		public static int Encode(byte[] buffer, int offset, int count, char[] hexBuffer, int hexBufferOffset)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			if (offset + count > buffer.Length) throw new ArgumentOutOfRangeException("count");
			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (hexBufferOffset < 0) throw new ArgumentOutOfRangeException("hexBufferOffset");
			if (hexBufferOffset + count * 2 > hexBuffer.Length) throw new ArgumentOutOfRangeException("hexBufferOffset");

			if (count == 0)
				return 0;

			var outputOffset = hexBufferOffset;
			var end = offset + count;
			for (var index = offset; index < end; index++)
			{
				var value = buffer[index];
				hexBuffer[outputOffset] = HexChar[(value >> 4) & 15u];
				hexBuffer[outputOffset + 1] = HexChar[value & 15u];
				outputOffset += 2;
			}
			return outputOffset - hexBufferOffset;
		}
		/// <summary>
		/// Decode part of <paramref name="hexBuffer"/> and store decoded bytes into specified part of <paramref name="buffer"/>.
		/// </summary>
		/// <param name="hexBuffer">Char array contains hex encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="hexBuffer"/>.</param>
		/// <param name="count">Number of chars to decode in <paramref name="hexBuffer"/>. Array should fit decoded bytes or exception will be thrown.</param>
		/// <param name="buffer">Byte array to store decoded bytes from <paramref name="hexBuffer"/>. </param>
		/// <param name="bufferOffset">Storage offset in <paramref name="buffer"/>.</param>
		/// <returns>Number of bytes written to <paramref name="buffer"/></returns>
		public static int Decode(char[] hexBuffer, int offset, int count, byte[] buffer, int bufferOffset)
		{
			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + count > hexBuffer.Length) throw new ArgumentOutOfRangeException("offset");
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (bufferOffset < 0) throw new ArgumentOutOfRangeException("bufferOffset");
			if (bufferOffset + (count + 1) / 2 > buffer.Length) throw new ArgumentOutOfRangeException("count");

			if (count == 0)
				return 0;

			var end = offset + count;
			for (; offset < end; offset += 2, bufferOffset++)
				buffer[bufferOffset] = ToUInt8(hexBuffer, offset);

			return (count + 1) / 2;
		}
		/// <summary>
		/// Decode part of <paramref name="hexString"/> and store decoded bytes into specified part of <paramref name="buffer"/>.
		/// </summary>
		/// <param name="hexString">String contains hex encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="hexString"/>.</param>
		/// <param name="count">Number of chars to decode in <paramref name="hexString"/>. Array should fit decoded bytes or exception will be thrown.</param>
		/// <param name="buffer">Byte array to store decoded bytes from <paramref name="hexString"/>. </param>
		/// <param name="bufferOffset">Storage offset in <paramref name="buffer"/>.</param>
		/// <returns>Number of bytes written to <paramref name="buffer"/></returns>
		public static int Decode(string hexString, int offset, int count, byte[] buffer, int bufferOffset)
		{
			if (hexString == null) throw new ArgumentNullException("hexString");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + count > hexString.Length) throw new ArgumentOutOfRangeException("offset");
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (bufferOffset < 0) throw new ArgumentOutOfRangeException("bufferOffset");
			if (bufferOffset + (count + 1) / 2 > buffer.Length) throw new ArgumentOutOfRangeException("count");

			if (count == 0)
				return 0;

			var end = offset + count;
			for (; offset < end; offset += 2, bufferOffset++)
				buffer[bufferOffset] = ToUInt8(hexString, offset);

			return (count + 1) / 2;
		}

		/// <summary>
		/// Decode <see cref="UInt64"/> from hex char array. Minimal required length of <paramref name="hexBuffer"/> is 1. Maximal used length is 16.
		/// </summary>
		/// <param name="hexBuffer">Char array contains hex encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="hexBuffer"/>.</param>
		/// <returns>Decoded number.</returns>
		public static ulong ToUInt64(char[] hexBuffer, int offset)
		{
			const int MAX_LENGTH = 16;

			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + 1 > hexBuffer.Length) throw new ArgumentOutOfRangeException("offset");

			var end = Math.Min(hexBuffer.Length, offset + MAX_LENGTH);
			var result = 0ul;
			for (var i = 0; offset < end; offset++, i++)
			{
				var hexChar = hexBuffer[offset];
				var hexNum = ToNumber(hexChar);

				if (i % 2 == 1)
					result |= hexNum << (i - 1) * 4;
				else
					result |= hexNum << (i + 1) * 4;
			}

			return result;
		}
		/// <summary>
		/// Decode <see cref="UInt64"/> from hex string. Minimal required length of <paramref name="hexString"/> is 1. Maximal used length is 16.
		/// </summary>
		/// <param name="hexString">String contains hex encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="hexString"/>.</param>
		/// <returns>Decoded number.</returns>
		public static ulong ToUInt64(string hexString, int offset)
		{
			const int MAX_LENGTH = 16;

			if (hexString == null) throw new ArgumentNullException("hexString");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + 1 > hexString.Length) throw new ArgumentOutOfRangeException("offset");

			var end = Math.Min(hexString.Length, offset + MAX_LENGTH);
			var result = 0ul;
			for (var i = 0; offset < end; offset++, i++)
			{
				var hexChar = hexString[offset];
				var hexNum = ToNumber(hexChar);

				if (i % 2 == 1)
					result |= hexNum << (i - 1) * 4;
				else
					result |= hexNum << (i + 1) * 4;
			}

			return result;
		}
		/// <summary>
		/// Decode <see cref="UInt32"/> from hex char array. Minimal required length of <paramref name="hexBuffer"/> is 1. Maximal used length is 8.
		/// </summary>
		/// <param name="hexBuffer">Char array contains hex encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="hexBuffer"/>.</param>
		/// <returns>Decoded number.</returns>
		public static uint ToUInt32(char[] hexBuffer, int offset)
		{
			const int MAX_LENGTH = 8;

			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + 1 > hexBuffer.Length) throw new ArgumentOutOfRangeException("offset");

			var end = Math.Min(hexBuffer.Length, offset + MAX_LENGTH);
			var result = 0u;
			for (var i = 0; offset < end; offset++, i++)
			{
				var hexChar = hexBuffer[offset];
				var hexNum = ToNumber(hexChar);

				if (i % 2 == 1)
					result |= hexNum << (i - 1) * 4;
				else
					result |= hexNum << (i + 1) * 4;
			}

			return result;
		}
		/// <summary>
		/// Decode <see cref="UInt32"/> from hex string. Minimal required length of <paramref name="hexString"/> is 1. Maximal used length is 8.
		/// </summary>
		/// <param name="hexString">String contains hex encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="hexString"/>.</param>
		/// <returns>Decoded number.</returns>
		public static uint ToUInt32(string hexString, int offset)
		{
			const int MAX_LENGTH = 8;

			if (hexString == null) throw new ArgumentNullException("hexString");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + 1 > hexString.Length) throw new ArgumentOutOfRangeException("offset");

			var end = Math.Min(hexString.Length, offset + MAX_LENGTH);
			var result = 0u;
			for (var i = 0; offset < end; offset++, i++)
			{
				var hexChar = hexString[offset];
				var hexNum = ToNumber(hexChar);

				if (i % 2 == 1)
					result |= hexNum << (i - 1) * 4;
				else
					result |= hexNum << (i + 1) * 4;
			}


			return result;
		}
		/// <summary>
		/// Decode <see cref="UInt16"/> from hex char array. Minimal required length of <paramref name="hexBuffer"/> is 1. Maximal used length is 4.
		/// </summary>
		/// <param name="hexBuffer">Char array contains hex encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="hexBuffer"/>.</param>
		/// <returns>Decoded number.</returns>
		public static ushort ToUInt16(char[] hexBuffer, int offset)
		{
			const int MAX_LENGTH = 4;

			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + 1 > hexBuffer.Length) throw new ArgumentOutOfRangeException("offset");

			var end = Math.Min(hexBuffer.Length, offset + MAX_LENGTH);
			var result = 0u;
			for (var i = 0; offset < end; offset++, i++)
			{
				var hexChar = hexBuffer[offset];
				var hexNum = ToNumber(hexChar);

				if (i % 2 == 1)
					result |= hexNum << (i - 1) * 4;
				else
					result |= hexNum << (i + 1) * 4;
			}

			return checked((ushort)result);
		}
		/// <summary>
		/// Decode <see cref="UInt16"/> from hex string. Minimal required length of <paramref name="hexString"/> is 1. Maximal used length is 4.
		/// </summary>
		/// <param name="hexString">String contains hex encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="hexString"/>.</param>
		/// <returns>Decoded number.</returns>
		public static ushort ToUInt16(string hexString, int offset)
		{
			const int MAX_LENGTH = 4;

			if (hexString == null) throw new ArgumentNullException("hexString");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + 1 > hexString.Length) throw new ArgumentOutOfRangeException("offset");

			var end = Math.Min(hexString.Length, offset + MAX_LENGTH);
			var result = 0u;
			for (var i = 0; offset < end; offset++, i++)
			{
				var hexChar = hexString[offset];
				var hexNum = ToNumber(hexChar);

				if (i % 2 == 1)
					result |= hexNum << (i - 1) * 4;
				else
					result |= hexNum << (i + 1) * 4;
			}

			return checked((ushort)result);
		}
		/// <summary>
		/// Decode <see cref="Byte"/> from hex char array. Minimal required length of <paramref name="hexBuffer"/> is 1. Maximal used length is 2.
		/// </summary>
		/// <param name="hexBuffer">Char array contains hex encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="hexBuffer"/>.</param>
		/// <returns>Decoded number.</returns>
		public static byte ToUInt8(char[] hexBuffer, int offset)
		{
			const int MAX_LENGTH = 2;

			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + 1 > hexBuffer.Length) throw new ArgumentOutOfRangeException("offset");

			var end = Math.Min(hexBuffer.Length, offset + MAX_LENGTH);
			var result = 0u;
			for (var i = 0; offset < end; offset++, i++)
			{
				var hexChar = hexBuffer[offset];
				var hexNum = ToNumber(hexChar);

				if (i % 2 == 1)
					result |= hexNum << (i - 1) * 4;
				else
					result |= hexNum << (i + 1) * 4;
			}

			return checked((byte)result);
		}
		/// <summary>
		/// Decode <see cref="Byte"/> from hex string. Minimal required length of <paramref name="hexString"/> is 1. Maximal used length is 2.
		/// </summary>
		/// <param name="hexString">String contains hex encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="hexString"/>.</param>
		/// <returns>Decoded number.</returns>
		public static byte ToUInt8(string hexString, int offset)
		{
			const int MAX_LENGTH = 2;

			if (hexString == null) throw new ArgumentNullException("hexString");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + 1 > hexString.Length) throw new ArgumentOutOfRangeException("offset");

			var end = Math.Min(hexString.Length, offset + MAX_LENGTH);
			var result = 0u;
			for (var i = 0; offset < end; offset++, i++)
			{
				var hexChar = hexString[offset];
				var hexNum = ToNumber(hexChar);

				if (i % 2 == 1)
					result |= hexNum << (i - 1) * 4;
				else
					result |= hexNum << (i + 1) * 4;
			}

			return checked((byte)result);
		}

		/// <summary>
		/// Encode number and store it into specified part <paramref name="hexBuffer"/>. Number of character written is 16. 
		/// </summary>
		/// <param name="value">Numeric value to encode.</param>
		/// <param name="hexBuffer">Char array to store encoded number.</param>
		/// <param name="offset">Storage offset for <paramref name="hexBuffer"/>.</param>
		/// <returns>Number of characters written into <paramref name="hexBuffer"/>. Always 16.</returns>
		public static int WriteTo(ulong value, char[] hexBuffer, int offset)
		{
			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");

			const int MAX_LENGTH = 16;

			if (value == 0)
			{
				for (var i = 0; i < MAX_LENGTH; i++)
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

			return MAX_LENGTH;
		}
		/// <summary>
		/// Encode number and store it into specified part <paramref name="hexBuffer"/>. Number of character written is 8. 
		/// </summary>
		/// <param name="value">Numeric value to encode.</param>
		/// <param name="hexBuffer">Char array to store encoded number.</param>
		/// <param name="offset">Storage offset for <paramref name="hexBuffer"/>.</param>
		/// <returns>Number of characters written into <paramref name="hexBuffer"/>. Always 8.</returns>
		public static int WriteTo(uint value, char[] hexBuffer, int offset)
		{
			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");

			const int MAX_LENGTH = 8;

			if (value == 0)
			{
				for (var i = 0; i < MAX_LENGTH; i++)
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

			return MAX_LENGTH;
		}
		/// <summary>
		/// Encode number and store it into specified part <paramref name="hexBuffer"/>. Number of character written is 4. 
		/// </summary>
		/// <param name="value">Numeric value to encode.</param>
		/// <param name="hexBuffer">Char array to store encoded number.</param>
		/// <param name="offset">Storage offset for <paramref name="hexBuffer"/>.</param>
		/// <returns>Number of characters written into <paramref name="hexBuffer"/>. Always 4.</returns>
		public static int WriteTo(ushort value, char[] hexBuffer, int offset)
		{
			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");

			const int MAX_LENGTH = 4;

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

			return MAX_LENGTH;
		}
		/// <summary>
		/// Encode number and store it into specified part <paramref name="hexBuffer"/>. Number of character written is 2. 
		/// </summary>
		/// <param name="value">Numeric value to encode.</param>
		/// <param name="hexBuffer">Char array to store encoded number.</param>
		/// <param name="offset">Storage offset for <paramref name="hexBuffer"/>.</param>
		/// <returns>Number of characters written into <paramref name="hexBuffer"/>. Always 2.</returns>
		public static int WriteTo(byte value, char[] hexBuffer, int offset)
		{
			if (hexBuffer == null) throw new ArgumentNullException("hexBuffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");

			const int MAX_LENGTH = 2;

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

			return MAX_LENGTH;
		}

		private static uint ToNumber(char hexChar)
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
