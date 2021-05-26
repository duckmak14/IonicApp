using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WEB.Filters;
using WebMatrix.WebData;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using WebModels;
using Newtonsoft.Json.Linq;
using System.Data;
using System;
using System.Collections.Generic;
using Common;
using System.IO;
using Kendo.Mvc;
using System.Data.Entity;
using WEB.Models;

namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public class WebLinkController : BaseController
    {
        private WebContext db = new WebContext();

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            return  View();
        }

   

        public ActionResult WebLink_Read([DataSourceRequest] DataSourceRequest request)
        {
            var slides = db.WebLinks.Select(x => new { x.ID, x.Title,x.Link,x.Culture,x.Target, x.ModifiedBy, x.ModifiedDate,x.Order }); 
            if (request.Sorts.Count == 0)
            {
                request.Sorts.Add(new SortDescriptor("Order", System.ComponentModel.ListSortDirection.Descending));
            }

            return Json(slides.ToDataSourceResult(request));
        }

        

        public ActionResult Add()
        {
            var weblink = new WebLink();
            weblink.Culture = ApplicationService.Culture;
            
            return View(weblink);
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(WebLink model, HttpPostedFileBase image)
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
                
                db.Set<WebLink>().Add(model);
                db.SaveChanges();
                ViewBag.StartupScript = "create_success();";
                return View(model);
            }
            else
            {
                return View(model);
            }
        }

        public ActionResult Edit(int id)
        {
            
            var model = db.Set<WebLink>().Find(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(WebLink model, HttpPostedFileBase image)
        {
            var now = DateTime.Now;
            if (ModelState.IsValid)
            {
                try
                {
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

                    db.WebLinks.Attach(model);
                    db.Entry(model).Property(a => a.Title).IsModified = true;
                    db.Entry(model).Property(a => a.Link).IsModified = true;
                    db.Entry(model).Property(a => a.Description).IsModified = true;
                    db.Entry(model).Property(a => a.Target).IsModified = true;
                    db.Entry(model).Property(a => a.Culture).IsModified = true;
                    db.Entry(model).Property(a => a.Order).IsModified = true;
                    db.Entry(model).Property(a => a.Image).IsModified = true;
                    db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                    db.Entry(model).Property(a => a.ModifiedDate).IsModified = true; 
                    db.SaveChanges();
                    ViewBag.StartupScript = "edit_success();";
                    return View(model);

                }
                catch (Exception ex)
                {

                    ModelState.AddModelError("", ex.Message);
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }

        public ActionResult Delete(int id)
        {
            {
                var model = db.Set<WebLink>().Find(id);

                if (model == null)
                {
                    return HttpNotFound();
                }

                return View("Delete", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Delete(WebLink model)
        {
            try
            {
                db.Entry(model).State = EntityState.Deleted;
                db.SaveChanges();
                ViewBag.StartupScript = "delete_success();";

                return View(model);
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
            var lstObjId = new List<int>();

            foreach (var obj in objects)
            {
                try
                {
                    lstObjId.Add(int.Parse(obj.ToString()));
                }
                catch (Exception)
                { }
            }

            var temp = from p in db.Set<WebLink>()
                       where lstObjId.Contains(p.ID)
                       select p;

            return View(temp.ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<WebLink> model)
        {
            var temp = new List<WebLink>();
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
                ViewBag.StartupScript = "top.$('#grid').data('kendoGrid').dataSource.read();";
                ModelState.AddModelError("", Resources.Common.ErrorDeleteItems);
                return View(temp);
            }
            else
            {
                ViewBag.StartupScript = "deletes_success();";
                return View();
            }
        }
    }
}
