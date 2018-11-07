// ReSharper disable once CheckNamespace
namespace System
{
	using ByteSegment = ArraySegment<byte>;
	using CharSegment = ArraySegment<char>;

	/// <summary>
	/// Base64 bytes array to string and vice versa conversion method.
	/// </summary>
	public static class Base64Convert
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

		// ReSharper disable StringLiteralTypo
		/// <summary>
		/// Default Base64 alphabet.
		/// </summary>
		public static readonly Base64Alphabet Base64Alphabet = new Base64Alphabet("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".ToCharArray(), padding: '=');
		/// <summary>
		/// Url-safe Base64 alphabet. Where (+) is replaced with (-) and (/) is replaced with (_).
		/// </summary>
		public static readonly Base64Alphabet Base64UrlAlphabet = new Base64Alphabet("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_".ToCharArray(), padding: '=');
		// ReSharper restore StringLiteralTypo

		/// <summary>
		/// Encode byte array to Base64 string.
		/// </summary>
		/// <param name="buffer">Byte array to encode.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Base64-encoded string.</returns>
		public static string ToString(byte[] buffer, Base64Alphabet base64Alphabet = null)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");

			return ToString(buffer, 0, buffer.Length, base64Alphabet);
		}
		/// <summary>
		/// Encode part of byte array to Base64 string.
		/// </summary>
		/// <param name="buffer">Byte array to encode.</param>
		/// <param name="offset">Encode start index in <paramref name="buffer"/>.</param>
		/// <param name="count">Number of bytes to encode in <paramref name="buffer"/>.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Base64-encoded string.</returns>
		public static string ToString(byte[] buffer, int offset, int count, Base64Alphabet base64Alphabet = null)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			if (offset + count > buffer.Length) throw new ArgumentOutOfRangeException("count");
			if (count >= int.MaxValue / 4 * 3) throw new ArgumentOutOfRangeException("count");

			if (count == 0) return string.Empty;
			if (base64Alphabet == null) base64Alphabet = Base64Alphabet;

			var outputCount = GetBase64OutputLength(count, base64Alphabet.HasPadding);
			var inputBuffer = new ByteSegment(buffer, offset, count);
			var outputBuffer = new CharSegment(new char[outputCount], 0, outputCount);
			int inputUsed, outputUsed;

			EncodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed, base64Alphabet);

			return new string(outputBuffer.Array);
		}
		/// <summary>
		/// Encode byte array to Base64 char array.
		/// </summary>
		/// <param name="buffer">Byte array to encode.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Base64-encoded char array.</returns>
		public static char[] ToCharArray(byte[] buffer, Base64Alphabet base64Alphabet = null)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");

			return ToCharArray(buffer, 0, buffer.Length, base64Alphabet);
		}
		/// <summary>
		/// Encode part of byte array to Base64 char array.
		/// </summary>
		/// <param name="buffer">Byte array to encode.</param>
		/// <param name="offset">Encode start index in <paramref name="buffer"/>.</param>
		/// <param name="count">Number of bytes to encode in <paramref name="buffer"/>.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Base64-encoded char array.</returns>
		public static char[] ToCharArray(byte[] buffer, int offset, int count, Base64Alphabet base64Alphabet = null)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			if (offset + count > buffer.Length) throw new ArgumentOutOfRangeException("count");

			if (count == 0) return new char[0];
			if (base64Alphabet == null) base64Alphabet = Base64Alphabet;

			var outputCount = GetBase64OutputLength(count, base64Alphabet.HasPadding);
			var inputBuffer = new ByteSegment(buffer, offset, count);
			var outputBuffer = new CharSegment(new char[outputCount], 0, outputCount);
			int inputUsed, outputUsed;

			EncodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed, base64Alphabet);

			return outputBuffer.Array;
		}

		/// <summary>
		/// Decode Base64 char array into byte array.
		/// </summary>
		/// <param name="base64Buffer">Char array contains Base64 encoded bytes.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Decoded bytes.</returns>
		public static byte[] ToBytes(char[] base64Buffer, Base64Alphabet base64Alphabet = null)
		{
			if (base64Buffer == null) throw new ArgumentNullException("base64Buffer");

			return ToBytes(base64Buffer, 0, base64Buffer.Length, base64Alphabet);
		}
		/// <summary>
		/// Decode part of Base64 char array into byte array.
		/// </summary>
		/// <param name="base64Buffer">Char array contains Base64 encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="base64Buffer"/>.</param>
		/// <param name="count">Number of chars to decode in <paramref name="base64Buffer"/>.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Decoded bytes.</returns>
		public static byte[] ToBytes(char[] base64Buffer, int offset, int count, Base64Alphabet base64Alphabet = null)
		{
			if (base64Buffer == null) throw new ArgumentNullException("base64Buffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			if (offset + count > base64Buffer.Length) throw new ArgumentOutOfRangeException("count");

			if (count == 0) return new byte[0];

			var outputCount = GetBytesCount(base64Buffer, offset, count, base64Alphabet);
			var inputBuffer = new CharSegment(base64Buffer, offset, count);
			var outputBuffer = new ByteSegment(new byte[outputCount], offset, count);
			int inputUsed, outputUsed;

			DecodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed, base64Alphabet);

			return outputBuffer.Array;
		}
		/// <summary>
		/// Decode Base64 string into byte array.
		/// </summary>
		/// <param name="base64String">Base64 string contains Base64 encoded bytes.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Decoded bytes.</returns>
		public static byte[] ToBytes(string base64String, Base64Alphabet base64Alphabet = null)
		{
			if (base64String == null) throw new ArgumentNullException("base64String");

			return ToBytes(base64String, 0, base64String.Length, base64Alphabet);
		}
		/// <summary>
		/// Decode part of Base64 string into byte array.
		/// </summary>
		/// <param name="base64String">Base64 string contains Base64 encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="base64String"/>.</param>
		/// <param name="count">Number of chars to decode in <paramref name="base64String"/>.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Decoded bytes.</returns>
		public static byte[] ToBytes(string base64String, int offset, int count, Base64Alphabet base64Alphabet = null)
		{
			if (base64String == null) throw new ArgumentNullException("base64String");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			if (offset + count > base64String.Length) throw new ArgumentOutOfRangeException("count");

			if (count == 0) return new byte[0];

			var outputCount = GetBytesCount(base64String, offset, count, base64Alphabet);
			var inputBuffer = new StringSegment(base64String, offset, count);
			var outputBuffer = new ByteSegment(new byte[outputCount], offset, count);
			int inputUsed, outputUsed;

			DecodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed, base64Alphabet);

			return outputBuffer.Array;
		}

		/// <summary>
		/// Encode <paramref name="inputBuffer"/> bytes and store base64-encoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Bytes to encode.</param>
		/// <param name="outputBuffer">Char array to store encoded bytes. Minimum length is 4.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of characters encoded into <paramref name="outputBuffer"/>.</returns>
		public static int Encode(ByteSegment inputBuffer, CharSegment outputBuffer, Base64Alphabet base64Alphabet = null)
		{
			int inputUsed, outputUsed;
			return EncodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed, base64Alphabet);
		}
		/// <summary>
		/// Encode <paramref name="inputBuffer"/> bytes and store base64-encoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Bytes to encode.</param>
		/// <param name="outputBuffer">Char array to store encoded bytes. Minimum length is 4.</param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during encoding.</param>
		/// <param name="outputUsed">Number of characters written in <paramref name="outputBuffer"/> during encoding.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of characters encoded into <paramref name="outputBuffer"/>.</returns>
		public static int Encode(ByteSegment inputBuffer, CharSegment outputBuffer, out int inputUsed, out int outputUsed, Base64Alphabet base64Alphabet = null)
		{
			return EncodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed, base64Alphabet);
		}
		/// <summary>
		/// Encode <paramref name="inputBuffer"/> bytes and store base64-encoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Bytes to encode.</param>
		/// <param name="outputBuffer">Char array to store encoded bytes. Minimum length is 4.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of characters encoded into <paramref name="outputBuffer"/>.</returns>
		public static int Encode(ByteSegment inputBuffer, ByteSegment outputBuffer, Base64Alphabet base64Alphabet = null)
		{
			int inputUsed, outputUsed;
			return EncodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed, base64Alphabet);
		}
		/// <summary>
		/// Encode <paramref name="inputBuffer"/> bytes and store base64-encoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Bytes to encode.</param>
		/// <param name="outputBuffer">Char array to store encoded bytes. Minimum length is 4.</param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during encoding.</param>
		/// <param name="outputUsed">Number of characters written in <paramref name="outputBuffer"/> during encoding.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of characters encoded into <paramref name="outputBuffer"/>.</returns>
		public static int Encode(ByteSegment inputBuffer, ByteSegment outputBuffer, out int inputUsed, out int outputUsed, Base64Alphabet base64Alphabet = null)
		{
			return EncodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed, base64Alphabet);
		}
		/// <summary>
		/// Encode <paramref name="inputBuffer"/> bytes and store base64-encoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Bytes to encode.</param>
		/// <param name="inputCount">Number of bytes to read from <paramref name="inputBuffer"/>.</param>
		/// <param name="outputBuffer">Char array to store encoded bytes. Minimum length is 4.</param>
		/// <param name="outputCount">Max number of bytes to write into <paramref name="outputBuffer"/>.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of characters encoded into <paramref name="outputBuffer"/>.</returns>
		public static unsafe int Encode(byte* inputBuffer, int inputCount, byte* outputBuffer, int outputCount, Base64Alphabet base64Alphabet = null)
		{
			int inputUsed, outputUsed;
			return Encode(inputBuffer, inputCount, outputBuffer, outputCount, out inputUsed, out outputUsed, base64Alphabet);
		}
		/// <summary>
		/// Encode <paramref name="inputBuffer"/> bytes and store base64-encoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Bytes to encode.</param>
		/// <param name="inputCount">Number of bytes to read from <paramref name="inputBuffer"/>.</param>
		/// <param name="outputBuffer">Char array to store encoded bytes. Minimum length is 4.</param>
		/// <param name="outputCount">Max number of bytes to write into <paramref name="outputBuffer"/>.</param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during encoding.</param>
		/// <param name="outputUsed">Number of characters written in <paramref name="outputBuffer"/> during encoding.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of characters encoded into <paramref name="outputBuffer"/>.</returns>
		public static unsafe int Encode(byte* inputBuffer, int inputCount, byte* outputBuffer, int outputCount, out int inputUsed, out int outputUsed, Base64Alphabet base64Alphabet = null)
		{
			inputUsed = outputUsed = 0;
			base64Alphabet = base64Alphabet ?? Base64Alphabet;

			var base64Chars = base64Alphabet.Alphabet;
			var inputEnd = inputBuffer + inputCount;
			var originalOutputAddress = outputBuffer;
			var originalInputAddress = inputBuffer;

			for (; inputBuffer < inputEnd; inputBuffer += 3)
			{
				char first, second, third, forth;
				var charsCount = 0;

				switch (inputEnd - inputBuffer)
				{
					case 2:
						first = base64Chars[(inputBuffer[0] & 0xFC) >> 2];
						second = base64Chars[(inputBuffer[0] & 3) << 4 | (inputBuffer[1] & 0xF0) >> 4];
						third = base64Chars[(inputBuffer[1] & 0xF) << 2];
						charsCount = 3;
						if (base64Alphabet.HasPadding)
						{
							forth = base64Alphabet.Padding;
						}
						else
						{
							forth = '\0';
						}

						if (outputCount < 3)
						{
							goto end;
						}

						break;
					case 1:
						first = base64Chars[(inputBuffer[0] & 0xFC) >> 2];
						second = base64Chars[(inputBuffer[0] & 3) << 4];
						charsCount = 2;
						if (base64Alphabet.HasPadding)
						{
							third = base64Alphabet.Padding;
							forth = base64Alphabet.Padding;
						}
						else
						{
							third = '\0';
							forth = '\0';
						}

						if (outputCount < 2)
						{
							goto end;
						}

						break;
					default:
						first = base64Chars[(inputBuffer[0] & 0xFC) >> 2];
						second = base64Chars[(inputBuffer[0] & 3) << 4 | (inputBuffer[1] & 0xF0) >> 4];
						third = base64Chars[(inputBuffer[1] & 0xF) << 2 | (inputBuffer[2] & 0xC0) >> 6];
						forth = base64Chars[inputBuffer[2] & 0x3F];
						charsCount = 4;

						if (outputCount < 4)
						{
							goto end;
						}

						break;
				}


				outputBuffer[0] = (byte)first;
				outputBuffer[1] = (byte)second;
				if (charsCount > 2)
					outputBuffer[2] = (byte)third;
				if (charsCount > 3)
					outputBuffer[3] = (byte)forth;

				outputBuffer += charsCount;
				outputCount -= charsCount;
			}

			end:
			inputUsed = (int)(inputBuffer - originalInputAddress);
			outputUsed = (int)(outputBuffer - originalOutputAddress);

			return outputUsed;
		}

		/// <summary>
		/// Decode base64-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// Only symbols from <paramref name="base64Alphabet"/> is counted. Other symbols are skipped.
		/// </summary>
		/// <param name="inputBuffer">Char array contains Base64 encoded bytes.</param>
		/// <param name="outputBuffer">Byte array to store decoded bytes from <paramref name="inputBuffer"/>. </param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static int Decode(CharSegment inputBuffer, ByteSegment outputBuffer, Base64Alphabet base64Alphabet = null)
		{
			int inputUsed, outputUsed;
			return DecodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed, base64Alphabet);
		}
		/// <summary>
		/// Decode base64-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// Only symbols from <paramref name="base64Alphabet"/> is counted. Other symbols are skipped.
		/// </summary>
		/// <param name="inputBuffer">Char array contains Base64 encoded bytes.</param>
		/// <param name="outputBuffer">Byte array to store decoded bytes from <paramref name="inputBuffer"/>. </param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during decoding.</param>
		/// <param name="outputUsed">Number of bytes written in <paramref name="outputBuffer"/> during decoding.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static int Decode(CharSegment inputBuffer, ByteSegment outputBuffer, out int inputUsed, out int outputUsed, Base64Alphabet base64Alphabet = null)
		{
			return DecodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed, base64Alphabet);
		}
		/// <summary>
		/// Decode base64-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// Only symbols from <paramref name="base64Alphabet"/> is counted. Other symbols are skipped.
		/// </summary>
		/// <param name="inputBuffer">String contains Base64 encoded bytes.</param>
		/// <param name="inputOffset">Decode start index in <paramref name="inputBuffer"/>.</param>
		/// <param name="inputCount">Number of chars to decode in <paramref name="inputBuffer"/>.</param>
		/// <param name="outputBuffer">Byte array to store decoded bytes from <paramref name="inputBuffer"/>.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static int Decode(string inputBuffer, int inputOffset, int inputCount, ByteSegment outputBuffer, Base64Alphabet base64Alphabet = null)
		{
			if (inputBuffer == null) throw new ArgumentNullException("inputBuffer");

			var stringSegment = new StringSegment(inputBuffer, inputOffset, inputCount);
			int inputUsed, outputUsed;
			return DecodeInternal(ref stringSegment, ref outputBuffer, out inputUsed, out outputUsed, base64Alphabet);
		}
		/// <summary>
		/// Decode base64-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// Only symbols from <paramref name="base64Alphabet"/> is counted. Other symbols are skipped.
		/// </summary>
		/// <param name="inputBuffer">String contains Base64 encoded bytes.</param>
		/// <param name="inputOffset">Decode start index in <paramref name="inputBuffer"/>.</param>
		/// <param name="inputCount">Number of chars to decode in <paramref name="inputBuffer"/>.</param>
		/// <param name="outputBuffer">Byte array to store decoded bytes from <paramref name="inputBuffer"/>.</param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during decoding.</param>
		/// <param name="outputUsed">Number of bytes written in <paramref name="outputBuffer"/> during decoding.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static int Decode(string inputBuffer, int inputOffset, int inputCount, ByteSegment outputBuffer, out int inputUsed, out int outputUsed, Base64Alphabet base64Alphabet = null)
		{
			if (inputBuffer == null) throw new ArgumentNullException("inputBuffer");

			var stringSegment = new StringSegment(inputBuffer, inputOffset, inputCount);
			return DecodeInternal(ref stringSegment, ref outputBuffer, out inputUsed, out outputUsed, base64Alphabet);
		}
		/// <summary>
		/// Decode base64-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// Only symbols from <paramref name="base64Alphabet"/> is counted. Other symbols are skipped.
		/// </summary>
		/// <param name="inputBuffer">Buffer contains Base64 encoded bytes.</param>
		/// <param name="outputBuffer">Byte array to store decoded bytes from <paramref name="inputBuffer"/>. </param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static int Decode(ByteSegment inputBuffer, ByteSegment outputBuffer, Base64Alphabet base64Alphabet = null)
		{
			int inputUsed, outputUsed;
			return DecodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed, base64Alphabet);
		}
		/// <summary>
		/// Decode base64-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// Only symbols from <paramref name="base64Alphabet"/> is counted. Other symbols are skipped.
		/// </summary>
		/// <param name="inputBuffer">Buffer contains Base64 encoded bytes.</param>
		/// <param name="outputBuffer">Byte array to store decoded bytes from <paramref name="inputBuffer"/>. </param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during decoding.</param>
		/// <param name="outputUsed">Number of bytes written in <paramref name="outputBuffer"/> during decoding.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static int Decode(ByteSegment inputBuffer, ByteSegment outputBuffer, out int inputUsed, out int outputUsed, Base64Alphabet base64Alphabet = null)
		{
			return DecodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed, base64Alphabet);
		}
		/// <summary>
		/// Decode base64-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// Only symbols from <paramref name="base64Alphabet"/> is counted. Other symbols are skipped.
		/// </summary>
		/// <param name="inputBuffer">Byte pointer to Base64 encoded bytes.</param>
		/// <param name="inputCount">Number of bytes(chars) to decode in <paramref name="inputBuffer"/>.</param>
		/// <param name="outputBuffer">Byte pointer to place to store decoded bytes from <paramref name="inputBuffer"/>.</param>
		/// <param name="outputCount">Number of bytes available in in <paramref name="outputBuffer"/>.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static unsafe int Decode(byte* inputBuffer, int inputCount, byte* outputBuffer, int outputCount, Base64Alphabet base64Alphabet = null)
		{
			int inputUsed, outputUsed;
			return Decode(inputBuffer, inputCount, outputBuffer, outputCount, out inputUsed, out outputUsed, base64Alphabet);
		}
		/// <summary>
		/// Decode base64-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// Only symbols from <paramref name="base64Alphabet"/> is counted. Other symbols are skipped.
		/// </summary>
		/// <param name="inputBuffer">Byte pointer to Base64 encoded bytes.</param>
		/// <param name="inputCount">Number of bytes(chars) to decode in <paramref name="inputBuffer"/>.</param>
		/// <param name="outputBuffer">Byte pointer to place to store decoded bytes from <paramref name="inputBuffer"/>.</param>
		/// <param name="outputCount">Number of bytes available in in <paramref name="outputBuffer"/>.</param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during decoding.</param>
		/// <param name="outputUsed">Number of bytes written in <paramref name="outputBuffer"/> during decoding.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static unsafe int Decode(byte* inputBuffer, int inputCount, byte* outputBuffer, int outputCount, out int inputUsed, out int outputUsed, Base64Alphabet base64Alphabet = null)
		{
			if (inputBuffer == null) throw new ArgumentNullException("inputBuffer");
			if (outputBuffer == null) throw new ArgumentNullException("outputBuffer");
			if (outputCount < 0) throw new ArgumentOutOfRangeException("outputCount");
			if (inputCount < 0) throw new ArgumentOutOfRangeException("inputCount");

			inputUsed = outputUsed = 0;

			if (inputCount == 0)
				return 0;

			base64Alphabet = base64Alphabet ?? Base64Alphabet;
			var alphabetInverse = base64Alphabet.AlphabetInverse;
			var startingOutputPointer = outputBuffer;
			var startingInputPointer = inputBuffer;
			var inputEnd = inputBuffer + inputCount;
			var outputCapacity = outputBuffer + outputCount;

			for (; inputBuffer < inputEnd; inputBuffer += 4)
			{
				var number = 0u;
				int j;
				for (j = 0; j < 4 && inputBuffer + j < inputEnd; j++)
				{
					uint base64Code = inputBuffer[j];
					uint base64CodeIndex;

					if ((base64Code > 127) || (base64CodeIndex = alphabetInverse[base64Code]) == Base64Alphabet.NOT_IN_ALPHABET)
					{
						inputBuffer++;
						j--;
						continue;
					}
					number = unchecked(number | base64CodeIndex << (18 - 6 * j));
				}

				switch (j)
				{
					case 2:
						if (outputBuffer >= outputCapacity)
							goto default;

						outputBuffer[0] = (byte)((number >> 16) & 255);
						outputBuffer++;
						break;
					case 3:
						if (outputBuffer + 1 >= outputCapacity)
							goto default;

						outputBuffer[0] = (byte)((number >> 16) & 255);
						outputBuffer[1] = (byte)((number >> 8) & 255);
						outputBuffer += 2;
						break;
					case 4:
						if (outputBuffer + 2 >= outputCapacity)
							goto default;

						outputBuffer[0] = (byte)((number >> 16) & 255);
						outputBuffer[1] = (byte)((number >> 8) & 255);
						outputBuffer[2] = (byte)((number >> 0) & 255);
						outputBuffer += 3;
						break;
					default:
						inputBuffer -= j;
						goto end;
				}
			}

			end:
			outputUsed = (int)(outputBuffer - startingOutputPointer);
			inputUsed = (int)(inputBuffer - startingInputPointer);

			return outputUsed;
		}
