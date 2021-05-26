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
using WEB.Models;
using System.Data.Entity.Validation;

namespace WEB.Areas.ContentType.Controllers
{
    public partial class ContactController
    {
        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubIndex(int id)
        {
            WebModule webmodule = null;
            if (TempData["WebModule"] != null)
            {
                webmodule = TempData["WebModule"] as WebModule;
            }
            else webmodule = db.Set<WebModule>().Find(id);
            ViewBag.WebModule = webmodule;

            ViewBag.ID = id;
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [AllowAnonymous]
        public JsonResult _PubIndex(WebContact model)
        {
            if (ModelState.IsValid)
            {

                model.CreatedDate = DateTime.Now;
                db.Set<WebContact>().Add(model);
                db.SaveChanges();

                bool sendmail = ApplicationService.SendMail(model);

                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = ModelState.ToJson() });
            }
        }

        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubLienHeToaSoan(int id)
        {
            WebModule webmodule = null;
            if (TempData["WebModule"] != null)
            {
                webmodule = TempData["WebModule"] as WebModule;
            }
            else webmodule = db.Set<WebModule>().Find(id);
            ViewBag.WebModule = webmodule;

            ViewBag.ID = id;
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [AllowAnonymous]
        public JsonResult _PubLienHeToaSoan(WebContact model)
        {
            ModelState.Remove("Address");
            ModelState.Remove("NgayBatDau");
            ModelState.Remove("NgayKetThuc");
            ModelState.Remove("LoaiDonHang");
            ModelState.Remove("Mobile");

            if (ModelState.IsValid)
            {
                model.LoaiLienHe = (int)LoaiLienHe.LienHeToaSon;
                model.CreatedDate = DateTime.Now;
                db.Set<WebContact>().Add(model);
                db.SaveChanges();

                bool sendmail = ApplicationService.SendMail(model);

                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = ModelState.ToJson() });
            }
        }

        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubDatMua(int id)
        {
            WebModule webmodule = null;
            if (TempData["WebModule"] != null)
            {
                webmodule = TempData["WebModule"] as WebModule;
            }
            else webmodule = db.Set<WebModule>().Find(id);
            ViewBag.WebModule = webmodule;

            ViewBag.ID = id;
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [AllowAnonymous]
        public JsonResult _PubDatMua(WebContact model)
        {
            if (ModelState.IsValid)
            {
                model.LoaiLienHe = (int)LoaiLienHe.DatMua;
                model.CreatedDate = DateTime.Now;
                db.Set<WebContact>().Add(model);
                db.SaveChanges();

                ApplicationService.SendMailDatMua(model);

                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = ModelState.ToJson() });
            }
        }

        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubThuMoiNghienCuu(int id)
        {
            WebModule webmodule = null;
            if (TempData["WebModule"] != null)
            {
                webmodule = TempData["WebModule"] as WebModule;
            }
            else webmodule = db.Set<WebModule>().Find(id);
            ViewBag.WebModule = webmodule;

            var chuyenMuc = db.WebModules.Where(x => x.Parent.UID.Equals("chuyenmuc")).OrderBy(x=>x.Order).ToList();
            ViewBag.lstChuyenMuc = chuyenMuc;

            ViewBag.ID = id;
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [AllowAnonymous]
        public JsonResult _PubThuMoiNghienCuu(WebContact model, IEnumerable<HttpPostedFileBase> files)
        {
            ModelState.Remove("NgayBatDau");
            ModelState.Remove("NgayKetThuc");
            ModelState.Remove("LoaiDonHang");

            if (ModelState.IsValid)
            {
                model.LoaiLienHe = (int)LoaiLienHe.ThuMoiNghienCuu;
                model.CreatedDate = DateTime.Now;

                var entityToAdd = new WebContact
                {
                    LoaiLienHe = model.LoaiLienHe,
                    CreatedDate = model.CreatedDate,
                    FullName = model.FullName,
                    Body = model.Body,
                    Email = model.Email,
                    Mobile = "N/A",
                    Title = "Đăng ký nghiên cứu",
                    Address = "N/A",
                    NgayBatDau = DateTime.Now,
                    NgayKetThuc = DateTime.Now,
                    WebModuleID = model.WebModuleID
                };

                db.Set<WebContact>().Add(entityToAdd);
                db.SaveChanges();

                var tenChuyenMuc = db.WebModules.Where(x => x.ID == model.WebModuleID).FirstOrDefault().Title;
                ApplicationService.SendMailDangKyNghienCuu(model,files, tenChuyenMuc);

                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, Error = ModelState.ToJson() });
            }
        }

        [ChildActionOnly]
        [AllowAnonymous]
        public ActionResult _PubYKienCuaBan(int id, string webContentTitle)
        {
            WebModule webmodule = null;
            if (TempData["WebModule"] != null)
            {
                webmodule = TempData["WebModule"] as WebModule;
            }
            else webmodule = db.Set<WebModule>().Find(id);
            ViewBag.WebModule = webmodule;
            ViewBag.WebContentTitle = webContentTitle;

            ViewBag.ID = id;
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [AllowAnonymous]
        public JsonResult _PubYKienCuaBan(WebContact model, string webContentTitle)
        {
            ModelState.Remove("Address");
            ModelState.Remove("NgayBatDau");
            ModelState.Remove("NgayKetThuc");
            ModelState.Remove("LoaiDonHang");
            ModelState.Remove("Mobile");

            if (ModelState.IsValid)
            {
                try
                {
                    model.LoaiLienHe = (int)LoaiLienHe.YKienCuaBan;
                    model.CreatedDate = DateTime.Now;
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.Set<WebContact>().Add(model);
                    db.SaveChanges();
                    ApplicationService.SendMailYKienBanDoc(model, webContentTitle);
                    return Json(new { Success = true });
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }

                    return Json(new { Success = false });
                }
            }
            else
            {
                return Json(new { Success = false, Error = ModelState.ToJson() });
            }
        }


    }
}
