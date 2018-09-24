using System.Security.Cryptography;

// ReSharper disable once CheckNamespace
namespace System
{
	partial class HexConvert
	{
		/// <summary>
		/// Streaming converter for "bytes to hex" transformation. Could be used with <see cref="CryptoStream"/>.
		/// </summary>
		public struct ToHexTransform : ICryptoTransform
		{
			private static readonly char[] HexAlphabet = "0123456789abcdef".ToCharArray();

			/// <inheritdoc />
			public int InputBlockSize => 1;
			/// <inheritdoc />
			public int OutputBlockSize => 2;
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
				var outputCapacity = (outputBuffer.Length - outputOffset) / 2;
				var end = inputOffset + Math.Min(inputCount, outputCapacity);
				for (var index = inputOffset; index < end; index++)
				{
					var value = inputBuffer[index];
					outputBuffer[outputOffset] = (byte)HexAlphabet[(value >> 4) & 15u];
					outputBuffer[outputOffset + 1] = (byte)HexAlphabet[value & 15u];
					outputOffset += 2;
				}

				return outputOffset - startingOutputOffset;
			}
			/// <inheritdoc />
			public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
			{
				var outputBuffer = new byte[inputCount * 2];

				this.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, 0);

				return outputBuffer;
			}

			/// <inheritdoc />
			void IDisposable.Dispose()
			{
			}
		}
	}
}
