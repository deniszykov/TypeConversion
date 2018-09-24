// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// Base64 encoding/decoding alphabet.
    /// </summary>
    public sealed class Base64Alphabet
    {
        internal const byte NOT_IN_ALPHABET = 255;
        internal readonly char[] Alphabet;
        internal readonly byte[] AlphabetInverse;
        internal readonly char Padding;

        internal bool HasPadding { get { return this.Padding != '\u00ff'; } }

        /// <summary>
        /// Create baseX alphabet with passed character set and padding. If padding is set to '\u00ff' then no padding is used.
        /// </summary>
        /// <param name="alphabet">Character set which used as base alphabet for encoding/decoding. Characters should be between '\u0000' and '\u007f' and not <paramref name="padding"/>.</param>
        /// <param name="padding">Padding character which used to pad data. '\u00ff' character indicates that no padding is used. Padding should be between '\u0000' and '\u007f'.</param>
        public Base64Alphabet(char[] alphabet, char padding = '\u00ff')
        {
            if (alphabet == null) throw new ArgumentNullException("alphabet");
            if (alphabet.Length != 64) throw new ArgumentOutOfRangeException("alphabet");

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
                if (charNum < 0 || charNum > 127 || charNum == padding) throw new ArgumentOutOfRangeException("alphabet");

                this.AlphabetInverse[charNum] = (byte)i;
            }
        }
    }
}
