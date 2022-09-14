using System;
using System.Text.RegularExpressions;

namespace dii.storage.Utilities
{
    /// <summary>
    /// An utility to generate a unique timestamp driven hash.
    /// </summary>
    /// <remarks>
    /// It is up to the consumer of this utility to ensure the Id generated is truly unique within
    /// their data.
    /// </remarks>
    public static class IdHashUtility
    {
        /// <summary>
        /// Generates a new Id hash based on <see cref="DateTime.UtcNow"/>.
        /// </summary>
        public static string NewId()
        {
            return DateTimeOffset.UtcNow.ToIdHash();
        }

        /// <summary>
        /// Validates the Id hash meets the character restriction requirements.
        /// </summary>
        public static bool ValidateIdHash(string idHash)
        {
            if (!string.IsNullOrWhiteSpace(idHash))
            {
                var validator = new Regex(@"^[a-zA-Z0-9.=]+$");

                if (validator.IsMatch(idHash))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Converts a <see cref="DateTimeOffset"/> value to an Id hash.
        /// </summary>
        public static string ToIdHash(this DateTimeOffset value)
        {
            return value.Ticks.ToIdHash();
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> value to an Id hash.
        /// </summary>
        public static string ToIdHash(this DateTime value)
        {
            return value.Ticks.ToIdHash();
        }

        /// <summary>
        /// Converts a <see cref="long"/> value to an Id hash.
        /// </summary>
        public static string ToIdHash(this long value)
        {
            var bytes = BitConverter.GetBytes(value);
            return GenerateHash(bytes);
        }

        /// <summary>
        /// Converts a <see cref="ulong"/> value to an Id hash.
        /// </summary>
        public static string ToIdHash(this ulong value)
        {
            var bytes = BitConverter.GetBytes(value);
            return GenerateHash(bytes);
        }

        /// <summary>
        /// Converts a <see cref="DateTimeOffset"/> value to an Id hash.
        /// </summary>
        public static string AsIdHash(DateTimeOffset value)
        {
            return value.Ticks.ToIdHash();
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> value to an Id hash.
        /// </summary>
        public static string AsIdHash(DateTime value)
        {
            return value.Ticks.ToIdHash();
        }

        /// <summary>
        /// Converts a <see cref="long"/> value to an Id hash.
        /// </summary>
        public static string AsIdHash(long value)
        {
            var bytes = BitConverter.GetBytes(value);
            return GenerateHash(bytes);
        }

        /// <summary>
        /// Converts a <see cref="ulong"/> value to an Id hash.
        /// </summary>
        public static string AsIdHash(ulong value)
        {
            var bytes = BitConverter.GetBytes(value);
            return GenerateHash(bytes);
        }

        private static string GenerateHash(byte[] bytes)
        {
            return Convert.ToBase64String(bytes, Base64FormattingOptions.None).TrimEnd('=').Replace("/", "=").Replace("+", ".");
        }
    }
}