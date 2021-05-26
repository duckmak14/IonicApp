using Common;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebModels;
using System.Data.Entity;
using WebMatrix.WebData;
using WEB.Models;
using System.IO;

namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public class AdvPositionController : BaseController
    {
        
     
        private WebContext db = new WebContext();
         
        //
        // GET: /Admin/AdvertisementPosition/
        
        public ActionResult Index()
        {
           
            return View();
        }
     

        public ActionResult AdvertisementPosition_Read([DataSourceRequest] DataSourceRequest request)
        {
            var contents = db.AdvertisementPositions.Select(x=> new {x.ID,x.UID, x.Title,x.ModifiedBy,x.ModifiedDate}).ToList(); 

            return Json(contents.ToDataSourceResult(request));
        }

        public JsonResult GetAdvertisementPositions()
        {
            var countries = db.AdvertisementPositions.Select(x => new { x.ID, x.UID, x.Title, x.ModifiedBy, x.ModifiedDate }).ToList();
            return Json(countries, JsonRequestBehavior.AllowGet);

        }

        //
        // GET: /Admin/AdvertisementPosition/Create

        public ActionResult Add()
        { 
            return View();
        }


        
        //
        // POST: /Admin/AdvertisementPosition/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(AdvertisementPosition model, HttpPostedFileBase image)
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

                db.AdvertisementPositions.Add(model);
                db.SaveChanges(); 
                ViewBag.StartupScript = "create_success();";
                return View(model);
            } 
            return View(model);
        }

        //
        // GET: /Admin/AdvertisementPosition/Edit/5

        public ActionResult Edit(int id = 0)
        {
            AdvertisementPosition model = db.AdvertisementPositions.Find(id); 
            if (model == null)
            {
                return HttpNotFound();
            }  
            return View(model);
        }

        //
        // POST: /Admin/AdvertisementPosition/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(AdvertisementPosition model, HttpPostedFileBase image)
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
                db.AdvertisementPositions.Attach(model);
                db.Entry(model).Property(a => a.Title).IsModified = true;
                db.Entry(model).Property(a => a.Description).IsModified = true;
                db.Entry(model).Property(a => a.Image).IsModified = true;
                db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                db.Entry(model).Property(a => a.ModifiedDate).IsModified = true;
                db.SaveChanges(); 
                ViewBag.StartupScript = "edit_success();";
                return View(model);
            }
            return View(model);
        }



        public ActionResult Delete(int id)
        { 
            var role = db.Set<AdvertisementPosition>().Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View("Delete", role);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(AdvertisementPosition model)
        {
             
            try
            {
               
                var role = db.AdvertisementPositions.Attach(model);
                db.Set<AdvertisementPosition>().Remove(role);
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

            var temp = from p in db.Set<AdvertisementPosition>()
                       where lstSiteID.Contains(p.ID)
                       select p;

            return View(temp.ToList());

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<AdvertisementPosition> model)
        {

            var temp = new List<AdvertisementPosition>();
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



        
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
	}
}