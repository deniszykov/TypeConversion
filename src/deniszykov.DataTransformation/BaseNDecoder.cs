using System;
using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;

namespace deniszykov.BaseN
{
	using CharSafePtr = UIntPtr;
	using ByteSafePtr = IntPtr;

	/// <summary>
	/// Base-(Alphabet Length) binary data encoder (!) based on specified <see cref="Alphabet"/>.
	/// Class named "Decoder" because it is based on <see cref="Decoder"/>, but it is effectively encoder.
	/// </summary>
	public sealed class BaseNDecoder : Decoder
	{
		[NotNull]
		public BaseNAlphabet Alphabet { get; }

		/// <summary>
		/// Constructor of <see cref="BaseNDecoder"/>.
		/// </summary>
		/// <param name="baseNAlphabet"></param>
		public BaseNDecoder([NotNull] BaseNAlphabet baseNAlphabet)
		{
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));

			this.Alphabet = baseNAlphabet;
		}

#if NETSTANDARD1_6
		/// <summary>
		/// See description on similar conversion methods. This is just overload with different buffer types.
		/// </summary>
		public unsafe int GetCharCount(byte* bytes, int count, bool flush)
#else
		/// <inheritdoc />
		public override unsafe int GetCharCount(byte* bytes, int count, bool flush)
#endif
		{
			return this.GetCharCount(count, flush);
		}
		/// <inheritdoc />
		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			return this.GetCharCount(count, true);
		}
		/// <inheritdoc />
		public override int GetCharCount(byte[] bytes, int index, int count, bool flush)
		{
			return this.GetCharCount(count, flush);
		}
		/// <summary>
		/// Get max char count used to encoded data of specified <paramref name="byteCount"/>.
		/// </summary>
		/// <returns></returns>
		public int GetMaxCharCount(int byteCount)
		{
			return ((byteCount + this.Alphabet.EncodingBlockSize - 1) / this.Alphabet.EncodingBlockSize) * this.Alphabet.DecodingBlockSize;
		}

		/// <inheritdoc />
		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			this.Convert(bytes, byteIndex, byteCount, chars, charIndex, chars.Length - charIndex, true, out _, out var charsUsed, out _);
			return charsUsed;
		}
		/// <inheritdoc />
		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, bool flush)
		{
			this.Convert(bytes, byteIndex, byteCount, chars, charIndex, chars.Length - charIndex, flush, out _, out var charsUsed, out _);
			return charsUsed;
		}
#if NETSTANDARD1_6
		/// <summary>
		/// See description on similar conversion methods. This is just overload with different buffer types.
		/// </summary>
		public unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount, bool flush)
#else
		/// <inheritdoc />
		public override unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount, bool flush)
