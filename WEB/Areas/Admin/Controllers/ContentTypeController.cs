using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebModels;
using Common;
using Kendo.Mvc;
using WEB.Models;

namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public class ContentTypeController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        //
        // GET: /Admin/ContentType/

        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetContentTypes()
        {

            var ctypes = from x in db.ContentTypes.AsNoTracking() orderby x.Order select new { x.ID, x.Title, x.Description, x.Order };

                return Json(ctypes, JsonRequestBehavior.AllowGet);
            
        }
        public ActionResult ContentType_Read([DataSourceRequest] DataSourceRequest request)
        {

            var ctypes = from x in db.ContentTypes.AsNoTracking() select new { x.ID, x.Title, x.Description, x.Order };
            if (request.Sorts.Count == 0)
            {
                request.Sorts.Add(new SortDescriptor("Order", System.ComponentModel.ListSortDirection.Ascending));
            }
            return Json(ctypes.ToDataSourceResult(request));
        }
    }
}
