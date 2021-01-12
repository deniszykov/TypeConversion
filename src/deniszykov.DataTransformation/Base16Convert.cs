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

namespace deniszykov.BaseN
{
	/// <summary>
	/// Converts bytes to Base16 (aka Hexadecimal aka Hex) string and vice versa conversion method.
	/// Reference: https://en.wikipedia.org/wiki/Hexadecimal
	/// </summary>
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public static class Base16Convert
	{
		/// <summary>
		/// Encode byte array to Base16 string.
		/// </summary>
		/// <param name="bytes">Byte array to encode.</param>
		/// <param name="lowerCase">Use lower case characters for encoding.</param>
		/// <returns>Base16-encoded string.</returns>
		[NotNull]
		public static string ToString([NotNull] byte[] bytes, bool lowerCase = false)
		{
			if (bytes == null) throw new ArgumentNullException(nameof(bytes));

			return ToString(bytes, 0, bytes.Length, lowerCase);
		}
		/// <summary>
		/// Encode part of byte array to Base16 string.
		/// </summary>
		/// <param name="bytes">Byte array to encode.</param>
		/// <param name="offset">Encode start index in <paramref name="bytes"/>.</param>
		/// <param name="count">Number of bytes to encode in <paramref name="bytes"/>.</param>
		/// <param name="lowerCase">Use lower case characters for encoding.</param>
		/// <returns>Base16-encoded string.</returns>
		[NotNull]
		public static string ToString([NotNull] byte[] bytes, int offset, int count, bool lowerCase = false)
		{
			if (bytes == null) throw new ArgumentNullException(nameof(bytes));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > bytes.Length) throw new ArgumentOutOfRangeException(nameof(count));
			if (count >= int.MaxValue / 4 * 3) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return string.Empty;

			return (lowerCase ? BaseNEncoding.Base16LowerCase : BaseNEncoding.Base16UpperCase).GetString(bytes, offset, count);
		}
		/// <summary>
		/// Encode byte array to Base16 char array.
		/// </summary>
		/// <param name="bytes">Byte array to encode.</param>
		/// <param name="lowerCase">Use lower case characters for encoding.</param>
		/// <returns>Base16-encoded char array.</returns>
		[NotNull]
		public static char[] ToCharArray([NotNull] byte[] bytes, bool lowerCase = false)
		{
			if (bytes == null) throw new ArgumentNullException(nameof(bytes));

			return ToCharArray(bytes, 0, bytes.Length, lowerCase);
		}
		/// <summary>
		/// Encode part of byte array to Base16 char array.
		/// </summary>
		/// <param name="bytes">Byte array to encode.</param>
		/// <param name="offset">Encode start index in <paramref name="bytes"/>.</param>
		/// <param name="count">Number of bytes to encode in <paramref name="bytes"/>.</param>
		/// <param name="lowerCase">Use lower case characters for encoding.</param>
		/// <returns>Base16-encoded char array.</returns>
		[NotNull]
		public static char[] ToCharArray([NotNull] byte[] bytes, int offset, int count, bool lowerCase = false)
		{
			if (bytes == null) throw new ArgumentNullException(nameof(bytes));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > bytes.Length) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return new char[0];

