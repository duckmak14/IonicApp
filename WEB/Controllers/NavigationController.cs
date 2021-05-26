using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WEB.Models;
using WebModels;

namespace WEB.Controllers
{
    public class NavigationController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        //
        // GET: /Navigation/
        public ActionResult Index(string key)
        {

            var temp = db.ModuleNavigations.AsNoTracking().Where(
               x => x.Navigation.Key.ToLower().Equals(key.ToLower()) &&
                     (x.WebModule.Culture == null ||
                            (!string.IsNullOrEmpty(x.WebModule.Culture) && x.WebModule.Culture.Equals(ApplicationService.Culture)))
                            || (ApplicationService.Culture == null)

               ).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return View(webmodules);
        }
        [ChildActionOnly]
        //  [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _ColLeft(string key)
        {
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower())).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return PartialView(webmodules);
        }
        [ChildActionOnly]
        //  [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _Main(string key)
        {
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower())).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return PartialView(webmodules);

        }
        [ChildActionOnly]
        public ActionResult _Partner(string key)
        {
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower())).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return PartialView(webmodules);
        }
        [ChildActionOnly]
        public ActionResult _Guides(string key)
        {
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower())).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return PartialView(webmodules);
        }
        [ChildActionOnly]
        // [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _TopDestination(string key)
        {
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower())).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return PartialView(webmodules);
        }


        [ChildActionOnly]
        // [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _Facebox(string key)
        {
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower()) &&
                      (x.WebModule.Culture == null ||
                             (!string.IsNullOrEmpty(x.WebModule.Culture) && x.WebModule.Culture.Equals(ApplicationService.Culture)))
                             || (ApplicationService.Culture == null)

                ).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return PartialView(webmodules);
        }
        [ChildActionOnly]
        // [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _Left(string key)
        {
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower()) &&
                      (x.WebModule.Culture == null ||
                             (!string.IsNullOrEmpty(x.WebModule.Culture) && x.WebModule.Culture.Equals(ApplicationService.Culture)))
                             || (ApplicationService.Culture == null)

                ).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return PartialView(webmodules);
        }

        [ChildActionOnly]
        // [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _BoxHome(string key)
        {
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower())).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return PartialView(webmodules);
        }
        [ChildActionOnly]
        // [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _BoxTop(string key)
        {
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower()) &&
                      (x.WebModule.Culture == null ||
                             (!string.IsNullOrEmpty(x.WebModule.Culture) && x.WebModule.Culture.Equals(ApplicationService.Culture)))
                             || (ApplicationService.Culture == null)

                ).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return PartialView(webmodules);
        }
        [ChildActionOnly]
        // [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _SlideCat(string key)
        {
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower()) &&
                      (x.WebModule.Culture == null ||
                             (!string.IsNullOrEmpty(x.WebModule.Culture) && x.WebModule.Culture.Equals(ApplicationService.Culture)))
                             || (ApplicationService.Culture == null)

                ).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return PartialView(webmodules);
        }
        [ChildActionOnly]
        // [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _SlideContent(string key)
        {
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower()) &&
                      (x.WebModule.Culture == null ||
                             (!string.IsNullOrEmpty(x.WebModule.Culture) && x.WebModule.Culture.Equals(ApplicationService.Culture)))
                             || (ApplicationService.Culture == null)

                ).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return PartialView(webmodules);
        }
        [ChildActionOnly]
        // [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _BoxRight(string key)
        {
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower()) &&
                      (x.WebModule.Culture == null ||
                             (!string.IsNullOrEmpty(x.WebModule.Culture) && x.WebModule.Culture.Equals(ApplicationService.Culture)))
                             || (ApplicationService.Culture == null)

                ).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return PartialView(webmodules);
        }
        [ChildActionOnly]
        // [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _BoxFooter(string key)
        {
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower()) &&
                      (x.WebModule.Culture == null ||
                             (!string.IsNullOrEmpty(x.WebModule.Culture) && x.WebModule.Culture.Equals(ApplicationService.Culture)))
                             || (ApplicationService.Culture == null)

                ).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return PartialView(webmodules);
        }
        [ChildActionOnly]
        // [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _BoxClip(string key)
        {
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower()) &&
                      (x.WebModule.Culture == null ||
                             (!string.IsNullOrEmpty(x.WebModule.Culture) && x.WebModule.Culture.Equals(ApplicationService.Culture)))
                             || (ApplicationService.Culture == null)

                ).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return PartialView(webmodules);
        }
        [ChildActionOnly]
        // [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _LinkFooter(string key)
        {
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower()) &&
                      (x.WebModule.Culture == null ||
                             (!string.IsNullOrEmpty(x.WebModule.Culture) && x.WebModule.Culture.Equals(ApplicationService.Culture)))
                             || (ApplicationService.Culture == null)

                ).Select(x => x.WebModuleID);
            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return PartialView(webmodules);
        }

        [ChildActionOnly]
        // [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _Footer(string key)
        {
            var temp = db.ModuleNavigations.AsNoTracking().Where(
                x => x.Navigation.Key.ToLower().Equals(key.ToLower())).Select(x => x.WebModuleID);

            var webmodules = from x in db.WebModules
                             where temp.Contains(x.ID)
                             orderby x.Order
                             select x;

            return PartialView(webmodules);
        }

        [ChildActionOnly]
        // [OutputCache(Duration = 60, VaryByCustom = "culture")]
        public ActionResult _MenuCategory()
        {
            var webmodules = db.WebModules.Where(x => x.Parent.UID.Equals("sanpham"));
            return PartialView(webmodules);
        }

        
        public static WebContent GetTopNews(int webmoduleid)
        {
            WebContext db = new WebContext();
            var model = db.WebContents.Where(x => x.WebModuleID == webmoduleid && x.Status == (int)Status.Public).OrderByDescending(x => x.CreatedDate);
            if (model.Count() > 0)
            {
                return model.FirstOrDefault();
            }
            else
            {
                return null;
            }

        }

        public static List<WebModule> GetAllModule(int webmoduleid)
        {
            WebContext db = new WebContext();
            var model = new List<WebModule>();
            model = db.WebModules.Where(x => x.ParentID == webmoduleid && x.Status == (int)Status.Public).ToList();

            return model;
        }

        public static List<WebContent> GetListTopNews(int webmoduleid, int take)
        {
            WebContext db = new WebContext();
            var model = db.WebContents.Where(x => x.WebModuleID == webmoduleid && x.Status == (int)Status.Public).OrderByDescending(x => x.CreatedDate);
            return model.Take(take).ToList();
        }
        public static List<WebContent> GetNewsByWebmoduleID(int webmoduleid, int take)
        {
            WebContext db = new WebContext();
            var contents = db.WebContents.Where(x => x.WebModuleID == webmoduleid && x.Status == (int)Status.Public).OrderByDescending(x => x.CreatedDate).Take(take);
            return contents.Take(take).ToList();

        }
        public static List<WebContent> GetAllContent(int webmoduleid, int take)
        {
            WebContext db = new WebContext();
            var contents = new List<WebContent>();
            contents = db.WebContents.Where(x => x.WebModuleID == webmoduleid && x.Status == (int)Status.Public).ToList();
            var webmodule = db.WebModules.Where(x => x.ParentID == webmoduleid && x.Status == (int)Status.Public);
            if (webmodule.Any())
            {
                foreach (var item in webmodule)
                {
                    var lstnews = db.WebContents.Where(x => x.WebModuleID == item.ID && x.Status == (int)Status.Public);
                    if (lstnews.Any())
                    {
                        contents.AddRange(lstnews);
                    }
                }
            }
            return contents.OrderByDescending(x => x.PublishDate).Take(take).ToList();

        }
        public static string SubString(int lenght, string str)
        {
            if (str.Length > lenght)
            {
                str = str.Substring(0, lenght) + "...";
                return str;
            }
            else
            {
                return str;
            }

        }


    }
}