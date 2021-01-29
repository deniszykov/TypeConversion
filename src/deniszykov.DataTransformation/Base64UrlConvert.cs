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
	/// Converts bytes to Base64 (URL safe) string and vice versa conversion method.
	/// Reference: https://en.wikipedia.org/wiki/Base64#URL_applications
	/// </summary>
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public static class Base64UrlConvert
	{
		/// <summary>
		/// Encode byte array to Base64 string.
		/// </summary>
		/// <param name="bytes">Byte array to encode.</param>
		/// <returns>Base64-encoded string.</returns>
		[NotNull]
		public static string ToString([NotNull]byte[] bytes)
		{
			if (bytes == null) throw new ArgumentNullException(nameof(bytes));

			return ToString(bytes, 0, bytes.Length);
		}
		/// <summary>
		/// Encode part of byte array to Base64 string.
		/// </summary>
		/// <param name="bytes">Byte array to encode.</param>
		/// <param name="offset">Encode start index in <paramref name="bytes"/>.</param>
		/// <param name="count">Number of bytes to encode in <paramref name="bytes"/>.</param>
		/// <returns>Base64-encoded string.</returns>
		[NotNull]
		public static string ToString([NotNull] byte[] bytes, int offset, int count)
		{
			if (bytes == null) throw new ArgumentNullException(nameof(bytes));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > bytes.Length) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return string.Empty;

			return BaseNEncoding.Base64Url.GetString(bytes, offset, count);
		}
		/// <summary>
		/// Encode byte array to Base64 char array.
		/// </summary>
		/// <param name="bytes">Byte array to encode.</param>
		/// <returns>Base64-encoded char array.</returns>
		[NotNull]
		public static char[] ToCharArray([NotNull] byte[] bytes)
		{
			if (bytes == null) throw new ArgumentNullException(nameof(bytes));

			return ToCharArray(bytes, 0, bytes.Length);
		}
		/// <summary>
		/// Encode part of byte array to Base64 char array.
		/// </summary>
		/// <param name="bytes">Byte array to encode.</param>
		/// <param name="offset">Encode start index in <paramref name="bytes"/>.</param>
		/// <param name="count">Number of bytes to encode in <paramref name="bytes"/>.</param>
		/// <returns>Base64-encoded char array.</returns>
		[NotNull]
		public static char[] ToCharArray([NotNull] byte[] bytes, int offset, int count)
		{
			if (bytes == null) throw new ArgumentNullException(nameof(bytes));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > bytes.Length) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return new char[0];

			return BaseNEncoding.Base64Url.GetChars(bytes, offset, count);
		}

		/// <summary>
		/// Decode Base64 char array into byte array.
		/// </summary>
		/// <param name="base64Chars">Char array contains Base64 encoded bytes.</param>
		/// <returns>Decoded bytes.</returns>
		[NotNull]
		public static byte[] ToBytes([NotNull] char[] base64Chars)
		{
			if (base64Chars == null) throw new ArgumentNullException(nameof(base64Chars));

			return ToBytes(base64Chars, 0, base64Chars.Length);
		}
		/// <summary>
		/// Decode part of Base64 char array into byte array.
		/// </summary>
		/// <param name="base64Chars">Char array contains Base64 encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="base64Chars"/>.</param>
		/// <param name="count">Number of chars to decode in <paramref name="base64Chars"/>.</param>
		/// <returns>Decoded bytes.</returns>
		[NotNull]
		public static byte[] ToBytes([NotNull] char[] base64Chars, int offset, int count)
		{
			if (base64Chars == null) throw new ArgumentNullException(nameof(base64Chars));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > base64Chars.Length) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return new byte[0];

			return BaseNEncoding.Base64Url.GetBytes(base64Chars, offset, count);
		}
		/// <summary>
		/// Decode Base64 string into byte array.
		/// </summary>
		/// <param name="base64String">Base64 string contains Base64 encoded bytes.</param>
		/// <returns>Decoded bytes.</returns>
		[NotNull]
		public static byte[] ToBytes([NotNull] string base64String)
		{
			if (base64String == null) throw new ArgumentNullException(nameof(base64String));

			return ToBytes(base64String, 0, base64String.Length);
		}
		/// <summary>
		/// Decode part of Base64 string into byte array.
		/// </summary>
		/// <param name="base64String">Base64 string contains Base64 encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="base64String"/>.</param>
		/// <param name="count">Number of chars to decode in <paramref name="base64String"/>.</param>
		/// <returns>Decoded bytes.</returns>
		[NotNull]
		public static byte[] ToBytes([NotNull] string base64String, int offset, int count)
		{
			if (base64String == null) throw new ArgumentNullException(nameof(base64String));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > base64String.Length) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return new byte[0];

			return BaseNEncoding.Base64Url.GetBytes(base64String, offset, count);
		}
		/// <summary>
		/// Decode Base64 char array (in ASCII encoding) into byte array.
		/// </summary>
		/// <param name="base64Chars">Char array contains Base64 encoded bytes.</param>
		/// <returns>Decoded bytes.</returns>
		[NotNull]
		public static byte[] ToBytes([NotNull] byte[] base64Chars)
		{
			if (base64Chars == null) throw new ArgumentNullException(nameof(base64Chars));

			return ToBytes(base64Chars, 0, base64Chars.Length);
		}
		/// <summary>
		/// Decode part of Base64 char array (in ASCII encoding) into byte array.
		/// </summary>
		/// <param name="base64Chars">Char array contains Base64 encoded bytes.</param>
		/// <param name="offset">Decode start index in <paramref name="base64Chars"/>.</param>
		/// <param name="count">Number of chars to decode in <paramref name="base64Chars"/>.</param>
		/// <returns>Decoded bytes.</returns>
		[NotNull]
		public static byte[] ToBytes([NotNull] byte[] base64Chars, int offset, int count)
		{
			if (base64Chars == null) throw new ArgumentNullException(nameof(base64Chars));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (offset + count > base64Chars.Length) throw new ArgumentOutOfRangeException(nameof(count));

			if (count == 0) return new byte[0];

			var encoder = (BaseNEncoder)BaseNEncoding.Base64Url.GetEncoder();
			var outputCount = encoder.GetByteCount(base64Chars, offset, count, flush: true);
			var output = new byte[outputCount];
			encoder.Convert(base64Chars, offset, count, output, 0, outputCount, true, out var inputUsed, out var outputUsed, out _);

			Debug.Assert(outputUsed == outputCount && inputUsed == count, "outputUsed == outputCount && inputUsed == count");

			return output;
		}
	}
}
