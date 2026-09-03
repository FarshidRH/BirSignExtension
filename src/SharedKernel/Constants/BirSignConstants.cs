namespace MapIdeaHub.BirSign.SharedKernel.Constants
{
    /// <summary>
    /// Provides constant values used for BirSign authentication integration.
    /// </summary>
    public class BirSignConstants
    {
        public const string AuthenticationType = "BirSign";
        public const string LoginUri = "/BirSign/Login";

        /// <summary>
        /// Prefix BirSign puts on a role claim's type; what follows it is the
        /// <c>SourcePrimaryKey</c> the application registered for that role.
        /// </summary>
        public const string RoleClaimTypePrefix = "MPI_";
    }
}
