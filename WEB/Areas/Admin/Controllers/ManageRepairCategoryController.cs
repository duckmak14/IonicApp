using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;
using WebModels;
using System.Data;
using WEB.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Configuration;
using System.Data.OleDb;
using WEB.Areas.ContentType.Controllers;
using System.Web.Hosting;
using Newtonsoft.Json;

namespace WEB.Areas.Admin.Controllers
{
    [VanTaiAuthorize]

    public class ManageRepairCategoryController : Controller
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

        public ActionResult Categorys_Read([DataSourceRequest] DataSourceRequest request)
        {
            var categorys = from a in db.RepairCategorys.Where(x=>x.IsRemove != true)
                            select a;

            return Json(categorys.ToList().OrderByDescending(x => x.ID).ToDataSourceResult(request));
        }

        public ActionResult Add()
        {
            var model = new RepairCategory();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add([Bind(Exclude = "")] RepairCategory model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                model.CreatedDate = DateTime.Now;
                db.Set<RepairCategory>().Add(model);
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
            var model = db.RepairCategorys.Where(x => x.ID == id).FirstOrDefault();
            if (model == null)
            {
                return HttpNotFound();
            }
            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "")] RepairCategory model)
        {
            if (ModelState.IsValid)
            {
                model.ModifiedDate = DateTime.Now;
                model.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                db.RepairCategorys.Attach(model);
                db.Entry(model).Property(a => a.Category).IsModified = true;
                db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                db.Entry(model).Property(a => a.ModifiedDate).IsModified = true;
                db.SaveChanges();
                ViewBag.StartupScript = "edit_success();";
                return View(model);
            }
            else
            {
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult Deletes( string dataString)
        {
            List<RepairCategory> listData = new List<RepairCategory>();
            var dataListJson = dataString.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                RepairCategory dataObj = null;
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
                dataObj = JsonConvert.DeserializeObject<RepairCategory>(dataObjString);
             
                listData.Add(dataObj);
            }

            var temp = new List<RepairCategory>();
            foreach (var item in listData)
            {
                using (DbContextTransaction transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var findCategory = db.RepairCategorys.Where(x => x.Category == item.Category && x.IsRemove != true).FirstOrDefault();
                        if (findCategory != null)
                        {
                            findCategory.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                            findCategory.ModifiedDate = DateTime.Now;
                            findCategory.IsRemove = true;
                            db.SaveChanges();

                            var findRepair = db.RepairVehicles.Where(x => x.CategoryID == findCategory.ID && x.IsRemove != true).ToList();
                            foreach(var i in findRepair)
                            {
                                i.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                                i.ModifiedDate = DateTime.Now;
                                i.IsRemove = true;
                            }
                            db.SaveChanges();

                        }
                        else
                        {
                            transaction.Rollback();
                            temp.Add(item);
                            continue;
                        }

                        transaction.Commit();

                    }
                    catch (Exception)
                    {

                        transaction.Rollback();
                        temp.Add(item);
                    }
                }
            }

            if (temp.Count == 0)
            {
                ViewBag.StartupScript = "deletes_success();";
                return View("Index");
            }
            else if (temp.Count > 0)
            {
                ViewBag.StartupScript = "deletes_unsuccess();";
                return View("Index");
            }
            else
            {
                ViewBag.StartupScript = "deletes_success();";
                return View("Index");
            }

        }

        [HttpPost]
        public ActionResult ExportExcel(string dataString)
        {
            List<RepairCategory> listData = new List<RepairCategory>();
            var dataListJson = dataString.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                RepairCategory dataObj = null;
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
                dataObj = JsonConvert.DeserializeObject<RepairCategory>(dataObjString);
                listData.Add(dataObj);
            }
            var result = DownloadDrivePlan(listData);
            var fileStream = new MemoryStream(result);
            return File(fileStream, "application/ms-excel", "QL_Danh_Muc_Sua_Chua.xlsx");
        }
        public byte[] DownloadDrivePlan(List<RepairCategory> models)
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLDanhMucSuaChua.xlsx", HostingEnvironment.MapPath("/Uploads")));

            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var productWorksheet = p.Workbook.Worksheets[0];
                    productWorksheet.Select();
                    int i = 0;
                    foreach (var item in models)
                    {
                        productWorksheet.Cells[i + 2, 1].Value = i;
                        productWorksheet.Cells[i + 2, 2].Value = item.Category;
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
            var result = Session["UploadCategoryProgress"] == null ? 0 : (int)Session["UploadCategoryProgress"];

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
                var fileTemp = new FileInfo(string.Format(@"{0}\QLDanhMucSuaChua.xlsx", HostingEnvironment.MapPath("/Uploads")));
                var check = ExcelHelper.CheckHeaderFile(fileTemp, file, 1);
                if (check == 1)
                {
                    var result = new UploadRepairCategoryFromExcel().UploadProducts(file, Session["UploadCategoryProgress"]);

                   
                    if (result == null)
                    {
                        ViewBag.check = "Upload danh mục thành công!";
                        ViewBag.StartupScript = "upload_success();";
                        return View();
                    }
                    else if (result.Count() == 1)
                    {
                        ViewBag.check = "Đã xảy ra lỗi trong quá trình lưu! Vui lòng thử lại";
                        ViewBag.StartupScript = "hideLoading();";
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
                var result = new UploadRepairCategoryFromExcel().DownloadTemplate();
                var fileStream = new MemoryStream(result);
                return File(fileStream, "application/ms-excel", "QLDanh_Muc_Sua_Chua_Template.xlsx");
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

            return File(dateGet, "application/ms-excel", "QLDanh_Muc_Sua_Chua_UploadResult.xlsx");

        }
    }
}