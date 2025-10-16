namespace MvcNetFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPropsToRole : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetRoles", "IsPublicAccess", c => c.Boolean());
            AddColumn("dbo.AspNetRoles", "IsPersonnelAccess", c => c.Boolean());
            AddColumn("dbo.AspNetRoles", "Description", c => c.String());
            AddColumn("dbo.AspNetRoles", "Discriminator", c => c.String(nullable: false, maxLength: 128));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetRoles", "Discriminator");
            DropColumn("dbo.AspNetRoles", "Description");
            DropColumn("dbo.AspNetRoles", "IsPersonnelAccess");
            DropColumn("dbo.AspNetRoles", "IsPublicAccess");
        }
    }
}
