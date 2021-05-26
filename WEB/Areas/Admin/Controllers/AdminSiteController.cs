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
using Newtonsoft.Json.Linq; 
namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public class AdminSiteController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
 
        //
        // GET: /Admin/AdminSites/

        public ActionResult Index()
        {
            return View();
        }
         
        public JsonResult Read_ForTree(int? id)
        {

            var adminsites = from e in db.AdminSites.AsNoTracking()
                                 where (id.HasValue ? e.ParentID == id : e.ParentID == null) orderby e.Order
                                 select new
                                 {
                                     id = e.ID,
                                     text = e.Title,
                                     hasChildren = e.SubAdminSites.Any(),
                                     expanded = true,
                                     imageUrl = "",
                                     spriteCssClass = ""
                                 };

                return Json(adminsites, JsonRequestBehavior.AllowGet);
             
        } 
        public JsonResult Read_ByParent(int? id)
        {

            var adminsites = from e in db.AdminSites.AsNoTracking()
                             where (id.HasValue ? e.ParentID == id : e.ParentID == null)
                             orderby e.Order
                             select new
                             {
                                 ID = e.ID,
                                 Title = e.Title,
                                 AccessKey = e.AccessKey,
                                 Description = e.Description 
                             };

                return Json(adminsites.ToList(), JsonRequestBehavior.AllowGet);
              
        }

        public ActionResult Add()
        {
            
            List<SelectListItem> ListItems = new List<SelectListItem>();
            ListItems.Add(new SelectListItem { Value = "", Text =""});

            var hasaccess = db.AdminSites.AsNoTracking().Where(x => !string.IsNullOrEmpty(x.AccessKey)).Where(x => !string.IsNullOrEmpty(x.AccessKey)).Select(x => x.AccessKey).ToList();

            foreach (var item in Enum.GetValues(typeof(AccessKeys)))
            {
                if(!hasaccess.Contains(item.ToString()))
                ListItems.Add(new SelectListItem { Value = item.ToString(), Text = item.ToString() });
            }
            ViewBag.ListItems = ListItems; 
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add([Bind(Exclude = "")]AdminSite model)
        {
            var hasaccess = db.AdminSites.AsNoTracking().Where(x => !string.IsNullOrEmpty(x.AccessKey)).Select(x => x.AccessKey).ToList();
            List<SelectListItem> ListItems = new List<SelectListItem>();
            ListItems.Add(new SelectListItem { Value = "", Text = "" });
            foreach (var item in Enum.GetValues(typeof(AccessKeys)))
            {
                if (!hasaccess.Contains(item))
                    ListItems.Add(new SelectListItem { Value = item.ToString(), Text = item.ToString() });
            }

            ViewBag.ListItems = ListItems;

            if (ModelState.IsValid)
            {
                

               
                 if (hasaccess.Contains(model.AccessKey))
                 {
                     ModelState.AddModelError("", Resources.Common.AccessKeyUsed);
                     return View(model);
                 }         
 
                db.Set<AdminSite>().Add(model);
                db.SaveChanges(); 
                ViewBag.StartupScript = "create_success();";
                Session["AdminSite-"+Culture+"-" + HttpContext.User.Identity.Name] = null;
                return View(model);                    
                
            }
            else
            {
                return View(model);
            }
        }

        public ActionResult Edit(int id)
        {
           
            
                var site = db.Set<AdminSite>().Find(id);

                List<SelectListItem> ListItems = new List<SelectListItem>() { new SelectListItem { Value = "", Text = "" } };

                var hasaccess = db.AdminSites.AsNoTracking().Where(x => !string.IsNullOrEmpty(x.AccessKey) && x.ID != id).Select(x => x.AccessKey).ToList();
                foreach (var item in Enum.GetValues(typeof(AccessKeys)))
                {
                    if ( !hasaccess.Contains(item.ToString()) )
                        ListItems.Add(new SelectListItem { Value = item.ToString(), Text = item.ToString() });
                }
                ViewBag.ListItems = ListItems;

                if (site == null)
                {
                    return HttpNotFound();
                } 
                return View("Edit", site);
            
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AdminSite model)
        {
            
            if (ModelState.IsValid)
            {
                 
                List<SelectListItem> ListItems = new List<SelectListItem>() { new SelectListItem { Value = "", Text = "" } };
                var hasaccess = db.AdminSites.AsNoTracking().Where(x => !string.IsNullOrEmpty(x.AccessKey) && x.ID != model.ID).Select(x => x.AccessKey).ToList();
                foreach (var item in Enum.GetValues(typeof(AccessKeys)))
                {
                    if (!hasaccess.Contains(item.ToString()) )
                        ListItems.Add(new SelectListItem { Value = item.ToString(), Text = item.ToString() });
                }
                ViewBag.ListItems = ListItems;

                    try
                    { 
                            db.AdminSites.Attach(model);
                            db.Entry(model).Property(a => a.Title).IsModified = true;
                            db.Entry(model).Property(a => a.Description).IsModified = true;
                            db.Entry(model).Property(a => a.AccessKey).IsModified = true;
                            db.Entry(model).Property(a => a.Url).IsModified = true;
                            db.Entry(model).Property(a => a.ParentID).IsModified = true;
                            db.Entry(model).Property(a => a.Order).IsModified = true;
                            db.SaveChanges();  
                            ViewBag.StartupScript = "edit_success();";
                            Session["AdminSite-" + Culture + "-" + HttpContext.User.Identity.Name] = null;
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
            using (var db = new WebContext())
            {
                var role = db.Set<AdminSite>().Find(id);
                if (role == null)
                {
                    return HttpNotFound();
                }
                return View("Delete", role);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(AdminSite model)
        {

            using (var db = new WebContext())
            {

                try
                {
                    var role = db.AdminSites.Attach(model);
                    db.Set<AdminSite>().Remove(role);
                    db.SaveChanges(); 
                    ViewBag.StartupScript = "delete_success();";
                    Session["AdminSite-" + Culture + "-" + HttpContext.User.Identity.Name] = null;
                    return View();
                }
                catch (Exception ex)
                {

                    ModelState.AddModelError("", ex.Message);
                    return View(model);
                }
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
            using (var db = new WebContext())
            {
                var temp = from p in db.Set<AdminSite>()
                           where lstSiteID.Contains(p.ID)
                           select p;

                return View(temp.ToList());
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<AdminSite> model)
        {
            using (var db = new WebContext())
            {
                var temp = new List<AdminSite>();
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
        }

       
        public JsonResult GetByParent(int? id)
        {
            var dataContext = new WebContext();
            var adminsites = from e in dataContext.AdminSites.AsNoTracking()
                             where (id.HasValue ? e.ParentID == id : e.ParentID == null)
                             select new
                             {
                                 ID = e.ID,
                                 Title = e.Title,
                                 HasChildren = e.SubAdminSites.Any()
                             };
            return Json(adminsites, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetParentID(int id)
        {
            var lstint = new List<int>();
            var db = new WebContext();
            var temp = db.AdminSites.Find(id);

            if (temp.ParentID != null)
            {
                lstint.Add(temp.ParentID.Value);
                var temp2=db.AdminSites.Find(temp.ParentID.Value);
                if (temp2.ParentID != null)
                {
                    lstint.Add(temp2.ParentID.Value);
                    var temp3 = db.AdminSites.Find(temp2.ParentID.Value);
                    if (temp3.ParentID != null)
                    {
                        lstint.Add(temp3.ParentID.Value);
                        var temp4 = db.AdminSites.Find(temp3.ParentID.Value);
                        if (temp4.ParentID != null)
                        {
                            lstint.Add(temp4.ParentID.Value);
                        }
                    }
                }
            }
            return Json(lstint, JsonRequestBehavior.AllowGet);
        }

       
       
    }
}
