using Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WEB.Models;
using WebMatrix.WebData;
using WebModels;

namespace WEB.Areas.Admin.Controllers
{

    [VanTaiAuthorize]

    public class WebCategoryController : BaseController
    {
        WebContext db = new WebContext();
        int ctype = (int)CTypeCategories.Common;
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        //
        // GET: /Admin/ProductCategories/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Add()
        {
            ViewBag.ListCategory = new SelectList(GetCategories(null), "ID", "Title");
            return View(new WebCategory());
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Add([Bind(Exclude = "ID")]WebCategory model, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                if (model.ParentID == 0) model.ParentID = null;

                model.CreatedBy = WebSecurity.CurrentUserName;
                model.CreatedDate = DateTime.Now;
                model.ModifiedBy = WebSecurity.CurrentUserName;
                model.ModifiedDate = DateTime.Now;

                if (image != null)
                {
                    var name = image.FileName;
                    var newName = Utility.GeneratorFileName(name);
                    var dir = new System.IO.DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath("/content/uploads/auto/"));
                    if (!dir.Exists) dir.Create();
                    var fullpath = "/content/uploads/auto/" + newName;
                    image.SaveAs(System.Web.HttpContext.Current.Server.MapPath(fullpath));
                    try
                    {
                        if (ImageTools.ValidateImage(System.Web.HttpContext.Current.Server.MapPath(fullpath)))
                        {
                            var result = ImageTools.ResizeImage(System.Web.HttpContext.Current.Server.MapPath(fullpath), System.Web.HttpContext.Current.Server.MapPath(fullpath), 500, 500, true, 80);
                        }
                        else
                        {
                            Utility.DeleteFile(fullpath);
                        }
                    }
                    catch (Exception)
                    { }

                    model.Image = fullpath;
                }

                if (string.IsNullOrEmpty(model.MetaTitle))
                {
                    model.MetaTitle = model.Title.UnsignNormalize();
                }
                model.CType = ctype;
                db.WebCategories.Add(model);
                db.SaveChanges();
                ViewBag.StartupScript = "create_success();";
                return View(model);
            }
            ViewBag.ListCategory = new SelectList(GetCategories(null), "ID", "Title");
            return View(model);
        }

