using System;

namespace OneTimePassword
{
    /// <summary>
    /// Contains helper methods related to the one time password concept.
    /// </summary>
    public static class OneTimePasswordUtility
    {
        /// <summary>
        /// Truncates or zero-pads the input <paramref name="password"/> parameter to make it
        /// fit the <paramref name="digitCount"/> amount of output characters.
        /// </summary>
        /// <param name="password">The password to truncate or zero-pad.</param>
        /// <param name="digitCount">The total number of characters required on the output.</param>
        /// <returns>Returns the truncated of zero-padded input <paramref name="password"/>.</returns>
        public static string Truncate(string password, int digitCount)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            if (digitCount < 1 || digitCount > 8)
                throw new ArgumentException($"Invalid '{nameof(digitCount)}' argument. It must be in range [1, 8].", nameof(digitCount));

            int diff = password.Length - digitCount;

            if (diff > 0)
                return password.Substring(diff);

            if (diff < 0)
                return password.PadLeft(digitCount, '0');

            return password;
        }

        /// <summary>
        /// Get the amount of time remaining until the next password generation, from now, in seconds.
        /// </summary>
        /// <returns>Returns the amount of seconds remaining until the next password gets generated.</returns>
        public static ulong GetRemainingTime()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return 30ul - (ulong)Math.Floor(t.TotalSeconds % 30.0);
        }
    }
}
