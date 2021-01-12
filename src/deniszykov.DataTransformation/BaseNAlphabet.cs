/*
	Copyright (c) 2020 Denis Zykov
	
	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 

	License: https://opensource.org/licenses/MIT
*/

using System;
using JetBrains.Annotations;

namespace deniszykov.BaseN
{
	/// <summary>
	/// Base64 encoding/decoding alphabet.
	/// </summary>
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public sealed class BaseNAlphabet
	{
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

		internal const byte NOT_IN_ALPHABET = 255;
		internal readonly char[] Alphabet;
		internal readonly byte[] AlphabetInverse;
		internal readonly char Padding;

		internal bool HasPadding => this.Padding != '\u00ff';
		public readonly int EncodingBits;
		public readonly int EncodingBlockSize;
		public readonly int DecodingBlockSize;

		/// <summary>
		/// Create baseX alphabet with passed character set and padding. If padding is set to '\u00ff' then no padding is used.
		/// </summary>
		/// <param name="alphabet">Character set which used as base alphabet for encoding/decoding. Characters should be between '\u0000' and '\u007f' and not <paramref name="padding"/>.</param>
		/// <param name="padding">Padding character which used to pad data. '\u00ff' character indicates that no padding is used. Padding should be between '\u0000' and '\u007f'.</param>
		public BaseNAlphabet(char[] alphabet, char padding = '\u00ff')
		{
			if (alphabet == null) throw new ArgumentNullException(nameof(alphabet));

			switch (alphabet.Length)
			{
				case 64:
					this.EncodingBlockSize = 3;
					this.DecodingBlockSize = 4;
					this.EncodingBits = 6;
					break;
				case 32:
					this.EncodingBlockSize = 5;
					this.DecodingBlockSize = 8;
					this.EncodingBits = 5;
					break;
				case 16:
					this.EncodingBits = 4;
					this.EncodingBlockSize = 1;
					this.DecodingBlockSize = 2;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(alphabet));
			}

			this.Alphabet = alphabet;
			this.Padding = padding;
			this.AlphabetInverse = new byte[127];
			for (var i = 0; i < this.AlphabetInverse.Length; i++)
			{
				this.AlphabetInverse[i] = NOT_IN_ALPHABET;
			}

			for (var i = 0; i < this.Alphabet.Length; i++)
			{
				var charNum = (int)alphabet[i];
				if (charNum < 0 || charNum > 127 || charNum == padding) throw new ArgumentOutOfRangeException(nameof(alphabet));

				this.AlphabetInverse[charNum] = (byte)i;
			}
		}

		/// <inheritdoc />
		public override string ToString() => $"Base{this.Alphabet.Length}, Padding: '{this.Padding}'({(this.HasPadding ? "y" : "n")})";
	}
}
