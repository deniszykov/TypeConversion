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
	using ByteSegment = ArraySegment<byte>;
	using CharSegment = ArraySegment<char>;

	/// <summary>
	/// Utility class for Number/Bytes to Hex transformation.
	/// </summary>
	public static class HexConvert
	{
		private struct StringSegment
		{
			public readonly string Array;
			public readonly int Offset;
			public readonly int Count;

			public StringSegment(string array, int offset, int count)
			{
				if (array == null) throw new ArgumentNullException("array");
				if (count < 0 || count > array.Length) throw new ArgumentOutOfRangeException("count");
				if (offset < 0 || offset + count > array.Length) throw new ArgumentOutOfRangeException("offset");

				this.Array = array;
				this.Offset = offset;
				this.Count = count;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return (this.Array ?? "").Substring(this.Offset, this.Count);
			}
		}

		private static readonly char[] HexChars = "0123456789abcdef".ToCharArray();

		/// <summary>
		/// Set this property to true to use upper case letters in hex encoding methods. Default case is lower.
		/// </summary>
		public static bool UseUppercaseHex { set { for (var i = 0; i < HexChars.Length; i++) HexChars[i] = value ? char.ToLower(HexChars[i]) : char.ToUpper(HexChars[i]); } }

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
				hexString[hexStringIndex] = HexChars[(value >> 4) & 15u];
				hexString[hexStringIndex + 1] = HexChars[value & 15u];
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
			Encode(new ByteSegment(buffer, offset, count), new CharSegment(hexBuffer));
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
			Decode(new CharSegment(hexBuffer, offset, count), new ByteSegment(buffer));
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
			Decode(hexString, offset, count, new ByteSegment(buffer));
			return buffer;
		}

		/// <summary>
		/// Encode <paramref name="inputBuffer"/> bytes and store hex-encoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Bytes to encode.</param>
		/// <param name="outputBuffer">Char array to store encoded bytes. Minimum length is 4.</param>
		/// <returns>Number of characters encoded into <paramref name="outputBuffer"/>.</returns>
		public static int Encode(ByteSegment inputBuffer, CharSegment outputBuffer)
		{
			int inputUsed, outputUsed;
			return EncodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed);
		}
		/// <summary>
		/// Encode <paramref name="inputBuffer"/> bytes and store hex-encoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Bytes to encode.</param>
		/// <param name="outputBuffer">Char array to store encoded bytes. Minimum length is 4.</param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during encoding.</param>
		/// <param name="outputUsed">Number of characters written in <paramref name="outputBuffer"/> during encoding.</param>
		/// <returns>Number of characters encoded into <paramref name="outputBuffer"/>.</returns>
		public static int Encode(ByteSegment inputBuffer, CharSegment outputBuffer, out int inputUsed, out int outputUsed)
		{
			return EncodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed);
		}
		/// <summary>
		/// Encode <paramref name="inputBuffer"/> bytes and store hex-encoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Bytes to encode.</param>
		/// <param name="outputBuffer">Char array to store encoded bytes. Minimum length is 4.</param>
		/// <returns>Number of characters encoded into <paramref name="outputBuffer"/>.</returns>
		public static int Encode(ByteSegment inputBuffer, ByteSegment outputBuffer)
		{
			int inputUsed, outputUsed;
			return EncodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed);
		}
		/// <summary>
		/// Encode <paramref name="inputBuffer"/> bytes and store hex-encoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Bytes to encode.</param>
		/// <param name="outputBuffer">Char array to store encoded bytes. Minimum length is 4.</param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during encoding.</param>
		/// <param name="outputUsed">Number of characters written in <paramref name="outputBuffer"/> during encoding.</param>
		/// <returns>Number of characters encoded into <paramref name="outputBuffer"/>.</returns>
		public static int Encode(ByteSegment inputBuffer, ByteSegment outputBuffer, out int inputUsed, out int outputUsed)
		{
			return EncodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed);
		}

		/// <summary>
		/// Decode hex-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Char array contains hex encoded bytes.</param>
		/// <param name="outputBuffer">Byte array to store decoded bytes from <paramref name="inputBuffer"/>. </param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static int Decode(CharSegment inputBuffer, ByteSegment outputBuffer)
		{
			int inputUsed, outputUsed;
			return DecodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed);
		}
		/// <summary>
		/// Decode hex-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Char array contains hex encoded bytes.</param>
		/// <param name="outputBuffer">Byte array to store decoded bytes from <paramref name="inputBuffer"/>. </param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during decoding.</param>
		/// <param name="outputUsed">Number of bytes written in <paramref name="outputBuffer"/> during decoding.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static int Decode(CharSegment inputBuffer, ByteSegment outputBuffer, out int inputUsed, out int outputUsed)
		{
			return DecodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed);
		}
		/// <summary>
		/// Decode hex-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">String contains hex encoded bytes.</param>
		/// <param name="inputOffset">Decode start index in <paramref name="inputBuffer"/>.</param>
		/// <param name="inputCount">Number of chars to decode in <paramref name="inputBuffer"/>.</param>
		/// <param name="outputBuffer">Byte array to store decoded bytes from <paramref name="inputBuffer"/>.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static int Decode(string inputBuffer, int inputOffset, int inputCount, ByteSegment outputBuffer)
		{
			if (inputBuffer == null) throw new ArgumentNullException("inputBuffer");

			var stringSegment = new StringSegment(inputBuffer, inputOffset, inputCount);
			int inputUsed, outputUsed;
			return DecodeInternal(ref stringSegment, ref outputBuffer, out inputUsed, out outputUsed);
		}
		/// <summary>
		/// Decode hex-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">String contains hex encoded bytes.</param>
		/// <param name="inputOffset">Decode start index in <paramref name="inputBuffer"/>.</param>
		/// <param name="inputCount">Number of chars to decode in <paramref name="inputBuffer"/>.</param>
		/// <param name="outputBuffer">Byte array to store decoded bytes from <paramref name="inputBuffer"/>.</param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during decoding.</param>
		/// <param name="outputUsed">Number of bytes written in <paramref name="outputBuffer"/> during decoding.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static int Decode(string inputBuffer, int inputOffset, int inputCount, ByteSegment outputBuffer, out int inputUsed, out int outputUsed)
		{
			if (inputBuffer == null) throw new ArgumentNullException("inputBuffer");

			var stringSegment = new StringSegment(inputBuffer, inputOffset, inputCount);
			return DecodeInternal(ref stringSegment, ref outputBuffer, out inputUsed, out outputUsed);
		}
		/// <summary>
		/// Decode hex-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Buffer contains hex encoded bytes.</param>
		/// <param name="outputBuffer">Byte array to store decoded bytes from <paramref name="inputBuffer"/>. </param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static int Decode(ByteSegment inputBuffer, ByteSegment outputBuffer)
		{
			int inputUsed, outputUsed;
			return DecodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed);
		}
		/// <summary>
		/// Decode hex-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Buffer contains hex encoded bytes.</param>
		/// <param name="outputBuffer">Byte array to store decoded bytes from <paramref name="inputBuffer"/>. </param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during decoding.</param>
		/// <param name="outputUsed">Number of bytes written in <paramref name="outputBuffer"/> during decoding.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static int Decode(ByteSegment inputBuffer, ByteSegment outputBuffer, out int inputUsed, out int outputUsed)
		{
			return DecodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed);
		}

