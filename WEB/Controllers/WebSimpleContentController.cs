using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebModels;
using WEB.Models;
namespace WEB.Controllers
{
    public class WebSimpleContentController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [ChildActionOnly]
        public ActionResult _Body(string key)
        {
            var content = from x in db.WebSimpleContents where x.Key.ToLower().Equals(key.ToLower()) select x;
            if (content.Count() > 0)
            {
                return PartialView(content.First());
            }
            else { return PartialView(); }
        }
         [ChildActionOnly]
        public ActionResult _Index(string key)
        {
            var content = from x in db.WebSimpleContents where x.Key.ToLower().Equals(key.ToLower()) select x;
            if (content.Count() > 0)
            {
                return PartialView(content.First());
            }
            else { return PartialView(); }
        }
    }
}
