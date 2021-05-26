using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebModels;
using Kendo.Mvc;
using System.Data.Entity;
using System.Transactions;
using WEB.Models;

namespace WEB.Areas.Admin.Controllers
{
    [VanTaiAuthorize]

    public class NavigationController : BaseController
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

        public ActionResult Navigation_Read([DataSourceRequest] DataSourceRequest request)
        {
            var users = from x in db.Navigations.AsNoTracking() select new { x.ID, x.Key, x.Title, x.Description, x.Order };
            if (request.Sorts.Count == 0)
            {
                request.Sorts.Add(new SortDescriptor("Order", System.ComponentModel.ListSortDirection.Ascending));
            }                                        

            return Json(users.ToDataSourceResult(request));   
        }

        public ActionResult Add()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(Navigation model)
        {
            if (ModelState.IsValid)
            {
                db.Navigations.Add(model);
                db.SaveChanges();
                ViewBag.StartupScript = "create_success();";
                return View(model);
            }
            return View(model);
        }

        //
        // GET: /Admin/Navigation/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Navigation model = db.Navigations.Find(id);


            if (model == null)
            {
                return HttpNotFound();
            }




            return View(model);
        }

        //
        // POST: /Admin/Navigation/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Navigation model)
        {
            if (ModelState.IsValid)
            {

                db.Navigations.Attach(model);
                db.Entry(model).Property(a => a.Title).IsModified = true;
                db.Entry(model).Property(a => a.Description).IsModified = true;
                db.Entry(model).Property(a => a.Order).IsModified = true;
                db.Entry(model).Property(a => a.Key).IsModified = true;
                db.SaveChanges();

                ViewBag.StartupScript = "edit_success();";
                return View(model);
            }
            return View(model);
        }



        public ActionResult Delete(int id)
        {
            var navi = db.Set<Navigation>().Find(id);
            if (navi == null)
            {
                return HttpNotFound();
            }
            return View("Delete", navi);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Navigation model)
        {

            try
            {

                var navi = db.Navigations.Attach(model);
                db.Set<Navigation>().Remove(navi);
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

            var temp = from p in db.Set<Navigation>()
                       where lstSiteID.Contains(p.ID)
                       select p;

            return View(temp.ToList());

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<Navigation> model)
        {

            var temp = new List<Navigation>();
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


        public ActionResult ModulesMapping(int id)
        {
            var user = db.Navigations.Find(id);
            if (user == null)
                return HttpNotFound();
            var temp = user.ModuleNavigations.Where(
                x=>
                      (x.WebModule.Culture == null ||
                             (!string.IsNullOrEmpty(x.WebModule.Culture) && x.WebModule.Culture.Equals(ApplicationService.Culture)))
                             || (ApplicationService.Culture == null)

                ).Select(x => x.WebModule).Select(x => new { ID = x.ID }).ToArray();

            string[] accesses = temp.Count() > 0 ? new string[temp.Count()] : new string[0];
            for (int i = 0; i < temp.Count(); i++)
                accesses[i] = temp[i].ID.ToString();
            ViewBag.Tree = GetModuleNavigationsTree();
            ViewBag.ID = id;
            return View(accesses);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ModulesMapping(int id, string[] checkedNodes)
        {
            var user = db.Navigations.Find(id);
            if (user == null)
                return HttpNotFound();
            List<int> lstSiteID = new List<int>();
            try
            {
                using (var ts = new TransactionScope())
                {
                     
                    if (checkedNodes != null && checkedNodes.Count() > 0)
                        foreach (var x in checkedNodes)
                            lstSiteID.Add(int.Parse(x));
                 var clear=   user.ModuleNavigations.Where(
                x =>
                      (x.WebModule.Culture == null ||
                             (!string.IsNullOrEmpty(x.WebModule.Culture) && x.WebModule.Culture.Equals(ApplicationService.Culture)))
                             || (ApplicationService.Culture == null)

                ).AsEnumerable().ToList();
                    foreach(var item in clear)
                    {
                       // var navi = db.ModuleNavigations.Attach(new ModuleNavigation { NavigationID = item.NavigationID, WebModuleID = item.WebModuleID });

                        db.ModuleNavigations.Remove(item);
                    }

                    if (lstSiteID.Count > 0)
                        foreach (var x in lstSiteID)
                            user.ModuleNavigations.Add(new ModuleNavigation() { WebModuleID = x });
                    db.SaveChanges();
                    ts.Complete();
                    ViewBag.StartupScript = "create_success();";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var temp = user.ModuleNavigations.Where(
                x =>
                      (x.WebModule.Culture == null ||
                             (!string.IsNullOrEmpty(x.WebModule.Culture) && x.WebModule.Culture.Equals(ApplicationService.Culture)))
                             || (ApplicationService.Culture == null)

                ).Select(x => x.WebModule).Select(x => new { ID = x.ID }).ToArray();
                string[] accesses = temp.Count() > 0 ? new string[temp.Count()] : new string[0];
                for (int i = 0; i < temp.Count(); i++)
                    accesses[i] = temp[i].ID.ToString();
                ViewBag.Tree = GetModuleNavigationsTree();
                ViewBag.UserId = id;
                return View(accesses);
            }
        }

        //public JsonResult GetModuleNavigations(int? id)
        //{
        //    var adminSites = from e in db.WebModules.AsNoTracking()
        //                     where (id.HasValue ? e.ParentID == id : e.ParentID == null)
        //                     select new
        //                     {
        //                         id = e.ID,
        //                         Name = e.Title,
        //                         hasChildren = e.SubWebModules.Any()
        //                     };
        //    return Json(adminSites, JsonRequestBehavior.AllowGet);
        //}

        private IEnumerable<TreeViewItemModel> GetModuleNavigationsTree()
        {
            List<TreeViewItemModel> tree = new List<TreeViewItemModel>();
            var roots = db.WebModules.AsNoTracking().Where(x => x.ParentID == null      
                      &&
                             (x.Culture==null  ||
                             (!string.IsNullOrEmpty(x.Culture) && x.Culture.Equals(ApplicationService.Culture))) 
                             || (ApplicationService.Culture==null)      

                ).OrderBy(x=>x.Order).AsEnumerable();
            if (roots.Count() > 0)
                foreach (var item in roots)
                {
                    TreeViewItemModel node = new TreeViewItemModel();
                    node.Id = item.ID.ToString();
                    node.Text = item.Title;
                    node.HasChildren = item.SubWebModules.Any();
                    if (node.HasChildren)
                        SubModuleNavigationsTree(ref node, ref tree);
                    tree.Add(node);
                }
            return tree;
        }

        private void SubModuleNavigationsTree(ref TreeViewItemModel parentNode, ref List<TreeViewItemModel> tree)
        {
            int parentID = int.Parse(parentNode.Id);
            var nodes = db.WebModules.AsNoTracking().Where(x => x.ParentID == parentID).OrderBy(x => x.Order).AsEnumerable();
            foreach (var item in nodes)
            {
                TreeViewItemModel node = new TreeViewItemModel();
                node.Id = item.ID.ToString();
                node.Text = item.Title;
                node.HasChildren = item.SubWebModules.Any();
                parentNode.Items.Add(node);
                if (node.HasChildren)
                    SubModuleNavigationsTree(ref node, ref tree);
            }
        }


	}
}