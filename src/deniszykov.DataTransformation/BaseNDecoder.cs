using System;
using System.Diagnostics;
using System.Text;

namespace deniszykov.BaseN
{
	/// <summary>
	/// Base-(Alphabet Length) binary data encoder (!) based on specified <see cref="baseNAlphabet"/>.
	/// Class named "Decoder" because it is based on <see cref="Decoder"/>, but it is effectively encoder.
	/// </summary>
	public sealed class BaseNDecoder : Decoder
	{
		private readonly BaseNAlphabet baseNAlphabet;

		/// <summary>
		/// Constructor of <see cref="BaseNDecoder"/>.
		/// </summary>
		/// <param name="baseNAlphabet"></param>
		public BaseNDecoder(BaseNAlphabet baseNAlphabet)
		{
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));

			this.baseNAlphabet = baseNAlphabet;
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
			return ((byteCount + this.baseNAlphabet.EncodingBlockSize - 1) / this.baseNAlphabet.EncodingBlockSize) * this.baseNAlphabet.DecodingBlockSize;
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
		public void Convert(byte[] bytes, int byteIndex, int byteCount, byte[] chars, int charIndex, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
		{
			this.EncodeInternal(bytes, byteIndex, byteCount, chars, charIndex, charCount, flush, out bytesUsed, out charsUsed, out completed);
		}
		/// <summary>
		/// See description on similar conversion methods. This is just overload with different buffer types.
		/// </summary>
		public unsafe void Convert(byte* bytes, int byteCount, byte* chars, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
		{
			bytesUsed = charsUsed = 0;
			if (byteCount == 0)
			{
				completed = true;
			}

			var i = 0;
			var alphabetChars = this.baseNAlphabet.Alphabet;
			var inputBlockSize = this.baseNAlphabet.EncodingBlockSize;
			var outputBlockSize = this.baseNAlphabet.DecodingBlockSize;
			var encodingMask = (ulong)alphabetChars.Length - 1;
			var encodingBits = this.baseNAlphabet.EncodingBits;

			// #2: encoding whole blocks

			var wholeBlocksToProcess = Math.Min(byteCount / inputBlockSize, charCount / outputBlockSize);
			var inputBlock = 0UL; // 1 byte for Base16, 5 bytes for Base32 and 3 bytes for Base64
			var outputBlock = 0UL; // 2 bytes for Base16, 8 bytes for Base32 and 4 bytes for Base64
			while (wholeBlocksToProcess-- > 0)
			{
				// filling input
				for (i = 0; i < inputBlockSize; i++)
				{
					inputBlock <<= 8;
					inputBlock |= bytes[bytesUsed++];
				}
				// encoding
				for (i = 0; i < outputBlockSize; i++)
				{
					outputBlock <<= 8;
					outputBlock |= alphabetChars[(int)(inputBlock & encodingMask)];
					inputBlock >>= encodingBits;
				}
				// flush output
				for (i = 0; i < outputBlockSize; i++)
				{
					chars[charsUsed++] = (byte)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}

				charCount -= outputBlockSize;
				byteCount -= inputBlockSize;
			}

			// #3: encoding partial blocks
			outputBlock = 0;
			inputBlock = 0;
			var finalOutputBlockSize = (int)Math.Ceiling(Math.Min(byteCount, inputBlockSize) * 8.0 / encodingBits);

			// filling input for final block
			for (i = 0; i < inputBlockSize && i < byteCount; i++)
			{
				inputBlock <<= 8;
				inputBlock |= bytes[bytesUsed++];
			}
			// align with encodingBits
			inputBlock <<= encodingBits - Math.Min(inputBlockSize, byteCount) * 8 % encodingBits;

			// fill output with paddings
			for (i = 0; i < outputBlockSize; i++)
			{
				outputBlock <<= 8;
				outputBlock |= (byte)this.baseNAlphabet.Padding;
			}

			// encode final block
			for (i = 0; i < finalOutputBlockSize; i++)
			{
				outputBlock <<= 8;
				outputBlock |= alphabetChars[(int)(inputBlock & encodingMask)];
				inputBlock >>= encodingBits;
			}

			if (this.baseNAlphabet.HasPadding && byteCount > 0)
			{
				finalOutputBlockSize = outputBlockSize;
			}

			// flush final block
			if (finalOutputBlockSize > charCount || !flush)
			{
				finalOutputBlockSize = 0; // cancel flushing output
				bytesUsed -= Math.Min(inputBlockSize, byteCount); // rewind input
			}
			else
			{
				byteCount -= Math.Min(inputBlockSize, byteCount);
			}

			for (i = 0; i < finalOutputBlockSize; i++)
			{
				chars[charsUsed++] = (byte)(outputBlock & 0xFF);
				outputBlock >>= 8;
			}

			completed = byteCount == 0; // true if all input is used
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
			bytesUsed = charsUsed = 0;

			var i = 0;
			var alphabetChars = this.baseNAlphabet.Alphabet;
			var inputBlockSize = this.baseNAlphabet.EncodingBlockSize;
			var outputBlockSize = this.baseNAlphabet.DecodingBlockSize;
			var encodingMask = (ulong)alphabetChars.Length - 1;
			var encodingBits = this.baseNAlphabet.EncodingBits;

			bytesUsed = 0;
			charsUsed = 0;

			// #2: encoding whole blocks

			var wholeBlocksToProcess = Math.Min(byteCount / inputBlockSize, charCount / outputBlockSize);
			var inputBlock = 0UL; // 1 byte for Base16, 5 bytes for Base32 and 3 bytes for Base64
			var outputBlock = 0UL; // 2 bytes for Base16, 8 bytes for Base32 and 4 bytes for Base64
			while (wholeBlocksToProcess-- > 0)
			{
				// filling input
				for (i = 0; i < inputBlockSize; i++)
				{
					inputBlock <<= 8;
					inputBlock |= bytes[bytesUsed++];
				}
				// encoding
				for (i = 0; i < outputBlockSize; i++)
				{
					outputBlock <<= 8;
					outputBlock |= alphabetChars[(int)(inputBlock & encodingMask)];
					inputBlock >>= encodingBits;
				}
				// flush output
				for (i = 0; i < outputBlockSize; i++)
				{
					chars[charsUsed++] = (char)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}

				charCount -= outputBlockSize;
				byteCount -= inputBlockSize;
			}

			// #3: encoding partial blocks
			outputBlock = 0;
			inputBlock = 0;
			var finalOutputBlockSize = (int)Math.Ceiling(Math.Min(byteCount, inputBlockSize) * 8.0 / encodingBits);

			// filling input for final block
			for (i = 0; i < inputBlockSize && i < byteCount; i++)
			{
				inputBlock <<= 8;
				inputBlock |= bytes[bytesUsed++];
			}
			// align with encodingBits
			inputBlock <<= encodingBits - Math.Min(inputBlockSize, byteCount) * 8 % encodingBits;

			// fill output with paddings
			for (i = 0; i < outputBlockSize; i++)
			{
				outputBlock <<= 8;
				outputBlock |= (byte)this.baseNAlphabet.Padding;
			}

			// encode final block
			for (i = 0; i < finalOutputBlockSize; i++)
			{
				outputBlock <<= 8;
				outputBlock |= alphabetChars[(int)(inputBlock & encodingMask)];
				inputBlock >>= encodingBits;
			}

			if (this.baseNAlphabet.HasPadding && byteCount > 0)
			{
				finalOutputBlockSize = outputBlockSize;
			}

			// flush final block
			if (finalOutputBlockSize > charCount || !flush)
			{
				finalOutputBlockSize = 0; // cancel flushing output
				bytesUsed -= Math.Min(inputBlockSize, byteCount); // rewind input
			}
			else
			{
				byteCount -= Math.Min(inputBlockSize, byteCount);
			}

			for (i = 0; i < finalOutputBlockSize; i++)
			{
				chars[charsUsed++] = (char)(outputBlock & 0xFF);
				outputBlock >>= 8;
			}

			completed = byteCount == 0; // true if all input is used
		}
		/// <inheritdoc />
		public override void Convert(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
		{
			this.EncodeInternal(bytes, byteIndex, byteCount, chars, charIndex, charCount, flush, out bytesUsed, out charsUsed, out completed);
		}

#if NETCOREAPP
		/// <summary>
		/// See description on similar conversion methods. This is just overload with different buffer types.
		/// </summary>
		public void Convert(ReadOnlySpan<byte> bytes, Span<byte> chars, bool flush, out int bytesUsed, out int charCount, out bool completed)
		{
			completed = true;
			bytesUsed = charCount = 0;

			if (bytes.Length == 0 || chars.Length == 0)
			{
				return;
			}

			var i = 0;
			var alphabetChars = this.baseNAlphabet.Alphabet;
			var inputBlockSize = this.baseNAlphabet.EncodingBlockSize;
			var outputBlockSize = this.baseNAlphabet.DecodingBlockSize;
			var encodingMask = (ulong)alphabetChars.Length - 1;
			var encodingBits = this.baseNAlphabet.EncodingBits;
			var outputCapacity = chars.Length;
			var inputLeft = bytes.Length;

			// #2: encoding whole blocks

			var wholeBlocksToProcess = Math.Min(bytes.Length / inputBlockSize, outputCapacity / outputBlockSize);
			var inputBlock = 0UL; // 1 byte for Base16, 5 bytes for Base32 and 3 bytes for Base64
			var outputBlock = 0UL; // 2 bytes for Base16, 8 bytes for Base32 and 4 bytes for Base64
			while (wholeBlocksToProcess-- > 0)
			{
				// filling input
				for (i = 0; i < inputBlockSize; i++)
				{
					inputBlock <<= 8;
					inputBlock |= bytes[bytesUsed++];
				}
				// encoding
				for (i = 0; i < outputBlockSize; i++)
				{
					outputBlock <<= 8;
					outputBlock |= alphabetChars[(int)(inputBlock & encodingMask)];
					inputBlock >>= encodingBits;
				}
				// flush output
				for (i = 0; i < outputBlockSize; i++)
				{
					chars[charCount++] = (byte)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}

				outputCapacity -= outputBlockSize;
				inputLeft -= inputBlockSize;
			}

			// #3: encoding partial blocks
			outputBlock = 0;
			inputBlock = 0;
			var finalOutputBlockSize = (int)Math.Ceiling(Math.Min(inputLeft, inputBlockSize) * 8.0 / encodingBits);

			// filling input for final block
			for (i = 0; i < inputBlockSize && i < inputLeft; i++)
			{
				inputBlock <<= 8;
				inputBlock |= bytes[bytesUsed++];
			}
			// align with encodingBits
			inputBlock <<= encodingBits - Math.Min(inputBlockSize, inputLeft) * 8 % encodingBits;

			// fill output with paddings
			for (i = 0; i < outputBlockSize; i++)
			{
				outputBlock <<= 8;
				outputBlock |= (byte)this.baseNAlphabet.Padding;
			}

			// encode final block
			for (i = 0; i < finalOutputBlockSize; i++)
			{
				outputBlock <<= 8;
				outputBlock |= alphabetChars[(int)(inputBlock & encodingMask)];
				inputBlock >>= encodingBits;
			}

			if (this.baseNAlphabet.HasPadding && inputLeft > 0)
			{
				finalOutputBlockSize = outputBlockSize;
			}

			// flush final block
			if (finalOutputBlockSize > outputCapacity || !flush)
			{
				finalOutputBlockSize = 0; // cancel flushing output
				bytesUsed -= Math.Min(inputBlockSize, inputLeft); // rewind input
			}
			else
			{
				inputLeft -= Math.Min(inputBlockSize, inputLeft);
			}

			for (i = 0; i < finalOutputBlockSize; i++)
			{
				chars[charCount++] = (byte)(outputBlock & 0xFF);
				outputBlock >>= 8;
			}
			completed = inputLeft == 0; // true if all input is used
		}
		/// <inheritdoc />
		public override void Convert(ReadOnlySpan<byte> bytes, Span<char> chars, bool flush, out int bytesUsed, out int charCount, out bool completed)
		{
			completed = true;
			bytesUsed = charCount = 0;

			if (bytes.Length == 0 || chars.Length == 0)
			{
				return;
			}

			var i = 0;
			var alphabetChars = this.baseNAlphabet.Alphabet;
			var inputBlockSize = this.baseNAlphabet.EncodingBlockSize;
			var outputBlockSize = this.baseNAlphabet.DecodingBlockSize;
			var encodingMask = (ulong)alphabetChars.Length - 1;
			var encodingBits = this.baseNAlphabet.EncodingBits;
			var outputCapacity = chars.Length;
			var inputLeft = bytes.Length;

			// #2: encoding whole blocks

			var wholeBlocksToProcess = Math.Min(bytes.Length / inputBlockSize, outputCapacity / outputBlockSize);
			var inputBlock = 0UL; // 1 byte for Base16, 5 bytes for Base32 and 3 bytes for Base64
			var outputBlock = 0UL; // 2 bytes for Base16, 8 bytes for Base32 and 4 bytes for Base64
			while (wholeBlocksToProcess-- > 0)
			{
				// filling input
				for (i = 0; i < inputBlockSize; i++)
				{
					inputBlock <<= 8;
					inputBlock |= bytes[bytesUsed++];
				}
				// encoding
				for (i = 0; i < outputBlockSize; i++)
				{
					outputBlock <<= 8;
					outputBlock |= alphabetChars[(int)(inputBlock & encodingMask)];
					inputBlock >>= encodingBits;
				}
				// flush output
				for (i = 0; i < outputBlockSize; i++)
				{
					chars[charCount++] = (char)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}

				outputCapacity -= outputBlockSize;
				inputLeft -= inputBlockSize;
			}

			// #3: encoding partial blocks
			outputBlock = 0;
			inputBlock = 0;
			var finalOutputBlockSize = (int)Math.Ceiling(Math.Min(inputLeft, inputBlockSize) * 8.0 / encodingBits);

			// filling input for final block
			for (i = 0; i < inputBlockSize && i < inputLeft; i++)
			{
				inputBlock <<= 8;
				inputBlock |= bytes[bytesUsed++];
			}
			// align with encodingBits
			inputBlock <<= encodingBits - Math.Min(inputBlockSize, inputLeft) * 8 % encodingBits;

			// fill output with paddings
			for (i = 0; i < outputBlockSize; i++)
			{
				outputBlock <<= 8;
				outputBlock |= (byte)this.baseNAlphabet.Padding;
			}

			// encode final block
			for (i = 0; i < finalOutputBlockSize; i++)
			{
				outputBlock <<= 8;
				outputBlock |= alphabetChars[(int)(inputBlock & encodingMask)];
				inputBlock >>= encodingBits;
			}

			if (this.baseNAlphabet.HasPadding && inputLeft > 0)
			{
				finalOutputBlockSize = outputBlockSize;
			}

			// flush final block
			if (finalOutputBlockSize > outputCapacity || !flush)
			{
				finalOutputBlockSize = 0; // cancel flushing output
				bytesUsed -= Math.Min(inputBlockSize, inputLeft); // rewind input
			}
			else
			{
				inputLeft -= Math.Min(inputBlockSize, inputLeft);
			}

			for (i = 0; i < finalOutputBlockSize; i++)
			{
				chars[charCount++] = (char)(outputBlock & 0xFF);
				outputBlock >>= 8;
			}

			completed = inputLeft == 0; // true if all input is used
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

		private void EncodeInternal<BufferT>(byte[] input, int inputOffset, int inputCount, BufferT output, int outputIndex, int outputCount, bool flush, out int inputUsed, out int outputUsed, out bool completed)
		{
			if (inputCount == 0 || input == null)
			{
				inputUsed = outputUsed = 0;
				completed = true;
				return;
			}

			// #1: preparing
			var i = 0;
			var alphabetChars = this.baseNAlphabet.Alphabet;
			var inputBlockSize = this.baseNAlphabet.EncodingBlockSize;
			var outputBlockSize = this.baseNAlphabet.DecodingBlockSize;
			var encodingMask = (ulong)alphabetChars.Length - 1;
			var encodingBits = this.baseNAlphabet.EncodingBits;
			var originalInputOffset = inputOffset;
			var originalOutputOffset = outputIndex;
			var outputBytes = default(byte[]);
			var outputChars = default(char[]);

			if (output is byte[])
			{
				outputBytes = (byte[])(object)output;
			}
			else if (output is char[])
			{
				outputChars = (char[])(object)output;
			}
			else
			{
				throw new InvalidOperationException("Unknown type of output buffer: " + typeof(BufferT));
			}

			// #2: encoding whole blocks

			var wholeBlocksToProcess = Math.Min(inputCount / inputBlockSize, outputCount / outputBlockSize);
			var inputBlock = 0UL; // 1 byte for Base16, 5 bytes for Base32 and 3 bytes for Base64
			var outputBlock = 0UL; // 2 bytes for Base16, 8 bytes for Base32 and 4 bytes for Base64
			while (wholeBlocksToProcess-- > 0)
			{
				// filling input
				for (i = 0; i < inputBlockSize; i++)
				{
					inputBlock <<= 8;
					inputBlock |= input[inputOffset++];
				}
				// encoding
				for (i = 0; i < outputBlockSize; i++)
				{
					outputBlock <<= 8;
					outputBlock |= alphabetChars[(int)(inputBlock & encodingMask)];
					inputBlock >>= encodingBits;
				}
				// flush output
				if (output is byte[])
				{
					Debug.Assert(outputBytes != null, "byteSegment.Array != null");

					for (i = 0; i < outputBlockSize; i++)
					{
						outputBytes[outputIndex++] = (byte)(outputBlock & 0xFF);
						outputBlock >>= 8;
					}
				}
				else
				{
					Debug.Assert(outputChars != null, "charSegment.Array != null");

					for (i = 0; i < outputBlockSize; i++)
					{
						outputChars[outputIndex++] = (char)(outputBlock & 0xFF);
						outputBlock >>= 8;
					}
				}

				outputCount -= outputBlockSize;
				inputCount -= inputBlockSize;
			}

			// #3: encoding partial blocks
			outputBlock = 0;
			inputBlock = 0;
			var finalOutputBlockSize = (int)Math.Ceiling(Math.Min(inputCount, inputBlockSize) * 8.0 / encodingBits);

			// filling input for final block
			for (i = 0; i < inputBlockSize && i < inputCount; i++)
			{
				inputBlock <<= 8;
				inputBlock |= input[inputOffset++];
			}
			// align with encodingBits
			inputBlock <<= encodingBits - Math.Min(inputBlockSize, inputCount) * 8 % encodingBits;

			// fill output with paddings
			for (i = 0; i < outputBlockSize; i++)
			{
				outputBlock <<= 8;
				outputBlock |= (byte)this.baseNAlphabet.Padding;
			}

			// encode final block
			for (i = 0; i < finalOutputBlockSize; i++)
			{
				outputBlock <<= 8;
				outputBlock |= alphabetChars[(int)(inputBlock & encodingMask)];
				inputBlock >>= encodingBits;
			}

			if (this.baseNAlphabet.HasPadding && inputCount > 0)
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

			if (output is byte[])
			{
				Debug.Assert(outputBytes != null, "byteSegment.Array != null");

				for (i = 0; i < finalOutputBlockSize; i++)
				{
					outputBytes[outputIndex++] = (byte)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}
			}
			else
			{
				Debug.Assert(outputChars != null, "charSegment.Array != null");

				for (i = 0; i < finalOutputBlockSize; i++)
				{
					outputChars[outputIndex++] = (char)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}
			}

			inputUsed = inputOffset - originalInputOffset;
			outputUsed = outputIndex - originalOutputOffset;
			completed = inputCount == 0; // true if all input is used
		}
		private int GetCharCount(int count, bool flush)
		{
			if (count == 0)
				return 0;

			var wholeBlocksSize = checked(count / this.baseNAlphabet.EncodingBlockSize * this.baseNAlphabet.DecodingBlockSize);
			var finalBlockSize = (int)Math.Ceiling(count % this.baseNAlphabet.EncodingBlockSize * 8.0 / this.baseNAlphabet.EncodingBits);
			if (this.baseNAlphabet.HasPadding && finalBlockSize != 0)
			{
				finalBlockSize = this.baseNAlphabet.DecodingBlockSize;
			}
			if (!flush)
			{
				finalBlockSize = 0;
			}
			return checked(wholeBlocksSize + finalBlockSize);
		}

		/// <inheritdoc />
		public override string ToString() => $"Base{this.baseNAlphabet.Alphabet.Length}Decoder, {new string(this.baseNAlphabet.Alphabet)}";
	}
}