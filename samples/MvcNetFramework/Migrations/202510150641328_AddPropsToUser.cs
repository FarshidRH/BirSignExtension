namespace MvcNetFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPropsToUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Name", c => c.String(nullable: false));
            AddColumn("dbo.AspNetUsers", "Family", c => c.String());
            AddColumn("dbo.AspNetUsers", "BirthDay", c => c.String());
            AddColumn("dbo.AspNetUsers", "NationalCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "NationalCode");
            DropColumn("dbo.AspNetUsers", "BirthDay");
            DropColumn("dbo.AspNetUsers", "Family");
            DropColumn("dbo.AspNetUsers", "Name");
        }
    }
}
