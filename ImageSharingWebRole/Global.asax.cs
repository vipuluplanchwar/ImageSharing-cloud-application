using ImageSharingWebRole.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ImageSharingWebRole
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            System.Data.Entity.Database.SetInitializer(new ImageSharingDBInitializer());

            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                db.Database.Initialize(false);
            }

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ImageStorage.DeleteAllBlob();
            LogContext.DeleteTable();
            MessageQueue.ClearAllQueues();
        }
    }
}
