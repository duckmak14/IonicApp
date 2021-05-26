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
using Kendo.Mvc;
using WEB.Models;
using System.Data.Entity;
using System.Configuration;
using System.Transactions;
using System.Web.Routing;
using System.IO;
namespace WEB.Controllers
{
    public class HomeController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index(/*int? id, string uid, string lang, string metatitle, int? page*/)
        {
            //var home = new List<string> { "home", "index", "home.html", "index.html", "trangchu", "trang-chu", "trangchu.html", "trang-chu.html" };

            //if (id.HasValue)
            //{
            //    var module = db.Set<WebModule>().Find(id);
            //    TempData["WebModule"] = module;
            //    ViewBag.Page = page;
            //    return View(module);

            //}
            //else if (!string.IsNullOrEmpty(uid))
            //{

            //    var module = (from x in db.WebModules
            //                  where
            //                      (x.UID.ToLower().Equals(uid.ToLower()))
            //                  select x).AsNoTracking().FirstOrDefault();
            //    return View(module);
            //}
            //else
            //{
            //    var module = (from x in db.WebModules
            //                  where
            //                      (x.UID == null || home.Contains(x.ContentType.ID.ToLower()))
            //                  select x).AsNoTracking().FirstOrDefault();
            //    return View(module);
            //}
            return Redirect("/admin");

        }
        public ActionResult Detail(int id, string metatitle, int m_id, string m_metatitle)
        {
            ViewBag.ID = id;
            var module = db.Set<WebModule>().Find(m_id);
            return View(module);
        }
        [OutputCache(Duration = 120, VaryByCustom = "culture")]
        public ActionResult SiteMap()
        {
            ViewBag.Language = Language;
            var webmodules = from e in db.WebModules
                             where (e.ParentID == null)
                             orderby e.Order
                             select e;
            return View(webmodules);
        }

        public ActionResult ProductSearch(string keyword)
        {
            return View();
        }
        public ActionResult Order()
        {
            return View();
        }
        public ActionResult Search(int? page)
        {
            WebContext db = new WebContext();
            var keyword = Request.QueryString["keyword"];
            var skeyword = "";

            if (keyword != null) skeyword = keyword.ToString().ToLower();

            ViewBag.RouteValues = new RouteValueDictionary(new
            {
                controller = "Home",
                action = "Search",
                area = "",
                keyword = keyword
            });

            var a = skeyword.UnsignNormalize();

            var contents = db.WebContents.Where(x => x.WebModule.ContentTypeID.Equals("Article") && x.Status == (int)Status.Public).ToList();

            if (!string.IsNullOrWhiteSpace(skeyword))
            {
                contents = contents.Where(x => x.Status == (int)Status.Public
                && (x.Title.ToLower().Contains(skeyword)
                || (x.MetaTitle != null && x.MetaTitle.ToLower().Contains(a)
                || (x.MetaKeywords != null && x.MetaKeywords.ToLower().Contains(skeyword)))
                || (x.Body != null && x.Body.ToLower().Contains(a)
                ))).ToList();
            }

            var ipage = 1; if (page != null) ipage = page.Value;
            ViewBag.TotalItemCount = contents.Count();
            ViewBag.CurrentPage = ipage;
            return View(contents.Skip((ipage - 1) * ApplicationService.PageSize).Take(ApplicationService.PageSize).OrderByDescending(x => x.CreatedDate).ToList());
        }
        public ActionResult Event(int? page)
        {
            WebContext db = new WebContext();

            var date = Request.QueryString["date"];
            var sdate = DateTime.Now;
            if (date != null) sdate = Convert.ToDateTime(date);
            ViewBag.RouteValues = new RouteValueDictionary(new
            {
                controller = "Home",
                action = "Event",
                area = "",
                date = date
            });
            var contents = db.WebContents.Where(x => x.Status == (int)Status.Public && x.Event == sdate).ToList();
            var ipage = 1; if (page != null) ipage = page.Value;
            ViewBag.TotalItemCount = contents.Count();
            ViewBag.CurrentPage = ipage;
            return View(contents.Skip((ipage - 1) * ApplicationService.PageSize).Take(ApplicationService.PageSize).OrderByDescending(x => x.CreatedDate).ToList());
        }
        [ChildActionOnly]
        public ActionResult _Language()
        {
            return PartialView(this.Language);
        }
        public ActionResult PartnersHome()
        {
            var webmodul = db.WebModules.Where(x => x.UID == "partner" && x.Status == (int)Status.Public).FirstOrDefault();

            var webcontent = db.WebContents.Where(x => x.WebModuleID == webmodul.ID).ToList();
            return PartialView(webcontent);
        }
        [ChildActionOnly]
        public ActionResult _SiteMap(int id = 0, int m_id = 0)
        {
            id = m_id > 0 ? m_id : id;

            if (id == 0)
            {
                return PartialView(null);
            }

            WebModule module = this.db.WebModules.Find(id);
            Stack<WebModule> modules = new Stack<WebModule>();

            do
            {
                modules.Push(module);

                module = module.Parent;
            }
            while (module != null);

            return PartialView(modules);
        }
        public static WebConfig getconfig(string key)
        {
            WebContext db = new WebContext();

            var config = (from c in db.WebConfigs
                          where c.Key.Equals(key)
                          select c);

            return config.FirstOrDefault();
        }

        public List<WebContent> GetListContents(int webModuleId, List<WebContent> results)
        {
            var webContents = db.WebContents.Where(x => x.WebModuleID == webModuleId);
            results.AddRange(webContents);

            var childWebModules = db.WebModules.Where(x => x.ParentID == webModuleId);

            foreach (var childWebModule in childWebModules)
            {
                GetListContents(childWebModule.ID, results);
            }

            return results;
        }
    }
}
