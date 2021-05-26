using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebModels;
using Common;
using WEB.Models; 

namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public class CountryController : BaseController
    {
        private WebContext db = new WebContext();
        
        //
        // GET: /Admin/Country/

        public ActionResult Index()
        {
           
            return View();
        }
     

        public ActionResult Country_Read([DataSourceRequest] DataSourceRequest request)
        {
            var contents = db.Countries.Select(x=> new {x.ID,x.Title,x.IsoCode}).ToList(); 
            return Json(contents.ToDataSourceResult(request));
        }

        public JsonResult GetCountries()
        {
            var countries = db.Countries.Select(x => new { x.ID, x.Title, x.IsoCode }).ToList();
            return Json(countries, JsonRequestBehavior.AllowGet);

        }

        //
        // GET: /Admin/Country/Create

        public ActionResult Add()
        {
            var model = new Country();
            
            return View(model);
        }


        
        //
        // POST: /Admin/Country/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(Country model)
        {
            if (ModelState.IsValid)
            { 
                db.Countries.Add(model);
                db.SaveChanges(); 
                ViewBag.StartupScript = "create_success();";
                return View(model);
            } 
            return View(model);
        }

        //
        // GET: /Admin/Country/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Country model = db.Countries.Find(id);

             
            if (model == null)
            {
                return HttpNotFound();
            }
            
           
           

            return View(model);
        }

        //
        // POST: /Admin/Country/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Country model)
        {
            if (ModelState.IsValid)
            {
              
                db.Countries.Attach(model);
                db.Entry(model).Property(a => a.Title).IsModified = true;
                db.Entry(model).Property(a => a.IsoCode).IsModified = true; 
                db.SaveChanges();
                 
                ViewBag.StartupScript = "edit_success();";
                return View(model);
            }
            return View(model);
        }



        public ActionResult Delete(int id)
        { 
            var role = db.Set<Country>().Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View("Delete", role);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Country model)
        {
             
            try
            {
               
                var role = db.Countries.Attach(model);
                db.Set<Country>().Remove(role);
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

            var temp = from p in db.Set<Country>()
                       where lstSiteID.Contains(p.ID)
                       select p;

            return View(temp.ToList());

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<Country> model)
        {

            var temp = new List<Country>();
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



        
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}