			return (lowerCase ? BaseNEncoding.Base16LowerCase : BaseNEncoding.Base16UpperCase).GetChars(bytes, offset, count);
		}

		/// <summary>
		/// Decode Base16 char array into byte array.
		/// </summary>
		/// <param name="base16Chars">Char array contains Base16 encoded bytes.</param>
		/// <returns>Decoded bytes.</returns>
		[NotNull]
		public static byte[] ToBytes([NotNull] char[] base16Chars)
		{
			if (base16Chars == null) throw new ArgumentNullException(nameof(base16Chars));

			return ToBytes(base16Chars, 0, base16Chars.Length);
		}
		/// <summary>
		/// Decode part of Base16 char array into byte array.
		/// </summary>
		/// <param name="base16Chars">Char array contains Base16 encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="base16Chars"/>.</param>
		/// <param name="count">Number of chars to decode in <paramref name="base16Chars"/>.</param>
		/// <returns>Decoded bytes.</returns>
		[NotNull]
		public static byte[] ToBytes([NotNull] char[] base16Chars, int offset, int count)
		{
			if (base16Chars == null) throw new ArgumentNullException(nameof(base16Chars));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > base16Chars.Length) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return new byte[0];

			var isLowerCase = true;
			for (var i = offset; i < offset + count; i++)
			{
				if (base16Chars[i] >= 'A' && base16Chars[i] <= 'F')
				{
					isLowerCase = false;
				}
			}

			return (isLowerCase ? BaseNEncoding.Base16LowerCase : BaseNEncoding.Base16UpperCase).GetBytes(base16Chars, offset, count);
		}
		/// <summary>
		/// Decode Base16 string into byte array.
		/// </summary>
		/// <param name="base16String">Base16 string contains Base16 encoded bytes.</param>
		/// <returns>Decoded bytes.</returns>
		[NotNull]
		public static byte[] ToBytes([NotNull] string base16String)
		{
			if (base16String == null) throw new ArgumentNullException(nameof(base16String));

			return ToBytes(base16String, 0, base16String.Length);
		}
		/// <summary>
		/// Decode part of Base16 string into byte array.
		/// </summary>
		/// <param name="base16String">Base16 string contains Base16 encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="base16String"/>.</param>
		/// <param name="count">Number of chars to decode in <paramref name="base16String"/>.</param>
		/// <returns>Decoded bytes.</returns>
		[NotNull]
		public static byte[] ToBytes([NotNull] string base16String, int offset, int count)
		{
			if (base16String == null) throw new ArgumentNullException(nameof(base16String));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > base16String.Length) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return new byte[0];

			var isLowerCase = true;
			for (var i = offset; i < offset + count; i++)
			{
				if (base16String[i] >= 'A' && base16String[i] <= 'F')
				{
					isLowerCase = false;
				}
			}

			return (isLowerCase ? BaseNEncoding.Base16LowerCase : BaseNEncoding.Base16UpperCase).GetBytes(base16String, offset, count);
		}
		/// <summary>
		/// Decode Base16 char array (in ASCII encoding) into byte array.
		/// </summary>
		/// <param name="base16Chars">Char array contains Base16 encoded bytes.</param>
		/// <returns>Decoded bytes.</returns>
		[NotNull]
		public static byte[] ToBytes([NotNull] byte[] base16Chars)
		{
			if (base16Chars == null) throw new ArgumentNullException(nameof(base16Chars));

			return ToBytes(base16Chars, 0, base16Chars.Length);
		}
		/// <summary>
		/// Decode part of Base16 char array (in ASCII encoding) into byte array.
		/// </summary>
		/// <param name="base16Chars">Char array contains Base16 encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="base16Chars"/>.</param>
		/// <param name="count">Number of chars to decode in <paramref name="base16Chars"/>.</param>
		/// <returns>Decoded bytes.</returns>
		[NotNull]
		public static byte[] ToBytes([NotNull] byte[] base16Chars, int offset, int count)
		{
			if (base16Chars == null) throw new ArgumentNullException(nameof(base16Chars));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > base16Chars.Length) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return new byte[0];

			var isLowerCase = true;
			for (var i = offset; i < offset + count; i++)
			{
				if (base16Chars[i] >= 'A' && base16Chars[i] <= 'F')
				{
					isLowerCase = false;
				}
			}

			var encoder = (BaseNEncoder)(isLowerCase ? BaseNEncoding.Base16LowerCase : BaseNEncoding.Base16UpperCase).GetEncoder();
			var outputCount = encoder.GetByteCount(base16Chars, offset, count, flush: true);
			var output = new byte[outputCount];
			encoder.Convert(base16Chars, offset, count, output, 0, outputCount, true, out var inputUsed, out var outputUsed, out _);

			Debug.Assert(outputUsed == outputCount && inputUsed == count, "outputUsed == outputCount && inputUsed == count");

			return output;
		}
	}
}
