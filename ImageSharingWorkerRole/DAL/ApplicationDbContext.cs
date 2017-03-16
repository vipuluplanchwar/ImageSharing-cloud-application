using ImageSharingWebRole.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSharingWorkerRole.DAL
{
    class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext() : base(Microsoft.Azure.CloudConfigurationManager.GetSetting("AzureSQLDatabase"), throwIfV1Schema:false)
        {
            Database.CommandTimeout = 180;
        }

        public DbSet<Image> Images { get; set; }

        public DbSet<Tag> Tags { get; set; }
        
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
