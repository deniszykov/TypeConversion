/*
	Copyright (c) 2020 Denis Zykov
	
	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 

	License: https://opensource.org/licenses/MIT
*/

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using JetBrains.Annotations;

namespace deniszykov.DataTransformation
{
	using ByteSegment = ArraySegment<byte>;
	using CharSegment = ArraySegment<char>;

	/// <summary>
	/// BaseN bytes array to string and vice versa conversion method.
	/// </summary>
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public static class BaseNConvert
	{
		private readonly struct StringSegment
		{
			public readonly string Array;
			public readonly int Offset;
			public readonly int Count;

			public StringSegment(string array, int offset, int count)
			{
				if (array == null) throw new ArgumentNullException(nameof(array));
				if (count < 0 || count > array.Length) throw new ArgumentOutOfRangeException(nameof(count));
				if (offset < 0 || offset + count > array.Length) throw new ArgumentOutOfRangeException(nameof(offset));

				this.Array = array;
				this.Offset = offset;
				this.Count = count;
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return (this.Array ?? "").Substring(this.Offset, this.Count);
			}
		}

		// ReSharper disable StringLiteralTypo
		/// <summary>
		/// Default Base16 (Hex) alphabet. Upper case.
		/// </summary>
		public static readonly BaseNAlphabet Base16UpperCaseAlphabet = new BaseNAlphabet("0123456789ABCDEF".ToCharArray());
		/// <summary>
		/// Default Base16 (Hex) alphabet. Lower case.
		/// </summary>
		public static readonly BaseNAlphabet Base16LowerCaseAlphabet = new BaseNAlphabet("0123456789abcdef".ToCharArray());
		/// <summary>
		/// Default Base32 alphabet.
		/// </summary>
		public static readonly BaseNAlphabet Base32Alphabet = new BaseNAlphabet("ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray(), padding: '=');
		/// <summary>
		/// Alternative ZBase32 alphabet.
		/// </summary>
		public static readonly BaseNAlphabet ZBase32Alphabet = new BaseNAlphabet("ybndrfg8ejkmcpqxot1uwisza345h769".ToCharArray());
		/// <summary>
		/// Default Base64 alphabet.
		/// </summary>
		public static readonly BaseNAlphabet Base64Alphabet = new BaseNAlphabet("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".ToCharArray(), padding: '=');
		/// <summary>
		/// Url-safe Base64 alphabet. Where (+) is replaced with (-) and (/) is replaced with (_).
		/// </summary>
		public static readonly BaseNAlphabet Base64UrlAlphabet = new BaseNAlphabet("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_".ToCharArray(), padding: '=');
		// ReSharper restore StringLiteralTypo

		/// <summary>
		/// Encode byte array to BaseN string.
		/// </summary>
		/// <param name="buffer">Byte array to encode.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>BaseN-encoded string.</returns>
		public static string ToString(byte[] buffer, BaseNAlphabet baseNAlphabet)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));

