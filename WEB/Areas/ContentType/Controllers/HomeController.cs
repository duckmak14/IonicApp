using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using WebModels;
using WEB.Models;
using System.Web.Caching;
using Newtonsoft.Json.Linq;
using Kendo.Mvc;
using WebMatrix.WebData;
using Common;
using System.Text.RegularExpressions;

namespace WEB.Areas.ContentType.Controllers
{
    [AdminAuthorize]
    public class HomeController : BaseController
    {
        WebModels.WebContext db = new WebModels.WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        public ActionResult Index()
        {
            return Redirect("/admin");
        }
        
    }
}
