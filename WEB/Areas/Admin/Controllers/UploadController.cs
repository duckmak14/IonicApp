using Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WEB.Models;
using WebMatrix.WebData;
using WebModels;

namespace WEB.Areas.Admin.Controllers
{
    [VanTaiAuthorize]

    public class UploadController : BaseController
    {
        long maxsize =1073741824;
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        public ActionResult File(string uid)
        {
            ViewBag.UID = uid;
            return View();
        }
        public ActionResult Image(string uid)
        {
            ViewBag.UID = uid;
            return View();
        }
        public ActionResult Flash(string uid)
        {
            ViewBag.UID = uid;
            return View();
        }
        //protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        //{
        //    var uid = requestContext.RouteData.Values["uid"];
        //    if (uid != null)
        //    {
        //        ViewBag.UID = uid;
        //    }
        //    else { ViewBag.UID = ""; }
        //    base.Initialize(requestContext);
        //}
        private bool ValidImage(string path)
        {
            try
            {
                var ext = Path.GetExtension(path).ToLower();
                return (ext == ".gif" || ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp");
            }
            catch (Exception)
            {

                return false;
            }
        }
        private bool ValidFlash(string path)
        {
            try
            {
                var ext = Path.GetExtension(path).ToLower();
                return (ext == ".swf"  );
            }
            catch (Exception)
            {

                return false;
            }
        }
        [HttpGet]
        public JsonResult JImage(string uid,   int? folder)
        {
             
                var model = db.WebContentUploads.Where(x =>
                    (folder == null || (x.FolderID != null && x.FolderID == folder))
                    && x.UID.Equals(uid)).OrderByDescending(x=>x.ID).AsEnumerable().Where(x => ValidImage(x.FullPath));
              
                return RenderJson(model);
          

           // return Json(new { Success = false }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult JFile(string uid, int? folder)
        {

            var model = db.WebContentUploads.Where(x =>
                (folder == null || (x.FolderID != null && x.FolderID == folder))
                && x.UID.Equals(uid)).OrderByDescending(x => x.ID).AsEnumerable();

            return RenderJson(model);


            // return Json(new { Success = false }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult JFlash(string uid, int? folder)
        {

            var model = db.WebContentUploads.Where(x =>
                (folder == null || (x.FolderID != null && x.FolderID == folder))
                && x.UID.Equals(uid)).OrderByDescending(x => x.ID).AsEnumerable().Where(x => ValidFlash(x.FullPath));

            return RenderJson(model);


            // return Json(new { Success = false }, JsonRequestBehavior.AllowGet);
        }
        private JsonResult  RenderJson(IEnumerable<WebContentUpload> model)
        {
            var jarray = new JArray();
            foreach(var item in model)
            {
                var jobj = new JObject();
                jobj.Add(new JProperty("ID", item.ID));
                jobj.Add(new JProperty("Title", item.Title));
                jobj.Add(new JProperty("Extension", Path.GetExtension(item.FullPath)));
                jobj.Add(new JProperty("MimeType", item.MimeType));
                jobj.Add(new JProperty("Size", GetFileSize(item.FullPath)));
                jobj.Add(new JProperty("FolderID", item.FolderID));
                jobj.Add(new JProperty("FullPath", item.FullPath));
                jobj.Add(new JProperty("CreatedBy", item.CreatedBy));
                jobj.Add(new JProperty("CreatedDate", item.CreatedDate));
                jarray.Add(jobj);
            }
            return Json(new { Success = true, Json = jarray.ToString() }, JsonRequestBehavior.AllowGet);
        }
        private double GetFileSize(string path)
        {
            try
            {
                FileInfo file = new FileInfo(Server.MapPath(path));

                return file.Length;
            }
            catch (Exception)
            {

                return 0;
            }     
        }

        [HttpPost]
        public ContentResult ImageCk(string uid)
        {
            var CKEditorFuncNum = "";
            HttpPostedFileBase file = Request.Files["upload"];
            CKEditorFuncNum = Request["CKEditorFuncNum"];
            var now = DateTime.Now;
            var fullPath = file.ImageSave("/uploads/" + (now.Month.ToString("00") + now.Year), 1366, 1366);
            db.WebContentUploads.Add(new WebContentUpload()
            {
                Title = file.FileName,
                MetaTitle = file.FileName.UnsignNormalize(),
                FullPath = fullPath,
                UID = uid,
                MimeType = ApplicationService.GetMimeType(Path.GetExtension(fullPath)),

                CreatedBy = WebSecurity.CurrentUserName,
                CreatedDate = DateTime.Now
            });
            db.SaveChanges();
            return Content("<script>window.parent.CKEDITOR.tools.callFunction(" + CKEditorFuncNum + ", \"" + fullPath + "\");</script>");
        }
        [HttpPost]
        public ContentResult FlashCk(string uid)
        {
            var CKEditorFuncNum = "";  
            HttpPostedFileBase file = Request.Files["upload"];
            CKEditorFuncNum = Request["CKEditorFuncNum"];
            var now = DateTime.Now;
            var fullPath = file.FileSave(maxsize, "/uploads/" + (now.Month.ToString("00") + now.Year));
            db.WebContentUploads.Add(new WebContentUpload() { Title = file.FileName, MetaTitle = file.FileName.UnsignNormalize(), 
                FullPath = fullPath, UID= uid,  MimeType = ApplicationService.GetMimeType(Path.GetExtension(fullPath)) ,
            
               CreatedBy = WebSecurity.CurrentUserName,
              CreatedDate = DateTime.Now 
            });
            db.SaveChanges();  
            return Content("<script>window.parent.CKEDITOR.tools.callFunction(" + CKEditorFuncNum + ", \"" + fullPath + "\");</script>");
        }
        [HttpPost]
        public ContentResult FileCk(string uid)
        {
            var CKEditorFuncNum = "";
            HttpPostedFileBase file = Request.Files["upload"];
            CKEditorFuncNum = Request["CKEditorFuncNum"];
            var now = DateTime.Now;
            var fullPath = file.FileSave(maxsize, "/uploads/" + (now.Month.ToString("00") + now.Year));
            db.WebContentUploads.Add(new WebContentUpload()
            {
                Title = file.FileName,
                MetaTitle = file.FileName.UnsignNormalize(),
                FullPath = fullPath,
                UID = uid,
                MimeType = ApplicationService.GetMimeType(Path.GetExtension(fullPath)),

                CreatedBy = WebSecurity.CurrentUserName,
                CreatedDate = DateTime.Now
            });
            db.SaveChanges();
            return Content("<script>window.parent.CKEDITOR.tools.callFunction(" + CKEditorFuncNum + ", \"" + fullPath + "\");</script>");
        }
        [HttpPost]
        public ActionResult UploadImage(string uid, FormCollection form)
        {                  
            var now = DateTime.Now;
            foreach (string files in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[files];

                var mime = "";
                if (file != null)
                {
                    var fullPath = file.ImageSave("/uploads/" + (now.Month.ToString("00") + now.Year), 1366, 1366);
                    try
                    {
                        mime = ApplicationService.GetMimeType(Path.GetExtension(file.FileName));
                    }
                    catch (Exception)
                    { }
                    var title = form["title"];
                    if (string.IsNullOrEmpty(title)) title = file.FileName;
                    var content = new WebContentUpload()
                    {
                        Title = title,
                        MetaTitle = file.FileName.UnsignNormalize(),
                        FullPath = fullPath,
                        UID = uid, 
                        MimeType = (!String.IsNullOrEmpty(mime) && !String.IsNullOrWhiteSpace(mime)) ? mime : null
                    };
                    content.CreatedBy = WebSecurity.CurrentUserName;
                    content.CreatedDate = DateTime.Now;
                    db.WebContentUploads.Add(content);
                    db.SaveChanges();
                }
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult UploadFlash(string uid, FormCollection form)
        {
            var now = DateTime.Now;
            foreach (string files in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[files];

                var mime = "";
                if (file != null)
                {
                    var fullPath = file.FileSave(maxsize, "/uploads/" + (now.Month.ToString("00") + now.Year));
                    try
                    {
                        mime = ApplicationService.GetMimeType(Path.GetExtension(file.FileName));
                    }
                    catch (Exception)
                    { }
                    var title = form["title"];
                    if (string.IsNullOrEmpty(title)) title = file.FileName;
                    var content = new WebContentUpload()
                    {
                        Title = title,
                        MetaTitle = file.FileName.UnsignNormalize(),
                        FullPath = fullPath,
                        UID = uid,
                        MimeType = (!String.IsNullOrEmpty(mime) && !String.IsNullOrWhiteSpace(mime)) ? mime : null
                    };
                    content.CreatedBy = WebSecurity.CurrentUserName;
                    content.CreatedDate = DateTime.Now;
                    db.WebContentUploads.Add(content);
                    db.SaveChanges();
                }
            }
            return Json(new { success = true });
        }
        [HttpPost]
        public ActionResult UploadFile(string uid,string title, FormCollection form)
        {
            var now = DateTime.Now;
            foreach (string files in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[files];

                var mime = "";
                if (file != null)
                {
                    var fullPath = file.FileSave(maxsize, "/uploads/" + (now.Month.ToString("00") + now.Year));
                    try
                    {
                        mime = ApplicationService.GetMimeType(Path.GetExtension(file.FileName));
                    }
                    catch (Exception)
                    { }
                   // var title = form["title"];
                    if (string.IsNullOrEmpty(title)) title = file.FileName;
                    var content = new WebContentUpload()
                    {
                        Title = title,
                        MetaTitle = file.FileName.UnsignNormalize(),
                        FullPath = fullPath,
                        UID = uid,
                        MimeType = (!String.IsNullOrEmpty(mime) && !String.IsNullOrWhiteSpace(mime)) ? mime : null
                    };
                    content.CreatedBy = WebSecurity.CurrentUserName;
                    content.CreatedDate = DateTime.Now;
                    db.WebContentUploads.Add(content);
                    db.SaveChanges();
                }
            }
            return Json(new { success = true });
        }

        public ActionResult ImageLink(string uid, string func )
        {
            ViewBag.UID = uid;  
            ViewBag.func = func;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]    
        [ValidateInput(false)]
        public ActionResult ImageLink(string uid, string func, string link )
        {
            try
            {
                ViewBag.func = func;
                 
                var now = DateTime.Now;
                WebClient wc = new WebClient();
                var filename = Path.GetFileName(link); var newName = filename.GeneratorFileName();
                string localpath = "/uploads/" + (now.Month.ToString("00") + now.Year);
                var fullpath = localpath + "/" + newName;
                var dir = new System.IO.DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath(localpath));
                if (!dir.Exists) dir.Create();
                wc.DownloadFile(link, System.Web.HttpContext.Current.Server.MapPath(fullpath));       
                try
                {
                    if (ImageTools.ValidateImage(System.Web.HttpContext.Current.Server.MapPath(fullpath)))
                    {
                        var result = ImageTools.ResizeImage(System.Web.HttpContext.Current.Server.MapPath(fullpath), System.Web.HttpContext.Current.Server.MapPath(fullpath), 1000, 1000, true, 80);

                        db.WebContentUploads.Add(new WebContentUpload() { Title = filename, 
                            MetaTitle = filename.UnsignNormalize(), 
                            FullPath = fullpath, 
                            UID = uid,
                             MimeType = ApplicationService.GetMimeType(Path.GetExtension(fullpath)), 
                        
                             CreatedBy = WebSecurity.CurrentUserName,
                 CreatedDate = DateTime.Now 
                        });
                        db.SaveChanges();
                    }
                    else
                    {
                        Utility.DeleteFile(fullpath); fullpath = link;
                        ModelState.AddModelError("", "Liên kết không hợp lệ.");
                    }
                }
                catch (Exception)
                { }                
                return View(new List<string>() { fullpath });
            }
            catch (Exception ex)
            {
                 
                ModelState.AddModelError("",ex.Message);
                return View(new List<string>() { link });
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.WebContentUploads.Find(id);
            if (item == null)
                return Json(new { success = false });
            db.WebContentUploads.Remove(item);
            db.SaveChanges();
            var filePath = Server.MapPath(item.FullPath);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(Server.MapPath(item.FullPath));
            return Json(new { success = true });
        }
            
        [HttpPost]
        public ActionResult UpdateTitle(int id,string title)
        {
            var item = db.WebContentUploads.Find(id);
            if (item == null)
                return Json(new { success = false });
            item.Title = title;
            db.Entry(item).Property(a => a.Title).IsModified = true;
            db.SaveChanges();       
            return Json(new { success = true });
        }

        public ActionResult Index()
        {
            return View();
        }



        [HttpPost]
        public ContentResult CkImageUpload(int id)
        {


            var url = ""; var CKEditorFuncNum = "";

            HttpPostedFileBase uploads = Request.Files["upload"];
            CKEditorFuncNum = Request["CKEditorFuncNum"];
            if (uploads != null)
            {
                try
                {

                    var name = uploads.FileName;
                    var newName = Utility.GeneratorFileName(name);
                    var dir = new System.IO.DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath("/Uploads/ckfiles/"));
                    if (!dir.Exists) dir.Create();
                    var fullpath = "/Uploads/ckfiles/" + newName;
                    uploads.SaveAs(System.Web.HttpContext.Current.Server.MapPath(fullpath));
                    url = fullpath;

                    try
                    {
                        if (ImageTools.ValidateImage(System.Web.HttpContext.Current.Server.MapPath(fullpath)))
                        {
                            var result = ImageTools.ResizeImage(System.Web.HttpContext.Current.Server.MapPath(fullpath), System.Web.HttpContext.Current.Server.MapPath(fullpath), 1000, 3000, true, 80);
                        }
                        else
                        {
                            Utility.DeleteFile(fullpath);
                        }
                    }
                    catch (Exception)
                    { }
                }
                catch (Exception)
                { }
            }
            return Content("<script>window.parent.CKEDITOR.tools.callFunction(" + CKEditorFuncNum + ", \"" + url + "\");</script>");
        }
      
    }
}