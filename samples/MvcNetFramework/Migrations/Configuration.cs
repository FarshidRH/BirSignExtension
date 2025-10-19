namespace MvcNetFramework.Migrations
{
    using MvcNetFramework.Models;
    using MvcNetFramework.Models.DbContext;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "MvcNetFramework.Models.ApplicationDbContext";
        }

        protected override void Seed(ApplicationDbContext context)
        {
            base.Seed(context);

            context.Set<ApplicationRole>().AddOrUpdate(x => x.Name,
                new ApplicationRole
                {
                    Name = "Admin",
                    Description = "This is admin role."
                },
                new ApplicationRole
                {
                    Name = "User",
                    IsPublicAccess = true,
                    IsPersonnelAccess = true,
                    Description = "This is user role."
                });
        }
    }
}
