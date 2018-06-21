using System;

namespace OneTimePassword
{
    /// <summary>
    /// Represent a kind of HMAC algorithm.
    /// </summary>
    public enum HmacAlgorithmType
    {
        /// <summary>
        /// Represent the SHA1 HMAC hash algorithm.
        /// </summary>
        SHA1,

        /// <summary>
        /// Represent the SHA256 HMAC hash algorithm.
        /// </summary>
        SHA256,

        /// <summary>
        /// Represent the SHA512 HMAC hash algorithm.
        /// </summary>
        SHA512,
    }
}
