using System;
using System.Collections.Generic;
using System.Linq;

namespace OneTimePassword
{
    /// <summary>
    /// An Base32 encode/decoder.
    /// </summary>
    public class Base32Encoder
    {
        /// <summary>
        /// Encodes a binary data into a base32 string.
        /// </summary>
        /// <param name="data">Binary data to encode.</param>
        /// <param name="base32CharCount">The number of 5-bits characters contained in the input <paramref name="data"/>.</param>
        /// <returns>Returns a base32 encoded string.</returns>
        public string Encode(byte[] data, int base32CharCount)
        {
            int k = 0;
            var bits = new bool[base32CharCount * 5];

            foreach (byte b in data)
            {
                for (int i = 7; i >= 0 && k < bits.Length; i--)
                    bits[k++] = (b & (1 << i)) != 0;
            }

            k = 0;

            int count = bits.Length;
            byte currentByte = 0;

            var result = new List<char>();

            for (int i = 0; i < count; i++)
            {
                if (bits[i])
                    currentByte |= (byte)(1 << (4 - k));

                k++;

                if (k >= 5)
                {
                    result.Add(EncodeByte(currentByte));
                    currentByte = 0;
                    k = 0;
                }
            }

            return new string(result.ToArray());
        }

        /// <summary>
        /// Decodes a base32 string into a binary data.
        /// </summary>
        /// <param name="base32">The base32 encoded string to decode.</param>
        /// <returns>Returns a decoded binary data.</returns>
        public byte[] Decode(string base32)
        {
            var unspacedDecoded = base32
                .Where(x => char.IsWhiteSpace(x) == false)
                .Select(x => DecodeChar(x))
                .ToArray();

            var bits = new bool[unspacedDecoded.Length * 5];

            int k = 0;

            foreach (byte b in unspacedDecoded)
            {
                for (int i = 4; i >= 0; i--)
                    bits[k++] = (b & (1 << i)) != 0;
            }

            int remain;
            int byteSize = MathDivRem(bits.Length, 8, out remain);
            if (remain > 0)
                byteSize++;

            k = 0;
            int n = 0;
            byte currentByte = 0;
            var result = new byte[byteSize];
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    currentByte |= (byte)(1 << (7 - k));
                k++;

                if (k >= 8 || i == bits.Length - 1)
                {
                    result[n++] = currentByte;
                    currentByte = 0;
                    k = 0;
                }
            }

            return result;
        }

        private static int MathDivRem(int a, int b, out int remainder)
        {
            int result = a / b;
            remainder = a - result * b;
            return result;
        }

        /// <summary>
        /// Encodes a single byte into a base32 character.
        /// </summary>
        /// <param name="octet">Byte data to encore.</param>
        /// <param name="upperCase">Tells whether to produces letters as upper case or not.</param>
        /// <returns>Returns a base32 encoded character.</returns>
        public static char EncodeByte(byte octet, bool upperCase = false)
        {
            if (octet >= 32)
                throw new ArgumentException($"Invalid '{nameof(octet)}' argument. It must be in range [0, 31].", nameof(octet));

            if (octet <= 25)
            {
                if (upperCase)
                    return (char)((byte)'A' + octet);

                return (char)((byte)'a' + octet);
            }

            return (char)((byte)'2' + octet - 26);
        }

        /// <summary>
        /// Decodes a single base32 encoded character into a byte data.
        /// </summary>
        /// <param name="character">Base32 encoded character to decode.</param>
        /// <returns>Returns a decoded byte data.</returns>
        public static byte DecodeChar(char character)
        {
            if (character >= 'a' && character <= 'z')
                return (byte)(character - 'a');

            if (character >= 'A' && character <= 'Z')
                return (byte)(character - 'A');

            if (character >= '2' && character <= '7')
                return (byte)(character - '2' + 26);

            throw new ArgumentException($"Invalid '{nameof(character)}' argument. The character '{character}' is not a base32 character.", nameof(character));
        }

        /// <summary>
        /// Checks whether the given character is a legal base32 character or not.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>Returns true if the character is a legal base32 character, false otherwise.</returns>
        public static bool IsValidBase32Char(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '2' && c <= '7');
        }
    }
}
