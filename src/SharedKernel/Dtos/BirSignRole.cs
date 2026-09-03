namespace MapIdeaHub.BirSign.SharedKernel.Dtos
{
    /// <summary>
    /// A role BirSign issued for the current application, as read back from the token.
    /// </summary>
    public class BirSignRole
    {
        /// <summary>
        /// The key the application itself supplied when it registered the role
        /// (<see cref="RoleInfo.SourcePrimaryKey"/>). This is the stable identifier:
        /// match on it, not on <see cref="Name"/>, which an administrator can rename.
        /// </summary>
        public string SourcePrimaryKey { get; set; }

        /// <summary>
        /// The role's display name in BirSign.
        /// </summary>
        public string Name { get; set; }
    }
}