			return ToString(buffer, 0, buffer.Length, baseNAlphabet);
		}
		/// <summary>
		/// Encode part of byte array to BaseN string.
		/// </summary>
		/// <param name="buffer">Byte array to encode.</param>
		/// <param name="offset">Encode start index in <paramref name="buffer"/>.</param>
		/// <param name="count">Number of bytes to encode in <paramref name="buffer"/>.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>BaseN-encoded string.</returns>
		public static string ToString(byte[] buffer, int offset, int count, BaseNAlphabet baseNAlphabet)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > buffer.Length) throw new ArgumentOutOfRangeException(nameof(count));
			if (count >= int.MaxValue / 4 * 3) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return string.Empty;

			var outputCount = GetEncodedLength(count, baseNAlphabet);
			var inputBuffer = new ByteSegment(buffer, offset, count);
			var outputBuffer = new CharSegment(new char[outputCount], 0, outputCount);

			EncodeInternal(ref inputBuffer, ref outputBuffer, out _, out _, baseNAlphabet);

			return new string(outputBuffer.Array);
		}
		/// <summary>
		/// Encode byte array to BaseN char array.
		/// </summary>
		/// <param name="buffer">Byte array to encode.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>BaseN-encoded char array.</returns>
		public static char[] ToCharArray(byte[] buffer, BaseNAlphabet baseNAlphabet)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));

			return ToCharArray(buffer, 0, buffer.Length, baseNAlphabet);
		}
		/// <summary>
		/// Encode part of byte array to BaseBaseN64 char array.
		/// </summary>
		/// <param name="buffer">Byte array to encode.</param>
		/// <param name="offset">Encode start index in <paramref name="buffer"/>.</param>
		/// <param name="count">Number of bytes to encode in <paramref name="buffer"/>.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>BaseN-encoded char array.</returns>
		public static char[] ToCharArray(byte[] buffer, int offset, int count, BaseNAlphabet baseNAlphabet)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > buffer.Length) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return new char[0];

			var outputCount = GetEncodedLength(count, baseNAlphabet);
			var inputBuffer = new ByteSegment(buffer, offset, count);
			var outputBuffer = new CharSegment(new char[outputCount], 0, outputCount);

			EncodeInternal(ref inputBuffer, ref outputBuffer, out _, out _, baseNAlphabet);

			return outputBuffer.Array;
		}

		/// <summary>
		/// Decode BaseN char array into byte array.
		/// </summary>
		/// <param name="baseNBuffer">Char array contains BaseN encoded bytes.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>Decoded bytes.</returns>
		public static byte[] ToBytes(char[] baseNBuffer, BaseNAlphabet baseNAlphabet)
		{
			if (baseNBuffer == null) throw new ArgumentNullException(nameof(baseNBuffer));
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));

			return ToBytes(baseNBuffer, 0, baseNBuffer.Length, baseNAlphabet);
		}
		/// <summary>
		/// Decode part of BaseN char array into byte array.
		/// </summary>
		/// <param name="baseNBuffer">Char array contains BaseN encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="baseNBuffer"/>.</param>
		/// <param name="count">Number of chars to decode in <paramref name="baseNBuffer"/>.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>Decoded bytes.</returns>
		public static byte[] ToBytes(char[] baseNBuffer, int offset, int count, BaseNAlphabet baseNAlphabet)
		{
			if (baseNBuffer == null) throw new ArgumentNullException(nameof(baseNBuffer));
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > baseNBuffer.Length) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return new byte[0];

			var outputCount = GetDecodedLength(baseNBuffer, offset, count, baseNAlphabet);
			var inputBuffer = new CharSegment(baseNBuffer, offset, count);
			var outputBuffer = new ByteSegment(new byte[outputCount], 0, outputCount);

			DecodeInternal(ref inputBuffer, ref outputBuffer, out _, out _, baseNAlphabet);

			return outputBuffer.Array;
		}
		/// <summary>
		/// Decode BaseN string into byte array.
		/// </summary>
		/// <param name="baseNString">BaseN string contains BaseN encoded bytes.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>Decoded bytes.</returns>
		public static byte[] ToBytes(string baseNString, BaseNAlphabet baseNAlphabet)
		{
			if (baseNString == null) throw new ArgumentNullException(nameof(baseNString));
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));

			return ToBytes(baseNString, 0, baseNString.Length, baseNAlphabet);
		}
		/// <summary>
		/// Decode part of BaseN string into byte array.
		/// </summary>
		/// <param name="baseNString">BaseN string contains BaseN encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="baseNString"/>.</param>
		/// <param name="count">Number of chars to decode in <paramref name="baseNString"/>.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>Decoded bytes.</returns>
		public static byte[] ToBytes(string baseNString, int offset, int count, BaseNAlphabet baseNAlphabet)
		{
			if (baseNString == null) throw new ArgumentNullException(nameof(baseNString));
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > baseNString.Length) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return new byte[0];

			var outputCount = GetDecodedLength(baseNString, offset, count, baseNAlphabet);
			var inputBuffer = new StringSegment(baseNString, offset, count);
			var outputBuffer = new ByteSegment(new byte[outputCount], 0, outputCount);

			DecodeInternal(ref inputBuffer, ref outputBuffer, out _, out _, baseNAlphabet);

			return outputBuffer.Array;
		}

		/// <summary>
		/// Encode <paramref name="inputBuffer"/> bytes and store BaseN-encoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Bytes to encode.</param>
		/// <param name="outputBuffer">Char array to store encoded bytes.</param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during encoding.</param>
		/// <param name="outputUsed">Number of characters written in <paramref name="outputBuffer"/> during encoding.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>Number of characters encoded into <paramref name="outputBuffer"/>.</returns>
		public static void Encode(ByteSegment inputBuffer, CharSegment outputBuffer, out int inputUsed, out int outputUsed, BaseNAlphabet baseNAlphabet)
		{
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));

			EncodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed, baseNAlphabet);
		}
		/// <summary>
		/// Encode <paramref name="inputBuffer"/> bytes and store BaseN-encoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Bytes to encode.</param>
		/// <param name="outputBuffer">Char array to store encoded bytes.</param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during encoding.</param>
		/// <param name="outputUsed">Number of characters written in <paramref name="outputBuffer"/> during encoding.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>Number of characters encoded into <paramref name="outputBuffer"/>.</returns>
		public static void Encode(ByteSegment inputBuffer, ByteSegment outputBuffer, out int inputUsed, out int outputUsed, BaseNAlphabet baseNAlphabet)
		{
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));

			EncodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed, baseNAlphabet);
		}
		/// <summary>
		/// Encode <paramref name="inputBuffer"/> bytes and store BaseN-encoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Bytes to encode.</param>
		/// <param name="inputCount">Number of bytes to read from <paramref name="inputBuffer"/>.</param>
		/// <param name="outputBuffer">Char array to store encoded bytes.</param>
		/// <param name="outputCount">Max number of bytes to write into <paramref name="outputBuffer"/>.</param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during encoding.</param>
		/// <param name="outputUsed">Number of characters written in <paramref name="outputBuffer"/> during encoding.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>Number of characters encoded into <paramref name="outputBuffer"/>.</returns>
		public static unsafe void Encode(byte* inputBuffer, int inputCount, byte* outputBuffer, int outputCount, out int inputUsed, out int outputUsed, BaseNAlphabet baseNAlphabet)
		{
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));

			inputUsed = outputUsed = 0;

			var i = 0;
			var alphabetChars = baseNAlphabet.Alphabet;
			var inputBlockSize = baseNAlphabet.EncodingBlockSize;
			var outputBlockSize = baseNAlphabet.DecodingBlockSize;
			var encodingMask = (ulong)alphabetChars.Length - 1;
			var encodingBits = baseNAlphabet.EncodingBits;

			inputUsed = 0;
			outputUsed = 0;

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
					inputBlock |= inputBuffer[inputUsed++];
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
					outputBuffer[outputUsed++] = (byte)(outputBlock & 0xFF);
					outputBlock >>= 8;
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
				inputBlock |= inputBuffer[inputUsed++];
			}
			// align with encodingBits
			inputBlock <<= encodingBits - Math.Min(inputBlockSize, inputCount) * 8 % encodingBits;

			// fill output with paddings
			for (i = 0; i < outputBlockSize; i++)
			{
				outputBlock <<= 8;
				outputBlock |= (byte)baseNAlphabet.Padding;
			}

			// encode final block
			for (i = 0; i < finalOutputBlockSize; i++)
			{
				outputBlock <<= 8;
				outputBlock |= alphabetChars[(int)(inputBlock & encodingMask)];
				inputBlock >>= encodingBits;
			}

			if (baseNAlphabet.HasPadding && inputCount > 0)
			{
				finalOutputBlockSize = outputBlockSize;
			}

			// flush final block
			if (finalOutputBlockSize > outputCount)
			{
				finalOutputBlockSize = 0; // cancel flushing output
				inputUsed -= Math.Min(inputBlockSize, inputCount); // rewind input
			}

			for (i = 0; i < finalOutputBlockSize; i++)
			{
				outputBuffer[outputUsed++] = (byte)(outputBlock & 0xFF);
				outputBlock >>= 8;
			}
		}

		/// <summary>
		/// Decode BaseN-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// Only symbols from <paramref name="baseNAlphabet"/> is counted. Other symbols are skipped.
		/// </summary>
		/// <param name="inputBuffer">Char array contains BaseN encoded bytes.</param>
		/// <param name="outputBuffer">Byte array to store decoded bytes from <paramref name="inputBuffer"/>. </param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during decoding.</param>
		/// <param name="outputUsed">Number of bytes written in <paramref name="outputBuffer"/> during decoding.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static void Decode(CharSegment inputBuffer, ByteSegment outputBuffer, out int inputUsed, out int outputUsed, BaseNAlphabet baseNAlphabet)
		{
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));

			DecodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed, baseNAlphabet);
		}
		/// <summary>
		/// Decode BaseN-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// Only symbols from <paramref name="baseNAlphabet"/> is counted. Other symbols are skipped.
		/// </summary>
		/// <param name="inputBuffer">String contains BaseN encoded bytes.</param>
		/// <param name="inputOffset">Decode start index in <paramref name="inputBuffer"/>.</param>
		/// <param name="inputCount">Number of chars to decode in <paramref name="inputBuffer"/>.</param>
		/// <param name="outputBuffer">Byte array to store decoded bytes from <paramref name="inputBuffer"/>.</param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during decoding.</param>
		/// <param name="outputUsed">Number of bytes written in <paramref name="outputBuffer"/> during decoding.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static void Decode(string inputBuffer, int inputOffset, int inputCount, ByteSegment outputBuffer, out int inputUsed, out int outputUsed, BaseNAlphabet baseNAlphabet)
		{
			if (inputBuffer == null) throw new ArgumentNullException(nameof(inputBuffer));
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));

			var stringSegment = new StringSegment(inputBuffer, inputOffset, inputCount);
			DecodeInternal(ref stringSegment, ref outputBuffer, out inputUsed, out outputUsed, baseNAlphabet);
		}
		/// <summary>
		/// Decode BaseN-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// Only symbols from <paramref name="baseNAlphabet"/> is counted. Other symbols are skipped.
		/// </summary>
		/// <param name="inputBuffer">Buffer contains BaseN encoded bytes.</param>
		/// <param name="outputBuffer">Byte array to store decoded bytes from <paramref name="inputBuffer"/>. </param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during decoding.</param>
		/// <param name="outputUsed">Number of bytes written in <paramref name="outputBuffer"/> during decoding.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static void Decode(ByteSegment inputBuffer, ByteSegment outputBuffer, out int inputUsed, out int outputUsed, BaseNAlphabet baseNAlphabet)
		{
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));

			DecodeInternal(ref inputBuffer, ref outputBuffer, out inputUsed, out outputUsed, baseNAlphabet);
		}
		/// <summary>
		/// Decode BaseN-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// Only symbols from <paramref name="baseNAlphabet"/> is counted. Other symbols are skipped.
		/// </summary>
		/// <param name="inputBuffer">Byte pointer to BaseN encoded bytes.</param>
		/// <param name="inputCount">Number of bytes(chars) to decode in <paramref name="inputBuffer"/>.</param>
		/// <param name="outputBuffer">Byte pointer to place to store decoded bytes from <paramref name="inputBuffer"/>.</param>
		/// <param name="outputCount">Number of bytes available in in <paramref name="outputBuffer"/>.</param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during decoding.</param>
		/// <param name="outputUsed">Number of bytes written in <paramref name="outputBuffer"/> during decoding.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static unsafe void Decode(byte* inputBuffer, int inputCount, byte* outputBuffer, int outputCount, out int inputUsed, out int outputUsed, BaseNAlphabet baseNAlphabet)
		{
			if (inputBuffer == null) throw new ArgumentNullException(nameof(inputBuffer));
			if (outputBuffer == null) throw new ArgumentNullException(nameof(outputBuffer));
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));
			if (outputCount < 0) throw new ArgumentOutOfRangeException(nameof(outputCount));
			if (inputCount < 0) throw new ArgumentOutOfRangeException(nameof(inputCount));

			inputUsed = outputUsed = 0;

			if (inputCount == 0)
				return;

			var alphabetInverse = baseNAlphabet.AlphabetInverse;
			var inputBlockSize = baseNAlphabet.DecodingBlockSize;
			var encodingBits = baseNAlphabet.EncodingBits;

			while (outputCount > 0)
			{
				// filling input & decoding
				var outputBlock = 0UL;// 1 byte for Base16, 5 bytes for Base32 and 3 bytes for Base64
				var originalInputUsed = inputUsed;
				var i = 0;
				for (i = 0; i < inputBlockSize && inputCount > 0; i++)
				{

					var baseNCode = inputBuffer[inputUsed++];
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
				if (outputSize > outputCount)
				{
					inputUsed = originalInputUsed; // unwind inputUsed
					break;
				}

				for (i = 0; i < outputSize; i++)
				{
					outputBuffer[outputUsed + (outputSize - 1 - i)] = (byte)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}
				outputUsed += outputSize;
				outputCount -= outputSize;
			}
		}
