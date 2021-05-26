using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebModels;
using System.Data;
using Common;
using WebMatrix.WebData;
using WEB.Models;
using System.Data.Entity;
using System.IO;
namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public class WebSimpleContentController : BaseController
    {
        private WebContext db = new WebContext();
         

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult WebSimpleContent_Read([DataSourceRequest] DataSourceRequest request)
        {
            var contents = db.WebSimpleContents.AsNoTracking();

            return Json(contents.ToDataSourceResult(request));
        }


        //
        // GET: /Admin/WebSimpleContent/Create

        public ActionResult Add()
        {
            var model = new WebSimpleContent();
            model.Culture = ApplicationService.Culture;
            return View(model);
        }

        //
        // POST: /Admin/WebSimpleContent/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(WebSimpleContent model, HttpPostedFileBase image)
        {
            var now = DateTime.Now;
            if (ModelState.IsValid)
            {
                model.CreatedBy = WebSecurity.CurrentUserName;
                model.CreatedDate = DateTime.Now;
                model.ModifiedBy = WebSecurity.CurrentUserName;
                model.ModifiedDate = DateTime.Now;

                if (image != null)
                {
                    model.Image = image.ImageSave("/uploads/image/" + (now.Month.ToString("00") + now.Year), 1366, 1366);
                    db.WebContentUploads.Add(new WebContentUpload()
                    {
                        Title = image.FileName,
                        MetaTitle = image.FileName.UnsignNormalize(),
                        FullPath = model.Image,
                        UID = ViewBag.GAK,
                        MimeType = ApplicationService.GetMimeType(Path.GetExtension(image.FileName)),
                        CreatedBy = WebSecurity.CurrentUserName,
                        CreatedDate = DateTime.Now
                    });
                    db.SaveChanges();
                } 
                

                db.WebSimpleContents.Add(model);
                db.SaveChanges(); 
                
                
                ViewBag.StartupScript = "create_success();";
                return View(model);
            }

            return View(model);
        }

        //
        // GET: /Admin/WebSimpleContent/Edit/5

        public ActionResult Edit(int id = 0)
        {
            WebSimpleContent model = db.WebSimpleContents.Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
             
            return View(model);
        }

        //
        // POST: /Admin/WebSimpleContent/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken][ValidateInput(false)]
        public ActionResult Edit(WebSimpleContent model, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                model.ModifiedBy = WebSecurity.CurrentUserName;
                model.ModifiedDate = DateTime.Now;

                if (image != null)
                {
                    var now = DateTime.Now;

                    model.Image = image.ImageSave("/uploads/image/" + (now.Month.ToString("00") + now.Year), 1366, 1366);
                    db.WebContentUploads.Add(new WebContentUpload()
                    {
                        Title = image.FileName,
                        MetaTitle = image.FileName.UnsignNormalize(),
                        FullPath = model.Image,
                        UID = ViewBag.GAK,
                        MimeType = ApplicationService.GetMimeType(Path.GetExtension(image.FileName)),
                        CreatedBy = WebSecurity.CurrentUserName,
                        CreatedDate = DateTime.Now
                    });
                    db.SaveChanges();
                } 
                db.WebSimpleContents.Attach(model);
                db.Entry(model).Property(a => a.Title).IsModified = true;
                db.Entry(model).Property(a => a.Description).IsModified = true;
                db.Entry(model).Property(a => a.Key).IsModified = true;
                db.Entry(model).Property(a => a.Body).IsModified = true;
                db.Entry(model).Property(a => a.Image).IsModified = true;
                db.Entry(model).Property(a => a.Link).IsModified = true; 

                db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                db.Entry(model).Property(a => a.ModifiedDate).IsModified = true;
                db.Entry(model).Property(a => a.Culture).IsModified = true;
                db.SaveChanges();

                  
                ViewBag.StartupScript = "edit_success();";
                return View(model);
            }
            return View(model);
        }



        public ActionResult Delete(int id)
        {

            var role = db.Set<WebSimpleContent>().Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View("Delete", role);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(WebSimpleContent model)
        {



            try
            {
                var role = db.WebSimpleContents.Attach(model);
                db.Set<WebSimpleContent>().Remove(role);
                db.SaveChanges();
                 

                ViewBag.StartupScript = "delete_success();";
                return View();
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

        }


        public ActionResult Deletes(string id)
        {

            var objects = id.Split(',');
            var lstSiteID = new List<int>();
            foreach (var obj in objects)
            {
                try
                {
                    lstSiteID.Add(int.Parse(obj.ToString()));
                }
                catch (Exception)
                { }
            }

            var temp = from p in db.Set<WebSimpleContent>()
                       where lstSiteID.Contains(p.ID)
                       select p;

            return View(temp.ToList());

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<WebSimpleContent> model)
        {

            var temp = new List<WebSimpleContent>();
            foreach (var item in model)
            {
                try
                {
                    db.Entry(item).State = EntityState.Deleted;
                    db.SaveChanges();
                     

                }
                catch (Exception)
                {
                    db.Entry(item).State = EntityState.Unchanged;
                    temp.Add(item);
                }
            }

            if (temp.Count == 0)
            {
                ViewBag.StartupScript = "deletes_success();";
                return View(temp);
            }
            else if (temp.Count > 0)
            {
                ViewBag.StartupScript = "top.$('#grid').data('kendoGrid').dataSource.read(); top.treeview.dataSource.read();";
                ModelState.AddModelError("", Resources.Common.ErrorDeleteItems);
                return View(temp);
            }
            else
            {
                ViewBag.StartupScript = "deletes_success();";
                return View();
            }

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
                    var dir = new System.IO.DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath("/content/uploads/simplecontent/auto/"));
                    if (!dir.Exists) dir.Create();
                    var fullpath = "/content/uploads/simplecontent/auto/" + newName;
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

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}