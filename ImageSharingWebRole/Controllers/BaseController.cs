using ImageSharingWebRole.DAL;
using ImageSharingWebRole.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ImageSharingWebRole.Controllers
{
    public class BaseController : Controller
    {
        protected ApplicationDbContext ApplicationDbContext { get; set; }
        protected UserManager<ApplicationUser> userManager { get; set; }

        protected BaseController()
        {
            this.ApplicationDbContext = new ApplicationDbContext();
            this.userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(ApplicationDbContext));
        }
        protected void CheckAda()
        {
            try
            {
                HttpCookie cookie = Request.Cookies.Get("ImageSharing");
                ViewBag.WelcomeMessage = "Hello, ";
                if (cookie != null)
                {
                    ViewBag.isAda = "true".ToUpper().Equals(cookie["ADA"].ToString().ToUpper()) ? true : false;
                }
                else
                {
                    ViewBag.isAda = false;
                }
            }
            catch (Exception)
            {
                RedirectToAction("Error", "Home");
            }
        }

        protected void SaveCookie(bool ADA)
        {
            HttpCookie cookie = new HttpCookie("ImageSharing");
            cookie.Expires = DateTime.Now.AddMonths(3);
            cookie.HttpOnly = true;
            cookie["ADA"] = ADA ? "true" : " false";
            Response.Cookies.Add(cookie);
        }

        protected IEnumerable<ApplicationUser> Activeusers()
        {
            var db = new ApplicationDbContext();
            return db.Users.Where(u => u.Active);
        }

        protected IEnumerable<Image> ApprovedImages(IEnumerable<Image> images)
        {
            return images.Where(img => img.Approved);
        }

        protected IEnumerable<Image> ApprovedImages()
        {
            var db = new ApplicationDbContext();
            return ApprovedImages(db.Images);
        }

        protected SelectList UserSelectList()
        {
            string defaultId = GetLoggedInUser().Id;
            return new SelectList(Activeusers(), "Id", "UserName", defaultId);
        }

        protected ApplicationUser GetLoggedInUser()
        {
            return userManager.FindById(User.Identity.GetUserId());
        }

        #region GetLoggedInUser & ForceLogin
        /*
        protected String GetLoggedInUser()
        {
            try
            {
                HttpCookie cookie = Request.Cookies.Get("ImageSharing");
                if (cookie != null && cookie["UserId"] != null)
                {
                    return cookie["UserId"];
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
               RedirectToAction("Error", "Home");
               return null;
            }
            
        }

        protected ActionResult ForceLogin()
        {
            return RedirectToAction("Login", "Account");
        }
        */
        #endregion
    }
}