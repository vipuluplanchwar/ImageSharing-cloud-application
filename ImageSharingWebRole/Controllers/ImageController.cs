using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Data.Entity;
//using WebMatrix.WebData;
using ImageSharingWebRole.Models;
using ImageSharingWebRole.DAL;
using System.Linq;

namespace ImageSharingWebRole.Controllers
{
    [Authorize]
    public class ImageController : BaseController
    {

        private ValidationQueue ValidationQue = new ValidationQueue();

        [HttpGet]
        public ActionResult Upload()
        {
            CheckAda();
            if (TempData["UploadSuccessMessage"] != null)
            {
                ModelState.Clear();
                ViewBag.SuccessMessage = TempData["UploadSuccessMessage"];
            }
            else ViewBag.SuccessMessage = string.Empty;

            ViewBag.Message = "";
            SelectList tags = new SelectList(ApplicationDbContext.Tags, "Id", "Name", 1);
            ViewBag.Tags = tags;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(ImageView image,
                                   HttpPostedFileBase ImageFile)
        {
            CheckAda();

            TryUpdateModel(image);
            ViewBag.Tags = new SelectList(ApplicationDbContext.Tags, "Id", "Name", image.TagId);
            if (ModelState.IsValid)
            {
                ApplicationUser AppUser = GetLoggedInUser();
                if (AppUser != null)
                {
                    if (!ImageFile.FileName.Split('.')[1].ToUpper().Equals("JPG"))
                    {
                        ViewBag.ImageValidation = "File type must be JPG";
                        return View();
                    }

                    double fileSize = 4;
                    if (((double)ImageFile.ContentLength / (1024 * 1024)) > fileSize)
                    {
                        ViewBag.ImageValidation = "File size is greater";
                        return View();
                    }
                    Image imageEntity = new Image();
                    imageEntity.Caption = image.Caption;
                    imageEntity.Description = image.Description;
                    imageEntity.DateTaken = image.DateTaken;

                    imageEntity.User = AppUser;
                    imageEntity.Approved = false;
                    imageEntity.TagId = image.TagId;
                    imageEntity.Validated = false;

                    ImageView imageView = new ImageView();
                    imageView.Uri = ImageStorage.ImageURI(Url, imageEntity.Id);
                    imageView.Caption = imageEntity.Caption;
                    imageView.Description = imageEntity.Description;
                    imageView.DateTaken = imageEntity.DateTaken;
                    imageView.UserId = imageEntity.User.UserName;

                    if (ImageFile != null && ImageFile.ContentLength > 0)
                    {
                        ApplicationDbContext.Images.Add(imageEntity);
                        ApplicationDbContext.SaveChanges();

                        ImageStorage.SaveFile(Server, ImageFile, imageEntity.Id);
                        imageView.Id = imageEntity.Id;
                        LogContext.addLogEntry(User.Identity.Name, imageView);

                        ValidationRequest ValidationReq = new ValidationRequest();
                        ValidationReq.ImageId = imageEntity.Id;
                        ValidationReq.UserId = imageView.UserId;

                        //Send message in the queue
                        ValidationQueue.Send(ValidationReq);

                        TempData["UploadSuccessMessage"] = "Image has been uploaded successfully.";
                        return RedirectToAction("Upload");
                    }
                    else
                    {
                        ViewBag.Message = "No image file specified";
                        return View();
                    }
                }
                else
                {
                    ViewBag.Message = "No such user registered";
                    return View();
                }
            }
            else
            {
                ViewBag.Message = "Please correct the errors in the form!";
                return View();
            }
        }

        [HttpGet]
        public ActionResult Query()
        {
            CheckAda();

            ViewBag.Message = "";
            return View();
        }

        [HttpGet]
        public ActionResult Details(int Id)
        {
            CheckAda();

            Image imageEntity = ApplicationDbContext.Images.Find(Id);

            bool isPreview = false;
            if (TempData["Preview"] != null)
            {
                isPreview = Convert.ToBoolean(TempData["Preview"]);
            }

            if (imageEntity != null)
            {
                ImageView imageView = new ImageView();
                imageView.Id = imageEntity.Id;
                imageView.Uri = ImageStorage.ImageURI(Url, imageEntity.Id);
                imageView.Caption = imageEntity.Caption;
                imageView.Description = imageEntity.Description;
                imageView.DateTaken = imageEntity.DateTaken;
                imageView.TagName = imageEntity.Tag.name;
                imageView.UserId = imageEntity.User.UserName;
                LogContext.addLogEntry(User.Identity.Name, imageView);

                return View(imageView);
            }
            else
            {
                return View((ImageView)null);
            }
        }

        [HttpGet]
        public ActionResult Edit(int Id)
        {
            CheckAda();

            Image imageEntity = ApplicationDbContext.Images.Find(Id);
            if (imageEntity != null)
            {
                ApplicationUser User = GetLoggedInUser();
                if (imageEntity.User.UserName.Equals(User.Email))
                {
                    ViewBag.Message = "";
                    ViewBag.Tags = new SelectList(ApplicationDbContext.Tags, "Id", "name", imageEntity.TagId);
                    ImageView image = new ImageView();
                    image.Id = imageEntity.Id;
                    image.Uri = ImageStorage.ImageURI(Url, Id);
                    image.TagId = imageEntity.TagId;
                    image.Caption = imageEntity.Caption;
                    image.Description = imageEntity.Description;
                    image.DateTaken = imageEntity.DateTaken;

                    return View("Edit", image);
                }
                else
                {
                    return RedirectToAction("Error", "Home", new { errid = "EditNotAuth" });
                }
            }
            else
            {
                ViewBag.Message = "Image with identifier" + Id + " not found";
                ViewBag.Id = Id;
                return RedirectToAction("Error", "Home", new { errid = "EditNotFound" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int Id, ImageView image)
        {
            CheckAda();

            ApplicationUser User = GetLoggedInUser();
            try
            {
                Image imageEntity = ApplicationDbContext.Images.Find(Id);
                ViewBag.Tags = new SelectList(ApplicationDbContext.Tags, "Id", "name", imageEntity.TagId);
                if (ModelState.IsValid)
                {
                    if (imageEntity != null)
                    {
                        if (imageEntity.User.UserName.Equals(User.Email))
                        {
                            ViewBag.Message = string.Empty;
                            imageEntity.TagId = image.TagId;
                            imageEntity.Caption = image.Caption;
                            imageEntity.Description = image.Description;
                            imageEntity.Approved = false;
                            imageEntity.DateTaken = image.DateTaken;

                            ApplicationDbContext.Entry(imageEntity).State = EntityState.Modified;
                            ApplicationDbContext.SaveChanges();

                            ViewBag.SuccessMessage = "Image changes saved.";
                            TempData["Preview"] = true;
                            return RedirectToAction("Details", new { Id = Id });
                        }
                        else
                        {
                            return RedirectToAction("Error", "Home", new { errid = "EditNotAuth" });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Error", "Home", new { errid = "EditNotFind" });
                    }
                }
                else
                {
                    ViewBag.Message = "Please correct all errors!";
                    image.Id = Id;
                    return View("Edit", image);
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        public ActionResult Delete(int Id)
        {
            CheckAda();
            try
            {
                Image imageEntity = ApplicationDbContext.Images.Find(Id);
                if (imageEntity != null)
                {
                    ImageView imageView = new ImageView();
                    imageView.Id = imageEntity.Id;
                    imageView.Caption = imageEntity.Caption;
                    imageView.Description = imageEntity.Description;
                    imageView.DateTaken = imageEntity.DateTaken;
                    imageView.TagName = imageEntity.Tag.name;
                    imageView.UserId = imageEntity.User.UserName;
                    imageView.Uri = ImageStorage.ImageURI(Url, imageEntity.Id);
                    return View(imageView);
                }
                else
                {
                    return RedirectToAction("Error", "Home", new { errid = "Details" });
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(FormCollection values, int Id)
        {
            CheckAda();
            ApplicationUser User = GetLoggedInUser();
            if (User != null)
            {
                try
                {
                    Image imageEntity = ApplicationDbContext.Images.Find(Id);
                    if (imageEntity != null)
                    {
                        if (imageEntity.User.UserName.Equals(User.Email))
                        {
                            ApplicationDbContext.Images.Remove(imageEntity);
                            ApplicationDbContext.SaveChanges();
                            List<int> lstDeleteImage = new List<int>();

                            lstDeleteImage.Add(imageEntity.Id);
                            ImageStorage.DeleteBlobs(lstDeleteImage, Server);

                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            return RedirectToAction("Error", "Home", new { errid = "DeleteNotAuth" });
                        }
                    }
                    else
                    {
                        return RedirectToAction("Error", "Home", new { errid = "DeleteNotFound" });
                    }
                }
                catch (Exception)
                {
                    return RedirectToAction("Error", "Home");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpGet]
        public ActionResult ListAll()
        {
            CheckAda();

            IEnumerable<Image> appImage = ApprovedImages().ToList();

            string userid = User.Identity.Name;

            ViewBag.UserId = userid;
            return View(appImage);

        }

        [HttpGet]
        public ActionResult ListByUser()
        {
            CheckAda();
            List<ApplicationUser> ActiveUsers = new List<ApplicationUser>();
            foreach (var user in ApplicationDbContext.Users)
            {
                if (user.Active)
                {
                    ActiveUsers.Add(user);
                }
            }
            SelectList users = new SelectList(ActiveUsers, "Id", "UserName", 1);
            return View(users);
        }

        [HttpGet]
        public ActionResult DoListByUser(string Id)
        {
            CheckAda();
            ApplicationUser User = GetLoggedInUser();

            ApplicationUser user = ApplicationDbContext.Users.Find(Id);
            if (user != null)
            {
                ViewBag.UserId = User.Email;
                return View("ListAll", ApprovedImages(user.Images));
            }
            else
            {
                return RedirectToAction("Error", "Home", new { errid = "ListByUser" });
            }

        }

        [HttpGet]
        public ActionResult ListByTag()
        {
            CheckAda();
            SelectList tags = new SelectList(ApplicationDbContext.Tags, "Id", "Name", 1);
            return View(tags);
        }

        [HttpGet]
        public ActionResult DoListByTag(int Id)
        {
            CheckAda();
            ApplicationUser User = GetLoggedInUser();

            Tag tag = ApplicationDbContext.Tags.Find(Id);
            if (tag != null)
            {
                ViewBag.UserId = User.Email;
                return View("ListAll", ApprovedImages(tag.Images));
            }
            else
            {
                return RedirectToAction("Error", "Home", new { errid = "ListByTag" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Approver")]
        //[Authorize(Users = "jfk@example.org")]
        public ActionResult Approve()
        {
            CheckAda();
            ViewBag.Message = string.Empty;

            return View(GetNotApprovedImages());
        }

        [HttpPost]
        [Authorize(Roles = "Approver")]
        //[Authorize(Users = "jfk@example.org")]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(IList<SelectImageView> model)
        {
            CheckAda();
            ViewBag.Message = string.Empty;
            List<Image> LstDeleteImages = new List<Image>();
            List<int> LstDeleteImageBlobs = new List<int>();

            foreach (var item in model)
            {
                Image image = ApplicationDbContext.Images.Find(item.Id);

                if (image != null)
                {
                    if (!image.Approved && item.Approved && !item.Delete)
                    {
                        ApplicationDbContext.Images.Find(item.Id).Approved = true;
                        ViewBag.Message = "Image(s) approved";
                    }
                    else if (!image.Approved && !item.Approved && item.Delete)
                    {
                        LstDeleteImages.Add(image);
                        LstDeleteImageBlobs.Add(image.Id);
                    }
                }
            }

            ApplicationDbContext.Images.RemoveRange(LstDeleteImages);

            foreach (var image in LstDeleteImages)
            {
                //String imgFileName = Server.MapPath("~/Content/Images/" + image.Id + ".jpg");
                ImageStorage.DeleteBlobs(LstDeleteImageBlobs, Server);
            }

            ApplicationDbContext.SaveChanges();
            ViewBag.SuccessMessage = "Images are approved/deleted successfully.";
            return View(GetNotApprovedImages());
        }

        private List<SelectImageView> GetNotApprovedImages()
        {
            List<SelectImageView> model = new List<SelectImageView>();

            foreach (var item in ApplicationDbContext.Images)
            {
                if (!item.Approved && item.Validated)
                {
                    model.Add(new SelectImageView(item.Id, item.Caption, item.Approved));
                }
            }

            return model;
        }

        [HttpGet]
        [Authorize(Roles = "Supervisor")]
        public ActionResult ImageList()
        {
            CheckAda();
            DateTime date = DateTime.Now;
            List<SelectListItem> dateList = new List<SelectListItem>();
            for (int i = 0; i <= 13; i++)
            {
                dateList.Add(new SelectListItem
                {
                    Value = date.AddDays(-i).ToShortDateString(),
                    Text = date.AddDays(-i).ToShortDateString()
                });
                ViewBag.dateList = dateList;

            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Supervisor")]
        public ActionResult ImageViews(DateTime dateTaken)
        {
            try
            {
                CheckAda();
                IEnumerable<LogEntry> entries = LogContext.select(dateTaken);
                List<LogEntry> PrintEntries = new List<LogEntry>();

                foreach (var item in entries)
                {
                    Image img = ApplicationDbContext.Images.Find(item.ImageId);
                    if (img != null)
                    {
                        if (img.Approved)
                        {
                            PrintEntries.Add(item);
                        }
                    }
                }

                return View(PrintEntries.AsEnumerable<LogEntry>());
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        public ActionResult ListUploadQueue()
        {
            CheckAda();
            ViewBag.SuccessMessage = string.Empty;
            ApplicationUser User = GetLoggedInUser();
            
            ApplicationUser user = ApplicationDbContext.Users.Find(User.Id);

            if (user != null)
            {
                ViewBag.UserId = user.Email;
                return View("ListUploadQueue", MessageQueue.GetResponseMessages(user.Email.Substring(0, user.Email.IndexOf('@'))).AsEnumerable<ValidationInfo>());
            }
            else
            {
                ViewBag.UserId = string.Empty;
                return View("ListUploadQueue", null);
            }
        }

        [HttpPost]
        public ActionResult ClearQueue()
        {
            CheckAda();
            ApplicationUser User = GetLoggedInUser();

            ApplicationUser user = ApplicationDbContext.Users.Find(User.Id);
            if (user != null)
            {
                MessageQueue.DeleteResponseMessages(user.Email.Substring(0, user.Email.IndexOf('@')));
                ViewBag.SuccessMessage = "Queue cleared.";
                return View("ListUploadQueue", MessageQueue.GetResponseMessages(user.Email.Substring(0, user.Email.IndexOf('@'))).AsEnumerable<ValidationInfo>());
            }
            else
            {
                ViewBag.SuccessMessage = "Queue not cleared.";
                return View("ListUploadQueue", MessageQueue.GetResponseMessages(user.Email.Substring(0, user.Email.IndexOf('@'))).AsEnumerable<ValidationInfo>());
            }
        }
    }
}

