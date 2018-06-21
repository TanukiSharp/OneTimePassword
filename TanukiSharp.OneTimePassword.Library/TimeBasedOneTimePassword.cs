using System;
using System.Security.Cryptography;

namespace OneTimePassword
{
    /// <summary>
    /// The time-based implementation of the one time password concept.
    /// </summary>
    public class TimeBasedOneTimePassword
    {
        private readonly string secret;
        private HashAlgorithm hashAlgoritm;

        /// <summary>
        /// Initializes the <see cref="TimeBasedOneTimePassword"/> instance.
        /// </summary>
        /// <param name="secret">The secret, as a base32 encoded string.</param>
        public TimeBasedOneTimePassword(string secret)
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentException($"Invalid '{nameof(secret)}' argument.");

            this.secret = secret;
        }

        /// <summary>
        /// Generates a time-based one time password.
        /// </summary>
        /// <returns>Returns a time-based one time password.</returns>
        public string GenerateOneTimePassword()
        {
            return GenerateOneTimePassword(DateTime.UtcNow, HmacAlgorithmType.SHA1);
        }

        /// <summary>
        /// Generates a time-based one time password.
        /// </summary>
        /// <param name="time">The time corresponding to the password generation.
        /// This can be a past or future time. Usually <c>DateTime.UtcNow</c> is passed to
        /// generate a password to be used right away.</param>
        /// <returns>Returns a time-based one time password.</returns>
        public string GenerateOneTimePassword(DateTime time)
        {
            return GenerateOneTimePassword(time, HmacAlgorithmType.SHA1);
        }

        /// <summary>
        /// Generates a time-based one time password.
        /// </summary>
        /// <param name="hmacAlgorithm">The kind of HMAC algorithm to use.</param>
        /// <returns>Returns a time-based one time password.</returns>
        public string GenerateOneTimePassword(HmacAlgorithmType hmacAlgorithm)
        {
            return GenerateOneTimePassword(DateTime.UtcNow, hmacAlgorithm);
        }

        /// <summary>
        /// Generates a time-based one time password.
        /// </summary>
        /// <param name="time">The time corresponding to the password generation.
        /// This can be a past or future time. Usually <c>DateTime.UtcNow</c> is passed to
        /// generate a password to be used right away.</param>
        /// <param name="hmacAlgorithm">The kind of HMAC algorithm to use.</param>
        /// <returns>Returns a time-based one time password.</returns>
        public string GenerateOneTimePassword(DateTime time, HmacAlgorithmType hmacAlgorithm)
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentException($"Invalid '{nameof(secret)}' argument.");

            byte[] secretData = Base32Encoder.Decode(secret);

            if (hmacAlgorithm == HmacAlgorithmType.SHA1)
                hashAlgoritm = new HMACSHA1(secretData);
            else if (hmacAlgorithm == HmacAlgorithmType.SHA256)
                hashAlgoritm = new HMACSHA256(secretData);
            else if (hmacAlgorithm == HmacAlgorithmType.SHA512)
                hashAlgoritm = new HMACSHA512(secretData);
            else
                throw new ArgumentException($"Invalid '{nameof(hmacAlgorithm)}' argument. Unknown value.", nameof(hmacAlgorithm));

            byte[] timeData = BitConverter.GetBytes(GetTimeStep(time));

            // swap time
            for (int i = 0; i < 4; i++)
            {
                byte temp = timeData[i];
                timeData[i] = timeData[7 - i];
                timeData[7 - i] = temp;
            }

            byte[] hash = hashAlgoritm.ComputeHash(timeData);

            int offset = hash[hash.Length - 1] & 0xF;

            int truncatedHash = 0;
            // swap hash
            for (int i = 0; i < 4; i++)
            {
                truncatedHash <<= 8;
                truncatedHash |= hash[offset + i];
            }

            truncatedHash &= 0x7FFFFFFF;

            return truncatedHash.ToString();
        }

        private ulong GetTimeStep(DateTime time)
        {
            TimeSpan ts = time - new DateTime(1970, 1, 1);
            return (ulong)Math.Floor(ts.TotalSeconds / 30.0);
        }
    }
}
