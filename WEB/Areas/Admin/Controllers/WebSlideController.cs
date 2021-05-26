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

    public class WebSlideController : BaseController
    {
        private WebContext db = new WebContext();

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            return PartialView();
        }

        //[ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _IndexPublish()
        {
            IList<WebSlide> slides = this.db.WebSlides.Where(m => m.Status == (int)Status.Public).OrderBy(m => m.Order).ToList();
            return PartialView(slides);
        }

        public ActionResult WebSlide_Read([DataSourceRequest] DataSourceRequest request)
        {
            var slides = db.WebSlides.Select(x => new { x.ID,x.Order, x.Title,x.Culture,x.Image, x.ModifiedBy, x.ModifiedDate });
         
            if (request.Sorts.Count == 0)
            {
                request.Sorts.Add(new SortDescriptor("Order", System.ComponentModel.ListSortDirection.Ascending));
            }

            return Json(slides.ToDataSourceResult(request));
        }

      
        public ActionResult Add()
        {
            var webslide = new WebSlide();
            webslide.Culture = ApplicationService.Culture;   
            return View(webslide);
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add( WebSlide model, HttpPostedFileBase image)
        {
            
            if (ModelState.IsValid)
            {
                model.CreatedBy = WebSecurity.CurrentUserName;
                model.CreatedDate = DateTime.Now;
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

                db.Set<WebSlide>().Add(model);
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
           
            var model = db.Set<WebSlide>().Find(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Exclude = "Object")]WebSlide model, HttpPostedFileBase image)
        {
            ViewBag.Language = Language;
            if (ModelState.IsValid)
            {
                try
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

                    db.WebSlides.Attach(model);
                    db.Entry(model).Property(a => a.Title).IsModified = true;
                    db.Entry(model).Property(a => a.Link).IsModified = true;
                    db.Entry(model).Property(a => a.Description).IsModified = true;
                    db.Entry(model).Property(a => a.Target).IsModified = true;
                    db.Entry(model).Property(a => a.Order).IsModified = true; db.Entry(model).Property(a => a.Culture).IsModified = true;
                    db.Entry(model).Property(a => a.Status).IsModified = true;
                    db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                    db.Entry(model).Property(a => a.ModifiedDate).IsModified = true;
                    db.Entry(model).Property(a => a.Image).IsModified = true;
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
                var model = db.Set<WebSlide>().Find(id);

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
        public ActionResult Delete(WebSlide model)
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

            var temp = from p in db.Set<WebSlide>()
                       where lstObjId.Contains(p.ID)
                       select p;

            return View(temp.ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<WebSlide> model)
        {
            var temp = new List<WebSlide>();
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
