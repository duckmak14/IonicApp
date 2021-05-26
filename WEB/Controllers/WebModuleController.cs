using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WEB.Models;
using WebModels;

namespace WEB.Controllers
{
    public class WebModuleController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            return View();
        }
        [ChildActionOnly]
        [AllowAnonymous]
        [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _Navigation(string uId)
        {
            var webmodules = from e in db.WebModules
                             where (e.ParentID == null)
                             orderby e.Order
                             select e;
            //WebModule webmodules = this.db.WebModules.Where(m => m.ParentID == id && m.Culture == ApplicationService.Culture).FirstOrDefault();
            return PartialView(webmodules);
        }
        [AllowAnonymous]
        [ChildActionOnly]
        public PartialViewResult _ColLeft()
        {
            var module = this.db.WebModules.Where(x => x.Parent.UID.Equals("other-service")).ToList();

            return PartialView(module);
        }
        //public PartialViewResult _ColLeft(string uId)
        //{
        //    WebModule module = this.db.WebModules.Where(m => m.UID == uId && m.Culture == ApplicationService.Culture).FirstOrDefault();
        //    return PartialView(module);
        //}
    }
}
