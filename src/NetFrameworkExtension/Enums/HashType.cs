namespace MapIdeaHub.BirSign.NetFrameworkExtension.Enums
{
    /// <summary>
    /// Defines the supported cryptographic hash algorithms.
    /// </summary>
    public enum HashType
    {
        MD5,
        SHA1,
        SHA256,
        SHA384,
        SHA512,

        /// <summary>
        /// SHA-3 (Keccak). Requires .NET 6 or later.
        /// </summary>
        SHA3_256,

        /// <summary>
        /// SHA-3 (Keccak). Requires .NET 6 or later.
        /// </summary>
        SHA3_384,

        /// <summary>
        /// SHA-3 (Keccak). Requires .NET 6 or later.
        /// </summary>
        SHA3_512
    }
}