        public JsonResult GetByParent(int? id)
        {
            var webmodules = from e in db.WebCategories
                             where (id.HasValue ? e.ParentID == id : e.ParentID == null) && e.CType == ctype
                             orderby e.Order
                             select new
                             {
                                 ID = e.ID,
                                 Title = e.Title,
                                 HasChildren = e.SubWebCategories.Any()
                             };
            return Json(webmodules, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetParentID(int id)
        {
            var lstint = new List<int>();
            var temp = db.WebCategories.Find(id);
            if (temp.ParentID != null)
            {
                lstint.Add(temp.ParentID.Value);
                var temp2 = db.WebCategories.Find(temp.ParentID.Value);
                if (temp2.ParentID != null)
                {
                    lstint.Add(temp2.ParentID.Value);
                    var temp3 = db.WebCategories.Find(temp2.ParentID.Value);
                    if (temp3.ParentID != null)
                    {
                        lstint.Add(temp3.ParentID.Value);
                        var temp4 = db.WebCategories.Find(temp3.ParentID.Value);
                        if (temp4.ParentID != null)
                            lstint.Add(temp4.ParentID.Value);
                    }
                }
            }
            return Json(lstint, JsonRequestBehavior.AllowGet);
        }

        private List<WebCategory> GetCategories(int? excludeID)
        {

            var root = new WebCategory() { ID = 0, Title = "ROOT" };
            var lstCategory = new List<WebCategory>();
            lstCategory.Add(root);
            var allItems = excludeID.HasValue ? db.WebCategories.AsNoTracking().Where(x => x.ID != excludeID && x.CType == ctype).AsEnumerable() : db.WebCategories.AsNoTracking().Where(x => x.CType == ctype).AsEnumerable();
            var roots = allItems.Where(x => x.ParentID == null).OrderBy(x => x.Order).AsEnumerable();
            int level = 0;
            foreach (var item in roots)
            {
                lstCategory.Add(item);
                var subs = allItems.Where(x => x.ParentID == item.ID);
                if (subs.Count() > 0)
                    SubCategories(ref allItems, subs, level + 1, ref lstCategory);
            }
            return lstCategory;
        }

        private void SubCategories(ref IEnumerable<WebCategory> allItems, IEnumerable<WebCategory> subCategories, int level, ref List<WebCategory> lstCategory)
        {
            string levelText = "";
            for (int i = 0; i < level; i++)
                levelText += "- - ";
            foreach (var item in subCategories)
            {
                item.Title = levelText + item.Title;
                lstCategory.Add(item);
                var subSubs = allItems.Where(x => x.ParentID == item.ID);
                if (subSubs.Count() > 0)
                    SubCategories(ref allItems, subSubs, level + 1, ref lstCategory);
            }
        }

        public ActionResult Edit(int id)
        {
            var model = db.WebCategories.Find(id);
            if (model == null)
                return HttpNotFound();
            ViewBag.ListCategory = new SelectList(GetCategories(id), "ID", "Title", new { ID = model.ParentID });
            ViewBag.PrevParentID = model.ParentID ?? null;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(WebCategory model, int? prevParentID, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                if (model.ParentID == 0)
                    model.ParentID = null;
                else if (!ValidateParentCategory(model.ParentID, model.ID))
                {
                    ModelState.AddModelError("", "Danh mục cha không được nằm trong danh mục con hoặc chính nó.");
                    ViewBag.ListCategory = new SelectList(GetCategories(model.ID), "ID", "Title", new { ID = prevParentID ?? null });
                    ViewBag.PrevParentID = prevParentID ?? null;
                    return View(model);
                }
                model.ModifiedBy = WebSecurity.CurrentUserName;
                model.ModifiedDate = DateTime.Now;

                if (image != null)
                {
                    var name = image.FileName;
                    var newName = Utility.GeneratorFileName(name);
                    var dir = new System.IO.DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath("/content/uploads/auto/"));
                    if (!dir.Exists) dir.Create();
                    var fullpath = "/content/uploads/auto/" + newName;
                    image.SaveAs(System.Web.HttpContext.Current.Server.MapPath(fullpath));
                    try
                    {
                        if (ImageTools.ValidateImage(System.Web.HttpContext.Current.Server.MapPath(fullpath)))
                        {
                            var result = ImageTools.ResizeImage(System.Web.HttpContext.Current.Server.MapPath(fullpath), System.Web.HttpContext.Current.Server.MapPath(fullpath), 500, 500, true, 80);
                        }
                        else
                        {
                            Utility.DeleteFile(fullpath);
                        }
                    }
                    catch (Exception)
                    { }

                    model.Image = fullpath;
                }

                db.WebCategories.Attach(model);
                db.Entry(model).Property(x => x.Title).IsModified = true;
                db.Entry(model).Property(x => x.MetaTitle).IsModified = true;
                db.Entry(model).Property(x => x.Description).IsModified = true;
                db.Entry(model).Property(x => x.Status).IsModified = true;
                db.Entry(model).Property(x => x.Order).IsModified = true;
                db.Entry(model).Property(x => x.MetaDescription).IsModified = true;
                db.Entry(model).Property(x => x.MetaKeywords).IsModified = true;
                db.Entry(model).Property(x => x.Image).IsModified = true;
                db.Entry(model).Property(x => x.Body).IsModified = true;
                db.Entry(model).Property(x => x.ParentID).IsModified = true;
                db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                db.Entry(model).Property(a => a.ModifiedDate).IsModified = true;
                db.SaveChanges();
                ViewBag.StartupScript = "edit_success();";
                return View(model);
            }
            ViewBag.ListCategory = new SelectList(GetCategories(model.ID), "ID", "Title", new { ID = model.ParentID });
            return View(model);
        }

        private bool ValidateParentCategory(int? parentID, int id)
        {
            if (!parentID.HasValue)
                return true;
            if (parentID == id)
                return false;
            var roots = db.WebCategories.AsNoTracking().Where(x => x.ParentID == id && x.CType == ctype).AsEnumerable();
            List<int> lstChild = new List<int>();
            foreach (var item in roots)
            {
                lstChild.Add(item.ID);
                var subs = item.SubWebCategories.AsEnumerable();
                if (subs.Count() > 0)
                    SubValidateParentCategory(ref subs, ref lstChild);
            }
            return !lstChild.Any(x => x == parentID);
        }

        private void SubValidateParentCategory(ref IEnumerable<WebCategory> subCategories, ref List<int> lstChild)
        {
            foreach (var item in subCategories)
            {
                lstChild.Add(item.ID);
                var subSubs = item.SubWebCategories.AsEnumerable();
                if (subSubs.Count() > 0)
                    SubValidateParentCategory(ref subSubs, ref lstChild);
            }
        }

        public ActionResult Delete(int id)
        {
            var model = db.WebCategories.Find(id);
            if (model == null)
                return HttpNotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(WebCategory model)
        {
            if (ModelState.IsValid)
            {
                var item = db.WebCategories.Find(model.ID);
                if (item.SubWebCategories.Count > 0)
                {
                    ModelState.AddModelError("", String.Format("Không thể xóa danh mục [{0}] vì danh mục này có chứa các danh mục con", model.Title));
                    return View(model);
                }
                db.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                db.SaveChanges();
                ViewBag.StartupScript = "delete_success();";
                return View();
            }
            return View(model);
        }



        [HttpGet]
        public JsonResult TreeTable(int? s)
        {
            var allItems = from x in db.WebCategories where x.CType == ctype orderby x.Order select x;
            var roots = from x in allItems where x.ParentID == null select x;
            var jArray = new JArray();
            foreach (var i in roots)
            {
                if (s.HasValue && i.ID == s) continue;
                var subs = from x in allItems where x.ParentID == i.ID select x;
                var hasChild = false;
                if (subs.Count() > 0)
                    hasChild = true;
                jArray.Add(new JObject(
                    new JProperty("ID", i.ID),
                    new JProperty("Level", 0), new JProperty("Order", 0),
                    new JProperty("Title", i.Title),
                    new JProperty("MetaTitle", i.MetaTitle),
                    new JProperty("MetaDescription", i.MetaDescription),
                    new JProperty("Parent", null),
                    new JProperty("HasChild", hasChild)));
                if (subs.Count() > 0)
                    SubTreeTable(ref jArray, subs, allItems, 1, s);
            }
            return Json(new { json = jArray.ToString() }, JsonRequestBehavior.AllowGet);
        }

        [NonAction]
        private JArray SubTreeTable(ref JArray jArray, IEnumerable<WebCategory> subs, IEnumerable<WebCategory> allItems, int level, int? select)
        {
            foreach (var i in subs)
            {
                if (select.HasValue && i.ID == select)
                    continue;
                var subSubs = from x in allItems where x.ParentID == i.ID orderby x.Order select x;
                var hasChild = false;
                if (subSubs.ToList().Count() > 0)
                    hasChild = true;
                jArray.Add(new JObject(
                    new JProperty("ID", i.ID),
                    new JProperty("Level", level), new JProperty("Order", 0),
                    new JProperty("Title", i.Title),
                    new JProperty("MetaTitle", i.MetaTitle),
                    new JProperty("MetaDescription", i.MetaDescription),
                    new JProperty("Parent", i.ParentID.Value),
                    new JProperty("HasChild", hasChild)));
                if (subSubs.Count() > 0)
                    SubTreeTable(ref jArray, subSubs, allItems, level + 1, select);
            }
            return jArray;
        }

    }
}