using System.Security.Cryptography;


// ReSharper disable once CheckNamespace
namespace System
{
	partial class HexConvert
	{
		/// <summary>
		/// Streaming converter for "hex to bytes" transformation. Could be used with <see cref="CryptoStream"/>.
		/// </summary>
		public struct FromHexTransform : ICryptoTransform
		{
			/// <inheritdoc />
			public int InputBlockSize => 2;
			/// <inheritdoc />
			public int OutputBlockSize => 1;
			/// <inheritdoc />
			public bool CanTransformMultipleBlocks => true;
			/// <inheritdoc />
			public bool CanReuseTransform => true;

			/// <inheritdoc />
			public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
			{
				if (inputBuffer == null) throw new ArgumentNullException(nameof(inputBuffer));
				if (outputBuffer == null) throw new ArgumentNullException(nameof(outputBuffer));
				if (inputOffset < 0) throw new ArgumentOutOfRangeException(nameof(inputOffset));
				if (outputOffset < 0 || outputOffset > outputBuffer.Length) throw new ArgumentOutOfRangeException(nameof(outputOffset));
				if (inputCount < 0 || inputOffset + inputCount > inputBuffer.Length) throw new ArgumentOutOfRangeException(nameof(inputCount));

				var startingOutputOffset = outputOffset;
				var end = inputOffset + Math.Min(inputCount - inputCount % 2, (outputBuffer.Length - outputOffset) * 2);
				for (var offset = inputOffset; offset < end; offset += 2)
				{
					var hexNum1 = ToNumber((char)inputBuffer[offset]);
					var hexNum2 = ToNumber((char)inputBuffer[offset + 1]);
					var result = (hexNum1 << 4) | hexNum2;

					outputBuffer[outputOffset] = (byte)result;
					outputOffset++;
				}

				return outputOffset - startingOutputOffset;
			}
			/// <inheritdoc />
			public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
			{
				var outputBuffer = new byte[inputCount / 2];

				this.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, 0);

				return outputBuffer;
			}

			/// <inheritdoc />
			void IDisposable.Dispose()
			{
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
}
