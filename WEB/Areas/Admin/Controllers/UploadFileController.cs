using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebModels;
using WEB.Models;
using Kendo.Mvc;
using System.IO;

namespace WEB.Areas.Admin.Controllers
{
    [VanTaiAuthorize]

    public class UploadFileController : BaseController
    {
        WebContext db = new WebContext();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UploadFile_Read([DataSourceRequest] DataSourceRequest request)
        {
            var c = (from x in db.UploadFiles select new { x.ID, x.Title, x.Link });
            if (request.Sorts.Count == 0)
            {
                request.Sorts.Add(new SortDescriptor("ID", System.ComponentModel.ListSortDirection.Descending));
            }

            return Json(c.ToDataSourceResult(request));
        }



        public ActionResult Add()
        {
            var support = new UploadFile();

            return View(support);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(UploadFile model, HttpPostedFileBase file)
        {
            string path ="";
            if (ModelState.IsValid)
            {
                if (file != null && file.ContentLength > 0)
                {
                    path = "/uploads/file/" + file.FileName;
                    file.SaveAs(System.Web.HttpContext.Current.Server.MapPath(path));
                }
            }

            model.Link = path;

            db.UploadFiles.Add(model);
            db.SaveChanges();
            ViewBag.StartupScript = "create_success();";
            return View(model);
        }

        public ActionResult Edit(int id = 0)
        {
            UploadFile model = db.UploadFiles.Find(id);

            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(UploadFile model, HttpPostedFileBase file)
        {
            string path = "";
            if (ModelState.IsValid)
            {
                if (ModelState.IsValid)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        path = "/uploads/file/" + file.FileName;
                        file.SaveAs(System.Web.HttpContext.Current.Server.MapPath(path));
                    }
                }

                model.Link = path;

                db.UploadFiles.Attach(model);

                db.Entry(model).Property(a => a.Title).IsModified = true;
                db.Entry(model).Property(a => a.Link).IsModified = true;
                db.SaveChanges();
                ViewBag.StartupScript = "edit_success();";
                return View(model);
            }
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var s = db.Set<UploadFile>().Find(id);
            if (s == null)
            {
                return HttpNotFound();
            }
            return View("Delete", s);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(UploadFile model)
        {
            try
            {
                var s = db.UploadFiles.Attach(model);
                db.Set<UploadFile>().Remove(s);
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

            var temp = from p in db.Set<UploadFile>()
                       where lstSiteID.Contains(p.ID)
                       select p;

            return View(temp.ToList());

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<UploadFile> model)
        {

            var temp = new List<UploadFile>();
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
                ViewBag.StartupScript = "top.$('#grid').data('kendoGrid').dataSource.read();  ";
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
