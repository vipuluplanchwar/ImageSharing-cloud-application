using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using ImageSharingWebRole.Models;
using Microsoft.AspNet.Identity;

namespace ImageSharingWebRole.DAL
{
    public class ImageSharingDBInitializer : DropCreateDatabaseAlways<ApplicationDbContext>
        
    {
        public override void InitializeDatabase(ApplicationDbContext db)
        {
            if (db.Database.Exists())
            {
                db.Database.Delete();
            }

            db.Database.Create();
            Seed(db);
        }

        protected override void Seed(ApplicationDbContext db)
        {
            RoleStore<IdentityRole> roleStore = new RoleStore<IdentityRole>(db);
            UserStore<ApplicationUser> userRole = new UserStore<ApplicationUser>(db);

            RoleManager<IdentityRole> rm = new RoleManager<IdentityRole>(roleStore);
            UserManager<ApplicationUser> um = new UserManager<ApplicationUser>(userRole);

            IdentityResult ir;

            ApplicationUser nobody = CreateUser("nobody@example.org");
            ApplicationUser jfk = CreateUser("jfk@example.org");
            ApplicationUser nixon = CreateUser("nixon@example.org");
            ApplicationUser fdr = CreateUser("fdr@example.org");

            ir = um.Create(nobody, "nobody1234");
            
            ir = um.Create(jfk, "jfk1234");
            
            ir = um.Create(nixon, "nixon1234");

            ir = um.Create(fdr, "fdr1234");
            

            rm.Create(new IdentityRole("User"));
            if (!um.IsInRole((string)nobody.Id,"User"))
            {
                um.AddToRole((string)nobody.Id, "User");
            }
            if (!um.IsInRole((string)jfk.Id, "User"))
            {
                um.AddToRole((string)jfk.Id, "User");
            }
            if (!um.IsInRole((string)nixon.Id, "User"))
            {
                um.AddToRole((string)nixon.Id, "User");
            }
            if (!um.IsInRole((string)fdr.Id, "User"))
            {
                um.AddToRole((string)fdr.Id, "User");
            }

            rm.Create(new IdentityRole("Admin"));
            if (!um.IsInRole(nixon.Id,"Admin"))
            {
                um.AddToRole(nixon.Id, "Admin");
            }

            rm.Create(new IdentityRole("Approver"));
            if (!um.IsInRole(jfk.Id, "Approver"))
            {
                um.AddToRole(jfk.Id, "Approver");
            }

            rm.Create(new IdentityRole("Supervisor"));
            if (!um.IsInRole(fdr.Id, "Supervisor"))
            {
                um.AddToRole(fdr.Id, "Supervisor");
            }

            db.Tags.Add(new Tag(Name: "portrait"));
            db.Tags.Add(new Tag(Name: "architecture"));

            db.SaveChanges();

            db.Images.Add(new Image
            {
                Caption = "Marine Drive",
                Description = "Beautiful Picture of Marine Drive",
                DateTaken = new DateTime(2016, 01, 01),
                UserId = jfk.Id,
                TagId = 1,
                Approved = true,
                Validated = true
            });

            db.SaveChanges();
            
            base.Seed(db);
        }

        private ApplicationUser CreateUser(string userName)
        {
            return new ApplicationUser { UserName = userName, Email = userName };
        }
    }
}