#if NETCOREAPP
		/// <summary>
		/// Decode BaseN-encoded <paramref name="inputBuffer"/> and store decoded bytes into <paramref name="outputBuffer"/>.
		/// Only symbols from <paramref name="baseNAlphabet"/> is counted. Other symbols are skipped.
		/// </summary>
		/// <param name="inputBuffer">Area of memory which contains BaseN encoded bytes.</param>
		/// <param name="outputBuffer">Area of memory to store decoded bytes from <paramref name="inputBuffer"/>.</param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during decoding.</param>
		/// <param name="outputUsed">Number of bytes written in <paramref name="outputBuffer"/> during decoding.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>Number of bytes decoded into <paramref name="outputBuffer"/>.</returns>
		public static void Decode(ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer, out int inputUsed, out int outputUsed, BaseNAlphabet baseNAlphabet)
		{
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));

			inputUsed = outputUsed = 0;

			if (inputBuffer.Length == 0)
				return;

			var inputLeft = inputBuffer.Length;
			var outputCapacity = outputBuffer.Length;
			var alphabetInverse = baseNAlphabet.AlphabetInverse;
			var inputBlockSize = baseNAlphabet.DecodingBlockSize;
			var encodingBits = baseNAlphabet.EncodingBits;

			while (outputCapacity > 0)
			{
				// filling input & decoding
				var outputBlock = 0UL;// 1 byte for Base16, 5 bytes for Base32 and 3 bytes for Base64
				var originalInputUsed = inputUsed;
				var i = 0;
				for (i = 0; i < inputBlockSize && inputLeft > 0; i++)
				{

					var baseNCode = inputBuffer[inputUsed++];
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
				if (outputSize > outputCapacity)
				{
					inputUsed = originalInputUsed; // unwind inputUsed
					break;
				}

				for (i = 0; i < outputSize; i++)
				{
					outputBuffer[outputUsed + (outputSize - 1 - i)] = (byte)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}
				outputUsed += outputSize;
				outputCapacity -= outputSize;
			}
		}
		/// <summary>
		/// Encode <paramref name="inputBuffer"/> bytes and store BaseN-encoded bytes into <paramref name="outputBuffer"/>.
		/// </summary>
		/// <param name="inputBuffer">Bytes to encode.</param>
		/// <param name="outputBuffer">Char array to store encoded bytes.</param>
		/// <param name="inputUsed">Number of bytes read from <paramref name="inputBuffer"/> during encoding.</param>
		/// <param name="outputUsed">Number of characters written in <paramref name="outputBuffer"/> during encoding.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>Number of characters encoded into <paramref name="outputBuffer"/>.</returns>
		public static void Encode(ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer, out int inputUsed, out int outputUsed, BaseNAlphabet baseNAlphabet)
		{
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));

			inputUsed = outputUsed = 0;

			var i = 0;
			var alphabetChars = baseNAlphabet.Alphabet;
			var inputBlockSize = baseNAlphabet.EncodingBlockSize;
			var outputBlockSize = baseNAlphabet.DecodingBlockSize;
			var encodingMask = (ulong)alphabetChars.Length - 1;
			var encodingBits = baseNAlphabet.EncodingBits;
			var outputCapacity = outputBuffer.Length;
			var inputLeft = inputBuffer.Length;

			// #2: encoding whole blocks

			var wholeBlocksToProcess = Math.Min(inputBuffer.Length / inputBlockSize, outputCapacity / outputBlockSize);
			var inputBlock = 0UL; // 1 byte for Base16, 5 bytes for Base32 and 3 bytes for Base64
			var outputBlock = 0UL; // 2 bytes for Base16, 8 bytes for Base32 and 4 bytes for Base64
			while (wholeBlocksToProcess-- > 0)
			{
				// filling input
				for (i = 0; i < inputBlockSize; i++)
				{
					inputBlock <<= 8;
					inputBlock |= inputBuffer[inputUsed++];
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
					outputBuffer[outputUsed++] = (byte)(outputBlock & 0xFF);
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
				inputBlock |= inputBuffer[inputUsed++];
			}
			// align with encodingBits
			inputBlock <<= encodingBits - Math.Min(inputBlockSize, inputLeft) * 8 % encodingBits;

			// fill output with paddings
			for (i = 0; i < outputBlockSize; i++)
			{
				outputBlock <<= 8;
				outputBlock |= (byte)baseNAlphabet.Padding;
			}

			// encode final block
			for (i = 0; i < finalOutputBlockSize; i++)
			{
				outputBlock <<= 8;
				outputBlock |= alphabetChars[(int)(inputBlock & encodingMask)];
				inputBlock >>= encodingBits;
			}

			if (baseNAlphabet.HasPadding && inputLeft > 0)
			{
				finalOutputBlockSize = outputBlockSize;
			}

			// flush final block
			if (finalOutputBlockSize > outputCapacity)
			{
				finalOutputBlockSize = 0; // cancel flushing output
				inputUsed -= Math.Min(inputBlockSize, inputLeft); // rewind input
			}

			for (i = 0; i < finalOutputBlockSize; i++)
			{
				outputBuffer[outputUsed++] = (byte)(outputBlock & 0xFF);
				outputBlock >>= 8;
			}
		}
