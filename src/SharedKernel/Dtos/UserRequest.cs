using System.Collections.Generic;

namespace MapIdeaHub.BirSign.SharedKernel.Dtos
{
    public class UserRequest
    {
        public string NationalCode { get; set; }

        public string BirthDate { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string ActiveDirectoryUser { get; set; }

        /// <summary>
        /// Roles to grant this user for the calling software, identified by the
        /// <see cref="RoleInfo.SourcePrimaryKey"/> values sent through <see cref="RoleRequest"/>.
        /// Roles the user holds that are absent from this list are revoked.
        /// </summary>
        /// <remarks>
        /// Optional. Older BirSign deployments ignore it and leave role assignment to the
        /// BirSign administrator, so leaving it null keeps the request valid everywhere.
        /// The registration endpoint blanks anything omitted, so send the complete set.
        /// </remarks>
        public List<string> RoleSourcePrimaryKeys { get; set; }

        /// <summary>
        /// Custom claim values to assign this user for the calling software. Claims the user
        /// holds that are absent from this list are revoked.
        /// </summary>
        /// <remarks>
        /// Optional, with the same compatibility note as <see cref="RoleSourcePrimaryKeys"/>.
        /// Each <see cref="UserClaimInfo.ClaimType"/> must already be registered through
        /// <see cref="RoleRequest.Claims"/>.
        /// </remarks>
        public List<UserClaimInfo> Claims { get; set; }
    }
}
