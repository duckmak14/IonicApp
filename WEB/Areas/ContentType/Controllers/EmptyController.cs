using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebModels;

namespace WEB.Areas.ContentType.Controllers
{
    public class EmptyController : BaseController
    {
        private WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
         

        [ChildActionOnly]
        public ActionResult _Index(int id)
        {
            ViewBag.ID = id;
            return PartialView();
        }
        [ChildActionOnly][AllowAnonymous]
        public ActionResult _PubIndex(int id)
        {
            WebModule webmodule = null;
            if (TempData["WebModule"] != null)
            {
                webmodule = TempData["WebModule"] as WebModule;
            }
            else webmodule = db.Set<WebModule>().Find(id);
            if (webmodule == null)
            {
                return HttpNotFound();
            }

            return PartialView(webmodule);
        }

        [OutputCache(Duration = 120, VaryByCustom = "culture")]
        public ActionResult _PubIndexSiteMap()
        {
            ViewBag.Language = Language;
            var webmodules = from e in db.WebModules.AsNoTracking()
                             where (e.ParentID == null)
                             orderby e.Order
                             select e;
            return View(webmodules);
        } 

         
    }
}
