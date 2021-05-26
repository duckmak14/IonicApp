using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using WEB.Areas.ContentType.Controllers;
using WEB.Models;
using WebMatrix.WebData;
using WebModels;

namespace WEB.Areas.Admin.Controllers
{
    [VanTaiAuthorize]
    public class ManagePartnerController : BaseController
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

        public ActionResult Partners_Read([DataSourceRequest] DataSourceRequest request)
        {
            var partners = db.Partner.Select(x => new { x.ID, x.PartnerName, x.Address, x.Email, x.Mobile, x.PartnerCode }).ToList();
            //var partners = from x in db.Partner.AsNoTracking() select new { x.ID, x.Address, x.Email, x.Mobile }.ToList();
            return Json(partners.OrderByDescending(x => x.ID).ToDataSourceResult(request));

        }
        [AllowAnonymous]
        public JsonResult GetPartners(string text)
        {
            var partners = from x in db.Partner.AsNoTracking() select x;
            if (!string.IsNullOrEmpty(text))
            {
                partners = partners.Where(p => p.PartnerName.Contains(text));
            }

            return Json(partners.Select(x => new
            {
                ID = x.ID,
                PartnerName = x.PartnerName
            }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Add()
        {

            var model = new Partner();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add(Partner model)
        {
            if (ModelState.IsValid)
            {
                var temp = (from p in db.Set<Partner>().AsNoTracking()
                            where p.PartnerName.Equals(model.PartnerName, StringComparison.OrdinalIgnoreCase)
                            select p).FirstOrDefault();
                if (temp != null)
                {
                    ModelState.AddModelError("", AccountResources.PartnerNameExists);
                    return View(model);
                }
                else
                {
                    try { 
                    model.CreatedDate = DateTime.Now;
                    var user = db.Set<UserProfile>().Find(WebSecurity.GetUserId(User.Identity.Name));
                    model.CreatedBy = user.UserId;
                    model.ModifiedDate = null;
                    db.Set<Partner>().Add(model);
                    db.SaveChanges();
                    ViewBag.StartupScript = "create_success();";
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

        public ActionResult Edit(int id)
        {
            var model = db.Set<Partner>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            return View("Edit", model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Partner model)
        {
            if (ModelState.IsValid)
            {
                var list = db.Partner.Where(x => x.ID != model.ID).ToList();
                var temp = (from p in list
                            where p.PartnerName.Equals(model.PartnerName, StringComparison.OrdinalIgnoreCase)
                            select p).FirstOrDefault();
                if (temp != null)
                {
                    ModelState.AddModelError("", AccountResources.PartnerNameExists);
                    return View(model);
                }
                else
                {
                    try
                    {
                        var user = db.Set<UserProfile>().Find(WebSecurity.GetUserId(User.Identity.Name));
                        model.ModifiedDate = DateTime.Now;
                        model.ModifiedBy = user.UserId;
                        db.Partner.Attach(model);
                        db.Entry(model).Property(a => a.PartnerName).IsModified = true;
                        db.Entry(model).Property(a => a.Address).IsModified = true;
                        db.Entry(model).Property(a => a.Email).IsModified = true;
                        db.Entry(model).Property(a => a.Mobile).IsModified = true;
                        db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                        db.Entry(model).Property(a => a.ModifiedDate).IsModified = true;
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


        [HttpPost]
        public ActionResult ExportExcel(string dataString)
        {
            List<Partner> listData = new List<Partner>();
            var dataListJson = dataString.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                Partner dataObj = null;
                var dataObjString = string.Empty;
                if (i == 0)
                {
                    dataObjString = dataObjSplit1[i] + "}";
                }
                else
                {
                    var dataObjString0 = dataObjSplit1[i].Substring(1);
                    dataObjString = dataObjString0 + "}";
                }
                dataObj = JsonConvert.DeserializeObject<Partner>(dataObjString);
                listData.Add(dataObj);
            }
            var result = DownloadDrivePlan(listData);
            var fileStream = new MemoryStream(result);
            return File(fileStream, "application/ms-excel", "QLDoiTac.xlsx");
        }
        public byte[] DownloadDrivePlan(List<Partner> models)
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLDoiTac.xlsx", HostingEnvironment.MapPath("/Uploads")));

            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var productWorksheet = p.Workbook.Worksheets[0];
                    productWorksheet.Select();
                    int i = 0;
                    foreach (var item in models)
                    {
                        productWorksheet.Cells[i + 2, 1].Value = item.PartnerName;
                        productWorksheet.Cells[i + 2, 2].Value = item.Address;
                        productWorksheet.Cells[i + 2, 3].Value = item.Email;
                        productWorksheet.Cells[i + 2, 4].Value = item.Mobile;
                        productWorksheet.Cells[i + 2, 5].Value = item.PartnerCode;
                        i++;
                    }
                
                return p.GetAsByteArray();
                }
            }
            else
            {
                return null;
            }
        }



      
        public ActionResult UploadExcel()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetUploadProgress()
        {
            var result = Session["UploadPartnerProgress"] == null ? 0 : (int)Session["UploadPartnerProgress"];

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UploadExcel(string submit)
        {
            if (submit == "Upload")
            {
                HttpPostedFileBase file = Request.Files[0];
                if (file == null || file.ContentLength == 0)
                {
                    return View();
                }
                var fileTemp = new FileInfo(string.Format(@"{0}\QLDoiTac.xlsx", HostingEnvironment.MapPath("/Uploads")));
                var check = ExcelHelper.CheckHeaderFile(fileTemp, file, 1);
                if (check == 1)
                {
                    var result = new UploadPartnerFromExcel().UploadProducts(file, Session["UploadPartnerProgress"]);
                    if (result == null)
                    {
                        ViewBag.check = "Upload đối tác thành công!";
                        ViewBag.StartupScript = "upload_success();";
                        return View();
                    }
                    else
                    {
                        String path = Server.MapPath("~/Uploads/FileResult/"); //Path

                        //Check if directory exist
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path); //Create directory if it doesn't exist
                        }

                        var randomFileName = Guid.NewGuid().ToString().Substring(0, 4) + ".xlsx";
                        string filePath = Path.Combine(path, randomFileName);

                        System.IO.File.WriteAllBytes(filePath, result);
                        ViewBag.check = "Vui lòng kiểm tra file được trả về!";
                        ViewBag.pathFileName = "/Uploads/FileResult/" + randomFileName;
                        ViewBag.StartupScript = "downLoadFile();";
                        return View();
                    }
                }
                else if (check == 0)
                {
                    ViewBag.check = "File không đúng định dạng Template";
                    ViewBag.StartupScript = "hideLoading();";
                    return View();
                }
                else
                {
                    ViewBag.check = "Đã có lỗi xảy ra! Vui lòng liên hệ quản trị viên.";
                    ViewBag.StartupScript = "hideLoading();";
                    return View();
                }
            }
            else
            {
                var result = new UploadPartnerFromExcel().DownloadTemplate();
                var fileStream = new MemoryStream(result);
                return File(fileStream, "application/ms-excel", "QLDoiTac_Template.xlsx");
            }
        }
        [HttpPost]
        public ActionResult DeleteFile(string dataString)
        {
            string path = Server.MapPath(dataString);
            FileInfo file = new FileInfo(path);

            var dateGet = System.IO.File.ReadAllBytes(path);
            if (file.Exists)//check file exsit or not  
            {
                file.Delete();
            }

            return File(dateGet, "application/ms-excel", "QLDoiTac_UploadResult.xlsx");

        }
    }
}
