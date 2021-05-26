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
using System.IO;

namespace WEB.Areas.ContentType.Controllers
{
    public partial class ProductController : BaseController
    {
        WebContext db = new WebContext();

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        protected override void Initialize(RequestContext requestContext)
        {

            base.Initialize(requestContext);
        }

        [ChildActionOnly]
        public ActionResult _Index(int id)
        {
            ViewBag.ID = id;
            return PartialView();
        }

        public ActionResult WebContent_Read([DataSourceRequest] DataSourceRequest request, int id)
        {

            var contents = db.WebContents.Where(x => x.WebModuleID == id).Select(x => new { x.ID, x.Title, x.Description, x.Image, x.ModifiedBy, x.ModifiedDate });
            if (request.Sorts.Count == 0)
            {
                request.Sorts.Add(new SortDescriptor("ID", System.ComponentModel.ListSortDirection.Descending));
            }
            return Json(contents.ToDataSourceResult(request));
        }

        public JsonResult GetWebContents(string text, int id)
        {

            {
                var contents = from x in db.WebContents where x.WebModuleID == id select x;
                if (!string.IsNullOrEmpty(text))
                {
                    contents = contents.Where(p => p.Title.Contains(text));
                }

                return Json(contents, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetCity()
        {
            var destinations = (from x in db.Destinations
                                from m in db.WebModules.Where(m => m.ID == x.WebModuleID)
                                select new { x.ID, x.Title }).ToList();

            destinations.Insert(0, new { ID = 0, Title = "-- Chọn Thành phố --", });

            var a = destinations.Count();

            return Json(destinations, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Add(int id)
        {
            ViewBag.UID = UniqueKeyGenerator.RNGTicks(10);
            ViewBag.ID = id;
            var model = new WebContent();
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(WebContent model, HttpPostedFileBase image)
        {
            ViewBag.UID = model.UID;
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                model.CreatedBy = WebSecurity.CurrentUserName;
                model.CreatedDate = DateTime.Now;
                model.ModifiedBy = WebSecurity.CurrentUserName;
                model.ModifiedDate = DateTime.Now;

                if (image != null)
                {
                    model.Image = image.ImageSave("/uploads/image/" + (now.Month.ToString("00") + now.Year), 1000, 1000);
                    db.WebContentUploads.Add(new WebContentUpload()
                    {
                        Title = image.FileName,
                        MetaTitle = image.FileName.UnsignNormalize(),
                        FullPath = model.Image,
                        UID = model.UID,
                        MimeType = ApplicationService.GetMimeType(Path.GetExtension(image.FileName)),
                        CreatedBy = WebSecurity.CurrentUserName,
                        CreatedDate = DateTime.Now
                    });
                    db.SaveChanges();
                }
                if (string.IsNullOrEmpty(model.MetaTitle))
                {
                    model.MetaTitle = model.Title.UnsignNormalize();
                }

                db.Set<WebContent>().Add(model);

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
            var model = db.Set<WebContent>().Find(id);
            List<string> result = new List<string>();
            //var productInfo = db.Set<ProductInfo>().Where(x => x.ID == id).FirstOrDefault();
            //model.ProductInfo = db.ProductInfos.Where(x => x.ID == id).FirstOrDefault();

            if (model == null)
            {
                return HttpNotFound();
            }

            if (model.ProductInfo == null)
            {
                model.ProductInfo = new ProductInfo();
            }

            ViewBag.UID = model.UID;
            return View("Edit", model);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(WebContent model, HttpPostedFileBase image)
        {
            ViewBag.UID = model.UID;
            if (ModelState.IsValid)
            {
                try
                {
                    var now = DateTime.Now;
                    model.ModifiedBy = WebSecurity.CurrentUserName;
                    model.ModifiedDate = DateTime.Now;
                    if (image != null)
                    {
                        model.Image = image.ImageSave("/uploads/image/" + (now.Month.ToString("00") + now.Year), 1000, 1000);
                        db.WebContentUploads.Add(new WebContentUpload()
                        {
                            Title = image.FileName,
                            MetaTitle = image.FileName.UnsignNormalize(),
                            FullPath = model.Image,
                            UID = model.UID,
                            MimeType = ApplicationService.GetMimeType(Path.GetExtension(image.FileName)),
                            CreatedBy = WebSecurity.CurrentUserName,
                            CreatedDate = DateTime.Now
                        });
                        db.SaveChanges();
                    }


                    var temp = db.ProductInfos.Where(x => x.ID == model.ID);
                    if (temp.Count() == 0)
                    {
                        db.Set<ProductInfo>().Add(new ProductInfo() { ID = model.ID });
                        db.SaveChanges();
                    }
                    else
                    {
                        model.ProductInfo.ID = model.ID;
                    }
                    
                    db.WebContents.Attach(model);

                    db.Entry(model).Property(a => a.Title).IsModified = true;
                    db.Entry(model).Property(a => a.Description).IsModified = true;
                    db.Entry(model).Property(a => a.MetaDescription).IsModified = true;
                    db.Entry(model).Property(a => a.MetaKeywords).IsModified = true;
                    db.Entry(model).Property(a => a.Image).IsModified = true;
                    db.Entry(model).Property(a => a.MetaTitle).IsModified = true;
                    db.Entry(model).Property(a => a.Status).IsModified = true;
                    db.Entry(model).Property(a => a.Body).IsModified = true;
                    db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                    db.Entry(model).Property(a => a.Order).IsModified = true;
                    db.Entry(model).Property(a => a.ModifiedDate).IsModified = true;
                    db.Entry(model).Property(a => a.SeoTitle).IsModified = true;

                    db.Entry(model.ProductInfo).Property(a => a.Price).IsModified = true;
                    db.Entry(model.ProductInfo).Property(a => a.Code).IsModified = true;

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

        public ActionResult UpdatePictures(int id)
        {
            var item = db.WebContents.Find(id);
            if (item == null)
                return HttpNotFound();
            ViewBag.UID = item.UID;
            return View(item);
        }

        [AllowAnonymous]
        public ActionResult _SlideContent(int id)
        {
            var lstPhoto = db.ContentImages.Where(x => x.WebContentID == id).ToList();
            return PartialView(lstPhoto);
        }
        public ActionResult Delete(int id)
        {
            {
                var model = db.Set<WebContent>().Find(id);
                if (model == null)
                {
                    return HttpNotFound();
                }
                return View("Delete", model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(WebContent model)
        {
            {

                try
                {
                    var deletes = db.ContentRelateds.Where(x => x.MainID == model.ID || x.RelatedID == model.ID).ToList();

                    for (int i = deletes.Count - 1; i >= 0; i--)
                    {
                        db.Entry(deletes[i]).State = EntityState.Deleted;
                        db.SaveChanges();
                    }

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

            {
                var temp = from p in db.Set<WebContent>()
                           where lstObjId.Contains(p.ID)
                           select p;

                return View(temp.ToList());
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deletes(List<WebContent> model)
        {

            {
                var temp = new List<WebContent>();
                foreach (var item in model)
                {
                    try
                    {
                        var deletes = db.ContentRelateds.Where(x => x.MainID == item.ID || x.RelatedID == item.ID).ToList();
                        for (int i = deletes.Count - 1; i >= 0; i--)
                        {
                            db.Entry(deletes[i]).State = EntityState.Deleted;
                            db.SaveChanges();
                        }
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
        [ChildActionOnly]
        [AllowAnonymous]
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

            contents = db.WebContents.Where(x => x.WebModuleID == id && x.Status.HasValue && x.Status.Value.Equals((int)Status.Public)).ToList();
          
            var sub = webmodule.SubWebModules.Where(x => x.Status == (int)Status.Public);
            if (sub.Any())
            {
                foreach (var item in sub)
                {
                    var lstContentSub = db.WebContents.Where(x => x.WebModuleID == item.ID && x.Status.HasValue && x.Status.Value.Equals((int)Status.Public)).ToList();
                    if (lstContentSub.Any())
                    {
                        contents.AddRange(lstContentSub);
                    }
                }
            }
            contents = contents.OrderBy(x => x.Order).ToList();
            var ipage = 1; if (page != null) ipage = page.Value;
            ViewBag.TotalItemCount = contents.Count();
            ViewBag.CurrentPage = ipage;
            return PartialView(contents.Skip((ipage - 1) * 10).Take(10).ToList());
        }

        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubIndexOutBoud(int id, string metatitle, int? page)
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

            return PartialView(contents);
        }

        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubDetail(int id)
        {
            var content = db.Set<WebContent>().Find(id);
            return PartialView(content);
        }

        private List<WebContent> GetListContents(int webModuleId, List<WebContent> results)
        {
            var webContents = db.WebContents.Where(x => x.WebModuleID == webModuleId && x.Status.HasValue && x.Status.Value.Equals((int)Status.Public));
            results.AddRange(webContents);

            var childWebModules = db.WebModules.Where(x => x.ParentID == webModuleId);

            foreach (var childWebModule in childWebModules)
            {
                GetListContents(childWebModule.ID, results);
            }

            return results;
        }
        [AllowAnonymous]
        public ActionResult _OtherTour(int id)
        {
            int sID = 0;

            if (id == 2)
            {
                sID = 3;
            }
            else
            {
                sID = 2;
            }

            List<WebContent> lstContent = new List<WebContent>();

            var item = db.WebModules.Single(x => x.ID == sID);

            ViewBag.WebModule = item;
            var contents = GetListContents(sID, lstContent);

            return PartialView(contents.Take(5).OrderByDescending(x => x.ID).ToList());
        }


        [AllowAnonymous]
        public ActionResult _OtherTourDetail(int id)
        {
            var item = db.WebContents.Single(x => x.ID == id);
            ViewBag.WebModule = item.WebModule;
            var contents = db.WebContents.Where(x => x.Status == (int)Status.Public && x.ID > id && x.WebModuleID == item.WebModuleID).Take(2).ToList();
            var contents2 = db.WebContents.Where(x => x.Status == (int)Status.Public && x.ID < id && x.WebModuleID == item.WebModuleID).OrderByDescending(x => x.CreatedDate).Take(3).ToList();
            contents.AddRange(contents2);
            return PartialView(contents.OrderByDescending(x => x.ID));
        }

          public JsonResult GetDestinations()
        {
            var destinations = db.WebModules.Where(x => x.ParentID == 2 || x.ParentID == 3).Select(x => new { x.ID, x.Title }).ToList();

            return Json(destinations, JsonRequestBehavior.AllowGet);
        }
        public ActionResult _ColNav(int id)
        {
            var module = db.WebModules.Where(x => x.ID == id).FirstOrDefault();

            var lstWebModule = new List<WebModule>();

            if (module.ParentID == null)
            {
                lstWebModule = db.WebModules.Where(x => x.ParentID == id).ToList();

            }
            else
            {
                lstWebModule = db.WebModules.Where(x => x.ParentID == module.ParentID).ToList();
            }

            var moduleactive = db.WebModules.Where(x => x.ID == id).FirstOrDefault();

            ViewBag.ModuleActive = moduleactive;

            //var contents = db.WebModules.Where(x => x.Parent.UID.Equals("list-tour") && x.Status == (int)Status.Public && x.Culture.Equals(ApplicationService.Culture)).ToList();

            return PartialView(lstWebModule);
        }

    }
}
