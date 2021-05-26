using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc; 
using System.Data;
using Common;
using WebModels;
using System.Data.Entity;
using WEB.Models;

namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public class AccessLogController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        //
        // GET: /Admin/AccessLog/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AccessLog_Read([DataSourceRequest] DataSourceRequest request)
        {

            var logs = from x in db.AccessLogs.AsNoTracking() orderby x.ID descending select new { x.ID, x.Title, x.Action, x.CreatedBy, x.CreatedDate };
            return Json(logs.ToDataSourceResult(request));
        }
        public ActionResult Delete(long id)
        {
             
                var log =   db.Set<AccessLog>().Find(id);
                if (log == null)
                {
                    return HttpNotFound();
                }
                return View("Delete", log);
          
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(AccessLog model)
        {

           
                try
                {
                    var log = db.AccessLogs.Attach(model);
                    db.Set<AccessLog>().Remove(log);
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
            var lstSiteID = new List<long>();
            foreach (var obj in objects)
            {
                try
                {
                    lstSiteID.Add(long.Parse(obj.ToString()));
                }
                catch (Exception)
                { }
            }
            
                var temp = from p in db.Set<AccessLog>()
                           where lstSiteID.Contains(p.ID)
                           select p;

                return View(temp.ToList());
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<AccessLog> model)
        {
            
                var temp = new List<AccessLog>();
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
    }
}
