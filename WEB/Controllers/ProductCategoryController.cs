using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WEB.Models;
using WebModels;

namespace WEB.Controllers
{
    public class ProductCategoryController : Controller
    {
        WebContext db = new WebContext();
        int ctype = (int)CTypeCategories.Product;
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        //
        // GET: /WebCategories/
        public ActionResult Index(int? id, int? page)
        {

            var cat = db.WebCategories.Find(id);
            ViewBag.WebCategory = cat;
            var contents = db.WebContentCategories.Where(x => x.WebCategoryID == cat.ID).Select(x => x.WebContent).ToList(); 
            var ipage = 1; if (page != null) ipage = page.Value;
            ViewBag.TotalItemCount = contents.Count();
            ViewBag.CurrentPage = ipage;   
            return  View(contents.Skip((ipage - 1) * ApplicationService.PageSize).Take(ApplicationService.PageSize).ToList());
        }

        [ChildActionOnly]
        [OutputCache(Duration = 300)]
        public ActionResult _WebCategoryMenu()
        {
            var model = db.WebCategories.AsNoTracking().Where(x => x.ParentID == null && x.CType==ctype).AsEnumerable();
            return PartialView(model);
        }
    }
}