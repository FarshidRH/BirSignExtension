namespace MvcNetFramework.Migrations
{
    using Microsoft.AspNet.Identity.EntityFramework;
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

            context.Set<IdentityRole>().AddOrUpdate(x => x.Name,
                new IdentityRole { Name = "Admin" },
                new IdentityRole { Name = "User" });
        }
    }
}
