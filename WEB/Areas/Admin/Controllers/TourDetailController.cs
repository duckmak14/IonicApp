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
using System.Configuration;

namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public partial class TourDetailController : BaseController
    {
        WebContext db = new WebContext();

        public ActionResult Index(int id)
        {
            ViewBag.ID = id;
            return View();
        }

        public ActionResult WebContent_Read([DataSourceRequest] DataSourceRequest request, int id)
        {
            var contents = db.DayOfTours.Where(x => x.WebContentID == id)
                .Select(x => new { x.ID, x.Title, x.Order });
            if (request.Sorts.Count == 0)
            {
                request.Sorts.Add(new SortDescriptor("ID", System.ComponentModel.ListSortDirection.Descending));
            }
            return Json(contents.ToDataSourceResult(request));
        }

        public ActionResult Add(int id)
        {
            ViewBag.ID = id;
            var model = new DayOfTour();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(DayOfTour model)
        {
            if (ModelState.IsValid)
            {
                db.Set<DayOfTour>().Add(model);

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
            var model = db.Set<DayOfTour>().Find(id);


            if (model == null)
            {
                return HttpNotFound();
            }
            return View("Edit", model);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(DayOfTour model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.DayOfTours.Attach(model);

                    db.Entry(model).Property(a => a.Title).IsModified = true;
                    db.Entry(model).Property(a => a.Body).IsModified = true;
                    db.Entry(model).Property(a => a.Order).IsModified = true;

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
                var model = db.Set<DayOfTour>().Find(id);
                if (model == null)
                {
                    return HttpNotFound();
                }
                return View("Delete", model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(DayOfTour model)
        {
            try
            {
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

        //[ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubIndex(int id)
        {
            IList<DayOfTour> slides = this.db.DayOfTours.Where(m => m.WebContentID == id).OrderBy(m => m.Order).ToList();

            return PartialView(slides);
        }
    }
}
