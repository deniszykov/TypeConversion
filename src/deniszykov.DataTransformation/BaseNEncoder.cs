using System;
using System.Text;

namespace deniszykov.BaseN
{
	/// <summary>
	/// Base-(Alphabet Length) binary data decoder (!) based on specified <see cref="baseNAlphabet"/>.
	/// Class named "Encoder" because it is based on <see cref="Encoder"/>, but it is effectively decoder.
	/// </summary>
	public sealed class BaseNEncoder : Encoder
	{
		private readonly BaseNAlphabet baseNAlphabet;

		/// <summary>
		/// Constructor fo <see cref="BaseNEncoder"/>.
		/// </summary>
		/// <param name="baseNAlphabet"></param>
		public BaseNEncoder(BaseNAlphabet baseNAlphabet)
		{
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));

			this.baseNAlphabet = baseNAlphabet;
		}

		/// <inheritdoc />
		public override int GetByteCount(char[] chars, int index, int count, bool flush)
		{
			if (count == 0)
				return 0;

			var alphabetInverse = this.baseNAlphabet.AlphabetInverse;
			var bitsPerInputChar = this.baseNAlphabet.EncodingBits;
			var inputEnd = index + count;
			for (; index < inputEnd; index++)
			{
				var baseNChar = chars[index];
				if (baseNChar > 127 || alphabetInverse[baseNChar] == BaseNAlphabet.NOT_IN_ALPHABET)
				{
					count--;
				}
			}

			if (!flush)
			{
				count -= count % this.baseNAlphabet.DecodingBlockSize;
			}

			var bytesCount = (int)checked((ulong)count * (ulong)bitsPerInputChar / 8);
			return bytesCount;
		}
#if NETSTANDARD1_6
		/// <summary>
		/// See description on similar conversion methods. This is just overload with different buffer types.
		/// </summary>
		public unsafe int GetByteCount(char* chars, int count, bool flush)
#else
		/// <inheritdoc />
		public override unsafe int GetByteCount(char* chars, int count, bool flush)
#endif
		{
			if (count == 0)
				return 0;

			var alphabetInverse = this.baseNAlphabet.AlphabetInverse;
			var bitsPerInputChar = this.baseNAlphabet.EncodingBits;
			var inputEnd = count;
			for (var index = 0; index < inputEnd; index++)
			{
				var baseNChar = chars[index];

				if (baseNChar > 127 || alphabetInverse[baseNChar] == BaseNAlphabet.NOT_IN_ALPHABET)
				{
					count--;
				}
			}

			if (!flush)
			{
				count -= count % this.baseNAlphabet.DecodingBlockSize;
			}

			var bytesCount = (int)checked((ulong)count * (ulong)bitsPerInputChar / 8);
			return bytesCount;
		}
		/// <summary>
		/// See other conversion methods for info.
		/// </summary>
		public int GetByteCount(string chars, int index, int count, bool flush)
		{
			if (count == 0 || chars == null)
				return 0;

			var alphabetInverse = this.baseNAlphabet.AlphabetInverse;
			var bitsPerInputChar = this.baseNAlphabet.EncodingBits;
			var inputEnd = index + count;
			for (; index < inputEnd; index++)
			{
				var baseNChar = chars[index];
				if (baseNChar > 127 || alphabetInverse[baseNChar] == BaseNAlphabet.NOT_IN_ALPHABET)
				{
					count--;
				}
			}

			if (!flush)
			{
				count -= count % this.baseNAlphabet.DecodingBlockSize;
			}

			var bytesCount = (int)checked((ulong)count * (ulong)bitsPerInputChar / 8);
			return bytesCount;
		}
		/// <summary>
		/// See other conversion methods for info.
		/// </summary>
		public int GetByteCount(byte[] chars, int index, int count, bool flush)
		{
			if (count == 0 || chars == null)
				return 0;

			var alphabetInverse = this.baseNAlphabet.AlphabetInverse;
			var bitsPerInputChar = this.baseNAlphabet.EncodingBits;
			var inputEnd = index + count;
			for (; index < inputEnd; index++)
			{
				var baseNChar = chars[index];

				if (baseNChar > 127 || alphabetInverse[baseNChar] == BaseNAlphabet.NOT_IN_ALPHABET)
				{
					count--;
				}
			}

			if (!flush)
			{
				count -= count % this.baseNAlphabet.DecodingBlockSize;
			}

			var bytesCount = (int)checked((ulong)count * (ulong)bitsPerInputChar / 8);
			return bytesCount;
		}
		/// <summary>
		/// Get max bytes count used to decode data of specified <paramref name="charCount"/>.
		/// </summary>
		/// <returns></returns>
		public int GetMaxByteCount(int charCount)
		{
			var bitsPerInputChar = this.baseNAlphabet.EncodingBits;
			var bytesCount = (int)checked((ulong)charCount * (ulong)bitsPerInputChar / 8);
			return bytesCount;
		}

		/// <inheritdoc />
		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, bool flush)
		{
			this.Convert(chars, charIndex, charCount, bytes, byteIndex, bytes.Length - byteIndex, flush, out _, out var bytesUsed, out _);
			return bytesUsed;
		}
