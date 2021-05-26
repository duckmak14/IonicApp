//using Kendo.Mvc.UI;
//using Kendo.Mvc.Extensions; 
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using WebModels;
//using WebMatrix.WebData;
//using Common;
//using System.Data;
//using WEB.Models;
//using System.Data.Entity;

using WEB.Controllers;
using WEB.Models;

namespace Web.Areas.Admin.Controllers
{
    [VanTaiAuthorize]

    public class Provincecontroller : BaseController
    {
        //        WebContext db = new WebContext();
        //        protected override void Dispose(bool disposing)
        //        {
        //            db.Dispose();
        //            base.Dispose(disposing);
        //        }

        //        public ActionResult Index()
        //        {
        //            return View();
        //        }


        //        public ActionResult Province_Read([DataSourceRequest] DataSourceRequest request)
        //        {
        //            var prov = db.Provinces.Select(x => new { x.ID, x.Title, x.Area }).ToList();
        //            return Json( prov.ToDataSourceResult(request));
        //        }
        //        public ActionResult Add()
        //        {
        //            var model = new Province();

        //            return View(model);
        //        }
        //        [HttpPost]
        //        [ValidateAntiForgeryToken]
        //        [ValidateInput(false)]
        //        public ActionResult Add(Province model)
        //        {
        //            if (ModelState.IsValid)
        //            {

        //                db.Provinces.Add(model);
        //                db.SaveChanges(); 
        //                ViewBag.StartupScript = "create_success();";
        //                return View(model);
        //            }

        //            return View(model);
        //        }


        //        public ActionResult Edit(int id)
        //        {

        //                var model = db.Set<Province>().Find(id);
        //                if (model == null)
        //                {
        //                    return HttpNotFound();
        //                }

        //                return View("Edit", model);

        //        }
        //        [HttpPost]
        //        [ValidateAntiForgeryToken]
        //        [ValidateInput(false)]
        //        public ActionResult Edit(Province model )
        //        {
        //            if (ModelState.IsValid)
        //            {

        //                {
        //                    try
        //                    {

        //                        db.Provinces.Attach(model);
        //                        db.Entry(model).Property(a => a.Title).IsModified = true; db.Entry(model).Property(a => a.CountryID).IsModified = true;
        //                        db.Entry(model).Property(a => a.Area).IsModified = true;
        //                        db.SaveChanges();

        //                        ViewBag.StartupScript = "edit_success();";
        //                        return View(model);

        //                    }
        //                    catch (Exception ex)
        //                    {

        //                        ModelState.AddModelError("", ex.Message);
        //                        return View(model);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                return View(model);
        //            }
        //        }
        //        public ActionResult Delete(int id)
        //        {

        //            {
        //                var model = db.Set<Province>().Find(id);
        //                if (model == null)
        //                {
        //                    return HttpNotFound();
        //                }
        //                return View("Delete", model);
        //            }
        //        }
        //        [HttpPost]
        //        [ValidateAntiForgeryToken]
        //        public ActionResult Delete(Province model)
        //        {


        //            {

        //                try
        //                { 
        //                    db.Entry(model).State = EntityState.Deleted;
        //                    db.SaveChanges();

        //                    ViewBag.StartupScript = "delete_success();";
        //                    return View(model);
        //                }
        //                catch (Exception ex)
        //                {

        //                    ModelState.AddModelError("", ex.Message);
        //                    return View(model);
        //                }
        //            }
        //        }


        //        public ActionResult Deletes(string id)
        //        {

        //            var objects = id.Split(',');
        //            var lstSiteID = new List<int>();
        //            foreach (var obj in objects)
        //            {
        //                try
        //                {
        //                    lstSiteID.Add(int.Parse(obj.ToString()));
        //                }
        //                catch (Exception)
        //                { }
        //            }

        //            var temp = from p in db.Set<Province>()
        //                       where lstSiteID.Contains(p.ID)
        //                       select p;

        //            return View(temp.ToList());

        //        }
        //        [HttpPost]
        //        [ValidateAntiForgeryToken]
        //        public ActionResult Deletes(List<Province> model)
        //        {

        //            var temp = new List<Province>();
        //            foreach (var item in model)
        //            {
        //                try
        //                {

        //                    db.Entry(item).State = EntityState.Deleted;
        //                    db.SaveChanges();


        //                }
        //                catch (Exception)
        //                {
        //                    db.Entry(item).State = EntityState.Unchanged;
        //                    temp.Add(item);
        //                }
        //            }

        //            if (temp.Count == 0)
        //            {
        //                ViewBag.StartupScript = "deletes_success();";
        //                return View(temp);
        //            }
        //            else if (temp.Count > 0)
        //            {
        //                ViewBag.StartupScript = "top.$('#grid').data('kendoGrid').dataSource.read(); top.treeview.dataSource.read();";
        //                ModelState.AddModelError("", Resources.Common.ErrorDeleteItems);
        //                return View(temp);
        //            }
        //            else
        //            {
        //                ViewBag.StartupScript = "deletes_success();";
        //                return View();
        //            }

        //        }


    }
}
