using MapIdeaHub.BirSign.NetFrameworkExtension.Enums;
using System;
using System.Security.Cryptography;
using System.Text;

namespace MapIdeaHub.BirSign.NetFrameworkExtension.Utilities
{
    /// <summary>
    /// A static helper class for computing hash values from strings.
    /// </summary>
    public static class HashComputer
    {
        /// <summary>
        /// Computes the hash of a string using the specified algorithm.
        /// </summary>
        /// <param name="input">The input string to hash.</param>
        /// <param name="hashType">The hash algorithm to use.</param>
        /// <param name="encoding">The text encoding to use for converting the string to bytes. Defaults to UTF-8.</param>
        /// <returns>A lowercase hexadecimal string representation of the hash.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the input string is null.</exception>
        /// <exception cref="ArgumentException">Thrown if an invalid hash type is specified.</exception>
        /// <exception cref="NotSupportedException">Thrown if the selected algorithm (e.g., SHA-3) is not supported on the current platform.</exception>
        public static string ComputeHash(this string input, HashType hashType, Encoding encoding = null)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            byte[] inputBytes = (encoding ?? Encoding.UTF8).GetBytes(input);
            string algorithmName = GetAlgorithmName(hashType);
            using (HashAlgorithm algorithm = HashAlgorithm.Create(algorithmName))
            {
                if (algorithm == null)
                {
                    throw new NotSupportedException(
                        $"Algorithm '{algorithmName}' is not supported on this platform. " +
                        (hashType >= HashType.SHA3_256 ? "SHA-3 algorithms require .NET 6 or later." : "")
                    );
                }

                byte[] hashBytes = algorithm.ComputeHash(inputBytes);
                var sb = new StringBuilder(hashBytes.Length * 2);
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Maps the enum to the algorithm name string required by HashAlgorithm.Create.
        /// </summary>
        private static string GetAlgorithmName(HashType hashType)
        {
            switch (hashType)
            {
                case HashType.MD5: return "MD5";
                case HashType.SHA1: return "SHA1";
                case HashType.SHA256: return "SHA256";
                case HashType.SHA384: return "SHA384";
                case HashType.SHA512: return "SHA512";
                case HashType.SHA3_256: return "SHA3-256";
                case HashType.SHA3_384: return "SHA3-384";
                case HashType.SHA3_512: return "SHA3-512";
                // The default case handles any future enum values that are not yet mapped.
                default: throw new ArgumentException("Invalid hash type specified.", nameof(hashType));
            }
        }
    }
}
