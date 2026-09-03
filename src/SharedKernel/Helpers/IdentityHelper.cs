using MapIdeaHub.BirSign.SharedKernel.Constants;
using MapIdeaHub.BirSign.SharedKernel.Dtos;
using System.Collections.Generic;
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
                .Where(c => c.Type.StartsWith(BirSignConstants.RoleClaimTypePrefix))
                .Select(c => c.Value)
                .ToArray();

            foreach (var role in userRoles)
            {
                identity.AddClaim(new Claim("role", role));
            }
        }

        /// <summary>
        /// Reads the roles BirSign issued for the current application, keyed by the
        /// <c>SourcePrimaryKey</c> the application itself registered.
        /// </summary>
        /// <remarks>
        /// BirSign encodes a role as one claim per role: the type is
        /// <c>MPI_{SourcePrimaryKey}</c> and the value is the role's display name. Match on
        /// <see cref="BirSignRole.SourcePrimaryKey"/>, not on the name — the name is a label a
        /// BirSign administrator can rename at any time, which would silently break an
        /// application that authorized on it. <see cref="AddUserRoles"/> maps the name, so it
        /// suits <c>[Authorize(Roles = ...)]</c> in a simple application; anything that
        /// synchronizes its own role store should use this method instead.
        /// </remarks>
        /// <param name="identity">The identity to read. Cannot be null.</param>
        /// <returns>One entry per role claim, in the order the claims appear.</returns>
        public static IReadOnlyList<BirSignRole> GetBirSignRoles(this ClaimsIdentity identity)
        {
            return identity.Claims
                .Where(c => c.Type.StartsWith(BirSignConstants.RoleClaimTypePrefix))
                .Select(c => new BirSignRole
                {
                    SourcePrimaryKey = c.Type.Substring(BirSignConstants.RoleClaimTypePrefix.Length),
                    Name = c.Value,
                })
                .ToList();
        }
    }
}
