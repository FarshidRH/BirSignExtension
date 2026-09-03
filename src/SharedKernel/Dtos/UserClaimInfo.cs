namespace MapIdeaHub.BirSign.SharedKernel.Dtos
{
    /// <summary>
    /// One custom claim assigned to a single user, scoped to the calling software.
    /// </summary>
    /// <remarks>
    /// The claim type must already be registered for the software through
    /// <see cref="RoleRequest.Claims"/>; this DTO only carries the per-user value.
    /// </remarks>
    public class UserClaimInfo
    {
        /// <summary>
        /// The registered claim type, matching a <see cref="ClaimInfo.ClaimType"/> sent earlier.
        /// </summary>
        public string ClaimType { get; set; }

        /// <summary>
        /// The value to assign to this user for that claim type.
        /// </summary>
        public string ClaimValue { get; set; }
    }
}
