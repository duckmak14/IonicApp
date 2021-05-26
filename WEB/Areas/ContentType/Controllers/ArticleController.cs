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
using Kendo.Mvc;
using WEB.Models;
using System.Data.Entity;
using System.Web.Routing;
using System.IO;

namespace WEB.Areas.ContentType.Controllers
{
    public partial class ArticleController : BaseController
    {
        WebContext db = new WebContext();

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        protected override void Initialize(RequestContext requestContext)
        {

            base.Initialize(requestContext);
        }

        [ChildActionOnly]
        public ActionResult _Index(int id)
        {
            ViewBag.ID = id;
            return PartialView();
        }

        public ActionResult WebContent_Read([DataSourceRequest] DataSourceRequest request, int id)
        {

            var contents = db.WebContents.Where(x => x.WebModuleID == id).ToList().Select(x => new { x.ID, x.StatusText, x.Title, x.Description, x.Image, x.ModifiedBy, x.ModifiedDate, x.PublishDate });
            if (request.Sorts.Count == 0)
            {
                request.Sorts.Add(new SortDescriptor("ID", System.ComponentModel.ListSortDirection.Descending));
            }
            return Json(contents.ToDataSourceResult(request));
        }

        public JsonResult GetWebContents(string text, int id)
        {

            {
                var contents = from x in db.WebContents where x.WebModuleID == id select x;
                if (!string.IsNullOrEmpty(text))
                {
                    contents = contents.Where(p => p.Title.Contains(text));
                }

                return Json(contents, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Add(int id)
        {
            ViewBag.UID = UniqueKeyGenerator.RNGTicks(10);
            ViewBag.ID = id;
            var model = new WebContent();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(WebContent model, HttpPostedFileBase image)
        {
            ViewBag.ID = model.WebModuleID;
            ViewBag.UID = model.UID;
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                model.CreatedBy = WebSecurity.CurrentUserName;
                model.CreatedDate = DateTime.Now;
                model.ModifiedBy = WebSecurity.CurrentUserName;
                model.ModifiedDate = DateTime.Now;
                model.CountView = 1;
                if (image != null)
                {
                    model.Image = image.ImageSave("/uploads/image/" + (now.Month.ToString("00") + now.Year), 700, 700);
                    db.WebContentUploads.Add(new WebContentUpload()
                    {
                        Title = image.FileName,
                        MetaTitle = image.FileName.UnsignNormalize(),
                        FullPath = model.Image,
                        UID = model.UID,
                        MimeType = ApplicationService.GetMimeType(Path.GetExtension(image.FileName)),
                        CreatedBy = WebSecurity.CurrentUserName,
                        CreatedDate = DateTime.Now
                    });
                    db.SaveChanges();
                }
                if (string.IsNullOrEmpty(model.MetaTitle))
                {
                    model.MetaTitle = model.Title.UnsignNormalize();

                    if (model.MetaTitle.Count() > 200)
                    {
                        model.MetaTitle = model.MetaTitle.Substring(0, 200);
                    }
                }

                db.Set<WebContent>().Add(model);
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


            var model = db.Set<WebContent>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            ViewBag.UID = model.UID;
            return View("Edit", model);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(WebContent model, HttpPostedFileBase image)
        {
            ViewBag.UID = model.UID;
            if (ModelState.IsValid)
            {
                {
                    try
                    {
                        var now = DateTime.Now;
                        model.ModifiedBy = WebSecurity.CurrentUserName;
                        model.ModifiedDate = DateTime.Now;
                        if (image != null)
                        {
                            model.Image = image.ImageSave("/uploads/image/" + (now.Month.ToString("00") + now.Year), 700, 700);
                            db.WebContentUploads.Add(new WebContentUpload()
                            {
                                Title = image.FileName,
                                MetaTitle = image.FileName.UnsignNormalize(),
                                FullPath = model.Image,
                                UID = model.UID,
                                MimeType = ApplicationService.GetMimeType(Path.GetExtension(image.FileName)),
                                CreatedBy = WebSecurity.CurrentUserName,
                                CreatedDate = DateTime.Now
                            });
                            db.SaveChanges();
                        }

                        if (model.MetaTitle.Count() > 200)
                        {
                            model.MetaTitle = model.MetaTitle.Substring(0, 200);
                        }

                        db.WebContents.Attach(model);
                        db.Entry(model).Property(a => a.Title).IsModified = true;
                        db.Entry(model).Property(a => a.Description).IsModified = true;
                        db.Entry(model).Property(a => a.MetaDescription).IsModified = true;
                        db.Entry(model).Property(a => a.MetaKeywords).IsModified = true;
                        db.Entry(model).Property(a => a.Image).IsModified = true;
                        db.Entry(model).Property(a => a.MetaTitle).IsModified = true;
                        db.Entry(model).Property(a => a.Status).IsModified = true;
                        db.Entry(model).Property(a => a.Body).IsModified = true;
                        db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                        db.Entry(model).Property(a => a.PublishDate).IsModified = true;
                        db.Entry(model).Property(a => a.ModifiedDate).IsModified = true;
                        db.Entry(model).Property(a => a.SeoTitle).IsModified = true;
                        db.Entry(model).Property(a => a.CountView).IsModified = true;
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
            }
            else
            {
                return View(model);
            }
        }
        public ActionResult Delete(int id)
        {
            {
                var model = db.Set<WebContent>().Find(id);
                if (model == null)
                {
                    return HttpNotFound();
                }
                return View("Delete", model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(WebContent model)
        {
            {

                try
                {
                    var deletes = db.ContentRelateds.Where(x => x.MainID == model.ID || x.RelatedID == model.ID).ToList();

                    for (int i = deletes.Count - 1; i >= 0; i--)
                    {
                        db.Entry(deletes[i]).State = EntityState.Deleted;
                        db.SaveChanges();
                    }

                    db.Entry(model).State = EntityState.Deleted;
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

            {
                var temp = from p in db.Set<WebContent>()
                           where lstObjId.Contains(p.ID)
                           select p;

                return View(temp.ToList());
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<WebContent> model)
        {

            {
                var temp = new List<WebContent>();
                foreach (var item in model)
                {
                    try
                    {
                        var deletes = db.ContentRelateds.Where(x => x.MainID == item.ID || x.RelatedID == item.ID).ToList();
                        for (int i = deletes.Count - 1; i >= 0; i--)
                        {
                            db.Entry(deletes[i]).State = EntityState.Deleted;
                            db.SaveChanges();
                        }
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

        public ActionResult Approve(int id)
        {
            var model = db.Set<WebContent>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            ViewBag.UID = model.UID;
            return View("Approve", model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Approve(WebContent model)
        {
            ViewBag.UID = model.UID;
            if (ModelState.IsValid)
            {
                try
                {
                    var content = db.WebContents.FirstOrDefault(x => x.ID == model.ID);
                    
                    var now = DateTime.Now;
                    content.ModifiedBy = WebSecurity.CurrentUserName;
                    content.ModifiedDate = now;
                    content.PublishDate = now;
                    content.Status = content.Status == (int)Status.Public ? (int)Status.Internal : (int)Status.Public;

                    //db.Entry(model).Property(a => a.CountView).IsModified = true;
                    //db.Entry(model).Property(a => a.ModifiedDate).IsModified = true;
                    //db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                    //db.Entry(model).Property(a => a.PublishDate).IsModified = true;

                    db.SaveChanges();
                    ViewBag.StartupScript = "approve_success();";
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
    }
}
