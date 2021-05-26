using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebModels;
using Common;
using WEB.Models;

namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public class WebConfigController : BaseController
    {
        private WebContext db = new WebContext();

        //
        // GET: /Admin/WebConfig/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult WebConfig_Read([DataSourceRequest] DataSourceRequest request)
        {
            var contents = db.WebConfigs.AsNoTracking();
            return Json(contents.ToDataSourceResult(request));
        }
       

        //
        // GET: /Admin/WebConfig/Create

        public ActionResult Add()
        {
            return View();
        }

        //
        // POST: /Admin/WebConfig/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(WebConfig model)
        {
            if (ModelState.IsValid)
            {
                db.WebConfigs.Add(model);
                db.SaveChanges();
                ViewBag.StartupScript = "create_success();";
                return View(model);
            }

            return View(model);
        }

        //
        // GET: /Admin/WebConfig/Edit/5

        public ActionResult Edit(int id = 0)
        {
            WebConfig webconfig = db.WebConfigs.Find(id);
            if (webconfig == null)
            {
                return HttpNotFound();
            }
            return View(webconfig);
        }

        //
        // POST: /Admin/WebConfig/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(WebConfig webconfig)
        {
            if (ModelState.IsValid)
            {
                db.Entry(webconfig).State = EntityState.Modified;
                db.SaveChanges();
                ViewBag.StartupScript = "edit_success();";
                return View(webconfig);
            }
            return View(webconfig);
        }



        public ActionResult Delete(int id)
        {

            var role = db.Set<WebConfig>().Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View("Delete", role);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(WebConfig model)
        {



            try
            {
                var role = db.WebConfigs.Attach(model);
                db.Set<WebConfig>().Remove(role);
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

            var temp = from p in db.Set<WebConfig>().AsNoTracking()
                       where lstSiteID.Contains(p.ID)
                       select p;

            return View(temp.ToList());

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<WebConfig> model)
        {

            var temp = new List<WebConfig>();
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