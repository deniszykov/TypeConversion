using System.Security.Cryptography;


// ReSharper disable once CheckNamespace
namespace System
{
	partial class Base64Convert
	{
		/// <summary>
		/// Streaming converter for "base64 to bytes" transformation. Could be used with <see cref="CryptoStream"/>.
		/// </summary>
		public struct FromBase64Transform : ICryptoTransform
		{
			private static readonly Base64Alphabet DefaultAlphabet = new Base64Alphabet("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".ToCharArray(), padding: '=');

			private readonly Base64Alphabet alphabet;

			/// <inheritdoc />
			public int InputBlockSize => 3;
			/// <inheritdoc />
			public int OutputBlockSize => 4;
			/// <inheritdoc />
			public bool CanTransformMultipleBlocks => true;
			/// <inheritdoc />
			public bool CanReuseTransform => true;

			/// <summary>
			/// Creates new instance with specified <paramref name="alphabet"/>.
			/// </summary>
			public FromBase64Transform(Base64Alphabet alphabet)
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
				var startingOffset = outputOffset;
				var end = inputOffset + inputCount;
				var alphabetInverse = base64Alphabet.AlphabetInverse;
				for (var i = inputOffset; i < end; i += 4)
				{
					var number = 0u;
					int j;
					uint base64Code, base64CodeIndex;
					for (j = 0; j < 4 && i + j < end; j++)
					{
						base64Code = inputBuffer[i + j];
						if ((base64Code > 127) || (base64CodeIndex = alphabetInverse[base64Code]) == Base64Alphabet.NOT_IN_ALPHABET)
						{
							i++;
							j--;
							continue;
						}
						number = (uint)unchecked(number | base64CodeIndex << (18 - 6 * j));
					}

					switch (j)
					{
						case 2:
							if (outputOffset + 0 >= outputBuffer.Length) return outputOffset - startingOffset;
							outputBuffer[outputOffset++] = (byte)((number >> 16) & 255);
							break;
						case 3:
							if (outputOffset + 1 >= outputBuffer.Length) return outputOffset - startingOffset;
							outputBuffer[outputOffset++] = (byte)((number >> 16) & 255);
							outputBuffer[outputOffset++] = (byte)((number >> 8) & 255);
							break;
						case 4:
							if (outputOffset + 2 >= outputBuffer.Length) return outputOffset - startingOffset;
							outputBuffer[outputOffset++] = (byte)((number >> 16) & 255);
							outputBuffer[outputOffset++] = (byte)((number >> 8) & 255);
							outputBuffer[outputOffset++] = (byte)((number >> 0) & 255);
							break;
						default: break;
					}
				}
				return outputOffset - startingOffset;
			}
			/// <inheritdoc />
			public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
			{
				var outputBuffer = new byte[Measure(inputBuffer, inputOffset, inputCount)];

				this.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, 0);

				return outputBuffer;
			}

			/// <inheritdoc />
			void IDisposable.Dispose()
			{
			}

			public static int Measure(byte[] inputBuffer, int inputOffset, int inputCount, Base64Alphabet base64Alphabet = null)
			{
				if (inputBuffer == null) throw new ArgumentNullException(nameof(inputBuffer));
				if (inputOffset < 0 || inputOffset >= inputBuffer.Length) throw new ArgumentOutOfRangeException(nameof(inputOffset));
				if (inputCount < 0 || inputOffset + inputCount > inputBuffer.Length) throw new ArgumentOutOfRangeException(nameof(inputCount));

				if (inputCount == 0) return 0;
				if (base64Alphabet == null) base64Alphabet = Base64Alphabet;

				var alphabetInverse = base64Alphabet.AlphabetInverse;
				var end = inputOffset + inputCount;
				for (var i = inputOffset; i < end; i++)
				{
					var base64Char = inputBuffer[i];

					if (base64Char > 127 || alphabetInverse[base64Char] == Base64Alphabet.NOT_IN_ALPHABET)
					{
						inputCount--;
					}
				}

				var bytesCount = inputCount / 4 * 3;

				switch (inputCount % 4)
				{
					case 2: bytesCount += 1; break;
					case 3: bytesCount += 2; break;
					default: break;
				}

				return bytesCount;
			}

		}
	}
}
