using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using ImageSharingWebRole.Models;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace ImageSharingWebRole.DAL
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext() : base("AzureSQLDatabase", throwIfV1Schema:false)
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