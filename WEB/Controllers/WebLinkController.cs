using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WEB.Models;
using WebModels;

namespace WEB.Controllers
{
    public class WebLinkController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        [ChildActionOnly]
        [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult Index()
        {
            var weblinks = db.WebLinks.Where(x =>
                            ((x.Culture == null ||
                              (!string.IsNullOrEmpty(x.Culture) && x.Culture.Equals(ApplicationService.Culture)))
                              || (ApplicationService.Culture == null))

                ).OrderBy(x => x.Order);
            return PartialView(weblinks.ToList());
        }
        [ChildActionOnly]
        [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _Index()
        {

            var weblinks = db.WebLinks.Where(x =>
                            ((x.Culture == null ||
                              (!string.IsNullOrEmpty(x.Culture) && x.Culture.Equals(ApplicationService.Culture)))
                              || (ApplicationService.Culture == null))

                ).OrderBy(x => x.Order);
            return PartialView(weblinks.ToList());
        }
        [HttpGet]
        public JsonResult JIndex(string position)
        {
            var adv = db.WebLinks.Where(x =>
                            ((x.Culture == null ||
                              (!string.IsNullOrEmpty(x.Culture) && x.Culture.Equals(ApplicationService.Culture)))
                              || (ApplicationService.Culture == null))

                ).OrderBy(x => x.Order).Select(x => new { x.ID, x.Title, x.Description, x.Link, x.Target });
            return Json(adv, JsonRequestBehavior.AllowGet);
        }
    }
}