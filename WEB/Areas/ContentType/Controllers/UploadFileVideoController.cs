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
    public partial class UploadFileVideoController : BaseController
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

            var contents = db.WebContents.Where(x => x.WebModuleID == id).ToList().Select(x => new { x.ID, x.Title, x.Link, x.ModifiedBy, x.ModifiedDate, x.StatusText });
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
        public ActionResult Add(WebContent model, HttpPostedFileBase file, HttpPostedFileBase image)
        {
            ViewBag.ID = model.WebModuleID;
            ViewBag.UID = model.UID;
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                model.CreatedBy = WebSecurity.CurrentUserName;
                model.CreatedDate = DateTime.Now;
                model.ModifiedBy = WebSecurity.CurrentUserName;
                model.ModifiedDate = DateTime.Now;
                model.Status = (int)StatusEnum.Internal;
                model.CountView = 1;

                if (file != null && file.ContentLength > 0)
                {
                    model.Link = file.FileSave(0, "/uploads/file/" + (now.Month.ToString("00") + now.Year));
                    db.WebContentUploads.Add(new WebContentUpload()
                    {
                        Title = file.FileName,
                        MetaTitle = file.FileName.UnsignNormalize(),
                        FullPath = model.Link,
                        UID = model.UID,
                        MimeType = ApplicationService.GetMimeType(Path.GetExtension(file.FileName)),
                        CreatedBy = WebSecurity.CurrentUserName,
                        CreatedDate = DateTime.Now
                    });
                    db.SaveChanges();
                }


                if (image != null)
                {
                    model.Image = image.ImageSave("/uploads/image/" + (now.Month.ToString("00") + now.Year), 700, 700);
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
            if (model == null)
            {
                return HttpNotFound();
            }
            ViewBag.UID = model.UID;
            return View("Edit", model);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(WebContent model, HttpPostedFileBase file, HttpPostedFileBase image)
        {
            ViewBag.UID = model.UID;
            if (ModelState.IsValid)
            {
                {
                    try
                    {
                        var now = DateTime.Now;
                        model.ModifiedBy = WebSecurity.CurrentUserName;
                        model.ModifiedDate = DateTime.Now;


                        if (file != null && file.ContentLength > 0)
                        {
                            model.Link = file.FileSave(0, "/uploads/video/" + (now.Month.ToString("00") + now.Year));

                            db.WebContentUploads.Add(new WebContentUpload()
                            {
                                Title = file.FileName,
                                MetaTitle = file.FileName.UnsignNormalize(),
                                FullPath = model.Link,
                                UID = model.UID,
                                MimeType = ApplicationService.GetMimeType(Path.GetExtension(file.FileName)),
                                CreatedBy = WebSecurity.CurrentUserName,
                                CreatedDate = DateTime.Now
                            });
                            db.SaveChanges();
                        }

                        if (image != null)
                        {
                            model.Image = image.ImageSave("/uploads/image/" + (now.Month.ToString("00") + now.Year), 700, 700);
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

                        db.WebContents.Attach(model);
                        db.Entry(model).Property(a => a.Title).IsModified = true;
                        db.Entry(model).Property(a => a.Link).IsModified = true;
                        db.Entry(model).Property(a => a.Image).IsModified = true;
                        db.Entry(model).Property(a => a.Status).IsModified = true;
                        db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                        db.Entry(model).Property(a => a.ModifiedDate).IsModified = true;
                        db.Entry(model).Property(a => a.CountView).IsModified = true;

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
            }
            else
            {
                return View(model);
            }
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
        //List new
        public ActionResult _PubIndexVideo(int id, string metatitle, int? page)
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
            contents = contents.OrderByDescending(x => x.CreatedDate).ToList();
            var ipage = 1; if (page != null) ipage = page.Value;
            ViewBag.TotalItemCount = contents.Count();
            ViewBag.CurrentPage = ipage;
            return PartialView(contents.Skip((ipage - 1) * ApplicationService.PageSize).Take(ApplicationService.PageSize).ToList());

        }
        [ChildActionOnly]
        [AllowAnonymous]
        //List new
        public ActionResult _PubIndexImg(int id, string metatitle, int? page)
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
            contents = contents.OrderByDescending(x => x.CreatedDate).ToList();
            var ipage = 1; if (page != null) ipage = page.Value;
            ViewBag.TotalItemCount = contents.Count();
            ViewBag.CurrentPage = ipage;
            return PartialView(contents.Skip((ipage - 1) * 20).Take(20).ToList());

        }

        public decimal UpdateVideoViewCount(int videoId)
        {
            var videoToUpdate = db.WebContents.FirstOrDefault(x => x.ID == videoId);
            if (videoToUpdate == null)
            {
                return -1;
            }

            videoToUpdate.CountView = (videoToUpdate.CountView ?? 0) + 1;
            db.Entry(videoToUpdate).State = EntityState.Modified;
            db.SaveChanges();

            return videoToUpdate.CountView.Value;
        }
    }
}
