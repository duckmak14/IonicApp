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
using WebMatrix.WebData;
using Common;
using WEB.Models;

namespace WEB.Areas.Admin.Controllers
{
    [VanTaiAuthorize]

    public class SubscribeNoticeController : BaseController
    {
        private WebContext db = new WebContext();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SubscribeNotice_Read([DataSourceRequest] DataSourceRequest request)
        {
            var c = (from x in db.SubscribeNotices select new { x.ID, x.Email, x.ModifiedBy, x.ModifiedDate });
            return Json(c.ToDataSourceResult(request));
        }
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(SubscribeNotice model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                model.CreatedBy = WebSecurity.CurrentUserName;
                model.ModifiedBy = WebSecurity.CurrentUserName;

                db.SubscribeNotices.Add(model);
                db.SaveChanges();
                ViewBag.StartupScript = "create_success();";
                return View(model);
            }

            return View(model);
        }
        public ActionResult Edit(int id = 0)
        {
            SubscribeNotice model = db.SubscribeNotices.Find(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        //
        // POST: /Admin/Country/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(SubscribeNotice model)
        {
            if (ModelState.IsValid)
            {
                model.ModifiedDate = DateTime.Now;
                model.ModifiedBy = WebSecurity.CurrentUserName;

                db.SubscribeNotices.Attach(model);
                db.Entry(model).Property(a => a.Email).IsModified = true;
                db.Entry(model).Property(a => a.ModifiedDate).IsModified = true;
                db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                db.Entry(model).Property(a => a.Status).IsModified = true;
                // db.Entry(model).Property(a => a.IsoCode).IsModified = true; 
                db.SaveChanges();
                ViewBag.StartupScript = "edit_success();";
                return View(model);
            }
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var role = db.Set<SubscribeNotice>().Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View("Delete", role);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(SubscribeNotice model)
        {
            try
            {
                var subscribenotice = db.SubscribeNotices.Attach(model);
                db.Set<SubscribeNotice>().Remove(subscribenotice);
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

            var temp = from p in db.Set<SubscribeNotice>()
                       where lstSiteID.Contains(p.ID)
                       select p;

            return View(temp.ToList());

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<SubscribeNotice> model)
        {

            var temp = new List<SubscribeNotice>();
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
