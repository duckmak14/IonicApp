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
using System.Web.Routing;

namespace WEB.Areas.ContentType.Controllers
{
    public partial class ArticleController
    {
        [ChildActionOnly]
        [AllowAnonymous]
        //List new
        public ActionResult _PubIndex(int id, string metatitle, int? page)
        {
            ViewBag.RouteValues = new RouteValueDictionary(new
            {
                controller = ControllerContext.ParentActionViewContext.RouteData.Values["controller"],
                action = ControllerContext.ParentActionViewContext.RouteData.Values["action"],
                area = ControllerContext.ParentActionViewContext.RouteData.Values["area"]
            });
            WebModule webmodule = null;
            if (TempData["WebModule"] != null)
            {
                webmodule = TempData["WebModule"] as WebModule;
            }
            else webmodule = db.Set<WebModule>().Find(id);
            var contents = new List<WebContent>();
            ViewBag.WebModule = webmodule;
            //contents = db.WebContents.Where(x => x.WebModuleID == id && x.Status.HasValue && x.Status.Value.Equals((int)Status.Public)).ToList();

            contents = GetListContents(id, contents).Where(x => x.Status.HasValue
                && x.WebModule.Culture == WEB.Models.ApplicationService.Culture && x.Status.Value.Equals((int)Status.Public)).ToList();
            //var sub = webmodule.SubWebModules.Where(x => x.Status == (int)Status.Public);
            //if (sub.Any())
            //{
            //    foreach (var item in sub)
            //    {
            //        var lstContentSub = db.WebContents.Where(x => x.WebModuleID == item.ID && x.Status.HasValue && x.Status.Value.Equals((int)Status.Public)).ToList();
            //        if (lstContentSub.Any())
            //        {
            //            contents.AddRange(lstContentSub);
            //        }
            //    }
            //}
            contents = contents.OrderByDescending(x => x.CreatedDate).ToList();
            var ipage = 1; if (page != null) ipage = page.Value;
            ViewBag.TotalItemCount = contents.Count();
            ViewBag.CurrentPage = ipage;
            return PartialView(contents.Skip((ipage - 1) * ApplicationService.PageSize).Take(ApplicationService.PageSize).ToList());
        }

        [ChildActionOnly]
        [AllowAnonymous]
        //List new
        public ActionResult _PubIndexEnterprise(int id, string metatitle, int? page)
        {
            ViewBag.RouteValues = new RouteValueDictionary(new
            {
                controller = ControllerContext.ParentActionViewContext.RouteData.Values["controller"],
                action = ControllerContext.ParentActionViewContext.RouteData.Values["action"],
                area = ControllerContext.ParentActionViewContext.RouteData.Values["area"]
            });
            WebModule webmodule = null;
            if (TempData["WebModule"] != null)
            {
                webmodule = TempData["WebModule"] as WebModule;
            }
            else webmodule = db.Set<WebModule>().Find(id);
            var contents = new List<WebContent>();
            ViewBag.WebModule = webmodule;
            //contents = db.WebContents.Where(x => x.WebModuleID == id && x.Status.HasValue && x.Status.Value.Equals((int)Status.Public)).ToList();
            //contents = contents.OrderByDescending(x => x.CreatedDate).ToList();

            contents = GetListContents(id, contents).Where(x => x.Status.HasValue
             && x.WebModule.Culture == WEB.Models.ApplicationService.Culture && x.Status.Value.Equals((int)Status.Public))
             .OrderByDescending(x => x.CreatedDate).ToList();

            var ipage = 1; if (page != null) ipage = page.Value;
            ViewBag.TotalItemCount = contents.Count();
            ViewBag.CurrentPage = ipage;
            ViewBag.SubModules = webmodule.SubWebModules.Where(x => x.Status == (int)Status.Public).ToList();

            return PartialView(contents.Skip((ipage - 1) * ApplicationService.PageSize).Take(ApplicationService.PageSize).ToList());
        }

        [ChildActionOnly]
        [AllowAnonymous]
        //List new
        public ActionResult _PubIndexEdition(int id, string metatitle)
        {
            ViewBag.WebModule = db.Set<WebModule>().Find(id);

            var webmodule = db.WebModules.Where(x => x.ParentID == id);
            return PartialView(webmodule);
        }

        private List<WebContent> GetListContents(int webModuleId, List<WebContent> results)
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

        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubDetail(int id)
        {
            var content = db.Set<WebContent>().Find(id);

            // update CountView
            if (content.CountView == null)
            {
                content.CountView = 1;
            }
            else
            {
                content.CountView = content.CountView + 1;
            }
            ViewBag.WebContentTitle = content.Title;
            db.WebContents.Attach(content);
            db.Entry(content).Property(a => a.CountView).IsModified = true;
            db.SaveChanges();
            //
            return PartialView(content);
        }
        [AllowAnonymous]
        public static List<WebModule> GetWebModuleByParentID(int webmoduleid)
        {
            WebContext db = new WebContext();
            var contents = db.WebModules.Where(x => x.ParentID == webmoduleid && x.Status == (int)Status.Public).ToList();
            return contents;
        }
        [AllowAnonymous]
        public ActionResult _OtherNews(int id, string position)
        {
            var item = db.WebContents.Single(x => x.ID == id);
            ViewBag.WebModule = item.WebModule;

            var contents = new List<WebContent>();
            //if (position == "top")
            //{
            //    contents = db.WebContents.Where(x => x.Status == (int)Status.Public && x.ID > id 
            //    && x.WebModuleID == item.WebModuleID).OrderByDescending(x => x.PublishDate).Take(5).ToList();
            //}
            //else
            //{
            contents = db.WebContents.Where(x => x.Status == (int)Status.Public && x.ID != id
            && x.WebModuleID == item.WebModuleID).OrderByDescending(x => x.PublishDate).Take(5).ToList();
            //}

            return PartialView(contents);
        }

        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _BoxDocNhieu()
        {
            var webContent = db.WebContents.Where(x => x.WebModule.ContentTypeID.Equals("Article") && x.Status == (int)Status.Public).OrderBy(x => x.CountView).Take(5);
            return PartialView(webContent);
        }

    }
}
