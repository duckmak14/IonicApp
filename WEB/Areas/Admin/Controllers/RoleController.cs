using Common;
using System;
using System.Collections.Generic;
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
using System.Data.Entity;
using System.Transactions;
using WEB.Models;

namespace WEB.Areas.Admin.Controllers
{
    [VanTaiAuthorize]
    public class RoleController : BaseController
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

        [AllowAnonymous]
        public ActionResult Roles_Read([DataSourceRequest] DataSourceRequest request)
        {
            var role = db.WebRoles.AsNoTracking().Select(x => new { x.RoleId, x.RoleName, x.Title, x.Description });
            return Json(role.ToDataSourceResult(request));
        }

        [AllowAnonymous]
        public JsonResult GetRoles(string text)
        {
            var roles = from x in db.WebRoles.AsNoTracking() select x;
            if (!string.IsNullOrEmpty(text))
            {
                roles = roles.Where(p => p.RoleName.Contains(text));
            }

            return Json(roles.Select(x => new
            {
                RoleId = x.RoleId,
                RoleName = x.RoleName
            }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add([Bind(Exclude = "RoleId")] WebRole model)
        {
            if (ModelState.IsValid)
            {


                var temp = (from p in db.Set<WebRole>().AsNoTracking()
                            where p.RoleName.Equals(model.RoleName, StringComparison.OrdinalIgnoreCase)
                            select p).FirstOrDefault();
                if (temp != null)
                {
                    ModelState.AddModelError("", AccountResources.RoleExists);
                    return View(model);
                }
                else
                {
                    db.Set<WebRole>().Add(model);
                    db.SaveChanges();

                    (new WebModels.AccessLog("Entity: Role, Item: " + model.RoleId + ": " + model.RoleName, WebModels.AccessLogActions.Add.ToString(), WebSecurity.CurrentUserId + ":" + WebSecurity.CurrentUserName)).Insert();

                    ViewBag.StartupScript = "create_success();";
                    return View(model);
                }

            }
            else
            {
                return View(model);
            }
        }

        public ActionResult Edit(int id)
        {

            var role = db.Set<WebRole>().Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            ViewBag.cRoleName = role.RoleName;
            return View("Edit", role);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(WebRole model, string cRoleName)
        {
            ViewBag.cRoleName = cRoleName;
            if (ModelState.IsValid)
            {

                try
                {
                    var temp = (from p in db.Set<WebRole>().AsNoTracking()
                                where p.RoleName.Equals(model.RoleName, StringComparison.OrdinalIgnoreCase) && !p.RoleName.Equals(cRoleName, StringComparison.OrdinalIgnoreCase)
                                select p).FirstOrDefault();
                    if (temp != null)
                    {
                        ModelState.AddModelError("", AccountResources.RoleExists);
                        return View(model);
                    }
                    else
                    {
                        var roleupdate = new WebRole { RoleId = model.RoleId };
                        db.WebRoles.Attach(model);
                        db.Entry(model).Property(a => a.RoleId).IsModified = true;
                        db.Entry(model).Property(a => a.Description).IsModified = true;
                        db.Entry(model).Property(a => a.RoleName).IsModified = true;
                        db.Entry(model).Property(a => a.Title).IsModified = true;
                        db.SaveChanges();

                        (new WebModels.AccessLog("Entity: Role, Item: " + model.RoleId + ": " + model.RoleName, WebModels.AccessLogActions.Edit.ToString(), WebSecurity.CurrentUserId + ":" + WebSecurity.CurrentUserName)).Insert();

                        ViewBag.StartupScript = "edit_success();";
                        return View(model);
                    }
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

            var role = db.Set<WebRole>().Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View("Delete", role);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(WebRole model)
        {
            try
            {
                var log = (new WebModels.AccessLog("Entity: Role, Item: " + model.RoleId + ": " + model.RoleName, WebModels.AccessLogActions.Delete.ToString(), WebSecurity.CurrentUserId + ":" + WebSecurity.CurrentUserName));
                var role = db.WebRoles.Attach(model);
                db.Set<WebRole>().Remove(role);
                db.SaveChanges();
                log.Insert();
                ViewBag.StartupScript = "delete_success();";
                return View(model);
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
            var lstRoleId = new List<int>();
            foreach (var obj in objects)
            {
                try
                {
                    lstRoleId.Add(int.Parse(obj.ToString()));
                }
                catch (Exception)
                { }
            }

            var temp = from p in db.Set<WebRole>().AsNoTracking()
                       where lstRoleId.Contains(p.RoleId)
                       select p;

            return View(temp.ToList());

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<WebRole> model)
        {
            var temp = new List<WebRole>();
            foreach (var item in model)
            {
                try
                {
                    db.Entry(item).State = EntityState.Deleted;
                    db.SaveChanges();
                    var log = new WebModels.AccessLog("Entity: Role, Item: " + item.RoleId + ":" + item.RoleName, WebModels.AccessLogActions.Delete.ToString(), WebSecurity.CurrentUserId + ":" + WebSecurity.CurrentUserName);

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
                return View(model);
            }

        }

        #region Module and Site

        public ActionResult AdminSitesMapping(int id)
        {
            var role = db.WebRoles.Find(id);
            if (role == null)
                return HttpNotFound();

            var temp = role.AccessAdminSiteRoles.Select(x => x.AdminSite).Where(x => x != null).Select(y => new { ID = y.ID }).ToArray();
            string[] accesses = temp.Count() > 0 ? new string[temp.Count()] : new string[0];
            for (int i = 0; i < temp.Count(); i++)
                accesses[i] = temp[i].ID.ToString();
            ViewBag.Tree = GetAdminSitesTree();
            ViewBag.RoleId = id;
            return View(accesses);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdminSitesMapping(int id, string[] checkedNodes)
        {
            var role = db.WebRoles.Find(id);
            if (role == null)
                return HttpNotFound();

            List<int> lstSiteID = new List<int>();
            try
            {
                using (var ts = new TransactionScope())
                {
                    if (checkedNodes != null && checkedNodes.Count() > 0)
                        foreach (var x in checkedNodes)
                            lstSiteID.Add(int.Parse(x));
                    role.AccessAdminSiteRoles.Clear();
                    if (lstSiteID.Count > 0)
                        foreach (var x in lstSiteID)
                            role.AccessAdminSiteRoles.Add(new AccessAdminSiteRole() { AdminSiteID = x });

                    db.SaveChanges();
                    ts.Complete();
                    ViewBag.StartupScript = "create_success();";

                    // clear cache
                    var sessionKey = "AdminSite-" + Culture + "-" + HttpContext.User.Identity.Name;
                    Session[sessionKey] = null;

                    return View();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var temp = role.AccessAdminSiteRoles.Select(x => x.AdminSite).Select(x => new { ID = x.ID }).ToArray();
                string[] accesses = temp.Count() > 0 ? new string[temp.Count()] : new string[0];
                for (int i = 0; i < temp.Count(); i++)
                    accesses[i] = temp[i].ID.ToString();
                ViewBag.Tree = GetAdminSitesTree();
                ViewBag.RoleId = id;
                return View(accesses);
            }
        }

        [AllowAnonymous]
        public JsonResult GetAccessAdminSites(int? id)
        {
            var adminSites = from e in db.AdminSites.AsNoTracking()
                             where (id.HasValue ? e.ParentID == id : e.ParentID == null)
                             select new
                             {
                                 id = e.ID,
                                 Name = e.Title,
                                 hasChildren = e.SubAdminSites.Any()
                             };

            return Json(adminSites, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        private IEnumerable<TreeViewItemModel> GetAdminSitesTree()
        {
            List<TreeViewItemModel> tree = new List<TreeViewItemModel>();
            var roots = db.AdminSites.AsNoTracking().Where(x => x.ParentID == null).AsEnumerable();
            if (roots.Count() > 0)
                foreach (var item in roots)
                {
                    TreeViewItemModel node = new TreeViewItemModel();
                    node.Id = item.ID.ToString();
                    node.Text = item.Title;
                    node.HasChildren = item.SubAdminSites.Any();
                    if (node.HasChildren)
                        SubAdminSitesTree(ref node, ref tree);
                    tree.Add(node);
                }
            return tree;
        }

        private void SubAdminSitesTree(ref TreeViewItemModel parentNode, ref List<TreeViewItemModel> tree)
        {
            int parentID = int.Parse(parentNode.Id);
            var nodes = db.AdminSites.AsNoTracking().Where(x => x.ParentID == parentID).AsEnumerable();
            foreach (var item in nodes)
            {
                TreeViewItemModel node = new TreeViewItemModel();
                node.Id = item.ID.ToString();
                node.Text = item.Title;
                node.HasChildren = item.SubAdminSites.Any();
                parentNode.Items.Add(node);
                if (node.HasChildren)
                    SubAdminSitesTree(ref node, ref tree);
            }
        }

        public ActionResult ModulesMapping(int id)
        {
            ViewBag.RoleId = id;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult GetModulesPermissions(int id)
        {
            var role = db.WebRoles.Find(id);
            if (role == null)
                return Json(null);

            var modulesPermissions = role.AccessWebModuleRoles
                .Select(x =>
                    new
                    {
                        WebModuleID = x.WebModuleID,
                        View = x.View,
                        Add = x.Add,
                        Edit = x.Edit,
                        Delete = x.Delete
                    });
            return Json(modulesPermissions, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateModulePerm(AccessWebModuleRole data)
        {
            if (ModelState.IsValid)
            {
                var item = db.AccessWebModuleRoles.AsNoTracking().SingleOrDefault(x => x.RoleId == data.RoleId && x.WebModuleID == data.WebModuleID);
                if (item != null)
                {
                    db.Entry(item).State = EntityState.Detached;
                    db.AccessWebModuleRoles.Attach(data);
                    db.Entry(data).Property(x => x.View).IsModified = true;
                    db.Entry(data).Property(x => x.Add).IsModified = true;
                    db.Entry(data).Property(x => x.Edit).IsModified = true;
                    db.Entry(data).Property(x => x.Delete).IsModified = true;
                    db.SaveChanges();
                }
                else
                {
                    db.AccessWebModuleRoles.Add(data);
                    db.SaveChanges();
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public JsonResult UpdateModulePermAll(AccessWebModuleRole data)
        {
            if (ModelState.IsValid)
            {
                var allWebModules = db.WebModules.ToList();

                foreach (var webModule in allWebModules)
                {
                    var item = db.AccessWebModuleRoles.SingleOrDefault(x => x.RoleId == data.RoleId && x.WebModuleID == webModule.ID);
                    if (item != null)
                    {
                        if (data.CheckAllName == "chkview")
                        {
                            item.View = data.View;
                            db.Entry(item).Property(x => x.View).IsModified = true;
                        }
                        else if (data.CheckAllName == "chkadd")
                        {
                            item.Add = data.Add;
                            db.Entry(item).Property(x => x.Add).IsModified = true;
                        }
                        else if (data.CheckAllName == "chkedit")
                        {
                            item.Edit = data.Edit;
                            db.Entry(item).Property(x => x.Edit).IsModified = true;
                        }
                        else if (data.CheckAllName == "chkdelete")
                        {
                            item.Delete = data.Delete;
                            db.Entry(item).Property(x => x.Delete).IsModified = true;
                        }
                    }
                    else
                    {
                        db.AccessWebModuleRoles.Add(new AccessWebModuleRole
                        {
                            RoleId = data.RoleId,
                            WebModuleID = webModule.ID,
                            View = data.CheckAllName == "chkview" ? data.View : false,
                            Add = data.CheckAllName == "chkadd" ? data.Add : false,
                            Edit = data.CheckAllName == "chkedit" ? data.Edit : false,
                            Delete = data.CheckAllName == "chkdelete" ? data.Delete : false
                        });
                    }
                }

                db.SaveChanges();

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        #endregion
    }
}
