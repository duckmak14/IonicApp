using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebModels;
using Kendo.Mvc.Extensions;
using System.Data.Entity;
using Kendo.Mvc;
using WEB.Models;

namespace WEB.Areas.Admin.Controllers
{
    [VanTaiAuthorize]

    public class HelpController : Controller
    {
        private WebContext db = new WebContext();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Help_Read([DataSourceRequest] DataSourceRequest request)
        {
            var helps = from h in this.db.Helps select new { h.ID, h.Title };

            if (request.Sorts.Count == 0)
            {
                request.Sorts.Add(new SortDescriptor("ID", System.ComponentModel.ListSortDirection.Ascending));
            }

            return Json(helps.ToDataSourceResult(request));
        }

        public ContentResult GetContent(int id)
        {
            string body = (from h in this.db.Helps where h.ID == id select h.Body).FirstOrDefault();

            return Content(body);
        }

        public ActionResult Add()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Add(Help model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            this.db.Helps.Add(model);
            this.db.SaveChanges();

            ViewBag.StartupScript = "create_success();";

            return View();
        }

        public ActionResult Edit(int id)
        {
            Help help = this.db.Helps.Where(h => h.ID == id).FirstOrDefault();

            if (help == null)
            {
                return HttpNotFound();
            }

            return View(help);
        }

        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Edit(Help model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            this.db.Helps.Attach(model);
            this.db.Entry(model).Property(m => m.Title).IsModified = true;
            this.db.Entry(model).Property(m => m.Body).IsModified = true;
            this.db.SaveChanges();

            ViewBag.StartupScript = "edit_success();";

            return View();
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

            var temp = from p in db.Set<Help>()
                       where lstObjId.Contains(p.ID)
                       select p;

            return View(temp.ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<Help> model)
        {
            var temp = new List<Help>();
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
