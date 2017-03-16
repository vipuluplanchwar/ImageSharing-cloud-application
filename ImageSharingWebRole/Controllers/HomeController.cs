using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ImageSharingWebRole.Models;
using System.IO;
using System.Web.Script.Serialization;

namespace ImageSharingWebRole.Controllers
{
    public class HomeController : BaseController
    {
        [AllowAnonymous]
        // GET: Home
        public ActionResult Index(String id = "Stranger")
        {
            CheckAda();
            ViewBag.Title = "Welcome ";
            ApplicationUser User = GetLoggedInUser();
            if (User == null)
            {
                ViewBag.Id = id;
            }
            else
            {
                ViewBag.Id = User.UserName;
            }
            return View();
        }

        public ActionResult Error(String errid = "Unspecified")
        {
            if ("Details".Equals(errid))
            {
                ViewBag.Message = "Problem with Details action!";
            }
            else
            {
                ViewBag.Message = "Unspecified error!";
            }
            return View();
            {

            }
        }
    }
}