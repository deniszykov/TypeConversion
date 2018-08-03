using System.Security.Cryptography;


// ReSharper disable once CheckNamespace
namespace System
{
	public sealed class FromHexTransform : ICryptoTransform
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
			if (outputOffset < 0) throw new ArgumentOutOfRangeException(nameof(outputOffset));
			if (inputCount < 0 || inputOffset + inputCount > inputBuffer.Length) throw new ArgumentOutOfRangeException(nameof(inputCount));

			var bytesWritten = 0;
			var end = inputOffset + (inputCount - inputCount % 2);
			for (var offset = inputOffset; offset < end; offset++)
			{
				var result = 0u;
				for (var i = 0; offset < end; offset++, i++)
				{
					var hexChar = (char)inputBuffer[offset];
					var hexNum = ToNumber(hexChar);

					if (i % 2 == 1)
						result |= hexNum << (i - 1) * 4;
					else
						result |= hexNum << (i + 1) * 4;
				}

				inputBuffer[offset] = (byte)result;
				bytesWritten++;
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
