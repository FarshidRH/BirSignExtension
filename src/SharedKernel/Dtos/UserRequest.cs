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
        /// Whether the calling application has established that the phone number belongs to this
        /// person.
        /// </summary>
        /// <remarks>
        /// An account registered through this API has no password, so its owner can only get in
        /// through BirSign's password recovery - and recovery requires a confirmed channel. Left
        /// false, the account exists and can never be signed into.
        /// <para>
        /// Set it only from a check the application actually performed. It asserts a recovery
        /// factor, so a careless true is a route to taking the account over.
        /// </para>
        /// </remarks>
        public bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        /// Whether the calling application has established that the email address belongs to this
        /// person. Same warning as <see cref="PhoneNumberConfirmed"/>.
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// Street address, stored by BirSign as part of the standard OpenID Connect address
        /// claim - the same one the user edits in its account pages.
        /// </summary>
        /// <remarks>
        /// Null leaves whatever BirSign holds alone; an empty string clears it.
        /// </remarks>
        public string StreetAddress { get; set; }

        /// <summary>Postal code, stored alongside <see cref="StreetAddress"/>.</summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Custom claim values to assign this user for the calling software. Claims the user
        /// holds that are absent from this list are revoked.
        /// </summary>
        /// <remarks>
        /// Optional: older BirSign deployments ignore it, so leaving it null keeps the request
        /// valid everywhere. Each <see cref="UserClaimInfo.ClaimType"/> must already be
        /// registered through <see cref="RoleRequest.Claims"/>.
        /// <para>
        /// Claims, and no roles. Roles are registered as a catalogue through
        /// <see cref="RoleRequest"/> and BirSign alone decides who holds which of them. An
        /// application's own copy of a user's roles is a cache it refreshes when the user signs
        /// in, so sending it back would let a stale copy revoke what BirSign had granted.
        /// </para>
        /// </remarks>
        public List<UserClaimInfo> Claims { get; set; }
    }
}
