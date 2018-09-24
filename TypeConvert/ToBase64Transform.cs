using System.Security.Cryptography;

// ReSharper disable once CheckNamespace
namespace System
{
	partial class Base64Convert
	{
		/// <summary>
		/// Streaming converter for "bytes to base64" transformation. Could be used with <see cref="CryptoStream"/>.
		/// </summary>
		public struct ToBase64Transform : ICryptoTransform
		{
			private static readonly Base64Alphabet DefaultAlphabet = new Base64Alphabet("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".ToCharArray(), padding: '=');

			private readonly Base64Alphabet alphabet;

			/// <inheritdoc />
			public int InputBlockSize => 4;
			/// <inheritdoc />
			public int OutputBlockSize => 3;
			/// <inheritdoc />
			public bool CanTransformMultipleBlocks => true;
			/// <inheritdoc />
			public bool CanReuseTransform => true;

			/// <summary>
			/// Creates new instance with specified <paramref name="alphabet"/>.
			/// </summary>
			public ToBase64Transform(Base64Alphabet alphabet)
			{
				if (alphabet == null) throw new ArgumentNullException("alphabet");

				this.alphabet = alphabet;
			}

			/// <inheritdoc />
			public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
			{
				if (inputBuffer == null) throw new ArgumentNullException(nameof(inputBuffer));
				if (outputBuffer == null) throw new ArgumentNullException(nameof(outputBuffer));
				if (inputOffset < 0) throw new ArgumentOutOfRangeException(nameof(inputOffset));
				if (outputOffset < 0 || outputOffset > outputBuffer.Length) throw new ArgumentOutOfRangeException(nameof(outputOffset));
				if (inputCount < 0 || inputOffset + inputCount > inputBuffer.Length) throw new ArgumentOutOfRangeException(nameof(inputCount));

				if (inputCount == 0) return 0;

				var base64Alphabet = this.alphabet ?? DefaultAlphabet;
				var lastChars = inputCount % 3;
				var startingOutputOffset = outputOffset;
				var quartetEnd = inputOffset + (inputCount - lastChars);
				var base64Chars = Base64Alphabet.Alphabet;
				for (; inputOffset < quartetEnd; inputOffset += 3)
				{
					outputBuffer[outputOffset] = (byte)base64Chars[(inputBuffer[inputOffset] & 0xFC) >> 2];
					outputBuffer[outputOffset + 1] = (byte)base64Chars[(inputBuffer[inputOffset] & 3) << 4 | (inputBuffer[inputOffset + 1] & 0xF0) >> 4];
					outputBuffer[outputOffset + 2] = (byte)base64Chars[(inputBuffer[inputOffset + 1] & 0xF) << 2 | (inputBuffer[inputOffset + 2] & 0xC0) >> 6];
					outputBuffer[outputOffset + 3] = (byte)base64Chars[inputBuffer[inputOffset + 2] & 0x3F];
					outputOffset += 4;
				}
				inputOffset = quartetEnd;

				switch (lastChars)
				{
					case 2:
						outputBuffer[outputOffset] = (byte)base64Chars[(inputBuffer[inputOffset] & 0xFC) >> 2];
						outputBuffer[outputOffset + 1] = (byte)base64Chars[(inputBuffer[inputOffset] & 3) << 4 | (inputBuffer[inputOffset + 1] & 0xF0) >> 4];
						outputBuffer[outputOffset + 2] = (byte)base64Chars[(inputBuffer[inputOffset + 1] & 0xF) << 2];
						if (base64Alphabet.HasPadding)
						{
							outputBuffer[outputOffset + 3] = (byte)base64Alphabet.Padding;
							outputOffset += 1;
						}
						outputOffset += 3;
						break;
					case 1:
						outputBuffer[outputOffset] = (byte)base64Chars[(inputBuffer[inputOffset] & 0xFC) >> 2];
						outputBuffer[outputOffset + 1] = (byte)base64Chars[(inputBuffer[inputOffset] & 3) << 4];
						if (base64Alphabet.HasPadding)
						{
							outputBuffer[outputOffset + 2] = (byte)base64Alphabet.Padding;
							outputBuffer[outputOffset + 3] = (byte)base64Alphabet.Padding;
							outputOffset += 2;
						}
						outputOffset += 2;
						break;
					default: break;
				}

				return outputOffset - startingOutputOffset;
			}
			/// <inheritdoc />
			public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
			{
				var outputBuffer = new byte[Measure(inputCount, (this.alphabet ?? DefaultAlphabet).HasPadding)];

				this.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, 0);

				return outputBuffer;
			}

			/// <inheritdoc />
			void IDisposable.Dispose()
			{
			}

			public static int Measure(int bytesCount, bool withPadding)
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

		}
	}
}
