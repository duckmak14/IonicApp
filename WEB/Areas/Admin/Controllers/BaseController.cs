using Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.SessionState;
using WEB.Models;
using WebMatrix.WebData;
using WebModels;

namespace WEB.Areas.Admin.Controllers
{


    public class BaseController : Controller
    {


        #region OnActionExecuting
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var action = filterContext.ActionDescriptor.ActionName;
            ViewBag.GAK = controller;
        }



        #endregion
        public List<Language> Language
        {
            get
            {
                return ApplicationService.Languages;

            }
        }

        public string Culture
        {
            get
            {
                return this.Session["culture"].ToString();
            }
        }
        protected override void ExecuteCore()
        {

            string culture = "";
            if (this.Session["culture"] != null)
            {
                culture = this.Session["culture"].ToString();
            }
            else
            {
                try
                {
                    if (this.Language != null)
                    {
                        culture = this.Language.First().ID;
                    }
                    //else
                    //    culture = CultureInfo.InvariantCulture.Name;

                }
                catch (Exception)
                {
                    culture = CultureInfo.InvariantCulture.Name;
                }
                this.Session["culture"] = culture;
            }
            if (culture != "")
                try
                {
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
                    Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
                }
                catch (Exception)
                { }
            base.ExecuteCore();
        }

        protected override bool DisableAsyncSupport
        {
            get { return true; }
        }
        public virtual ActionResult ChangeLanguage(string lang, string returnUrl)
        {
            try
            {
                Session["culture"] = lang;
            }
            catch
            { }
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect(Request.UrlReferrer.ToString());

            }
        }

        [HttpGet]
        public virtual JsonResult _AdminSiteMap(string ak)
        {
            using (var db = new WebContext())
            {
                var item = db.AdminSites.AsNoTracking().Where(x => x.AccessKey.ToLower().Equals(ak.ToLower()));
                var s = item.FirstOrDefault();
                if (s != null)
                {
                    var jarray = new JArray();
                    jarray.Add(new JObject(new JProperty("ID", s.ID), new JProperty("Title", s.Title), new JProperty("Url", s.Url)));
                    if (s.ParentID != null)
                    {
                        AdminSiteMapParent(s.ParentID.Value, ref jarray, db);
                    }
                    return Json(new { Json = jarray.ToString() }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Json = new JArray().ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        private void AdminSiteMapParent(int id, ref JArray jarray, WebContext db)
        {

            var s = db.AdminSites.AsNoTracking().Single(x => x.ID == id);
            jarray.Add(new JObject(new JProperty("ID", s.ID), new JProperty("Title", s.Title), new JProperty("Url", s.Url)));
            if (s.ParentID != null)
            {
                AdminSiteMapParent(s.ParentID.Value, ref jarray, db);
            }
        }

        public ContentResult GetHtmlSite()
        {
            int userid = WebSecurity.CurrentUserId;
            string html = string.Empty;

            var shtml = Session["AdminSite-" + Culture + "-" + HttpContext.User.Identity.Name];
            if (shtml == null)
            {
                using (var dataContext = new WebContext())
                {
                    var adminsites = from e in dataContext.AdminSites.AsNoTracking()
                                     where (e.ParentID == null && e.AccessAdminSites.Where(x => x.AdminSiteID == e.ID && x.UserId == userid).Count() > 0)
                                     orderby e.Order
                                     select e;
                    foreach (var item in adminsites)
                    {
                        html += "<li>";
                        var subsites = item.SubAdminSites.Where(e => e.AccessAdminSites.Where(x => x.AdminSiteID == e.ID && x.UserId == userid).Count() > 0);
                        if (subsites.Count() > 0)
                        {
                            var html2 = string.Empty;
                            html2 += "<ul class='dropdown-menu'> ";
                            foreach (var subitem in subsites)
                            {
                                var ssubsites = subitem.SubAdminSites.Where(e => e.AccessAdminSites.Where(x => x.AdminSiteID == e.ID && x.UserId == userid).Count() > 0);
                                if (ssubsites.Count() > 0)
                                {
                                    var html3 = string.Empty;
                                    html3 += "<ul class='dropdown-menu'> ";
                                    foreach (var ssitem in ssubsites)
                                    {
                                        html3 += "<li> <a href='" + ssitem.Url + "' >" + ssitem.Title + "</a>  </li>";
                                    }
                                    html3 += "</ul>";
                                    html2 += "<li  class='dropdown-submenu'> <a data-toggle='dropdown' class='dropdown-toggle' href='" + subitem.Url + "'>" + subitem.Title + "</a> " + html3 + "</li>";
                                }
                                else
                                {
                                    html2 += "<li> <a href='" + subitem.Url + "'>" + subitem.Title + "</a> </li>";
                                }
                            }
                            html2 += "</ul>";
                            html += " <a href='" + item.Url + "' data-toggle='dropdown' class='dropdown-toggle'> <span>" + item.Title + "</span> <span class='caret'></span> </a>" + html2;
                        }
                        else
                        {
                            html += "    <a href='" + item.Url + "'> <span>" + item.Title + "</span> </a>";
                        }
                        html += "</li>";
                    }
                    html = Regex.Replace(html, @"\s+", " ");
                    html = Regex.Replace(html, @"\s*\n\s*", "\n");
                    html = Regex.Replace(html, @"\s*\>\s*\<\s*", "><");
                    Session["AdminSite-" + Culture + "-" + HttpContext.User.Identity.Name] = html;
                }
            }
            else
            {
                html = Session["AdminSite-" + Culture + "-" + HttpContext.User.Identity.Name].ToString();
            }
            return Content(html);
        }

        public ContentResult GetHtmlSiteOnRole()
        {
            int userid = WebSecurity.CurrentUserId;

            string html = string.Empty;

            var shtml = Session["AdminSite-" + Culture + "-" + HttpContext.User.Identity.Name];
            if (shtml == null)
            {
                using (var dataContext = new WebContext())
                {
                    var roles = Roles.GetRolesForUser();
                    var userRoleIds = dataContext.WebRoles.Where(x => roles.Contains(x.RoleName)).Select(y => y.RoleId);

                    var adminsites = from e in dataContext.AdminSites.AsNoTracking()
                                     where (e.ParentID == null && e.AccessAdminSiteRoles.Where(x => x.AdminSiteID == e.ID && userRoleIds.Contains(x.RoleId)).Count() > 0)
                                     orderby e.Order
                                     select e;

                    foreach (var item in adminsites)
                    {
                        html += "<li>";
                        var subsites = item.SubAdminSites.Where(e => e.AccessAdminSiteRoles.Where(x => x.AdminSiteID == e.ID && userRoleIds.Contains(x.RoleId)).Count() > 0);
                        if (subsites.Count() > 0)
                        {
                            var html2 = string.Empty;
                            html2 += "<ul class='dropdown-menu'> ";
                            foreach (var subitem in subsites)
                            {
                                var ssubsites = subitem.SubAdminSites.Where(e => e.AccessAdminSiteRoles.Where(x => x.AdminSiteID == e.ID && userRoleIds.Contains(x.RoleId)).Count() > 0);
                                if (ssubsites.Count() > 0)
                                {
                                    var html3 = string.Empty;
                                    html3 += "<ul class='dropdown-menu'> ";
                                    foreach (var ssitem in ssubsites)
                                    {
                                        html3 += "<li> <a href='" + ssitem.Url + "' >" + ssitem.Title + "</a>  </li>";
                                    }
                                    html3 += "</ul>";
                                    html2 += "<li  class='dropdown-submenu'> <a data-toggle='dropdown' class='dropdown-toggle' href='" + subitem.Url + "'>" + subitem.Title + "</a> " + html3 + "</li>";
                                }
                                else
                                {
                                    html2 += "<li> <a href='" + subitem.Url + "'>" + subitem.Title + "</a> </li>";
                                }
                            }
                            html2 += "</ul>";
                            html += " <a href='" + item.Url + "' data-toggle='dropdown' class='dropdown-toggle'> <span>" + item.Title + "</span> <span class='caret'></span> </a>" + html2;
                        }
                        else
                        {
                            html += "    <a href='" + item.Url + "'> <span>" + item.Title + "</span> </a>";
                        }
                        html += "</li>";
                    }
                    html = Regex.Replace(html, @"\s+", " ");
                    html = Regex.Replace(html, @"\s*\n\s*", "\n");
                    html = Regex.Replace(html, @"\s*\>\s*\<\s*", "><");
                    Session["AdminSite-" + Culture + "-" + HttpContext.User.Identity.Name] = html;
                }
            }
            else
            {
                html = Session["AdminSite-" + Culture + "-" + HttpContext.User.Identity.Name].ToString();
            }
            return Content(html);
        }
    }
}
