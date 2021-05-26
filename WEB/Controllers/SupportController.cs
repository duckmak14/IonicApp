using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebModels;
using Kendo.Mvc.Extensions;
using WEB.Models;

namespace WEB.Controllers
{
    public class SupportController : BaseController
    {
        WebContext db = new WebContext();   
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [ChildActionOnly]
        [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _Index()
        {
            var support = db.SupportOnlines.Where(x =>
                            ((x.Culture == null ||
                              (!string.IsNullOrEmpty(x.Culture) && x.Culture.Equals(ApplicationService.Culture)))
                              || (ApplicationService.Culture == null))

                ).OrderBy(x => x.Order);
            return PartialView(support.ToList());
        }
    }
}
