using System.Security.Cryptography;

// ReSharper disable once CheckNamespace
namespace System
{
	public sealed class ToHexTransform : ICryptoTransform
	{
		private static readonly char[] HexChar = "0123456789abcdef".ToCharArray();

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
			if (outputOffset < 0) throw new ArgumentOutOfRangeException(nameof(outputOffset));
			if (inputCount < 0 || inputOffset + inputCount > inputBuffer.Length) throw new ArgumentOutOfRangeException(nameof(inputCount));

			var end = inputOffset + Math.Min(inputCount, (outputBuffer.Length - outputOffset) / 2);
			var bytesWritten = 0;
			for (var index = inputOffset; index < end; index++)
			{
				var value = inputBuffer[index];
				inputBuffer[outputOffset] = (byte)HexChar[(value >> 4) & 15u];
				inputBuffer[outputOffset + 1] = (byte)HexChar[value & 15u];
				outputOffset += 2;
				bytesWritten += 2;
			}
			return bytesWritten;
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
