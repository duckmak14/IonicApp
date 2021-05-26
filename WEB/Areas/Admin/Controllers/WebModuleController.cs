using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebModels;
using System.Data;
using System.Text.RegularExpressions;
using Common;
using WEB.Models;
using System.Data.Entity;
using System.Xml.Linq;
using WebMatrix.WebData;
using System.IO;

namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public class WebModuleController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            return View();
        }


        public JsonResult Read_ForTree(int? id, int? selected)
        {

            var webmodules = from e in db.WebModules
                             where (id.HasValue ? e.ParentID == id : e.ParentID == null &&
                                     (e.ParentID == null &&
                             (e.Culture == null ||
                             (!string.IsNullOrEmpty(e.Culture) && e.Culture.Equals(ApplicationService.Culture)))
                             || (ApplicationService.Culture == null)))

                             orderby e.Order
                             select new
                             {
                                 id = e.ID,
                                 text = e.Title,
                                 hasChildren = e.SubWebModules.Any(),
                                 expanded = true,
                                 imageUrl = "",
                                 spriteCssClass = "",
                                 selected = (selected != null && selected == e.ID)
                             };

            return Json(webmodules, JsonRequestBehavior.AllowGet);

        }
        public JsonResult Read_ByParent(int? id)
        {

            var webmodules = from e in db.WebModules
                             where (id.HasValue ? e.ParentID == id : (e.ParentID == null &&
                              (!string.IsNullOrEmpty(e.Culture) && e.Culture.Equals(ApplicationService.Culture)))
                             || (ApplicationService.Culture == null))
                             orderby e.Order
                             select new
                             {
                                 ID = e.ID,
                                 Title = e.Title,
                                 Description = e.Description,
                                 SubQuerys = e.SubQuerys,
                                 Culture = e.Culture
                             };
            return Json(webmodules.ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Add()
        {
            var model = new WebModule();
            model.ShowOnAdmin = true;
            model.ShowOnMenu = true;
            model.Culture = ApplicationService.Culture;
            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Add([Bind(Exclude = "")]WebModule model, HttpPostedFileBase image, HttpPostedFileBase icon)
        {
            if (model.UID == null) model.UID = UniqueKeyGenerator.UsingTicks();
            var now = DateTime.Now;
            if (ModelState.IsValid)
            {
                if (image != null)
                {
                    model.Image = image.ImageSave("/uploads/image/" + (now.Month.ToString("00") + now.Year), 1366, 1366);
                    db.WebContentUploads.Add(new WebContentUpload()
                    {
                        Title = image.FileName,
                        MetaTitle = image.FileName.UnsignNormalize(),
                        FullPath = model.Image,
                        UID = ViewBag.GAK,
                        MimeType = ApplicationService.GetMimeType(Path.GetExtension(image.FileName)),
                        CreatedBy = WebSecurity.CurrentUserName,
                        CreatedDate = DateTime.Now
                    });
                    db.SaveChanges();
                }
                if (icon != null)
                {
                    model.Icon = icon.ImageSave("/uploads/" + (now.Month.ToString("00") + now.Year), 1366, 1366);
                    db.WebContentUploads.Add(new WebContentUpload()
                    {
                        Title = icon.FileName,
                        MetaTitle = icon.FileName.UnsignNormalize(),
                        FullPath = model.Icon,
                        UID = ViewBag.GAK,
                        MimeType = ApplicationService.GetMimeType(Path.GetExtension(icon.FileName)),
                        CreatedBy = WebSecurity.CurrentUserName,
                        CreatedDate = DateTime.Now
                    });
                    db.SaveChanges();
                }
                if (string.IsNullOrEmpty(model.MetaTitle))
                {
                    model.MetaTitle = model.Title.UnsignNormalize();
                }
                model.ModifiedDate = DateTime.Now;
                db.Set<WebModule>().Add(model);
                db.SaveChanges();
                ViewBag.StartupScript = "create_success();";
                return View(model);

            }
            else
            {
                return View(model);
            }
        }

        public ActionResult Edit(int id)
        {
            var model = db.Set<WebModule>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            return View("Edit", model);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(WebModule model, HttpPostedFileBase image, HttpPostedFileBase icon)
        {
            var now = DateTime.Now;
            model.ModifiedDate = DateTime.Now;
            if (model.UID == null) model.UID = UniqueKeyGenerator.UsingTicks();
            if (ModelState.IsValid)
            {
                if (image != null)
                {
                    model.Image = image.ImageSave("/uploads/image/" + (now.Month.ToString("00") + now.Year), 1366, 1366);
                    db.WebContentUploads.Add(new WebContentUpload()
                    {
                        Title = image.FileName,
                        MetaTitle = image.FileName.UnsignNormalize(),
                        FullPath = model.Image,
                        UID = ViewBag.GAK,
                        MimeType = ApplicationService.GetMimeType(Path.GetExtension(image.FileName)),
                        CreatedBy = WebSecurity.CurrentUserName,
                        CreatedDate = DateTime.Now
                    });
                    db.SaveChanges();
                }
                if (icon != null)
                {
                    model.Icon = icon.ImageSave("/uploads/" + (now.Month.ToString("00") + now.Year), 1366, 1366);
                    db.WebContentUploads.Add(new WebContentUpload()
                    {
                        Title = icon.FileName,
                        MetaTitle = icon.FileName.UnsignNormalize(),
                        FullPath = model.Icon,
                        UID = ViewBag.GAK,
                        MimeType = ApplicationService.GetMimeType(Path.GetExtension(icon.FileName)),
                        CreatedBy = WebSecurity.CurrentUserName,
                        CreatedDate = DateTime.Now
                    });
                    db.SaveChanges();
                }
                try
                {
                    db.WebModules.Attach(model);
                    db.Entry(model).Property(a => a.Title).IsModified = true;
                    db.Entry(model).Property(a => a.ContentTypeID).IsModified = true;
                    db.Entry(model).Property(a => a.Description).IsModified = true;
                    db.Entry(model).Property(a => a.ParentID).IsModified = true;
                    db.Entry(model).Property(a => a.Body).IsModified = true;
                    db.Entry(model).Property(a => a.MetaDescription).IsModified = true;
                    db.Entry(model).Property(a => a.MetaKeywords).IsModified = true;
                    db.Entry(model).Property(a => a.MetaTitle).IsModified = true;
                    db.Entry(model).Property(a => a.SeoTitle).IsModified = true;
                    db.Entry(model).Property(a => a.Image).IsModified = true;
                    db.Entry(model).Property(a => a.Order).IsModified = true;
                    db.Entry(model).Property(a => a.Status).IsModified = true;
                    db.Entry(model).Property(a => a.Target).IsModified = true;
                    db.Entry(model).Property(a => a.UID).IsModified = true;
                    db.Entry(model).Property(a => a.URL).IsModified = true;
                    db.Entry(model).Property(a => a.ShowOnAdmin).IsModified = true;
                    db.Entry(model).Property(a => a.ShowOnMenu).IsModified = true;
                    db.Entry(model).Property(a => a.Culture).IsModified = true;
                    db.Entry(model).Property(a => a.Icon).IsModified = true;
                    db.Entry(model).Property(a => a.CodeColor).IsModified = true;
                    db.Entry(model).Property(a => a.ModifiedDate).IsModified = true;
                    db.Entry(model).Property(a => a.ActiveArticle).IsModified = true;
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



        public ActionResult Delete(int id)
        {

            var role = db.Set<WebModule>().Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View("Delete", role);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(WebModule model)
        {



            try
            {
                var role = db.WebModules.Attach(model);
                db.Set<WebModule>().Remove(role);
                db.SaveChanges();


                ViewBag.StartupScript = "delete_success();";
                return View();
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

        }

        public ActionResult Deletes(string id)
        {

            var objects = id.Split(',');
            var lstSiteID = new List<int>();
            foreach (var obj in objects)
            {
                try
                {
                    lstSiteID.Add(int.Parse(obj.ToString()));
                }
                catch (Exception)
                { }
            }

            var temp = from p in db.Set<WebModule>()
                       where lstSiteID.Contains(p.ID)
                       select p;

            return View(temp.ToList());

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<WebModule> model)
        {

            var temp = new List<WebModule>();
            foreach (var item in model)
            {
                try
                {
                    db.Entry(item).State = EntityState.Deleted;
                    db.SaveChanges();

                }
                catch (Exception)
                {
                    db.Entry(item).State = EntityState.Unchanged;
                    temp.Add(item);
                }
            }

            if (temp.Count == 0)
            {
                ViewBag.StartupScript = "deletes_success();";
                return View(temp);
            }
            else if (temp.Count > 0)
            {
                ViewBag.StartupScript = "top.$('#grid').data('kendoGrid').dataSource.read(); top.treeview.dataSource.read();";
                ModelState.AddModelError("", Resources.Common.ErrorDeleteItems);
                return View(temp);
            }
            else
            {
                ViewBag.StartupScript = "deletes_success();";
                return View();
            }

        }


        public JsonResult GetByParent(int? id)
        {
            var dataContext = new WebContext();
            var webmodules = from e in db.WebModules
                             where (id.HasValue ? e.ParentID == id : (e.ParentID == null &&
                              (!string.IsNullOrEmpty(e.Culture) && e.Culture.Equals(ApplicationService.Culture)))
                             || (ApplicationService.Culture == null))

                             orderby e.Order
                             select new
                             {
                                 ID = e.ID,
                                 Title = e.Title,
                                 HasChildren = e.SubWebModules.Any()
                             };
            return Json(webmodules, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetParentID(int id)
        {
            var lstint = new List<int>();

            var temp = db.WebModules.Find(id);

            if (temp.ParentID != null)
            {
                lstint.Add(temp.ParentID.Value);
                var temp2 = db.WebModules.Find(temp.ParentID.Value);
                if (temp2.ParentID != null)
                {
                    lstint.Add(temp2.ParentID.Value);
                    var temp3 = db.WebModules.Find(temp2.ParentID.Value);
                    if (temp3.ParentID != null)
                    {
                        lstint.Add(temp3.ParentID.Value);
                        var temp4 = db.WebModules.Find(temp3.ParentID.Value);
                        if (temp4.ParentID != null)
                        {
                            lstint.Add(temp4.ParentID.Value);
                        }
                    }
                }
            }
            return Json(lstint, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LayoutView(int id)
        {


            var model = db.Set<WebModule>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }


            try
            {
                var lstPubLayout = new List<SelectListItem>();
                var layouts = XElement.Load(Server.MapPath("/App_Data/publayouts.xml")).Elements("Layout");
                foreach (var l in layouts)
                {
                    var item = new SelectListItem() { Text = l.Attribute("Title").Value, Value = l.Attribute("Path").Value };
                    lstPubLayout.Add(item);
                }
                if (lstPubLayout.Count > 0) ViewBag.PubLayout = lstPubLayout;
            }
            catch (Exception)
            { }



            try
            {
                var lstPubIndexView = new List<SelectListItem>();
                if (!string.IsNullOrEmpty(model.ContentTypeID))
                {
                    var views = XElement.Load(Server.MapPath("/App_Data/pubindexviews.xml")).Element(model.ContentTypeID);
                    foreach (var v in views.Elements("Item"))
                    {
                        var item = new SelectListItem() { Text = v.Attribute("Text").Value, Value = v.Attribute("Value").Value };
                        lstPubIndexView.Add(item);
                    }
                }
                if (lstPubIndexView.Count > 0) ViewBag.PubIndexView = lstPubIndexView;
            }
            catch (Exception)
            { }

            try
            {
                var lstPubDetailView = new List<SelectListItem>();
                if (!string.IsNullOrEmpty(model.ContentTypeID))
                {
                    var views = XElement.Load(Server.MapPath("/App_Data/pubdetailviews.xml")).Element(model.ContentTypeID);
                    foreach (var v in views.Elements("Item"))
                    {
                        var item = new SelectListItem() { Text = v.Attribute("Text").Value, Value = v.Attribute("Value").Value };
                        lstPubDetailView.Add(item);
                    }
                }
                if (lstPubDetailView.Count > 0) ViewBag.PubDetailView = lstPubDetailView;
            }
            catch (Exception)
            { }



            try
            {
                var lstAdminView = new List<SelectListItem>();
                if (!string.IsNullOrEmpty(model.ContentTypeID))
                {
                    var views = XElement.Load(Server.MapPath("/App_Data/adminviews.xml")).Element(model.ContentTypeID);
                    foreach (var v in views.Elements("Item"))
                    {
                        var item = new SelectListItem() { Text = v.Attribute("Text").Value, Value = v.Attribute("Value").Value };
                        lstAdminView.Add(item);
                    }
                }
                if (lstAdminView.Count > 0) ViewBag.AdminView = lstAdminView;
            }
            catch (Exception)
            { }

            try
            {
                var lstAdminLayout = new List<SelectListItem>();
                var layouts = XElement.Load(Server.MapPath("/App_Data/adminlayouts.xml")).Elements("Layout");
                foreach (var l in layouts)
                {
                    var item = new SelectListItem() { Text = l.Attribute("Title").Value, Value = l.Attribute("Path").Value };
                    lstAdminLayout.Add(item);
                }
                if (lstAdminLayout.Count > 0)
                    if (lstAdminLayout.Count > 0) ViewBag.AdminLayout = lstAdminLayout;
            }
            catch (Exception)
            { }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LayoutView(WebModule model)
        {


            try
            {

                db.WebModules.Attach(model);
                db.Entry(model).Property(a => a.PublishDetailLayout).IsModified = true;
                db.Entry(model).Property(a => a.PublishDetailView).IsModified = true;
                db.Entry(model).Property(a => a.PublishIndexLayout).IsModified = true;
                db.Entry(model).Property(a => a.PublishIndexView).IsModified = true;
                db.Entry(model).Property(a => a.IndexView).IsModified = true; db.Entry(model).Property(a => a.IndexLayout).IsModified = true;
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




        //public ContentResult GetHtmlSite()
        //{
        //    string html = string.Empty;
        //    var shtml = Session["AdminSite-" + HttpContext.User.Identity.Name];
        //    if (shtml == null)
        //    {
        //        using (var dataContext = new WebContext())
        //        {
        //            var webmodules = from e in dataContext.WebModules
        //                             where (e.ParentID == null)
        //                             select new
        //                             {
        //                                 ID = e.ID,
        //                                 Title = e.Title,
        //                                 Subwebmodules = e.SubWebModules
        //                             };
        //            foreach (var item in webmodules)
        //            {
        //                html += "<li>";
        //                var subsites = item.Subwebmodules;
        //                if (subsites.Count > 0)
        //                {
        //                    var html2 = string.Empty;
        //                    html2 += "<ul class='dropdown-menu'> ";
        //                    foreach (var subitem in subsites)
        //                    {
        //                        var ssubsites = subitem.SubWebModules;
        //                        if (ssubsites.Count > 0)
        //                        {
        //                            var html3 = string.Empty;
        //                            html3 += "<ul class='dropdown-menu'> ";
        //                            foreach (var ssitem in ssubsites)
        //                            {
        //                                html3 += "<li> <a href='" + ssitem.Url + "' >" + ssitem.Title + "</a>  </li>";
        //                            }
        //                            html3 += "</ul>";
        //                            html2 += "<li  class='dropdown-submenu'> <a data-toggle='dropdown' class='dropdown-toggle' href='" + subitem.Url + "'>" + subitem.Title + "</a> " + html3 + "</li>";
        //                        }
        //                        else
        //                        {
        //                            html2 += "<li> <a href='" + subitem.Url + "'>" + subitem.Title + "</a> </li>";
        //                        }
        //                    }
        //                    html2 += "</ul>";
        //                    html += " <a href='" + item.Url + "' data-toggle='dropdown' class='dropdown-toggle'> <span>" + item.Title + "</span> <span class='caret'></span> </a>" + html2;
        //                }
        //                else
        //                {
        //                    html += "    <a href='" + item.Url + "'> <span>" + item.Title + "</span> </a>";
        //                }
        //                html += "</li>";
        //            }
        //            html = Regex.Replace(html, @"\s+", " ");
        //            html = Regex.Replace(html, @"\s*\n\s*", "\n");
        //            html = Regex.Replace(html, @"\s*\>\s*\<\s*", "><");
        //            Session["AdminSite-" + HttpContext.User.Identity.Name] = html;
        //        }
        //    }
        //    else
        //    {
        //        html = Session["AdminSite-" + HttpContext.User.Identity.Name].ToString();
        //    }
        //    return Content(html);
        //}
    }
}
