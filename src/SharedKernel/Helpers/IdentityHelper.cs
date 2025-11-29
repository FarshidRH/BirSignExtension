using System.Linq;
using System.Security.Claims;

namespace MapIdeaHub.BirSign.SharedKernel.Helpers
{
    /// <summary>
    /// Provides extension methods for working with user identity claims.
    /// </summary>
    public static class IdentityHelper
    {
        /// <summary>
        /// Adds all claims with types that start with "MPI_" as role claims to the specified identity.
        /// </summary>
        /// <remarks>This method scans the claims in the provided identity for any claim whose type begins
        /// with "MPI_" and adds a corresponding role claim for each. Existing role claims are not removed or modified.
        /// This is useful for mapping custom claim types to standard role claims for authorization purposes.</remarks>
        /// <param name="identity">The identity to which role claims will be added. Cannot be null.</param>
        public static void AddUserRoles(this ClaimsIdentity identity)
        {
            var userRoles = identity.Claims
                .Where(c => c.Type.StartsWith("MPI_"))
                .Select(c => c.Value)
                .ToArray();

            foreach (var role in userRoles)
            {
                identity.AddClaim(new Claim("role", role));
            }
        }
    }
}
