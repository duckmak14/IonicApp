using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Security;
using WEB.Areas.ContentType.Controllers;
using WEB.Models;
using WebMatrix.WebData;
using WebModels;

namespace WEB.Areas.Admin.Controllers
{
    [VanTaiAuthorize]

    public class ManageAddressController : Controller
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
        public JsonResult GetDistricts(string provinceId)
        {
            var districts = db.District.Where(x => x.ProvinceID == provinceId).ToList();

            return Json(districts.Select(x => new
            {
                districtID = x.ID,
                x.DistrictName
            }), JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public JsonResult GetProvinces()
        {
            var provinces = from x in db.Province.AsNoTracking() select x;
            return Json(provinces.Select(x => new
            {
                ID = x.ID,
                ProvinceName = x.ProvinceName
            }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Locations_Read([DataSourceRequest] DataSourceRequest request)
        {
            List<Location> locations = new List<Location>();
            var address = from a in db.Location
                          join b in db.District on a.DistrictID equals b.ID into group1
                          from b in group1.DefaultIfEmpty()
                          join c in db.Province on a.ProvinceID equals c.ID into group2
                          from c in group2.DefaultIfEmpty()
                          join d in db.Location on a.ParentID equals d.ID into group3
                          from d in group3.DefaultIfEmpty()

                          select new
                          {
                             a.ID,
                             a.ParentID,
                             a.LocationName,
                             a.AddressName,
                             b.DistrictName,
                             c.ProvinceName,
                             parentLocation = d.LocationName
                          };
            var test = address.ToList();

            foreach (var item in address)
            {
                var location = new Location();
                location.ID = item.ID;
                location.LocationName = item.LocationName;
                location.AddressName = item.AddressName;
                location.Province = new Province()
                {
                    ProvinceName = item.ProvinceName
                };
                location.District = new District()
                {
                    DistrictName = item.DistrictName
                };
                location.ProvinceName = item.parentLocation;
               

                locations.Add(location);
            }
            return Json(locations.OrderByDescending(x => x.ID).ToDataSourceResult(request));

        }
        public ActionResult Add()
        {
            var model = new Location();
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add([Bind(Exclude = "")] Location model)
        {
            if (ModelState.IsValid)
            {
                var temp = (from p in db.Set<Location>().AsNoTracking()
                            where p.LocationName.Equals(model.LocationName, StringComparison.OrdinalIgnoreCase)
                            select p).FirstOrDefault();
                if (temp != null)
                {
                    ModelState.AddModelError("", AccountResources.LocationNameExists);
                    return View(model);
                }
                else
                {
                    try
                    {
                        model.CreatedDate = DateTime.Now;
                        var user = db.Set<UserProfile>().Find(WebSecurity.GetUserId(User.Identity.Name));
                        model.CreatedBy = user.UserId;
                        model.ModifiedDate = null;
                        db.Set<Location>().Add(model);
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
            var model = db.Location.Where(x=>x.ID == id).FirstOrDefault();
            if (model == null)
            {
                return HttpNotFound();
            }
            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "")] Location model)
        {
            if (ModelState.IsValid)
            {
                var list = db.Location.Where(x => x.ID != model.ID).ToList();
                var temp = (from p in list
                            where p.LocationName.Equals(model.LocationName, StringComparison.OrdinalIgnoreCase)
                            select p).FirstOrDefault();
                if (temp != null)
                {
                    ModelState.AddModelError("", AccountResources.LocationNameExists);
                    return View(model);
                }
                else
                {
                    try
                    {
                        var user = db.Set<UserProfile>().Find(WebSecurity.GetUserId(User.Identity.Name));
                        model.ModifiedDate = DateTime.Now;
                        model.ModifiedBy = user.UserId;
                        db.Location.Attach(model);
                        db.Entry(model).Property(a => a.LocationName).IsModified = true;
                        db.Entry(model).Property(a => a.AddressName).IsModified = true;
                        db.Entry(model).Property(a => a.DistrictID).IsModified = true;
                        db.Entry(model).Property(a => a.ProvinceID).IsModified = true;
                        db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                        db.Entry(model).Property(a => a.ModifiedDate).IsModified = true;
                        db.Entry(model).Property(a => a.ParentID).IsModified = true;
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
            List<LocationExport> listData = new List<LocationExport>();
            var dataListJson = dataString.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                LocationExport dataObj = null;
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
                dataObj = JsonConvert.DeserializeObject<LocationExport>(dataObjString);
                listData.Add(dataObj);
            }
            var result = DownloadAddress(listData);
            var fileStream = new MemoryStream(result);
            return File(fileStream, "application/ms-excel", "QLDiaDiem.xlsx");
        }
        public byte[] DownloadAddress(List<LocationExport> models)
        {

            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLDiaDiem.xlsx", HostingEnvironment.MapPath("/Uploads")));
            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var productWorksheet = p.Workbook.Worksheets[0];
                    productWorksheet.Select();
                    int i = 0;
                    foreach (var item in models)
                    {
                        productWorksheet.Cells[i + 2, 1].Value = item.LocationName;
                        productWorksheet.Cells[i + 2, 2].Value = item.AddressName;
                        productWorksheet.Cells[i + 2, 3].Value = item.DistrictName;
                        productWorksheet.Cells[i + 2, 4].Value = item.ProvinceName;
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
            var result = Session["UploadAddressProgress"] == null ? 0 : (int)Session["UploadAddressProgress"];

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
                var fileTemp = new FileInfo(string.Format(@"{0}\QLDiaDiem.xlsx", HostingEnvironment.MapPath("/Uploads")));
                var check = ExcelHelper.CheckHeaderFile(fileTemp, file,1);
                if (check == 1)
                {
                    var result = new UploadAddressFromExcel().UploadProducts(file, Session["UploadAddressProgress"]);

                    

                    if (result == null)
                    {
                        ViewBag.check = "Upload địa điểm thành công!";
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

                    //var fileStream = new MemoryStream(result);
                    //return File(fileStream, "application/ms-excel", "QLKeHoachChay_UploadResult.xlsx");


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
                var result = new UploadAddressFromExcel().DownloadTemplate();
                var fileStream = new MemoryStream(result);
                return File(fileStream, "application/ms-excel", "QLDiaDiem_Template.xlsx");
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

            return File(dateGet, "application/ms-excel", "QLDiaDiem_UploadResult.xlsx");

        }
    }
}