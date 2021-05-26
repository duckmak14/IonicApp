using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WEB.Models;
using WebModels;

namespace WEB.Areas.ContentType.Controllers
{
    public class OnePageController : BaseController
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
            var model = db.Set<WebModule>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            
            return PartialView(model);
        }

        public ActionResult EditModule(int id)
        {

               var model = db.Set<WebModule>().Find(id);
                if (model == null)
                {
                    return HttpNotFound();
                }
                
                return View("EditModule", model);
             
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult EditModule(WebModule model)
        {

            if (ModelState.IsValid)
            { 
                try
                { 
                    db.WebModules.Attach(model);
                    db.Entry(model).Property(a => a.Body).IsModified = true; 
                    db.SaveChanges(); 
                    
                    ViewBag.StartupScript = "edit_success();";
                    return View(model);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(model);
                }

            }
            else
            {
                return View(model);
            }
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
        
    }
}