#if NETSTANDARD1_6
		/// <summary>
		/// See description on similar conversion methods. This is just overload with different buffer types.
		/// </summary>
		public unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, bool flush)
#else
		/// <inheritdoc />
		public override unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, bool flush)
#endif
		{
			this.Convert(chars, charCount, bytes, byteCount, flush, out _, out var bytesUsed, out _);
			return bytesUsed;
		}

		/// <summary>
		/// See other conversion methods for info.
		/// </summary>
		public unsafe void Convert(byte* chars, int charCount, byte* bytes, int byteCount, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
		{
			if (byteCount < 0) throw new ArgumentOutOfRangeException(nameof(byteCount));
			if (charCount < 0) throw new ArgumentOutOfRangeException(nameof(charCount));

			charsUsed = bytesUsed = 0;
			completed = true;

			if (charCount == 0)
				return;

			var alphabetInverse = this.baseNAlphabet.AlphabetInverse;
			var inputBlockSize = this.baseNAlphabet.DecodingBlockSize;
			var encodingBits = this.baseNAlphabet.EncodingBits;

			while (byteCount > 0)
			{
				// filling input & decoding
				var outputBlock = 0UL;// 1 byte for Base16, 5 bytes for Base32 and 3 bytes for Base64
				var originalInputUsed = charsUsed;
				var i = 0;
				for (i = 0; i < inputBlockSize && charCount > 0; i++)
				{

					var baseNCode = chars[charsUsed++];
					charCount--;

					if (baseNCode > 127 || alphabetInverse[baseNCode] == BaseNAlphabet.NOT_IN_ALPHABET)
					{
						i--;
						continue;
					}

					outputBlock <<= encodingBits;
					outputBlock |= alphabetInverse[baseNCode];
				}

				var outputSize = i * encodingBits / 8;
				outputBlock >>= i * encodingBits % 8; // align

				// flushing output
				if (outputSize > byteCount ||
					outputSize == 0 ||
					(i != inputBlockSize && !flush))
				{
					charsUsed = originalInputUsed; // unwind inputUsed
					break;
				}

				for (i = 0; i < outputSize; i++)
				{
					bytes[bytesUsed + (outputSize - 1 - i)] = (byte)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}
				bytesUsed += outputSize;
				byteCount -= outputSize;
			}
			completed = charCount == 0;
		}
#if NETSTANDARD1_6
		/// <summary>
		/// 
		/// </summary>
		/// <param name="chars"></param>
		/// <param name="charCount"></param>
		/// <param name="bytes"></param>
		/// <param name="byteCount"></param>
		/// <param name="flush"></param>
		/// <param name="charsUsed"></param>
		/// <param name="bytesUsed"></param>
		/// <param name="completed"></param>
		public unsafe void Convert(char* chars, int charCount, byte* bytes, int byteCount, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
#else
		/// <inheritdoc />
		public override unsafe void Convert(char* chars, int charCount, byte* bytes, int byteCount, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
#endif
		{
			if (byteCount < 0) throw new ArgumentOutOfRangeException(nameof(byteCount));
			if (charCount < 0) throw new ArgumentOutOfRangeException(nameof(charCount));

			charsUsed = bytesUsed = 0;
			completed = true;

			if (charCount == 0)
				return;

			var alphabetInverse = this.baseNAlphabet.AlphabetInverse;
			var inputBlockSize = this.baseNAlphabet.DecodingBlockSize;
			var encodingBits = this.baseNAlphabet.EncodingBits;

			while (byteCount > 0)
			{
				// filling input & decoding
				var outputBlock = 0UL;// 1 byte for Base16, 5 bytes for Base32 and 3 bytes for Base64
				var originalInputUsed = charsUsed;
				var i = 0;
				for (i = 0; i < inputBlockSize && charCount > 0; i++)
				{

					var baseNCode = chars[charsUsed++];
					charCount--;

					if (baseNCode > 127 || alphabetInverse[baseNCode] == BaseNAlphabet.NOT_IN_ALPHABET)
					{
						i--;
						continue;
					}

					outputBlock <<= encodingBits;
					outputBlock |= alphabetInverse[baseNCode];
				}

				var outputSize = i * encodingBits / 8;
				outputBlock >>= i * encodingBits % 8; // align

				// flushing output
				if (outputSize > byteCount ||
					outputSize == 0 ||
					(i != inputBlockSize && !flush))
				{
					charsUsed = originalInputUsed; // unwind inputUsed
					break;
				}

				for (i = 0; i < outputSize; i++)
				{
					bytes[bytesUsed + (outputSize - 1 - i)] = (byte)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}
				bytesUsed += outputSize;
				byteCount -= outputSize;
			}
			completed = charCount == 0;
		}
		/// <inheritdoc />
		public override void Convert(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, int byteCount, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
		{
			this.DecodeInternal(chars, charIndex, charCount, bytes, byteIndex, byteCount, flush, out charsUsed, out bytesUsed, out completed);
		}
		/// <summary>
		/// See description on similar conversion methods. This is just overload with different buffer types.
		/// </summary>
		public void Convert(byte[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, int byteCount, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
		{
			this.DecodeInternal(chars, charIndex, charCount, bytes, byteIndex, byteCount, flush, out charsUsed, out bytesUsed, out completed);
		}
		/// <summary>
		/// See description on similar conversion methods. This is just overload with different buffer types.
		/// </summary>
		public void Convert(string chars, int charIndex, int charCount, byte[] bytes, int byteIndex, int byteCount, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
		{
			this.DecodeInternal(chars, charIndex, charCount, bytes, byteIndex, byteCount, flush, out charsUsed, out bytesUsed, out completed);
		}
#if NETCOREAPP
		/// <inheritdoc />
		public override int GetBytes(ReadOnlySpan<char> chars, Span<byte> bytes, bool flush)
		{
			this.Convert(chars, bytes, flush, out _, out var bytesUsed, out _);
			return bytesUsed;
		}
		/// <summary>
		/// See description on similar conversion methods. This is just overload with different buffer types.
		/// </summary>
		public void Convert(ReadOnlySpan<byte> chars, Span<byte> bytes, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
		{
			charsUsed = bytesUsed = 0;
			completed = true;

			if (chars.Length == 0 || bytes.Length == 0)
			{
				return;
			}

			var inputLeft = chars.Length;
			var outputCapacity = bytes.Length;
			var alphabetInverse = this.baseNAlphabet.AlphabetInverse;
			var inputBlockSize = this.baseNAlphabet.DecodingBlockSize;
			var encodingBits = this.baseNAlphabet.EncodingBits;

			while (outputCapacity > 0)
			{
				// filling input & decoding
				var outputBlock = 0UL;// 1 byte for Base16, 5 bytes for Base32 and 3 bytes for Base64
				var originalInputUsed = charsUsed;
				var i = 0;
				for (i = 0; i < inputBlockSize && inputLeft > 0; i++)
				{

					var baseNCode = chars[charsUsed++];
					inputLeft--;

					if (baseNCode > 127 || alphabetInverse[baseNCode] == BaseNAlphabet.NOT_IN_ALPHABET)
					{
						i--;
						continue;
					}

					outputBlock <<= encodingBits;
					outputBlock |= alphabetInverse[baseNCode];
				}

				var outputSize = i * encodingBits / 8;
				outputBlock >>= i * encodingBits % 8; // align

				// flushing output
				if (outputSize > outputCapacity ||
					outputSize == 0 ||
					(i != inputBlockSize && !flush))
				{
					charsUsed = originalInputUsed; // unwind inputUsed
					break;
				}

				for (i = 0; i < outputSize; i++)
				{
					bytes[bytesUsed + (outputSize - 1 - i)] = (byte)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}
				bytesUsed += outputSize;
				outputCapacity -= outputSize;
			}

			completed = inputLeft == 0;
		}
		/// <inheritdoc />
		public override void Convert(ReadOnlySpan<char> chars, Span<byte> bytes, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
		{
			charsUsed = bytesUsed = 0;
			completed = true;

			if (chars.Length == 0 || bytes.Length == 0)
			{
				return;
			}

			var inputLeft = chars.Length;
			var outputCapacity = bytes.Length;
			var alphabetInverse = this.baseNAlphabet.AlphabetInverse;
			var inputBlockSize = this.baseNAlphabet.DecodingBlockSize;
			var encodingBits = this.baseNAlphabet.EncodingBits;

			while (outputCapacity > 0)
			{
				// filling input & decoding
				var outputBlock = 0UL;// 1 byte for Base16, 5 bytes for Base32 and 3 bytes for Base64
				var originalInputUsed = charsUsed;
				var i = 0;
				for (i = 0; i < inputBlockSize && inputLeft > 0; i++)
				{

					var baseNCode = chars[charsUsed++];
					inputLeft--;

					if (baseNCode > 127 || alphabetInverse[baseNCode] == BaseNAlphabet.NOT_IN_ALPHABET)
					{
						i--;
						continue;
					}

					outputBlock <<= encodingBits;
					outputBlock |= alphabetInverse[baseNCode];
				}

				var outputSize = i * encodingBits / 8;
				outputBlock >>= i * encodingBits % 8; // align

				// flushing output
				if (outputSize > outputCapacity ||
					outputSize == 0 ||
					(i != inputBlockSize && !flush))
				{
					charsUsed = originalInputUsed; // unwind inputUsed
					break;
				}

				for (i = 0; i < outputSize; i++)
				{
					bytes[bytesUsed + (outputSize - 1 - i)] = (byte)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}
				bytesUsed += outputSize;
				outputCapacity -= outputSize;
			}

			completed = inputLeft == 0;
		}
		/// <inheritdoc />
		public override int GetByteCount(ReadOnlySpan<char> chars, bool flush)
		{
			if (chars.Length == 0 || chars == null)
				return 0;

			var alphabetInverse = this.baseNAlphabet.AlphabetInverse;
			var bitsPerInputChar = this.baseNAlphabet.EncodingBits;
			var count = chars.Length;
			foreach (var baseNChar in chars)
			{
				if (baseNChar > 127 || alphabetInverse[baseNChar] == BaseNAlphabet.NOT_IN_ALPHABET)
				{
					count--;
				}
			}
			var bytesCount = (int)checked((ulong)count * (ulong)bitsPerInputChar / 8);
			return bytesCount;
		}
#endif
		private void DecodeInternal<BufferT>(BufferT input, int inputIndex, int inputCount, byte[] output, int outputIndex, int outputCount, bool flush, out int inputUsed, out int outputUsed, out bool completed)
		{
			inputUsed = outputUsed = 0;
			completed = true;

			if (inputCount == 0)
			{
				return;
			}

			var alphabetInverse = this.baseNAlphabet.AlphabetInverse;
			var inputBlockSize = this.baseNAlphabet.DecodingBlockSize;
			var encodingBits = this.baseNAlphabet.EncodingBits;
			var inputBytes = default(byte[]);
			var inputChars = default(char[]);
			var inputString = default(string);

			if (input is byte[])
			{
				inputBytes = input as byte[];
			}
			else if (input is char[])
			{
				inputChars = input as char[];
			}
			else if (input is string)
			{
				inputString = input as string;
			}
			else
			{
				throw new InvalidOperationException("Unknown input buffer type: " + typeof(BufferT));
			}

			while (outputCount > 0)
			{
				// filling input & decoding
				var outputBlock = 0UL;// 1 byte for Base16, 5 bytes for Base32 and 3 bytes for Base64
				var originalInputUsed = inputUsed;
				var i = 0;
				for (i = 0; i < inputBlockSize && inputCount > 0; i++)
				{

					uint baseNCode;
					if (input is byte[])
					{
						baseNCode = inputBytes[inputIndex + (inputUsed++)];
					}
					else if (input is char[])
					{
						baseNCode = inputChars[inputIndex + (inputUsed++)];
					}
					else
					{
						baseNCode = inputString[inputIndex + (inputUsed++)];
					}
					inputCount--;

					if (baseNCode > 127 || alphabetInverse[baseNCode] == BaseNAlphabet.NOT_IN_ALPHABET)
					{
						i--;
						continue;
					}

					outputBlock <<= encodingBits;
					outputBlock |= alphabetInverse[baseNCode];
				}

				var outputSize = i * encodingBits / 8;
				outputBlock >>= i * encodingBits % 8; // align

				// flushing output
				if (outputSize > outputCount ||
					outputSize == 0 ||
					(i != inputBlockSize && !flush))
				{
					inputUsed = originalInputUsed; // unwind inputUsed
					break;
				}

				for (i = 0; i < outputSize; i++)
				{
					output[outputIndex + outputUsed + (outputSize - 1 - i)] = (byte)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}
				outputUsed += outputSize;
				outputCount -= outputSize;
			}

			completed = inputCount == 0;
		}

		/// <inheritdoc />
		public override string ToString() => $"Base{this.baseNAlphabet.Alphabet.Length}Encoder, {new string(this.baseNAlphabet.Alphabet)}";
	}
}