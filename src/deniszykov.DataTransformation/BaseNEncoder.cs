using System;
using System.Text;

namespace deniszykov.BaseN
{
	using CharSafePtr = UIntPtr;
	using ByteSafePtr = IntPtr;

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
#if NETCOREAPP
			this.DecodeInternal(new ReadOnlySpan<byte>(chars, charCount), new Span<byte>(bytes, byteCount), flush, out charsUsed, out bytesUsed, out completed);
#else
			this.DecodeInternal((ByteSafePtr)chars, 0, charCount, (ByteSafePtr)bytes, 0, byteCount, flush, out charsUsed, out bytesUsed, out completed);
#endif
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
#if NETCOREAPP
			this.DecodeInternal(new ReadOnlySpan<char>(chars, charCount), new Span<byte>(bytes, byteCount), flush, out charsUsed, out bytesUsed, out completed);
#else
			this.DecodeInternal((CharSafePtr)chars, 0, charCount, (ByteSafePtr)bytes, 0, byteCount, flush, out charsUsed, out bytesUsed, out completed);
#endif
		}
		/// <inheritdoc />
		public override void Convert(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, int byteCount, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
		{
#if NETCOREAPP
			this.DecodeInternal<char, byte>(chars.AsSpan(charIndex, charCount), bytes.AsSpan(byteIndex, byteCount), flush, out charsUsed, out bytesUsed, out completed);
#else
			this.DecodeInternal(chars, charIndex, charCount, bytes, byteIndex, byteCount, flush, out charsUsed, out bytesUsed, out completed);
#endif
		}
		/// <summary>
		/// See description on similar conversion methods. This is just overload with different buffer types.
		/// </summary>
		public void Convert(byte[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, int byteCount, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
		{
#if NETCOREAPP
			this.DecodeInternal<byte, byte>(chars.AsSpan(charIndex, charCount), bytes.AsSpan(byteIndex, byteCount), flush, out charsUsed, out bytesUsed, out completed);
#else
			this.DecodeInternal(chars, charIndex, charCount, bytes, byteIndex, byteCount, flush, out charsUsed, out bytesUsed, out completed);
#endif

		}
		/// <summary>
		/// See description on similar conversion methods. This is just overload with different buffer types.
		/// </summary>
		public void Convert(string chars, int charIndex, int charCount, byte[] bytes, int byteIndex, int byteCount, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
		{
#if NETCOREAPP
			this.DecodeInternal(chars.AsSpan(charIndex, charCount), bytes.AsSpan(byteIndex, byteCount), flush, out charsUsed, out bytesUsed, out completed);
#else
			this.DecodeInternal(chars, charIndex, charCount, bytes, byteIndex, byteCount, flush, out charsUsed, out bytesUsed, out completed);
#endif
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
			this.DecodeInternal(chars, bytes, flush, out charsUsed, out bytesUsed, out completed);
		}
		/// <inheritdoc />
		public override void Convert(ReadOnlySpan<char> chars, Span<byte> bytes, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
		{
			this.DecodeInternal(chars, bytes, flush, out charsUsed, out bytesUsed, out completed);
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

#if NETCOREAPP
		private void DecodeInternal<InputT, OutputT>(ReadOnlySpan<InputT> input, Span<OutputT> output, bool flush, out int inputUsed, out int outputUsed, out bool completed) where InputT : unmanaged where OutputT : unmanaged
		{
			inputUsed = outputUsed = 0;
			completed = true;

			if (input.IsEmpty || output.IsEmpty)
			{
				return;
			}
#else
		private unsafe void DecodeInternal<InputT, OutputT>(InputT input, int inputOffset, int inputCount, OutputT output, int outputOffset, int outputCount, bool flush, out int inputUsed, out int outputUsed, out bool completed)
		{
			inputUsed = outputUsed = 0;
			completed = true;

			if (inputCount == 0 || outputCount == 0)
			{
				return;
			}
#endif

			var alphabetInverse = this.baseNAlphabet.AlphabetInverse;
			var inputBlockSize = this.baseNAlphabet.DecodingBlockSize;
			var encodingBits = this.baseNAlphabet.EncodingBits;
#if NETCOREAPP
			var inputOffset = 0;
			var inputCount = input.Length;
			var outputOffset = 0;
			var outputCount = output.Length;
#else
			var inputBytes = default(byte[]);
			var inputChars = default(char[]);
			var inputString = default(string);
			var inputBytePtr = default(byte*);
			var inputCharPtr = default(char*);

			var outputBytes = default(byte[]);
			var outputChars = default(char[]);
			var outputBytePtr = default(byte*);
			var outputCharPtr = default(char*);
			if (typeof(InputT) == typeof(byte[]))
			{
				inputBytes = input as byte[];
			}
			else if (typeof(InputT) == typeof(char[]))
			{
				inputChars = input as char[];
			}
			else if (typeof(InputT) == typeof(string))
			{
				inputString = input as string;
			}
			else if (typeof(InputT) == typeof(ByteSafePtr))
			{
				inputBytePtr = (byte*)(ByteSafePtr)(object)input;
			}
			else if (typeof(InputT) == typeof(ByteSafePtr))
			{
				inputCharPtr = (char*)(CharSafePtr)(object)input;
			}
			else
			{
				throw new InvalidOperationException("Unknown input buffer type: " + typeof(InputT));
			}

			if (typeof(OutputT) == typeof(byte[]))
			{
				outputBytes = output as byte[];
			}
			else if (typeof(OutputT) == typeof(char[]))
			{
				outputChars = output as char[];
			}
			else if (typeof(OutputT) == typeof(ByteSafePtr))
			{
				outputBytePtr = (byte*)(ByteSafePtr)(object)output;
			}
			else if (typeof(OutputT) == typeof(ByteSafePtr))
			{
				outputCharPtr = (char*)(CharSafePtr)(object)output;
			}
			else
			{
				throw new InvalidOperationException("Unknown output buffer type: " + typeof(OutputT));
			}
#endif

			while (outputCount > 0)
			{
				// filling input & decoding
				var outputBlock = 0UL;// 1 byte for Base16, 5 bytes for Base32 and 3 bytes for Base64
				var originalInputUsed = inputUsed;
				var i = 0;
				for (i = 0; i < inputBlockSize && inputCount > 0; i++)
				{

					uint baseNCode = 0;
#if NETCOREAPP
					if (typeof(InputT) == typeof(byte))
					{
						baseNCode = (byte)(object)input[inputOffset + inputUsed++];
					}
					if (typeof(InputT) == typeof(char))
					{
						baseNCode = (char)(object)input[inputOffset + inputUsed++];
					}
#else
					if (typeof(InputT) == typeof(byte[]))
					{
						baseNCode = inputBytes[inputOffset + inputUsed++];
					}
					if (typeof(InputT) == typeof(char[]))
					{
						baseNCode = inputChars[inputOffset + inputUsed++];
					}
					if (typeof(InputT) == typeof(string))
					{
						baseNCode = inputString[inputOffset + inputUsed++];
					}
					if (typeof(InputT) == typeof(ByteSafePtr))
					{
						baseNCode = inputBytePtr[inputOffset + inputUsed++];
					}
					if (typeof(InputT) == typeof(CharSafePtr))
					{
						baseNCode = inputCharPtr[inputOffset + inputUsed++];
					}
#endif
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
					i != inputBlockSize && !flush)
				{
					inputUsed = originalInputUsed; // unwind inputUsed
					break;
				}
#if NETCOREAPP
				if (typeof(OutputT) == typeof(byte))
				{
					for (i = 0; i < outputSize; i++)
					{
						output[outputOffset + outputUsed + (outputSize - 1 - i)] = (OutputT)(object)(byte)(outputBlock & 0xFF);
						outputBlock >>= 8;
					}
				}
				if (typeof(OutputT) == typeof(char))
				{
					for (i = 0; i < outputSize; i++)
					{
						output[outputOffset + outputUsed + (outputSize - 1 - i)] = (OutputT)(object)(char)(outputBlock & 0xFF);
						outputBlock >>= 8;
					}
				}
#else
				if (typeof(OutputT) == typeof(byte[]))
				{
					for (i = 0; i < outputSize; i++)
					{
						outputBytes[outputOffset + outputUsed + (outputSize - 1 - i)] = (byte)(outputBlock & 0xFF);
						outputBlock >>= 8;
					}
				}
				if (typeof(OutputT) == typeof(char[]))
				{
					for (i = 0; i < outputSize; i++)
					{
						outputChars[outputOffset + outputUsed + (outputSize - 1 - i)] = (char)(outputBlock & 0xFF);
						outputBlock >>= 8;
					}
				}
				if (typeof(OutputT) == typeof(ByteSafePtr))
				{
					for (i = 0; i < outputSize; i++)
					{
						outputBytePtr[outputOffset + outputUsed + (outputSize - 1 - i)] = (byte)(outputBlock & 0xFF);
						outputBlock >>= 8;
					}
				}
				if (typeof(OutputT) == typeof(CharSafePtr))
				{
					for (i = 0; i < outputSize; i++)
					{
						outputCharPtr[outputOffset + outputUsed + (outputSize - 1 - i)] = (char)(outputBlock & 0xFF);
						outputBlock >>= 8;
					}
				}
#endif
				outputUsed += outputSize;
				outputCount -= outputSize;
			}

			completed = inputCount == 0;
		}

		/// <inheritdoc />
		public override string ToString() => $"Base{this.baseNAlphabet.Alphabet.Length}Encoder, {new string(this.baseNAlphabet.Alphabet)}";
	}
}