#endif

		/// <summary>
		/// Calculate size of baseN output based on input's size.
		/// </summary>
		/// <param name="inputByteCount">Length of input buffer in bytes.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>Length of baseN output in bytes/letters.</returns>
		public static int GetEncodedLength(int inputByteCount, BaseNAlphabet baseNAlphabet)
		{
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));
			if (inputByteCount < 0) throw new ArgumentOutOfRangeException(nameof(inputByteCount));

			if (inputByteCount == 0)
				return 0;

			var wholeBlocksSize = checked(inputByteCount / baseNAlphabet.EncodingBlockSize * baseNAlphabet.DecodingBlockSize);
			var finalBlockSize = (int)Math.Ceiling(inputByteCount % baseNAlphabet.EncodingBlockSize * 8.0 / baseNAlphabet.EncodingBits);
			if (baseNAlphabet.HasPadding && finalBlockSize != 0)
			{
				finalBlockSize = baseNAlphabet.DecodingBlockSize;
			}
			return checked(wholeBlocksSize + finalBlockSize);
		}
		/// <summary>
		/// Get number of bytes encoded in passed <paramref name="baseNChars"/>. Only symbols from <paramref name="baseNAlphabet"/> are counted. Other symbols are ignored.
		/// </summary>
		/// <param name="baseNChars">Input data encoded by <paramref name="baseNAlphabet"/>.</param>
		/// <param name="offset">Offset in input data.</param>
		/// <param name="count">Length of input data.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>Number of bytes encoded in <paramref name="baseNChars"/>.</returns>
		public static int GetDecodedLength(char[] baseNChars, int offset, int count, BaseNAlphabet baseNAlphabet)
		{
			if (baseNChars == null) throw new ArgumentNullException(nameof(baseNChars));
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));
			if (offset < 0 || offset >= baseNChars.Length) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0 || offset + count > baseNChars.Length) throw new ArgumentOutOfRangeException(nameof(count));

			var byteSegment = new CharSegment(baseNChars, offset, count);
			return GetBytesCountInternal(ref byteSegment, baseNAlphabet);
		}
		/// <summary>
		/// Get number of bytes encoded in passed <paramref name="baseNString"/>. Only symbols from <paramref name="baseNAlphabet"/> are counted. Other symbols are ignored.
		/// </summary>
		/// <param name="baseNString">Input data encoded by <paramref name="baseNAlphabet"/>.</param>
		/// <param name="offset">Offset in input data.</param>
		/// <param name="count">Length of input data.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>Number of bytes encoded in <paramref name="baseNString"/>.</returns>
		public static int GetDecodedLength(string baseNString, int offset, int count, BaseNAlphabet baseNAlphabet)
		{
			if (baseNString == null) throw new ArgumentNullException(nameof(baseNString));
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));
			if (offset < 0 || offset >= baseNString.Length) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0 || offset + count > baseNString.Length) throw new ArgumentOutOfRangeException(nameof(count));

			var byteSegment = new StringSegment(baseNString, offset, count);
			return GetBytesCountInternal(ref byteSegment, baseNAlphabet);
		}
		/// <summary>
		/// Get number of bytes encoded in passed <paramref name="baseNBytes"/>. Only symbols from <paramref name="baseNAlphabet"/> are counted. Other symbols are ignored.
		/// </summary>
		/// <param name="baseNBytes">Input data encoded by <paramref name="baseNAlphabet"/>.</param>
		/// <param name="offset">Offset in input data.</param>
		/// <param name="count">Length of input data.</param>
		/// <param name="baseNAlphabet">BaseN alphabet used for encoding/decoding.</param>
		/// <returns>Number of bytes encoded in <paramref name="baseNBytes"/>.</returns>
		public static int GetDecodedLength(byte[] baseNBytes, int offset, int count, BaseNAlphabet baseNAlphabet)
		{
			if (baseNBytes == null) throw new ArgumentNullException(nameof(baseNBytes));
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));
			if (offset < 0 || offset >= baseNBytes.Length) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0 || offset + count > baseNBytes.Length) throw new ArgumentOutOfRangeException(nameof(count));

			var byteSegment = new ByteSegment(baseNBytes, offset, count);
			return GetBytesCountInternal(ref byteSegment, baseNAlphabet);
		}

		private static void EncodeInternal<BufferT>(ref ByteSegment inputBuffer, ref BufferT outputBuffer, out int inputUsed, out int outputUsed, BaseNAlphabet baseNAlphabet)
		{
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));

			inputUsed = outputUsed = 0;

			if (inputBuffer.Count == 0 || inputBuffer.Array == null)
				return;

			// #1: preparing
			var i = 0;
			var alphabetChars = baseNAlphabet.Alphabet;
			var inputBlockSize = baseNAlphabet.EncodingBlockSize;
			var outputBlockSize = baseNAlphabet.DecodingBlockSize;
			var encodingMask = (ulong)alphabetChars.Length - 1;
			var encodingBits = baseNAlphabet.EncodingBits;
			var input = inputBuffer.Array;
			var inputOffset = inputBuffer.Offset;
			var inputLeft = inputBuffer.Count;
			int outputOffset, originalOutputOffset, outputCapacity;
			var outputBytes = default(ByteSegment);
			var outputChars = default(CharSegment);

			if (outputBuffer is ByteSegment)
			{
				outputBytes = (ByteSegment)(object)outputBuffer;
				outputOffset = originalOutputOffset = outputBytes.Offset;
				outputCapacity = outputBytes.Count;
			}
			else if (outputBuffer is CharSegment)
			{
				outputChars = (CharSegment)(object)outputBuffer;
				outputOffset = originalOutputOffset = outputChars.Offset;
				outputCapacity = outputChars.Count;
			}
			else
			{
				throw new InvalidOperationException("Unknown type of output buffer: " + typeof(BufferT));
			}

			// #2: encoding whole blocks

			var wholeBlocksToProcess = Math.Min(inputBuffer.Count / inputBlockSize, outputCapacity / outputBlockSize);
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
				if (outputBuffer is ByteSegment)
				{
					Debug.Assert(outputBytes.Array != null, "byteSegment.Array != null");

					for (i = 0; i < outputBlockSize; i++)
					{
						outputBytes.Array[outputOffset++] = (byte)(outputBlock & 0xFF);
						outputBlock >>= 8;
					}
				}
				else
				{
					Debug.Assert(outputChars.Array != null, "charSegment.Array != null");

					for (i = 0; i < outputBlockSize; i++)
					{
						outputChars.Array[outputOffset++] = (char)(outputBlock & 0xFF);
						outputBlock >>= 8;
					}
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
				inputBlock |= input[inputOffset++];
			}
			// align with encodingBits
			inputBlock <<= encodingBits - Math.Min(inputBlockSize, inputLeft) * 8 % encodingBits;

			// fill output with paddings
			for (i = 0; i < outputBlockSize; i++)
			{
				outputBlock <<= 8;
				outputBlock |= (byte)baseNAlphabet.Padding;
			}

			// encode final block
			for (i = 0; i < finalOutputBlockSize; i++)
			{
				outputBlock <<= 8;
				outputBlock |= alphabetChars[(int)(inputBlock & encodingMask)];
				inputBlock >>= encodingBits;
			}

			if (baseNAlphabet.HasPadding && inputLeft > 0)
			{
				finalOutputBlockSize = outputBlockSize;
			}

			// flush final block
			if (finalOutputBlockSize > outputCapacity)
			{
				finalOutputBlockSize = 0; // cancel flushing output
				inputOffset -= Math.Min(inputBlockSize, inputLeft); // rewind input
			}

			if (outputBuffer is ByteSegment)
			{
				Debug.Assert(outputBytes.Array != null, "byteSegment.Array != null");

				for (i = 0; i < finalOutputBlockSize; i++)
				{
					outputBytes.Array[outputOffset++] = (byte)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}
			}
			else
			{
				Debug.Assert(outputChars.Array != null, "charSegment.Array != null");

				for (i = 0; i < finalOutputBlockSize; i++)
				{
					outputChars.Array[outputOffset++] = (char)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}
			}


			inputUsed = inputOffset - inputBuffer.Offset;
			outputUsed = outputOffset - originalOutputOffset;
		}
		private static void DecodeInternal<BufferT>(ref BufferT inputBuffer, ref ByteSegment outputBuffer, out int inputUsed, out int outputUsed, BaseNAlphabet baseNAlphabet)
		{
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));

			inputUsed = outputUsed = 0;

			if (outputBuffer.Count == 0 || outputBuffer.Array == null)
				return;

			int inputOffset, inputLeft;
			var outputCapacity = outputBuffer.Count;
			var output = outputBuffer.Array;
			var alphabetInverse = baseNAlphabet.AlphabetInverse;
			var inputBlockSize = baseNAlphabet.DecodingBlockSize;
			var encodingBits = baseNAlphabet.EncodingBits;
			var inputBytes = default(ByteSegment);
			var inputChars = default(CharSegment);
			var inputString = default(StringSegment);

			if (inputBuffer is ByteSegment)
			{
				inputBytes = (ByteSegment)(object)inputBuffer;
				if (inputBytes.Count == 0 || inputBytes.Array == null)
					return;
				inputOffset = inputBytes.Offset;
				inputLeft = inputBytes.Count;
			}
			else if (inputBuffer is CharSegment)
			{
				inputChars = (CharSegment)(object)inputBuffer;
				if (inputChars.Count == 0 || inputChars.Array == null)
					return;
				inputOffset = inputChars.Offset;
				inputLeft = inputChars.Count;
			}
			else if (inputBuffer is StringSegment)
			{
				inputString = (StringSegment)(object)inputBuffer;
				if (inputString.Count == 0 || inputString.Array == null)
					return;
				inputOffset = inputString.Offset;
				inputLeft = inputString.Count;
			}
			else
			{
				throw new InvalidOperationException("Unknown input buffer type: " + typeof(BufferT));
			}

			while (outputCapacity > 0)
			{
				// filling input & decoding
				var outputBlock = 0UL;// 1 byte for Base16, 5 bytes for Base32 and 3 bytes for Base64
				var originalInputUsed = inputUsed;
				var i = 0;
				for (i = 0; i < inputBlockSize && inputLeft > 0; i++)
				{

					uint baseNCode;
					if (inputBuffer is ByteSegment)
					{
						baseNCode = inputBytes.Array[inputOffset + (inputUsed++)];
					}
					else if (inputBuffer is CharSegment)
					{
						baseNCode = inputChars.Array[inputOffset + (inputUsed++)];
					}
					else
					{
						baseNCode = inputString.Array[inputOffset + (inputUsed++)];
					}
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
				if (outputSize > outputCapacity || outputSize == 0)
				{
					inputUsed = originalInputUsed; // unwind inputUsed
					break;
				}

				for (i = 0; i < outputSize; i++)
				{
					output[outputBuffer.Offset + outputUsed + (outputSize - 1 - i)] = (byte)(outputBlock & 0xFF);
					outputBlock >>= 8;
				}
				outputUsed += outputSize;
				outputCapacity -= outputSize;
			}
		}
		private static int GetBytesCountInternal<BufferT>(ref BufferT inputBuffer, BaseNAlphabet baseNAlphabet)
		{
			if (baseNAlphabet == null) throw new ArgumentNullException(nameof(baseNAlphabet));

			var alphabetInverse = baseNAlphabet.AlphabetInverse;
			var bitsPerInputChar = baseNAlphabet.EncodingBits;
			int inputEnd, inputOffset, inputChars;
			var byteSegment = default(ByteSegment);
			var charSegment = default(CharSegment);
			var stringSegment = default(StringSegment);

			if (inputBuffer is ByteSegment)
			{
				byteSegment = (ByteSegment)(object)inputBuffer;
				if (byteSegment.Count == 0 || byteSegment.Array == null)
					return 0;
				inputOffset = byteSegment.Offset;
				inputEnd = byteSegment.Offset + byteSegment.Count;
				inputChars = byteSegment.Count;
			}
			else if (inputBuffer is CharSegment)
			{
				charSegment = (CharSegment)(object)inputBuffer;
				if (charSegment.Count == 0 || charSegment.Array == null)
					return 0;
				inputOffset = charSegment.Offset;
				inputEnd = charSegment.Offset + charSegment.Count;
				inputChars = charSegment.Count;
			}
			else if (inputBuffer is StringSegment)
			{
				stringSegment = (StringSegment)(object)inputBuffer;
				if (stringSegment.Count == 0 || stringSegment.Array == null)
					return 0;
				inputOffset = stringSegment.Offset;
				inputEnd = stringSegment.Offset + stringSegment.Count;
				inputChars = stringSegment.Count;
			}
			else
			{
				throw new InvalidOperationException("Unknown input buffer type: " + typeof(BufferT));
			}

			for (; inputOffset < inputEnd; inputOffset++)
			{
				uint baseNChar;

				if (inputBuffer is ByteSegment)
				{
					baseNChar = byteSegment.Array[inputOffset];
				}
				else if (inputBuffer is CharSegment)
				{
					baseNChar = charSegment.Array[inputOffset];
				}
				else
				{
					baseNChar = stringSegment.Array[inputOffset];
				}

				if (baseNChar > 127 || alphabetInverse[baseNChar] == BaseNAlphabet.NOT_IN_ALPHABET)
				{
					inputChars--;
				}
			}

			var bytesCount = checked(inputChars * bitsPerInputChar / 8);
			return bytesCount;
		}
	}
}
