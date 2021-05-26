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
using WEB.Models;

namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public class WebContentController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        //
        // GET: /Admin/WebContent/

        public ActionResult Index(int? id)
        {
            ViewBag.TreeLeft = WebModuleStore.WebModuleTreeLeftOnRole();
            ViewBag.ID = id;
            if (id != null)
            {
                var module = db.Set<WebModule>().Find(id);
                return View(module);
            }
            else
                return View();
        }

        [ChildActionOnly]
        public ActionResult _Index()
        {

            return PartialView();
        }

        public ActionResult WebContent_Read([DataSourceRequest] DataSourceRequest request)
        {

            var contents = db.WebContents.OrderByDescending(x => x.ID).Select(x => new { x.ID, x.Title, x.ModifiedBy, x.ModifiedDate });
            return Json(contents.ToDataSourceResult(request));

        }

    }
}