#if NETCOREAPP2_1
		/// <summary>
		/// Decode base64-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// Only symbols from <paramref name="base64Alphabet"/> is counted. Other symbols are skipped.
		/// </summary>
		/// <param name="inputBuffer">Area of memory which contains Base64 encoded bytes.</param>
		/// <param name="outputBuffer">Area of memory to store decoded bytes from <paramref name="inputBuffer"/>.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static int Decode(ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer, Base64Alphabet base64Alphabet = null)
		{
			int inputUsed, outputUsed;
			return Decode(inputBuffer, outputBuffer, out inputUsed, out outputUsed, base64Alphabet);
		}
		/// <summary>
		/// Decode base64-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// Only symbols from <paramref name="base64Alphabet"/> is counted. Other symbols are skipped.
		/// </summary>
		/// <param name="inputBuffer">Area of memory which contains Base64 encoded bytes.</param>
		/// <param name="outputBuffer">Area of memory to store decoded bytes from <paramref name="inputBuffer"/>.</param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during decoding.</param>
		/// <param name="outputUsed">Number of bytes written in <paramref name="outputBuffer"/> during decoding.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static int Decode(ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer, out int inputUsed, out int outputUsed, Base64Alphabet base64Alphabet = null)
		{
			inputUsed = outputUsed = 0;

			if (inputBuffer.Length == 0)
				return 0;

			base64Alphabet = base64Alphabet ?? Base64Alphabet;
			var alphabetInverse = base64Alphabet.AlphabetInverse;
			var outputOffset = 0;
			var inputOffset = 0;
			var inputEnd = inputBuffer.Length;

			for (inputOffset = 0; inputOffset < inputEnd; inputOffset += 4)
			{
				var number = 0u;
				int j;
				for (j = 0; j < 4 && inputOffset + j < inputEnd; j++)
				{
					uint base64Code = inputBuffer[inputOffset + j];
					uint base64CodeIndex;

					if ((base64Code > 127) || (base64CodeIndex = alphabetInverse[base64Code]) == Base64Alphabet.NOT_IN_ALPHABET)
					{
						inputOffset++;
						j--;
						continue;
					}
					number = (uint)unchecked(number | base64CodeIndex << (18 - 6 * j));
				}

				switch (j)
				{
					case 2:
						if (outputOffset >= outputBuffer.Length)
							goto default;

						outputBuffer[outputOffset++] = (byte)((number >> 16) & 255);
						break;
					case 3:
						if (outputOffset + 1 >= outputBuffer.Length)
							goto default;

						outputBuffer[outputOffset++] = (byte)((number >> 16) & 255);
						outputBuffer[outputOffset++] = (byte)((number >> 8) & 255);
						break;
					case 4:
						if (outputOffset + 2 >= outputBuffer.Length)
							goto default;

						outputBuffer[outputOffset++] = (byte)((number >> 16) & 255);
						outputBuffer[outputOffset++] = (byte)((number >> 8) & 255);
						outputBuffer[outputOffset++] = (byte)((number >> 0) & 255);
						break;
					default:
						inputOffset -= j;
						goto end;
				}
			}

			end:
			outputUsed = outputOffset;
			inputUsed = inputOffset;

			return outputUsed;
		}
		/// <summary>
		/// Encode <paramref name="inputBuffer"/> bytes and store base64-encoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Bytes to encode.</param>
		/// <param name="outputBuffer">Char array to store encoded bytes. Minimum length is 4.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of characters encoded into <paramref name="outputBuffer"/>.</returns>
		public static int Encode(ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer, Base64Alphabet base64Alphabet = null)
		{
			int inputUsed, outputUsed;
			return Encode(inputBuffer, outputBuffer, out inputUsed, out outputUsed, base64Alphabet);
		}
		/// <summary>
		/// Encode <paramref name="inputBuffer"/> bytes and store base64-encoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Bytes to encode.</param>
		/// <param name="outputBuffer">Char array to store encoded bytes. Minimum length is 4.</param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during encoding.</param>
		/// <param name="outputUsed">Number of characters written in <paramref name="outputBuffer"/> during encoding.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of characters encoded into <paramref name="outputBuffer"/>.</returns>
		public static int Encode(ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer, out int inputUsed, out int outputUsed, Base64Alphabet base64Alphabet = null)
		{
			inputUsed = outputUsed = 0;
			base64Alphabet = base64Alphabet ?? Base64Alphabet;

			var base64Chars = base64Alphabet.Alphabet;
			var inputEnd = inputBuffer.Length;
			var outputCapacity = outputBuffer.Length;
			var outputOffset = 0;

			int inputOffset;
			for (inputOffset = 0; inputOffset < inputEnd; inputOffset += 3)
			{
				char first, second, third, forth;
				var charsCount = 0;

				switch (inputEnd - inputOffset)
				{
					case 2:
						first = base64Chars[(inputBuffer[inputOffset] & 0xFC) >> 2];
						second = base64Chars[(inputBuffer[inputOffset] & 3) << 4 | (inputBuffer[inputOffset + 1] & 0xF0) >> 4];
						third = base64Chars[(inputBuffer[inputOffset + 1] & 0xF) << 2];
						if (base64Alphabet.HasPadding)
						{
							forth = base64Alphabet.Padding;
							charsCount = 4;
						}
						else
						{
							forth = '\0';
							charsCount = 3;
						}

						if (outputCapacity < 3)
						{
							goto end;
						}

						break;
					case 1:
						first = base64Chars[(inputBuffer[inputOffset] & 0xFC) >> 2];
						second = base64Chars[(inputBuffer[inputOffset] & 3) << 4];
						if (base64Alphabet.HasPadding)
						{
							third = base64Alphabet.Padding;
							forth = base64Alphabet.Padding;
							charsCount = 4;
						}
						else
						{
							charsCount = 2;
							third = '\0';
							forth = '\0';
						}

						if (outputCapacity < 2)
						{
							goto end;
						}

						break;
					default:
						first = base64Chars[(inputBuffer[inputOffset] & 0xFC) >> 2];
						second = base64Chars[(inputBuffer[inputOffset] & 3) << 4 | (inputBuffer[inputOffset + 1] & 0xF0) >> 4];
						third = base64Chars[(inputBuffer[inputOffset + 1] & 0xF) << 2 | (inputBuffer[inputOffset + 2] & 0xC0) >> 6];
						forth = base64Chars[inputBuffer[inputOffset + 2] & 0x3F];
						charsCount = 4;

						if (outputCapacity < 4)
						{
							goto end;
						}

						break;
				}


				outputBuffer[outputOffset++] = (byte)first;
				outputBuffer[outputOffset++] = (byte)second;
				if (charsCount > 2)
					outputBuffer[outputOffset++] = (byte)third;
				if (charsCount > 3)
					outputBuffer[outputOffset++] = (byte)forth;

				outputCapacity -= charsCount;
			}

			end:
			inputUsed = inputOffset;
			outputUsed = outputOffset;

			return outputUsed;
		}
