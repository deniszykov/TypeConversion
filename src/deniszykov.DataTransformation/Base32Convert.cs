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
	/// Converts bytes to Base32 string and vice versa conversion method.
	/// Reference: https://en.wikipedia.org/wiki/Base32
	/// </summary>
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public static class Base32Convert
	{
		/// <summary>
		/// Encode byte array to Base32 string.
		/// </summary>
		/// <param name="bytes">Byte array to encode.</param>
		/// <returns>Base32-encoded string.</returns>
		[NotNull]
		public static string ToString([NotNull] byte[] bytes)
		{
			if (bytes == null) throw new ArgumentNullException(nameof(bytes));

			return ToString(bytes, 0, bytes.Length);
		}
		/// <summary>
		/// Encode part of byte array to Base32 string.
		/// </summary>
		/// <param name="bytes">Byte array to encode.</param>
		/// <param name="offset">Encode start index in <paramref name="bytes"/>.</param>
		/// <param name="count">Number of bytes to encode in <paramref name="bytes"/>.</param>
		/// <returns>Base32-encoded string.</returns>
		[NotNull]
		public static string ToString([NotNull] byte[] bytes, int offset, int count)
		{
			if (bytes == null) throw new ArgumentNullException(nameof(bytes));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > bytes.Length) throw new ArgumentOutOfRangeException(nameof(count));
			if (count >= int.MaxValue / 4 * 3) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return string.Empty;

			return BaseNEncoding.Base32.GetString(bytes, offset, count);
		}
		/// <summary>
		/// Encode byte array to Base32 char array.
		/// </summary>
		/// <param name="bytes">Byte array to encode.</param>
		/// <returns>Base32-encoded char array.</returns>
		[NotNull]
		public static char[] ToCharArray([NotNull] byte[] bytes)
		{
			if (bytes == null) throw new ArgumentNullException(nameof(bytes));

			return ToCharArray(bytes, 0, bytes.Length);
		}
		/// <summary>
		/// Encode part of byte array to Base32 char array.
		/// </summary>
		/// <param name="bytes">Byte array to encode.</param>
		/// <param name="offset">Encode start index in <paramref name="bytes"/>.</param>
		/// <param name="count">Number of bytes to encode in <paramref name="bytes"/>.</param>
		/// <returns>Base32-encoded char array.</returns>
		[NotNull]
		public static char[] ToCharArray([NotNull] byte[] bytes, int offset, int count)
		{
			if (bytes == null) throw new ArgumentNullException(nameof(bytes));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > bytes.Length) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return new char[0];

			return BaseNEncoding.Base32.GetChars(bytes, offset, count);
		}

		/// <summary>
		/// Decode Base32 char array into byte array.
		/// </summary>
		/// <param name="base32Chars">Char array contains Base32 encoded bytes.</param>
		/// <returns>Decoded bytes.</returns>
		[NotNull]
		public static byte[] ToBytes([NotNull] char[] base32Chars)
		{
			if (base32Chars == null) throw new ArgumentNullException(nameof(base32Chars));

			return ToBytes(base32Chars, 0, base32Chars.Length);
		}
		/// <summary>
		/// Decode part of Base32 char array into byte array.
		/// </summary>
		/// <param name="base32Chars">Char array contains Base32 encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="base32Chars"/>.</param>
		/// <param name="count">Number of chars to decode in <paramref name="base32Chars"/>.</param>
		/// <returns>Decoded bytes.</returns>
		[NotNull]
		public static byte[] ToBytes([NotNull] char[] base32Chars, int offset, int count)
		{
			if (base32Chars == null) throw new ArgumentNullException(nameof(base32Chars));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > base32Chars.Length) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return new byte[0];

			return BaseNEncoding.Base32.GetBytes(base32Chars, offset, count);
		}
		/// <summary>
		/// Decode Base32 string into byte array.
		/// </summary>
		/// <param name="base32String">Base32 string contains Base32 encoded bytes.</param>
		/// <returns>Decoded bytes.</returns>
		[NotNull]
		public static byte[] ToBytes([NotNull] string base32String)
		{
			if (base32String == null) throw new ArgumentNullException(nameof(base32String));

			return ToBytes(base32String, 0, base32String.Length);
		}
		/// <summary>
		/// Decode part of Base32 string into byte array.
		/// </summary>
		/// <param name="base32String">Base32 string contains Base32 encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="base32String"/>.</param>
		/// <param name="count">Number of chars to decode in <paramref name="base32String"/>.</param>
		/// <returns>Decoded bytes.</returns>
		[NotNull]
		public static byte[] ToBytes([NotNull] string base32String, int offset, int count)
		{
			if (base32String == null) throw new ArgumentNullException(nameof(base32String));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > base32String.Length) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return new byte[0];

			return BaseNEncoding.Base32.GetBytes(base32String, offset, count);
		}
		/// <summary>
		/// Decode Base32 char array (in ASCII encoding) into byte array.
		/// </summary>
		/// <param name="base32Chars">Char array contains Base32 encoded bytes.</param>
		/// <returns>Decoded bytes.</returns>
		[NotNull]
		public static byte[] ToBytes([NotNull] byte[] base32Chars)
		{
			if (base32Chars == null) throw new ArgumentNullException(nameof(base32Chars));

			return ToBytes(base32Chars, 0, base32Chars.Length);
		}
		/// <summary>
		/// Decode part of Base32 char array (in ASCII encoding) into byte array.
		/// </summary>
		/// <param name="base32Chars">Char array contains Base32 encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="base32Chars"/>.</param>
		/// <param name="count">Number of chars to decode in <paramref name="base32Chars"/>.</param>
		/// <returns>Decoded bytes.</returns>
		[NotNull]
		public static byte[] ToBytes([NotNull] byte[] base32Chars, int offset, int count)
		{
			if (base32Chars == null) throw new ArgumentNullException(nameof(base32Chars));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > base32Chars.Length) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return new byte[0];

			var encoder = (BaseNEncoder)BaseNEncoding.Base32.GetEncoder();
			var outputCount = encoder.GetByteCount(base32Chars, offset, count, flush: true);
			var output = new byte[outputCount];
			encoder.Convert(base32Chars, offset, count, output, 0, outputCount, true, out var inputUsed, out var outputUsed, out _);

			Debug.Assert(outputUsed == outputCount && inputUsed == count, "outputUsed == outputCount && inputUsed == count");

			return output;
		}
	}
}
