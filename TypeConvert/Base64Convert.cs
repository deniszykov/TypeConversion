using System.Linq;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System
{
	public static class Base64Convert
	{
		public static readonly Base64Alphabet Base64Alphabet = new Base64Alphabet("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".ToCharArray(), padding: '=');
		public static readonly Base64Alphabet Base64UrlAlphabet = new Base64Alphabet("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_".ToCharArray(), padding: '=');

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

			var outputLength = GetBase64OutputLength(count, base64Alphabet.HasPadding);
			var base64Buffer = new StringBuilder(outputLength);

			var lastChars = count % 3;
			var countWIthQuads = offset + (count - lastChars);
			var alphabet = base64Alphabet.Alphabet;
			int i;
			for (i = offset; i < countWIthQuads; i += 3)
			{
				base64Buffer.Append(alphabet[(buffer[i] & 0xFC) >> 2]);
				base64Buffer.Append(alphabet[(buffer[i] & 3) << 4 | (buffer[i + 1] & 0xF0) >> 4]);
				base64Buffer.Append(alphabet[(buffer[i + 1] & 0xF) << 2 | (buffer[i + 2] & 0xC0) >> 6]);
				base64Buffer.Append(alphabet[buffer[i + 2] & 0x3F]);
			}
			i = countWIthQuads;

			switch (lastChars)
			{
				case 2:
					base64Buffer.Append(alphabet[(buffer[i] & 0xFC) >> 2]);
					base64Buffer.Append(alphabet[(buffer[i] & 3) << 4 | (buffer[i + 1] & 0xF0) >> 4]);
					base64Buffer.Append(alphabet[(buffer[i + 1] & 0xF) << 2]);
					if (base64Alphabet.HasPadding)
					{
						base64Buffer.Append(base64Alphabet.Padding);
					}
					break;
				case 1:
					base64Buffer.Append(alphabet[(buffer[i] & 0xFC) >> 2]);
					base64Buffer.Append(alphabet[(buffer[i] & 3) << 4]);
					if (base64Alphabet.HasPadding)
					{
						base64Buffer.Append(base64Alphabet.Padding);
						base64Buffer.Append(base64Alphabet.Padding);
					}
					break;
			}

			return base64Buffer.ToString();
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

			var base64Buffer = new char[count * 2];
			Encode(buffer, offset, count, base64Buffer, 0, base64Alphabet);
			return base64Buffer;
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

			var buffer = new byte[(base64Buffer.Length + 1) / 2];
			Decode(base64Buffer, offset, count, buffer, 0, base64Alphabet);
			return buffer;
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
			if (count < 0 || count % 4 != 0) throw new ArgumentOutOfRangeException("count");
			if (offset + count > base64String.Length) throw new ArgumentOutOfRangeException("count");

			if (count == 0)
				return new byte[0];

			var outputBuffer = new byte[count / 4 * 3];
			Decode(base64String, offset, count, outputBuffer, 0, base64Alphabet);
			return outputBuffer;
		}

		/// <summary>
		/// Encode part of <paramref name="buffer"/> and store encoded bytes into specified part of <paramref name="base64Buffer"/>.
		/// </summary>
		/// <param name="buffer">Bytes to encode.</param>
		/// <param name="offset">Encode start index in <paramref name="buffer"/>.</param>
		/// <param name="count">Number of bytes to encode in <paramref name="buffer"/>.</param>
		/// <param name="base64Buffer">Char array to store Base64 encoded bytes from <paramref name="buffer"/>. Array should fit encoded bytes or exception will be thrown.</param>
		/// <param name="base64BufferOffset">Storage offset in <paramref name="base64Buffer"/>.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of characters written to <paramref name="base64Buffer"/>.</returns>
		public static int Encode(byte[] buffer, int offset, int count, char[] base64Buffer, int base64BufferOffset, Base64Alphabet base64Alphabet = null)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			if (offset + count > buffer.Length) throw new ArgumentOutOfRangeException("count");
			if (base64Buffer == null) throw new ArgumentNullException("base64Buffer");
			if (base64BufferOffset < 0) throw new ArgumentOutOfRangeException("base64BufferOffset");
			if (base64Alphabet == null) base64Alphabet = Base64Alphabet;
			if (base64BufferOffset + GetBase64OutputLength(count, base64Alphabet.HasPadding) > base64Buffer.Length) throw new ArgumentOutOfRangeException("base64BufferOffset");

			if (count == 0) return 0;

			var lastChars = count % 3;
			var outputOffset = base64BufferOffset;
			var quartetEnd = offset + (count - lastChars);
			var base64Chars = Base64Alphabet.Alphabet;
			int inputOffset;
			for (inputOffset = offset; inputOffset < quartetEnd; inputOffset += 3)
			{
				base64Buffer[outputOffset] = base64Chars[(buffer[inputOffset] & 0xFC) >> 2];
				base64Buffer[outputOffset + 1] = base64Chars[(buffer[inputOffset] & 3) << 4 | (buffer[inputOffset + 1] & 0xF0) >> 4];
				base64Buffer[outputOffset + 2] = base64Chars[(buffer[inputOffset + 1] & 0xF) << 2 | (buffer[inputOffset + 2] & 0xC0) >> 6];
				base64Buffer[outputOffset + 3] = base64Chars[buffer[inputOffset + 2] & 0x3F];
				outputOffset += 4;
			}
			inputOffset = quartetEnd;

			switch (lastChars)
			{
				case 2:
					base64Buffer[outputOffset] = base64Chars[(buffer[inputOffset] & 0xFC) >> 2];
					base64Buffer[outputOffset + 1] = base64Chars[(buffer[inputOffset] & 3) << 4 | (buffer[inputOffset + 1] & 0xF0) >> 4];
					base64Buffer[outputOffset + 2] = base64Chars[(buffer[inputOffset + 1] & 0xF) << 2];
					if (base64Alphabet.HasPadding)
					{
						base64Buffer[outputOffset + 3] = base64Alphabet.Padding;
						outputOffset += 1;
					}
					outputOffset += 3;
					break;
				case 1:
					base64Buffer[outputOffset] = base64Chars[(buffer[inputOffset] & 0xFC) >> 2];
					base64Buffer[outputOffset + 1] = base64Chars[(buffer[inputOffset] & 3) << 4];
					if (base64Alphabet.HasPadding)
					{
						base64Buffer[outputOffset + 2] = base64Alphabet.Padding;
						base64Buffer[outputOffset + 3] = base64Alphabet.Padding;
						outputOffset += 2;
					}
					outputOffset += 2;
					break;
			}

			return outputOffset - base64BufferOffset;
		}
		/// <summary>
		/// Decode part of <paramref name="base64Buffer"/> and store decoded bytes into specified part of <paramref name="buffer"/>.
		/// </summary>
		/// <param name="base64Buffer">Char array contains Base64 encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="base64Buffer"/>.</param>
		/// <param name="count">Number of chars to decode in <paramref name="base64Buffer"/>. Array should fit decoded bytes or exception will be thrown.</param>
		/// <param name="buffer">Byte array to store decoded bytes from <paramref name="base64Buffer"/>. </param>
		/// <param name="bufferOffset">Storage offset in <paramref name="buffer"/>.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		/// <returns>Number of bytes decoded into <paramref name="buffer"/>.</returns>
		public static int Decode(char[] base64Buffer, int offset, int count, byte[] buffer, int bufferOffset, Base64Alphabet base64Alphabet = null)
		{
			if (base64Buffer == null) throw new ArgumentNullException("base64String");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + count > base64Buffer.Length) throw new ArgumentOutOfRangeException("offset");
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (bufferOffset < 0) throw new ArgumentOutOfRangeException("bufferOffset");
			if (bufferOffset + GetBytesCount(base64Buffer, offset, count) > buffer.Length) throw new ArgumentOutOfRangeException("count");

			if (count == 0) return 0;
			if (base64Alphabet == null) base64Alphabet = Base64Alphabet;

			var decoded = 0;
			var end = offset + count;
			var alphabetInverse = base64Alphabet.AlphabetInverse;
			for (var i = offset; i < end; i += 4)
			{
				var number = 0u;
				int j, base64Code, base64CodeIndex;
				for (j = 0; j < 4; j++)
				{
					base64Code = base64Buffer[i + j];
					if ((base64Code > 127) || (base64CodeIndex = alphabetInverse[base64Code]) == Base64Alphabet.NOT_IN_ALPHABET)
					{
						i++;
						if (i + j >= end)
						{
							break;
						}
						else
						{
							continue;
						}
					}
					number = (uint)unchecked(number + base64CodeIndex << (18 - 6 * j));
				}

				switch (j)
				{
					case 1: buffer[bufferOffset++] = (byte)((number >> 16) & 255); goto case 2;
					case 2: buffer[bufferOffset++] = (byte)((number >> 8) & 255); goto case 3;
					case 3: buffer[bufferOffset++] = (byte)((number >> 0) & 255); break;
					default: break;
				}
				decoded += j;
			}
			return decoded;
		}
		/// <summary>
		/// Decode part of <paramref name="base64String"/> and store decoded bytes into specified part of <paramref name="buffer"/>.
		/// </summary>
		/// <param name="base64String">String contains Base64 encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="base64String"/>.</param>
		/// <param name="count">Number of chars to decode in <paramref name="base64String"/>. Array should fit decoded bytes or exception will be thrown.</param>
		/// <param name="buffer">Byte array to store decoded bytes from <paramref name="base64String"/>. </param>
		/// <param name="bufferOffset">Storage offset in <paramref name="buffer"/>.</param>
		/// <param name="base64Alphabet">Base alphabet used for encoding/decoding. <see cref="Base64Alphabet"/> is used if <paramref name="base64Alphabet"/> is null.</param>
		public static int Decode(string base64String, int offset, int count, byte[] buffer, int bufferOffset, Base64Alphabet base64Alphabet = null)
		{
			if (base64String == null) throw new ArgumentNullException("base64String");
			if (offset < 0) throw new ArgumentOutOfRangeException("offset");
			if (offset + count > base64String.Length) throw new ArgumentOutOfRangeException("offset");
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (bufferOffset < 0) throw new ArgumentOutOfRangeException("bufferOffset");
			if (bufferOffset + GetBytesCount(base64String, offset, count) > buffer.Length) throw new ArgumentOutOfRangeException("count");

			if (count == 0) return 0;
			if (base64Alphabet == null) base64Alphabet = Base64Alphabet;

			var decoded = 0;
			var end = offset + count;
			var alphabetInverse = base64Alphabet.AlphabetInverse;
			for (var i = offset; i < end; i += 4)
			{
				var number = 0u;
				int j, base64Code, base64CodeIndex;
				for (j = 0; j < 4; j++)
				{
					base64Code = base64String[i + j];
					if ((base64Code > 127) || (base64CodeIndex = alphabetInverse[base64Code]) == Base64Alphabet.NOT_IN_ALPHABET)
					{
						i++;
						if (i + j >= end)
						{
							break;
						}
						else
						{
							continue;
						}
					}
					number = (uint)unchecked(number + base64CodeIndex << (18 - 6 * j));
				}

				switch (j)
				{
					case 1: buffer[bufferOffset++] = (byte)((number >> 16) & 255); goto case 2;
					case 2: buffer[bufferOffset++] = (byte)((number >> 8) & 255); goto case 3;
					case 3: buffer[bufferOffset++] = (byte)((number >> 0) & 255); break;
					default: break;
				}
				decoded += j;
			}
			return decoded;
		}

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
		public static int GetBytesCount(char[] base64String, int offset, int count, Base64Alphabet base64Alphabet = null)
		{
			if (base64String == null) throw new ArgumentNullException(nameof(base64String));
			if (offset < 0 || offset >= base64String.Length) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0 || offset + count > base64String.Length || count % 4 != 0) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return 0;
			if (base64Alphabet == null) base64Alphabet = Base64Alphabet;

			var alphabetInverse = base64Alphabet.AlphabetInverse;
			foreach (var base64Char in base64String)
			{
				if (base64Char > 127 || alphabetInverse[base64Char] == Base64Alphabet.NOT_IN_ALPHABET)
				{
					count--;
				}
			}

			var bytesCount = count / 4 * 3;
			if (base64String[offset + count - 1] == base64Alphabet.Padding)
				bytesCount--;
			if (base64String[offset + count - 2] == base64Alphabet.Padding)
				bytesCount--;

			return bytesCount;
		}
		public static int GetBytesCount(string base64String, int offset, int count, Base64Alphabet base64Alphabet = null)
		{
			if (base64String == null) throw new ArgumentNullException(nameof(base64String));
			if (offset < 0 || offset >= base64String.Length) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0 || offset + count > base64String.Length || count % 4 != 0) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return 0;
			if (base64Alphabet == null) base64Alphabet = Base64Alphabet;

			var alphabetInverse = base64Alphabet.AlphabetInverse;
			foreach (var base64Char in base64String)
			{
				if (base64Char > 127 || alphabetInverse[base64Char] == Base64Alphabet.NOT_IN_ALPHABET)
				{
					count--;
				}
			}

			var bytesCount = count / 4 * 3;
			if (base64String[offset + count - 1] == base64Alphabet.Padding)
				bytesCount--;
			if (base64String[offset + count - 2] == base64Alphabet.Padding)
				bytesCount--;
			return bytesCount;
		}
	}

	/// <summary>
	/// Base64 encoding/decoding alphabet.
	/// </summary>
	public sealed class Base64Alphabet
	{
		internal const byte NOT_IN_ALPHABET = 255;
		internal readonly char[] Alphabet;
		internal readonly byte[] AlphabetInverse;
		internal readonly char Padding;

		internal bool HasPadding { get { return this.Padding != '\u00ff'; } }

		/// <summary>
		/// Create baseX alphabet with passed character set and padding. If padding is set to '\u00ff' then no padding is used.
		/// </summary>
		/// <param name="alphabet">Character set which used as base alphabet for encoding/decoding. Characters should be between '\u0000' and '\u007f' and not <paramref name="padding"/>.</param>
		/// <param name="padding">Padding character which used to pad data. '\u00ff' character indicates that no padding is used. Padding should be between '\u0000' and '\u007f'.</param>
		public Base64Alphabet(char[] alphabet, char padding = '\u00ff')
		{
			if (alphabet == null) throw new ArgumentNullException("alphabet");
			if (alphabet.Length != 64) throw new ArgumentOutOfRangeException("alphabet");

			this.Alphabet = alphabet;
			this.Padding = padding;
			this.AlphabetInverse = new byte[127];
			for (var i = 0; i < this.AlphabetInverse.Length; i++)
			{
				this.AlphabetInverse[i] = NOT_IN_ALPHABET;
			}

			for (var i = 0; i < this.Alphabet.Length; i++)
			{
				var charNum = (int)alphabet[i];
				if (charNum < 0 || charNum > 127 || charNum == padding) throw new ArgumentOutOfRangeException("alphabet");

				this.AlphabetInverse[charNum] = (byte)i;
			}
		}
	}
}
