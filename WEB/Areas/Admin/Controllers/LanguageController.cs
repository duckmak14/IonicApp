using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WEB.Models;
using WebModels;

namespace WEB.Areas.Admin.Controllers
{
    [VanTaiAuthorize]

    public class LanguageController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        //
        // GET: /Admin/Language/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult _Info()
        {
             
            return PartialView(this.Language);
        }
    }
}