#endif

		/// <summary>
		/// Calculate size of base64 output based on input's size.
		/// </summary>
		/// <param name="bytesCount">Length of input buffer in bytes.</param>
		/// <param name="withPadding">Flag indicating what output will be padded.</param>
		/// <returns>Length of base64 output in bytes/letters.</returns>
		public static int GetBase64OutputLength(int bytesCount, bool withPadding)
		{
			if (bytesCount < 0) throw new ArgumentOutOfRangeException("bytesCount");

			if (bytesCount == 0)
				return 0;

			if (withPadding)
			{
				return checked((bytesCount / 3 * 4) + ((bytesCount % 3 != 0) ? 4 : 0));
			}
			else
			{
				return checked((bytesCount / 3 * 4) + (bytesCount % 3) + 1);
			}
		}
		/// <summary>
		/// Get number of bytes encoded in passed <paramref name="base64Chars"/>. Only symbols from <paramref name="base64Alphabet"/> is counted. Other symbols are skipped.
		/// </summary>
		/// <param name="base64Chars">Input data encoded by <paramref name="base64Alphabet"/>.</param>
		/// <param name="offset">Offset in input data.</param>
		/// <param name="count">Length of input data.</param>
		/// <param name="base64Alphabet">Alphabet for base64 encoding. Can be null. Default is <see cref="Base64Alphabet"/>.</param>
		/// <returns>Number of bytes encoded in <paramref name="base64Chars"/>.</returns>
		public static int GetBytesCount(char[] base64Chars, int offset, int count, Base64Alphabet base64Alphabet = null)
		{
			if (base64Chars == null) throw new ArgumentNullException("base64Chars");
			if (offset < 0 || offset >= base64Chars.Length) throw new ArgumentOutOfRangeException("offset");
			if (count < 0 || offset + count > base64Chars.Length) throw new ArgumentOutOfRangeException("count");

			var byteSegment = new CharSegment(base64Chars, offset, count);
			return GetBytesCountInternal(ref byteSegment, base64Alphabet);
		}
		/// <summary>
		/// Get number of bytes encoded in passed <paramref name="base64String"/>. Only symbols from <paramref name="base64Alphabet"/> is counted. Other symbols are skipped.
		/// </summary>
		/// <param name="base64String">Input data encoded by <paramref name="base64Alphabet"/>.</param>
		/// <param name="offset">Offset in input data.</param>
		/// <param name="count">Length of input data.</param>
		/// <param name="base64Alphabet">Alphabet for base64 encoding. Can be null. Default is <see cref="Base64Alphabet"/>.</param>
		/// <returns>Number of bytes encoded in <paramref name="base64String"/>.</returns>
		public static int GetBytesCount(string base64String, int offset, int count, Base64Alphabet base64Alphabet = null)
		{
			if (base64String == null) throw new ArgumentNullException("base64String");
			if (offset < 0 || offset >= base64String.Length) throw new ArgumentOutOfRangeException("offset");
			if (count < 0 || offset + count > base64String.Length) throw new ArgumentOutOfRangeException("count");

			var byteSegment = new StringSegment(base64String, offset, count);
			return GetBytesCountInternal(ref byteSegment, base64Alphabet);
		}
		/// <summary>
		/// Get number of bytes encoded in passed <paramref name="base64Bytes"/>. Only symbols from <paramref name="base64Alphabet"/> is counted. Other symbols are skipped.
		/// </summary>
		/// <param name="base64Bytes">Input data encoded by <paramref name="base64Alphabet"/>.</param>
		/// <param name="offset">Offset in input data.</param>
		/// <param name="count">Length of input data.</param>
		/// <param name="base64Alphabet">Alphabet for base64 encoding. Can be null. Default is <see cref="Base64Alphabet"/>.</param>
		/// <returns>Number of bytes encoded in <paramref name="base64Bytes"/>.</returns>
		public static int GetBytesCount(byte[] base64Bytes, int offset, int count, Base64Alphabet base64Alphabet = null)
		{
			if (base64Bytes == null) throw new ArgumentNullException("base64Bytes");
			if (offset < 0 || offset >= base64Bytes.Length) throw new ArgumentOutOfRangeException("offset");
			if (count < 0 || offset + count > base64Bytes.Length) throw new ArgumentOutOfRangeException("count");

			var byteSegment = new ByteSegment(base64Bytes, offset, count);
			return GetBytesCountInternal(ref byteSegment, base64Alphabet);
		}

		private static int EncodeInternal<BufferT>(ref ByteSegment inputBuffer, ref BufferT outputBuffer, out int inputUsed, out int outputUsed, Base64Alphabet base64Alphabet)
		{
			inputUsed = outputUsed = 0;
			base64Alphabet = base64Alphabet ?? Base64Alphabet;

			if (inputBuffer.Count == 0 || inputBuffer.Array == null)
				return 0;

			var base64Chars = base64Alphabet.Alphabet;
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

			for (; inputOffset < inputEnd; inputOffset += 3)
			{
				char first, second, third, forth;
				var charsCount = 0;

				switch (inputEnd - inputOffset)
				{
					case 2:
						first = base64Chars[(input[inputOffset] & 0xFC) >> 2];
						second = base64Chars[(input[inputOffset] & 3) << 4 | (input[inputOffset + 1] & 0xF0) >> 4];
						third = base64Chars[(input[inputOffset + 1] & 0xF) << 2];
						if (base64Alphabet.HasPadding)
						{
							forth = base64Alphabet.Padding;
							charsCount = 4;
						}
						else
						{
							forth = '\0';
							charsCount = 3;
						}

						if (outputCapacity < 3)
						{
							goto end;
						}

						break;
					case 1:
						first = base64Chars[(input[inputOffset] & 0xFC) >> 2];
						second = base64Chars[(input[inputOffset] & 3) << 4];
						if (base64Alphabet.HasPadding)
						{
							third = base64Alphabet.Padding;
							forth = base64Alphabet.Padding;
							charsCount = 4;
						}
						else
						{
							charsCount = 2;
							third = '\0';
							forth = '\0';
						}

						if (outputCapacity < 2)
						{
							goto end;
						}

						break;
					default:
						first = base64Chars[(input[inputOffset] & 0xFC) >> 2];
						second = base64Chars[(input[inputOffset] & 3) << 4 | (input[inputOffset + 1] & 0xF0) >> 4];
						third = base64Chars[(input[inputOffset + 1] & 0xF) << 2 | (input[inputOffset + 2] & 0xC0) >> 6];
						forth = base64Chars[input[inputOffset + 2] & 0x3F];
						charsCount = 4;

						if (outputCapacity < 4)
						{
							goto end;
						}

						break;
				}

				if (outputBuffer is ByteSegment)
				{
					var byteSegment = (ByteSegment)(object)outputBuffer;
					byteSegment.Array[outputOffset++] = (byte)first;
					byteSegment.Array[outputOffset++] = (byte)second;
					if (charsCount > 2)
						byteSegment.Array[outputOffset++] = (byte)third;
					if (charsCount > 3)
						byteSegment.Array[outputOffset++] = (byte)forth;
				}
				else
				{
					var charSegment = (CharSegment)(object)outputBuffer;
					charSegment.Array[outputOffset++] = first;
					charSegment.Array[outputOffset++] = second;
					if (charsCount > 2)
						charSegment.Array[outputOffset++] = third;
					if (charsCount > 3)
						charSegment.Array[outputOffset++] = forth;
				}

				outputCapacity -= charsCount;
			}

			end:
			inputUsed = inputOffset - inputBuffer.Offset;
			outputUsed = outputOffset - originalOutputOffset;

			return outputUsed;
		}
		private static int DecodeInternal<BufferT>(ref BufferT inputBuffer, ref ByteSegment outputBuffer, out int inputUsed, out int outputUsed, Base64Alphabet base64Alphabet)
		{
			inputUsed = outputUsed = 0;
			base64Alphabet = base64Alphabet ?? Base64Alphabet;

			if (outputBuffer.Count == 0 || outputBuffer.Array == null)
				return 0;

			var originalInputOffset = 0;
			var inputOffset = 0;
			var inputEnd = 0;
			var outputOffset = outputBuffer.Offset;
			var outputCapacity = outputBuffer.Offset + outputBuffer.Count;
			var output = outputBuffer.Array;
			var alphabetInverse = base64Alphabet.AlphabetInverse;

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

			for (; inputOffset < inputEnd; inputOffset += 4)
			{
				var number = 0u;
				int j;
				for (j = 0; j < 4 && inputOffset + j < inputEnd; j++)
				{
					uint base64Code;
					uint base64CodeIndex;

					if (inputBuffer is ByteSegment)
					{
						var byteSegment = (ByteSegment)(object)inputBuffer;
						base64Code = byteSegment.Array[inputOffset + j];
					}
					else if (inputBuffer is CharSegment)
					{
						var charSegment = (CharSegment)(object)inputBuffer;
						base64Code = charSegment.Array[inputOffset + j];
					}
					else
					{
						var stringSegment = (StringSegment)(object)inputBuffer;
						base64Code = stringSegment.Array[inputOffset + j];
					}

					if ((base64Code > 127) || (base64CodeIndex = alphabetInverse[base64Code]) == Base64Alphabet.NOT_IN_ALPHABET)
					{
						inputOffset++;
						j--;
						continue;
					}
					number = unchecked(number | base64CodeIndex << (18 - 6 * j));
				}

				switch (j)
				{
					case 2:
						if (outputOffset >= outputCapacity)
							goto default;

						output[outputOffset++] = (byte)((number >> 16) & 255);
						break;
					case 3:
						if (outputOffset + 1 >= outputCapacity)
							goto default;

						output[outputOffset++] = (byte)((number >> 16) & 255);
						output[outputOffset++] = (byte)((number >> 8) & 255);
						break;
					case 4:
						if (outputOffset + 2 >= outputCapacity)
							goto default;

						output[outputOffset++] = (byte)((number >> 16) & 255);
						output[outputOffset++] = (byte)((number >> 8) & 255);
						output[outputOffset++] = (byte)((number >> 0) & 255);
						break;
					default:
						inputOffset -= j;
						goto end;
				}
			}

			end:
			outputUsed = outputOffset - outputBuffer.Offset;
			inputUsed = inputOffset - originalInputOffset;

			return outputUsed;
		}
		private static int GetBytesCountInternal<BufferT>(ref BufferT inputBuffer, Base64Alphabet base64Alphabet)
		{
			var alphabetInverse = (base64Alphabet ?? Base64Alphabet).AlphabetInverse;
			int inputEnd, inputOffset, inputCount;

			if (inputBuffer is ByteSegment)
			{
				var byteSegment = (ByteSegment)(object)inputBuffer;
				if (byteSegment.Count == 0 || byteSegment.Array == null)
					return 0;
				inputOffset = byteSegment.Offset;
				inputEnd = byteSegment.Offset + byteSegment.Count;
				inputCount = byteSegment.Count;
			}
			else if (inputBuffer is CharSegment)
			{
				var charSegment = (CharSegment)(object)inputBuffer;
				if (charSegment.Count == 0 || charSegment.Array == null)
					return 0;
				inputOffset = charSegment.Offset;
				inputEnd = charSegment.Offset + charSegment.Count;
				inputCount = charSegment.Count;
			}
			else if (inputBuffer is StringSegment)
			{
				var stringSegment = (StringSegment)(object)inputBuffer;
				if (stringSegment.Count == 0 || stringSegment.Array == null)
					return 0;
				inputOffset = stringSegment.Offset;
				inputEnd = stringSegment.Offset + stringSegment.Count;
				inputCount = stringSegment.Count;
			}
			else
			{
				throw new InvalidOperationException("Unknown input buffer type: " + typeof(BufferT));
			}

			for (; inputOffset < inputEnd; inputOffset++)
			{
				uint base64Code;

				if (inputBuffer is ByteSegment)
				{
					var byteSegment = (ByteSegment)(object)inputBuffer;
					base64Code = byteSegment.Array[inputOffset];
				}
				else if (inputBuffer is CharSegment)
				{
					var charSegment = (CharSegment)(object)inputBuffer;
					base64Code = charSegment.Array[inputOffset];
				}
				else
				{
					var stringSegment = (StringSegment)(object)inputBuffer;
					base64Code = stringSegment.Array[inputOffset];
				}

				if (base64Code > 127 || alphabetInverse[base64Code] == Base64Alphabet.NOT_IN_ALPHABET)
				{
					inputCount--;
				}
			}

			var bytesCount = inputCount / 4 * 3;

			switch (inputCount % 4)
			{
				case 2: bytesCount += 1; break;
				case 3: bytesCount += 2; break;
				// ReSharper disable once RedundantEmptySwitchSection
				default: break;
			}

			return bytesCount;
		}
	}
}
