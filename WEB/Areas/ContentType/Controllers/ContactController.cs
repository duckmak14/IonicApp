using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebModels;
using Common;
using WebMatrix.WebData;
using System.Data.Entity;
using Kendo.Mvc;

namespace WEB.Areas.ContentType.Controllers
{
    public partial class ContactController : BaseController
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [ChildActionOnly]
        public ActionResult _Index(int id)
        {
            ViewBag.ID = id;
            return PartialView();
        }

        public ActionResult WebContact_Read([DataSourceRequest] DataSourceRequest request, int id)
        {
            
            //var contents = db.WebContacts.Where(x => x.WebModuleID == id).Select(x => new { x.ID, x.Title, x.FullName, x.Mobile, x.Email, x.Address });
            var contents = db.WebContacts.Select(x => new { x.ID, x.Title, x.FullName, x.Mobile, x.Email, x.Address });

            if (request.Sorts.Count == 0)
            {
                request.Sorts.Add(new SortDescriptor("ID", System.ComponentModel.ListSortDirection.Descending));
            }
            return Json(contents.ToDataSourceResult(request));
        }

        //public JsonResult GetWebContacts(string text, int id)
        //{           
        //    var contents = from x in db.WebContacts where x.WebModuleID == id select x;
        //    if (!string.IsNullOrEmpty(text))
        //    {
        //        contents = contents.Where(p => p.Title.Contains(text));
        //    }      
        //    return Json(contents, JsonRequestBehavior.AllowGet);    
        //}

        public ActionResult Add(int id)
        {
            ViewBag.ID = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add( WebContact model )
        {
            //ViewBag.ID = model.WebModuleID; 
            if (ModelState.IsValid)
            { 
                 
                    model.CreatedDate = DateTime.Now;
                    db.Set<WebContact>().Add(model);
                    db.SaveChanges();
                    ViewBag.StartupScript = "success();";
                    return View(model);

                
            }
            else
            {
                return View(model);
            }
        }
        public ActionResult Edit(int id)
        {
            
                var model = db.Set<WebContact>().Find(id);
                if (model == null)
                {
                    return HttpNotFound();
                }
                return View("Edit", model);
             
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit( WebContact model)
        {
            if (ModelState.IsValid)
            {
                 
                    try
                    {
                        db.WebContacts.Attach(model);
                        db.Entry(model).Property(a => a.Title).IsModified = true;
                        db.Entry(model).Property(a => a.FullName).IsModified = true;
                        db.Entry(model).Property(a => a.Email).IsModified = true;
                        db.Entry(model).Property(a => a.Mobile).IsModified = true;
                        db.Entry(model).Property(a => a.Address).IsModified = true;
                        db.Entry(model).Property(a => a.Body).IsModified = true;
                        db.Entry(model).Property(a => a.Body).IsModified = true;

                        db.Entry(model).Property(a => a.CreatedDate).IsModified = true; 
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
             
                var model = db.Set<WebContact>().Find(id);
                if (model == null)
                {
                    return HttpNotFound();
                }
                return View("Delete", model);
             
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(WebContact model)
        {

            

                try
                {
                    db.Entry(model).State = EntityState.Deleted;
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
            var lstObjId = new List<int>();
            foreach (var obj in objects)
            {
                try
                {
                    lstObjId.Add(int.Parse(obj.ToString()));
                }
                catch (Exception)
                { }
            }
            
                var temp = from p in db.Set<WebContact>()
                           where lstObjId.Contains(p.ID)
                           select p;

                return View(temp.ToList());
             
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<WebContact> model)
        {
             
                var temp = new List<WebContact>();
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
                    ViewBag.StartupScript = "top.$('#grid').data('kendoGrid').dataSource.read();";
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
}