#if NETCOREAPP2_1
		/// <summary>
		/// Decode hex-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// Only symbols from <paramref name="base64Alphabet"/> is counted. Other symbols are skipped.
		/// </summary>
		/// <param name="inputBuffer">Area of memory which contains hex encoded bytes.</param>
		/// <param name="outputBuffer">Area of memory to store decoded bytes from <paramref name="inputBuffer"/>.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static int Decode(ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer)
		{
			int inputUsed, outputUsed;
			return Decode(inputBuffer, outputBuffer, out inputUsed, out outputUsed);
		}
		/// <summary>
		/// Decode hex-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// Only symbols from <paramref name="base64Alphabet"/> is counted. Other symbols are skipped.
		/// </summary>
		/// <param name="inputBuffer">Area of memory which contains hex encoded bytes.</param>
		/// <param name="outputBuffer">Area of memory to store decoded bytes from <paramref name="inputBuffer"/>.</param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during decoding.</param>
		/// <param name="outputUsed">Number of bytes written in <paramref name="outputBuffer"/> during decoding.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static int Decode(ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer, out int inputUsed, out int outputUsed)
		{
			inputUsed = outputUsed = 0;

			if (inputBuffer.Length == 0)
				return 0;

			var outputOffset = 0;
			var inputOffset = 0;
			var inputEnd = inputBuffer.Length;
			var outputCapacity = outputBuffer.Length;
			inputEnd = inputEnd - (inputEnd % 2); // only read by two letters
			
			for (inputOffset = 0; inputOffset < inputEnd && outputCapacity > 0; inputOffset += 2)
			{
				var hexHalfByte1 = default(uint);
				var hexHalfByte2 = default(uint);

				hexHalfByte1 = ToNumber((char)inputBuffer[inputOffset]);
				hexHalfByte2 = ToNumber((char)inputBuffer[inputOffset + 1]);

				outputBuffer[outputOffset++] = checked((byte)((hexHalfByte1 << 4) | hexHalfByte2));
				outputCapacity--;
			}

			outputUsed = outputOffset;
			inputUsed = inputOffset;

			return outputUsed;
		}
		/// <summary>
		/// Encode <paramref name="inputBuffer"/> bytes and store hex-encoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Bytes to encode.</param>
		/// <param name="outputBuffer">Char array to store encoded bytes. Minimum length is 4.</param>
		/// <returns>Number of characters encoded into <paramref name="outputBuffer"/>.</returns>
		public static int Encode(ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer)
		{
			int inputUsed, outputUsed;
			return Encode(inputBuffer, outputBuffer, out inputUsed, out outputUsed);
		}
		/// <summary>
		/// Encode <paramref name="inputBuffer"/> bytes and store hex-encoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Bytes to encode.</param>
		/// <param name="outputBuffer">Char array to store encoded bytes. Minimum length is 4.</param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during encoding.</param>
		/// <param name="outputUsed">Number of characters written in <paramref name="outputBuffer"/> during encoding.</param>
		/// <returns>Number of characters encoded into <paramref name="outputBuffer"/>.</returns>
		public static int Encode(ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer, out int inputUsed, out int outputUsed)
		{
			inputUsed = outputUsed = 0;

			var inputEnd = inputBuffer.Length;
			var outputCapacity = outputBuffer.Length;
			var outputOffset = 0;
			var hexChars = HexChars;

			int inputOffset;
			for (inputOffset = 0; inputOffset < inputEnd; inputOffset++)
				for (; inputOffset < inputEnd && outputCapacity >= 2; inputOffset++)
				{
					var value = inputBuffer[inputOffset];

					outputBuffer[outputOffset++] = (byte)hexChars[(value >> 4) & 15u];
					outputBuffer[outputOffset++] = (byte)hexChars[value & 15u];
					outputCapacity -= 2;
				}


			inputUsed = inputOffset;
			outputUsed = outputOffset;

			return outputUsed;
		}