#endif
		{
			this.Convert(bytes, byteCount, chars, charCount, flush, out _, out var charsUsed, out _);
			return charsUsed;
		}

		/// <summary>
		/// See description on similar conversion methods. This is just overload with different buffer types.
		/// </summary>
		public void Convert([NotNull] byte[] bytes, int byteIndex, int byteCount, [NotNull] byte[] chars, int charIndex, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
		{
			if (bytes == null) throw new NullReferenceException(nameof(bytes));
			if (byteIndex < 0) throw new ArgumentOutOfRangeException(nameof(byteIndex));
			if (byteCount < 0) throw new ArgumentOutOfRangeException(nameof(byteCount));
			if (byteIndex + byteCount > bytes.Length) throw new ArgumentOutOfRangeException(nameof(byteCount));
			if (chars == null) throw new NullReferenceException(nameof(chars));
			if (charIndex < 0) throw new ArgumentOutOfRangeException(nameof(charIndex));
			if (charIndex > chars.Length) throw new ArgumentOutOfRangeException(nameof(charIndex));

#if NETCOREAPP
			this.EncodeInternal<byte, byte>(bytes.AsSpan(byteIndex, byteCount), chars.AsSpan(charIndex, charCount), flush, out bytesUsed, out charsUsed, out completed);
#else
			this.EncodeInternal(bytes, byteIndex, byteCount, chars, charIndex, charCount, flush, out bytesUsed, out charsUsed, out completed);
#endif
		}
		/// <summary>
		/// See description on similar conversion methods. This is just overload with different buffer types.
		/// </summary>
		public unsafe void Convert(byte* bytes, int byteCount, byte* chars, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
		{
			if (bytes == null) throw new NullReferenceException(nameof(bytes));
			if (byteCount < 0) throw new ArgumentOutOfRangeException(nameof(byteCount));
			if (chars == null) throw new NullReferenceException(nameof(chars));
			if (charCount < 0) throw new ArgumentOutOfRangeException(nameof(charCount));

#if NETCOREAPP
			this.EncodeInternal(new ReadOnlySpan<byte>(bytes, byteCount), new Span<byte>(chars, charCount), flush, out bytesUsed, out charsUsed, out completed);
#else
			this.EncodeInternal((ByteSafePtr)bytes, 0, byteCount, (ByteSafePtr)chars, 0, charCount, flush, out bytesUsed, out charsUsed, out completed);
#endif
		}
#if NETSTANDARD1_6
		/// <summary>
		/// See description on similar conversion methods. This is just overload with different buffer types.
		/// </summary>
		public unsafe void Convert(byte* bytes, int byteCount, char* chars, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
#else
		/// <inheritdoc />
		public override unsafe void Convert(byte* bytes, int byteCount, char* chars, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
#endif
		{
			if (bytes == null) throw new NullReferenceException(nameof(bytes));
			if (byteCount < 0) throw new ArgumentOutOfRangeException(nameof(byteCount));
			if (chars == null) throw new NullReferenceException(nameof(chars));
			if (charCount < 0) throw new ArgumentOutOfRangeException(nameof(charCount));

#if NETCOREAPP
			this.EncodeInternal(new ReadOnlySpan<byte>(bytes, byteCount), new Span<char>(chars, charCount), flush, out bytesUsed, out charsUsed, out completed);
#else
			this.EncodeInternal((ByteSafePtr)bytes, 0, byteCount, (CharSafePtr)chars, 0, charCount, flush, out bytesUsed, out charsUsed, out completed);
#endif
		}
		/// <inheritdoc />
		public override void Convert(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
		{
			if (bytes == null) throw new NullReferenceException(nameof(bytes));
			if (byteIndex < 0) throw new ArgumentOutOfRangeException(nameof(byteIndex));
			if (byteCount < 0) throw new ArgumentOutOfRangeException(nameof(byteCount));
			if (byteIndex + byteCount > bytes.Length) throw new ArgumentOutOfRangeException(nameof(byteCount));
			if (chars == null) throw new NullReferenceException(nameof(chars));
			if (charIndex < 0) throw new ArgumentOutOfRangeException(nameof(charIndex));
			if (charIndex > chars.Length) throw new ArgumentOutOfRangeException(nameof(charIndex));

#if NETCOREAPP
			this.EncodeInternal<byte, char>(bytes.AsSpan(byteIndex, byteCount), chars.AsSpan(charIndex, charCount), flush, out bytesUsed, out charsUsed, out completed);
#else
			this.EncodeInternal(bytes, byteIndex, byteCount, chars, charIndex, charCount, flush, out bytesUsed, out charsUsed, out completed);
#endif
		}

#if NETCOREAPP
		/// <summary>
		/// See description on similar conversion methods. This is just overload with different buffer types.
		/// </summary>
		public void Convert(ReadOnlySpan<byte> bytes, Span<byte> chars, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
		{
			this.EncodeInternal(bytes, chars, flush, out bytesUsed, out charsUsed, out completed);
		}
		/// <inheritdoc />
		public override void Convert(ReadOnlySpan<byte> bytes, Span<char> chars, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
		{
			this.EncodeInternal(bytes, chars, flush, out bytesUsed, out charsUsed, out completed);
		}
		/// <inheritdoc />
		public override int GetCharCount(ReadOnlySpan<byte> bytes, bool flush)
		{
			return this.GetCharCount(bytes.Length, flush);
		}
		/// <inheritdoc />
		public override int GetChars(ReadOnlySpan<byte> bytes, Span<char> chars, bool flush)
		{
			this.Convert(bytes, chars, flush, out _, out var charsUsed, out _);
			return charsUsed;
		}
#endif

#if NETCOREAPP
		private void EncodeInternal<InputT, OutputT>(ReadOnlySpan<InputT> input, Span<OutputT> output, bool flush, out int inputUsed, out int outputUsed, out bool completed) where InputT : unmanaged where OutputT : unmanaged
		{
			if (input.IsEmpty || output.IsEmpty)
			{
				inputUsed = outputUsed = 0;
				completed = true;
				return;
			}
#else
		private unsafe void EncodeInternal<InputT, OutputT>(InputT input, int inputOffset, int inputCount, OutputT output, int outputOffset, int outputCount, bool flush, out int inputUsed, out int outputUsed, out bool completed)
		{
			if (inputCount == 0 || outputCount == 0)
			{
				inputUsed = outputUsed = 0;
				completed = true;
				return;
			}
#endif

			// #1: preparing
			var i = 0;
			var alphabetChars = this.Alphabet.Alphabet;
			var inputBlockSize = this.Alphabet.EncodingBlockSize;
			var outputBlockSize = this.Alphabet.DecodingBlockSize;
			var encodingMask = (ulong)alphabetChars.Length - 1;
			var encodingBits = this.Alphabet.EncodingBits;
#if NETCOREAPP
			var originalInputOffset = 0;
			var inputOffset = 0;
			var inputCount = input.Length;
			var originalOutputOffset = 0;
			var outputOffset = 0;
			var outputCount = output.Length;
#else
			var originalInputOffset = inputOffset;
			var originalOutputOffset = outputOffset;
			var outputBytes = default(byte[]);
			var outputChars = default(char[]);
			var outputBytePtr = default(byte*);
			var outputCharPtr = default(char*);
			var inputBytes = default(byte[]);
			var inputChars = default(char[]);
			var inputBytePtr = default(byte*);
			var inputCharPtr = default(char*);

			if (typeof(OutputT) == typeof(byte[]))
			{
				outputBytes = (byte[])(object)output;
			}
			else if (typeof(OutputT) == typeof(char[]))
			{
				outputChars = (char[])(object)output;
			}
			else if (typeof(OutputT) == typeof(ByteSafePtr))
			{
				outputBytePtr = (byte*)(ByteSafePtr)(object)output;
			}
			else if (typeof(OutputT) == typeof(CharSafePtr))
			{
				outputCharPtr = (char*)(CharSafePtr)(object)output;
			}
			else
			{
				throw new InvalidOperationException("Unknown type of output buffer: " + typeof(OutputT));
			}

			if (typeof(InputT) == typeof(byte[]))
			{
				inputBytes = (byte[])(object)input;
			}
			else if (typeof(InputT) == typeof(char[]))
			{
				inputChars = (char[])(object)input;
			}
			else if (typeof(InputT) == typeof(ByteSafePtr))
			{
				inputBytePtr = (byte*)(ByteSafePtr)(object)input;
			}
			else if (typeof(InputT) == typeof(CharSafePtr))
			{
				inputCharPtr = (char*)(CharSafePtr)(object)input;
			}
			else
			{
				throw new InvalidOperationException("Unknown type of input buffer: " + typeof(InputT));
			}
#endif
			// #2: encoding whole blocks

			var wholeBlocksToProcess = Math.Min(inputCount / inputBlockSize, outputCount / outputBlockSize);
			var inputBlock = 0UL; // 1 byte for Base16, 5 bytes for Base32 and 3 bytes for Base64
			var outputBlock = 0UL; // 2 bytes for Base16, 8 bytes for Base32 and 4 bytes for Base64
			while (wholeBlocksToProcess-- > 0)
			{
				// filling input
#if NETCOREAPP
				if (typeof(InputT) == typeof(byte))
				{
					for (i = 0; i < inputBlockSize; i++)
					{
						inputBlock <<= 8;
						inputBlock |= (byte)(object)input[inputOffset++];
					}
				}
				if (typeof(InputT) == typeof(char))
				{
					for (i = 0; i < inputBlockSize; i++)
					{
						inputBlock <<= 8;
						inputBlock |= (char)(object)input[inputOffset++];
					}
				}
#else
				if (typeof(InputT) == typeof(byte[]))
				{
					for (i = 0; i < inputBlockSize; i++)
					{
						inputBlock <<= 8;
						inputBlock |= inputBytes[inputOffset++];
					}
				}
				if (typeof(InputT) == typeof(char[]))
				{
					for (i = 0; i < inputBlockSize; i++)
					{
						inputBlock <<= 8;
						inputBlock |= inputChars[inputOffset++];
					}
				}
				if (typeof(InputT) == typeof(ByteSafePtr))
				{
					for (i = 0; i < inputBlockSize; i++)
					{
						inputBlock <<= 8;
						inputBlock |= inputBytePtr[inputOffset++];
					}
				}
				if (typeof(InputT) == typeof(CharSafePtr))
				{
					for (i = 0; i < inputBlockSize; i++)
					{
						inputBlock <<= 8;
						inputBlock |= inputCharPtr[inputOffset++];
					}
				}
#endif

				// encoding
				for (i = 0; i < outputBlockSize; i++)
				{
					outputBlock <<= 8;
					outputBlock |= alphabetChars[(int)(inputBlock & encodingMask)];
					inputBlock >>= encodingBits;
				}
				// flush output
#if NETCOREAPP
				if (typeof(OutputT) == typeof(byte))
				{
					for (i = 0; i < outputBlockSize; i++)
					{
						output[outputOffset++] = (OutputT)(object)(byte)(outputBlock & 0xFF);
						outputBlock >>= 8;
					}
				}
				if (typeof(OutputT) == typeof(char))
				{
					for (i = 0; i < outputBlockSize; i++)
					{
						output[outputOffset++] = (OutputT)(object)(char)(outputBlock & 0xFF);
						outputBlock >>= 8;
					}
				}
#else
				if (typeof(OutputT) == typeof(byte[]))
				{
					Debug.Assert(outputBytes != null, "byteSegment.Array != null");

					for (i = 0; i < outputBlockSize; i++)
					{
						outputBytes[outputOffset++] = (byte)(outputBlock & 0xFF);
						outputBlock >>= 8;
					}
				}
				if (typeof(OutputT) == typeof(char[]))
				{
					Debug.Assert(outputChars != null, "charSegment.Array != null");

					for (i = 0; i < outputBlockSize; i++)
					{
						outputChars[outputOffset++] = (char)(outputBlock & 0xFF);
						outputBlock >>= 8;
					}
				}
				if (typeof(OutputT) == typeof(ByteSafePtr))
				{
					Debug.Assert(outputBytePtr != default(byte*), "outputBytePtr != default(byte*)");

					for (i = 0; i < outputBlockSize; i++)
					{
						outputBytePtr[outputOffset++] = (byte)(outputBlock & 0xFF);
						outputBlock >>= 8;
					}
				}
				if (typeof(OutputT) == typeof(CharSafePtr))
				{
					Debug.Assert(outputCharPtr != default(char*), "outputCharPtr != default(char*)");

					for (i = 0; i < outputBlockSize; i++)
					{
						outputCharPtr[outputOffset++] = (char)(outputBlock & 0xFF);
						outputBlock >>= 8;
					}
				}
#endif


				outputCount -= outputBlockSize;
				inputCount -= inputBlockSize;
			}

			// #3: encoding partial blocks
			outputBlock = 0;
			inputBlock = 0;
			var finalOutputBlockSize = (int)Math.Ceiling(Math.Min(inputCount, inputBlockSize) * 8.0 / encodingBits);

			// filling input for final block
#if NETCOREAPP
			if (typeof(InputT) == typeof(byte))
			{
				for (i = 0; i < inputBlockSize && i < inputCount; i++)
				{
					inputBlock <<= 8;
					inputBlock |= (byte)(object)input[inputOffset++];
				}
			}
			if (typeof(InputT) == typeof(char))
			{
				for (i = 0; i < inputBlockSize; i++)
				{
					inputBlock <<= 8;
					inputBlock |= (char)(object)input[inputOffset++];
				}
			}
#else
			if (typeof(InputT) == typeof(byte[]))
			{
				for (i = 0; i < inputBlockSize && i < inputCount; i++)
				{
					inputBlock <<= 8;
					inputBlock |= inputBytes[inputOffset++];
				}
			}
			if (typeof(InputT) == typeof(char[]))
			{
				for (i = 0; i < inputBlockSize && i < inputCount; i++)
				{
					inputBlock <<= 8;
					inputBlock |= inputChars[inputOffset++];
				}
			}
			if (typeof(InputT) == typeof(ByteSafePtr))
			{
				for (i = 0; i < inputBlockSize && i < inputCount; i++)
				{
					inputBlock <<= 8;
					inputBlock |= inputBytePtr[inputOffset++];
				}
			}
			if (typeof(InputT) == typeof(CharSafePtr))
			{
				for (i = 0; i < inputBlockSize && i < inputCount; i++)
				{
					inputBlock <<= 8;
					inputBlock |= inputCharPtr[inputOffset++];
				}
			}
#endif
			// align with encodingBits
			inputBlock <<= encodingBits - Math.Min(inputBlockSize, inputCount) * 8 % encodingBits;

			// fill output with paddings
			for (i = 0; i < outputBlockSize; i++)
			{
				outputBlock <<= 8;
				outputBlock |= (byte)this.Alphabet.Padding;
			}

			// encode final block
			for (i = 0; i < finalOutputBlockSize; i++)
			{
				outputBlock <<= 8;
				outputBlock |= alphabetChars[(int)(inputBlock & encodingMask)];
				inputBlock >>= encodingBits;
			}

			if (this.Alphabet.HasPadding && inputCount > 0)
			{
				finalOutputBlockSize = outputBlockSize;
			}

			// flush final block
			if (finalOutputBlockSize > outputCount || !flush)
			{
				finalOutputBlockSize = 0; // cancel flushing output
				inputOffset -= Math.Min(inputBlockSize, inputCount); // rewind input
			}
			else
			{
				inputCount -= Math.Min(inputBlockSize, inputCount);
			}

#if NETCOREAPP
			if (typeof(OutputT) == typeof(byte))
			{
				for (i = 0; i < finalOutputBlockSize; i++)
				{
					output[outputOffset++] = (OutputT)(object)(byte)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}
			}
			if (typeof(OutputT) == typeof(char))
			{
				for (i = 0; i < finalOutputBlockSize; i++)
				{
					output[outputOffset++] = (OutputT)(object)(char)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}
			}
#else
			if (typeof(OutputT) == typeof(byte[]))
			{
				Debug.Assert(outputBytes != null, "byteSegment.Array != null");

				for (i = 0; i < finalOutputBlockSize; i++)
				{
					outputBytes[outputOffset++] = (byte)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}
			}
			if (typeof(OutputT) == typeof(char[]))
			{
				Debug.Assert(outputChars != null, "charSegment.Array != null");

				for (i = 0; i < finalOutputBlockSize; i++)
				{
					outputChars[outputOffset++] = (char)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}
			}
			if (typeof(OutputT) == typeof(ByteSafePtr))
			{
				Debug.Assert(outputBytePtr != default(byte*), "outputBytePtr != default(byte*)");

				for (i = 0; i < finalOutputBlockSize; i++)
				{
					outputBytePtr[outputOffset++] = (byte)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}
			}
			if (typeof(OutputT) == typeof(CharSafePtr))
			{
				Debug.Assert(outputCharPtr != default(char*), "outputCharPtr != default(char*)");

				for (i = 0; i < finalOutputBlockSize; i++)
				{
					outputCharPtr[outputOffset++] = (char)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}
			}
#endif

			inputUsed = inputOffset - originalInputOffset;
			outputUsed = outputOffset - originalOutputOffset;
			completed = inputCount == 0; // true if all input is used
		}

		private int GetCharCount(int count, bool flush)
		{
			if (count == 0)
				return 0;

			var wholeBlocksSize = checked(count / this.Alphabet.EncodingBlockSize * this.Alphabet.DecodingBlockSize);
			var finalBlockSize = (int)Math.Ceiling(count % this.Alphabet.EncodingBlockSize * 8.0 / this.Alphabet.EncodingBits);
			if (this.Alphabet.HasPadding && finalBlockSize != 0)
			{
				finalBlockSize = this.Alphabet.DecodingBlockSize;
			}
			if (!flush)
			{
				finalBlockSize = 0;
			}
			return checked(wholeBlocksSize + finalBlockSize);
		}

		/// <inheritdoc />
		public override string ToString() => $"Base{this.Alphabet.Alphabet.Length}Decoder, {new string(this.Alphabet.Alphabet)}";
	}
}