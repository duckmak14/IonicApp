using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Web;
using System.Web.Mvc;
using WebModels;
using System.Collections;
using Kendo.Mvc;
using Kendo.Mvc.Infrastructure;
using System.Linq.Expressions;
using System.ComponentModel;
using System.IO;
using Common;
using WebMatrix.WebData;
using System.Web.Security;
using System.Data;
using System.Transactions;
using WEB.Models;

namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public class UserController : BaseController
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

        public ActionResult Users_Read([DataSourceRequest] DataSourceRequest request)
        {
            var users = from x in db.UserProfiles.AsNoTracking() select new { x.UserId, x.UserName, x.Avatar, x.Email, x.FullName, x.Mobile };
            return Json(users.ToDataSourceResult(request));
        }
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add([Bind(Exclude = "")] RegisterModel model, string[] roles, HttpPostedFileBase image)
        {
            ModelState.Remove("UserProfile.UserName");
            model.UserProfile.UserName = model.UserName;
            if (ModelState.IsValid)
            {

                var temp = (from p in db.Set<UserProfile>().AsNoTracking()
                            where p.UserName.Equals(model.UserName, StringComparison.OrdinalIgnoreCase)
                            select p).FirstOrDefault();
                if (temp != null)
                {
                    ModelState.AddModelError("", AccountResources.UserNameExists);
                    return View(model);
                }
                else
                {
                    if (image != null)
                    {
                        var name = image.FileName;
                        string extension = Path.GetExtension(name);
                        var newName = model.UserName + extension;
                        var dir = new System.IO.DirectoryInfo(Server.MapPath("/content/uploads/avatars/"));
                        if (!dir.Exists) dir.Create();
                        var uri = "/content/uploads/avatars/" + newName;
                        image.SaveAs(HttpContext.Server.MapPath(uri));
                        try
                        {
                            if (ImageTools.ValidateImage(System.Web.HttpContext.Current.Server.MapPath(uri)))
                            {
                                var result = ImageTools.ResizeImage(Server.MapPath(uri), Server.MapPath(uri), 240, 240, true, 80);
                                model.UserProfile.Avatar = uri;
                            }
                            else
                            {
                                Utility.DeleteFile(uri);
                            }
                        }
                        catch (Exception)
                        { }
                    }
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password, new
                    {
                        model.UserProfile.FullName,
                        model.UserProfile.Email,
                        model.UserProfile.Mobile,
                        model.UserProfile.Avatar,
                        model.UserProfile.Position
                    });

                    try
                    {
                        Roles.AddUserToRoles(model.UserName, roles);
                    }
                    catch (Exception)
                    { }
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

            var user = db.Set<UserProfile>().Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.cUserName = user.UserName;
            ViewBag.Roles = Roles.GetRolesForUser(user.UserName);
            return View("Edit", user);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "")] UserProfile model, string cUserName, string[] roles, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Roles = roles;

                var temp = (from p in db.Set<UserProfile>().AsNoTracking()
                            where p.UserName.Equals(model.UserName, StringComparison.OrdinalIgnoreCase) && !p.UserName.Equals(cUserName, StringComparison.OrdinalIgnoreCase)
                            select p).FirstOrDefault();
                if (temp != null)
                {
                    ModelState.AddModelError("", AccountResources.UserNameExists);
                    return View(model);
                }
                else
                {
                    var delavatar = false;
                    if (image != null)
                    {
                        var name = image.FileName;
                        string extension = Path.GetExtension(name);
                        var newName = model.UserName + extension;
                        var dir = new System.IO.DirectoryInfo(Server.MapPath("/content/uploads/avatars/"));
                        if (!dir.Exists) dir.Create();
                        var uri = "/content/uploads/avatars/" + newName;
                        image.SaveAs(HttpContext.Server.MapPath(uri));
                        try
                        {
                            if (ImageTools.ValidateImage(System.Web.HttpContext.Current.Server.MapPath(uri)))
                            {
                                var result = ImageTools.ResizeImage(Server.MapPath(uri), Server.MapPath(uri), 240, 240, true, 80);
                                model.Avatar = uri;
                            }
                            else
                            {
                                Utility.DeleteFile(uri);
                                model.Avatar = null;
                            }
                        }
                        catch (Exception)
                        { }
                    }
                    else
                    {
                        if (model.Avatar == null)
                        {
                            delavatar = true;
                        }
                    }

                    db.UserProfiles.Attach(model);
                    db.Entry(model).Property(a => a.UserName).IsModified = false;
                    db.Entry(model).Property(a => a.FullName).IsModified = true;
                    db.Entry(model).Property(a => a.Email).IsModified = true;
                    db.Entry(model).Property(a => a.Mobile).IsModified = true;
                    db.Entry(model).Property(a => a.Avatar).IsModified = (model.Avatar != null) || delavatar;
                    db.Entry(model).Property(a => a.Position).IsModified = true;

                    db.SaveChanges();
                    try
                    {
                        foreach (var role in Roles.GetRolesForUser(model.UserName))
                        {
                            Roles.RemoveUserFromRole(model.UserName, role);
                        }
                        Roles.AddUserToRoles(model.UserName, roles);
                    }
                    catch (Exception)
                    { }
                    ViewBag.StartupScript = "create_success();";
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
            var user = db.Set<UserProfile>().Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View("Delete", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(UserProfile model)
        {
            try
            {
                ((SimpleMembershipProvider)Membership.Provider).DeleteUser(model.UserName, true);
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
            var lstUserId = new List<int>();
            foreach (var obj in objects)
            {
                try
                {
                    lstUserId.Add(int.Parse(obj.ToString()));
                }
                catch (Exception)
                { }
            }
            var temp = from p in db.Set<UserProfile>()
                       where lstUserId.Contains(p.UserId)
                       select p;
            return View(temp.ToList());

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<UserProfile> model)
        {
            var temp = new List<UserProfile>();
            foreach (var item in model)
            {
                try
                {
                    ((SimpleMembershipProvider)Membership.Provider).DeleteUser(item.UserName, true);
                    db.SaveChanges();
                }
                catch (Exception)
                {
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

        public ActionResult ChangePassword(string userName)
        {
            if (!WebSecurity.UserExists(userName))
            {
                return HttpNotFound();
            }

            ViewBag.UserName = userName;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(LocalPasswordModel model, string userName)
        {
            bool changePasswordSucceeded;
            try
            {
                string token = WebSecurity.GeneratePasswordResetToken(userName, 30);
                changePasswordSucceeded = WebSecurity.ResetPassword(token, model.NewPassword);
            }
            catch (Exception)
            {
                changePasswordSucceeded = false;
            }

            if (changePasswordSucceeded)
            {
                ViewBag.StartupScript = "change_success();";
            }
            else
            {
                ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
            }

            return View();
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
    }
}