#endif

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
				hexBuffer[offset + 0] = HexChars[(value >> 4) & 15u];
				hexBuffer[offset + 1] = HexChars[value & 15u];
				hexBuffer[offset + 2] = HexChars[(value >> 4 * 3) & 15u];
				hexBuffer[offset + 3] = HexChars[(value >> 4 * 2) & 15u];
				hexBuffer[offset + 4] = HexChars[(value >> 4 * 5) & 15u];
				hexBuffer[offset + 5] = HexChars[(value >> 4 * 4) & 15u];
				hexBuffer[offset + 6] = HexChars[(value >> 4 * 7) & 15u];
				hexBuffer[offset + 7] = HexChars[(value >> 4 * 6) & 15u];
				hexBuffer[offset + 8] = HexChars[(value >> 4 * 9) & 15u];
				hexBuffer[offset + 9] = HexChars[(value >> 4 * 8) & 15u];
				hexBuffer[offset + 10] = HexChars[(value >> 4 * 11) & 15u];
				hexBuffer[offset + 11] = HexChars[(value >> 4 * 10) & 15u];
				hexBuffer[offset + 12] = HexChars[(value >> 4 * 13) & 15u];
				hexBuffer[offset + 13] = HexChars[(value >> 4 * 12) & 15u];
				hexBuffer[offset + 14] = HexChars[(value >> 4 * 15) & 15u];
				hexBuffer[offset + 15] = HexChars[(value >> 4 * 14) & 15u];
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
				hexBuffer[offset + 0] = HexChars[(value >> 4) & 15u];
				hexBuffer[offset + 1] = HexChars[value & 15u];
				hexBuffer[offset + 2] = HexChars[(value >> 4 * 3) & 15u];
				hexBuffer[offset + 3] = HexChars[(value >> 4 * 2) & 15u];
				hexBuffer[offset + 4] = HexChars[(value >> 4 * 5) & 15u];
				hexBuffer[offset + 5] = HexChars[(value >> 4 * 4) & 15u];
				hexBuffer[offset + 6] = HexChars[(value >> 4 * 7) & 15u];
				hexBuffer[offset + 7] = HexChars[(value >> 4 * 6) & 15u];
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
				hexBuffer[offset + 0] = HexChars[(value >> 4) & 15u];
				hexBuffer[offset + 1] = HexChars[value & 15u];
				hexBuffer[offset + 2] = HexChars[(value >> 4 * 3) & 15u];
				hexBuffer[offset + 3] = HexChars[(value >> 4 * 2) & 15u];
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
				hexBuffer[offset] = HexChars[(value >> 4) & 15u];
				hexBuffer[offset + 1] = HexChars[value & 15u];
			}

			return MAX_LENGTH;
		}

		private static int EncodeInternal<BufferT>(ref ByteSegment inputBuffer, ref BufferT outputBuffer, out int inputUsed, out int outputUsed)
		{
			inputUsed = outputUsed = 0;
			var hexChars = HexChars;

			if (inputBuffer.Count == 0 || inputBuffer.Array == null)
				return 0;

			var input = inputBuffer.Array;
			var inputEnd = inputBuffer.Offset + inputBuffer.Count;
			var inputOffset = inputBuffer.Offset;
			int outputOffset, originalOutputOffset, outputCapacity;

			if (outputBuffer is ByteSegment)
			{
				var byteSegment = (ByteSegment)(object)outputBuffer;
				outputOffset = originalOutputOffset = byteSegment.Offset;
				outputCapacity = byteSegment.Count;
			}
			else if (outputBuffer is CharSegment)
			{
				var charSegment = (CharSegment)(object)outputBuffer;
				outputOffset = originalOutputOffset = charSegment.Offset;
				outputCapacity = charSegment.Count;
			}
			else
			{
				throw new InvalidOperationException("Unknown type of output buffer: " + typeof(BufferT));
			}

			for (; inputOffset < inputEnd && outputCapacity >= 2; inputOffset++)
			{
				var value = input[inputOffset];

				if (outputBuffer is ByteSegment)
				{
					var outputSegment = (ByteSegment)(object)outputBuffer;
					outputSegment.Array[outputOffset++] = (byte)hexChars[(value >> 4) & 15u];
					outputSegment.Array[outputOffset++] = (byte)hexChars[value & 15u];
				}
				else
				{
					var outputSegment = (CharSegment)(object)outputBuffer;
					outputSegment.Array[outputOffset++] = hexChars[(value >> 4) & 15u];
					outputSegment.Array[outputOffset++] = hexChars[value & 15u];
				}
				outputCapacity -= 2;
			}
			inputUsed = inputOffset - inputBuffer.Offset;
			outputUsed = outputOffset - originalOutputOffset;

			return outputUsed;
		}
		private static int DecodeInternal<BufferT>(ref BufferT inputBuffer, ref ByteSegment outputBuffer, out int inputUsed, out int outputUsed)
		{
			inputUsed = outputUsed = 0;

			if (outputBuffer.Count == 0 || outputBuffer.Array == null)
				return 0;

			var originalInputOffset = 0;
			var inputOffset = 0;
			var inputEnd = 0;
			var outputOffset = outputBuffer.Offset;
			var outputCapacity = outputBuffer.Count;
			var output = outputBuffer.Array;

			if (inputBuffer is ByteSegment)
			{
				var byteSegment = (ByteSegment)(object)inputBuffer;
				if (byteSegment.Count == 0 || byteSegment.Array == null)
					return 0;
				inputOffset = originalInputOffset = byteSegment.Offset;
				inputEnd = byteSegment.Offset + byteSegment.Count;
			}
			else if (inputBuffer is CharSegment)
			{
				var charSegment = (CharSegment)(object)inputBuffer;
				if (charSegment.Count == 0 || charSegment.Array == null)
					return 0;
				inputOffset = originalInputOffset = charSegment.Offset;
				inputEnd = charSegment.Offset + charSegment.Count;
			}
			else if (inputBuffer is StringSegment)
			{
				var stringSegment = (StringSegment)(object)inputBuffer;
				if (stringSegment.Count == 0 || stringSegment.Array == null)
					return 0;
				inputOffset = originalInputOffset = stringSegment.Offset;
				inputEnd = stringSegment.Offset + stringSegment.Count;
			}
			else
			{
				throw new InvalidOperationException("Unknown input buffer type: " + typeof(BufferT));
			}

			inputEnd = inputEnd - (inputEnd % 2); // only read by two letters

			for (; inputOffset < inputEnd && outputCapacity > 0; inputOffset += 2)
			{
				var hexHalfByte1 = default(uint);
				var hexHalfByte2 = default(uint);

				if (inputBuffer is ByteSegment)
				{
					var inputSegment = (ByteSegment)(object)inputBuffer;

					hexHalfByte1 = ToNumber((char)inputSegment.Array[inputOffset]);
					hexHalfByte2 = ToNumber((char)inputSegment.Array[inputOffset + 1]);
				}
				else if (inputBuffer is CharSegment)
				{
					var inputSegment = (CharSegment)(object)inputBuffer;
					hexHalfByte1 = ToNumber(inputSegment.Array[inputOffset]);
					hexHalfByte2 = ToNumber(inputSegment.Array[inputOffset + 1]);
				}
				else
				{
					var inputSegment = (StringSegment)(object)inputBuffer;
					hexHalfByte1 = ToNumber(inputSegment.Array[inputOffset]);
					hexHalfByte2 = ToNumber(inputSegment.Array[inputOffset + 1]);
				}

				output[outputOffset++] = checked((byte)((hexHalfByte1 << 4) | hexHalfByte2));
				outputCapacity--;
			}

			outputUsed = outputOffset - outputBuffer.Offset;
			inputUsed = inputOffset - originalInputOffset;

			return outputUsed;
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
