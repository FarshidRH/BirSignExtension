namespace MapIdeaHub.BirSign.NetFrameworkExtension.Dtos
{
    public class RoleInfo
    {
        public string Name { get; set; }

        public string SourcePrimaryKey { get; set; }

        public bool IsPublicForOrganUsers { get; set; }

        public bool IsPublicForAll { get; set; }

        public string Description { get; set; }
    }
}
