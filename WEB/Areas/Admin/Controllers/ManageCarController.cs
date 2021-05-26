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

    public class ManageCarController : Controller
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
        public JsonResult GetWeights(string text)
        {
            var weights = from x in db.VehicleWeight.AsNoTracking() select x ;
            if (!string.IsNullOrEmpty(text))
            {
                weights = weights.Where(p => p.WeightName.Contains(text));
            }

            List<VehicleWeight> listObjectSort = new List<VehicleWeight>();
            List<VehicleWeight> listNumberObject = new List<VehicleWeight>();
            List<Double> listNumber = new List<Double>();
            List <VehicleWeight> listString = new List<VehicleWeight>();

            foreach (var item in weights)
            {
                Double weightValue;
                if (Double.TryParse(item.WeightName, out weightValue))
                {
                    listNumber.Add(weightValue);
                    listNumberObject.Add(item);
                }
                else
                {
                    listString.Add(item);
                }    
            }
            var listObject = listString.OrderBy(x=>x.WeightName);
            var listNumberSort = listNumber.OrderBy(x => x);

            foreach(var i in listNumberSort)
            {
                var numberSortItem = listNumberObject.Where(x => Double.Parse(x.WeightName) == i).FirstOrDefault();
                listObjectSort.Add(numberSortItem);
            }
            foreach(var item in listObject)
            {
                listObjectSort.Add(item);
            }
            return Json(listObjectSort.ToList().Select(x => new
            {
                x.ID,
                x.WeightName
            }), JsonRequestBehavior.AllowGet); ;
        }
        public ActionResult Cars_Read([DataSourceRequest] DataSourceRequest request)
        {
            List<Vehicle> vehicles = new List<Vehicle>();
            var partners = from a in db.Vehicle
                           join b in db.Partner on a.PartnerID equals b.ID into group1
                           from b in group1.DefaultIfEmpty()
                           join c in db.VehicleWeight on a.WeightID equals c.ID into group2
                           from c in group2.DefaultIfEmpty()
                           select new
                           {
                               a.ID,
                               a.CarOwerName,
                               a.NumberPlate,
                               c.WeightName,
                               a.Mobile,
                               a.PartnerID,
                               b.PartnerName
                           };
            var test = partners.ToList();
            foreach (var item in partners)
            {
                var vehicle = new Vehicle();
                vehicle.ID = item.ID;
                vehicle.CarOwerName = item.CarOwerName;
                vehicle.NumberPlate = item.NumberPlate;
                vehicle.Mobile = item.Mobile;
                vehicle.PartnerID = item.PartnerID;
                vehicle.VehicleWeight = new VehicleWeight()
                {
                    WeightName = item.WeightName
                };
                vehicle.Partner = new Partner()
                {
                    PartnerName = item.PartnerName
                };
                vehicles.Add(vehicle);
            }
            return Json(vehicles.OrderByDescending(x => x.ID).ToDataSourceResult(request));

        }
        public ActionResult Add()
        {
            var model = new Vehicle();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add([Bind(Exclude = "")] Vehicle model)
        {
            if (ModelState.IsValid)
            {
                var temp = (from p in db.Set<Vehicle>()
                            where p.NumberPlate.Equals(model.NumberPlate, StringComparison.OrdinalIgnoreCase)
                            select p).FirstOrDefault();
                if (temp != null)
                {
                    ModelState.AddModelError("", AccountResources.NumberPlateExists);
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
                        db.Set<Vehicle>().Add(model);
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
            var model = db.Vehicle.Where(x => x.ID == id).FirstOrDefault();
            if (model == null)
            {
                return HttpNotFound();
            }
            ViewBag.Partners = model.PartnerID;
            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "")] Vehicle model)
        {
            if (ModelState.IsValid)
            {
                var list = db.Vehicle.Where(x => x.ID != model.ID && x.Mobile != model.Mobile).ToList();
                var temp = (from p in list
                            where p.NumberPlate.Equals(model.NumberPlate, StringComparison.OrdinalIgnoreCase)
                            select p).FirstOrDefault();
                if (temp != null)
                {
                    ModelState.AddModelError("", AccountResources.NumberPlateExists);
                    return View(model);
                }
                else
                {
                    try
                    {
                        var user = db.Set<UserProfile>().Find(WebSecurity.GetUserId(User.Identity.Name));
                        model.ModifiedDate = DateTime.Now;
                        model.ModifiedBy = user.UserId;
                        db.Vehicle.Attach(model);
                        db.Entry(model).Property(a => a.CarOwerName).IsModified = true;
                        db.Entry(model).Property(a => a.NumberPlate).IsModified = true;
                        db.Entry(model).Property(a => a.WeightID).IsModified = true;
                        db.Entry(model).Property(a => a.Mobile).IsModified = true;
                        db.Entry(model).Property(a => a.PartnerID).IsModified = true;
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
            List<CarExport> listData = new List<CarExport>();
            var dataListJson = dataString.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                CarExport dataObj = null;
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
                dataObj = JsonConvert.DeserializeObject<CarExport>(dataObjString);
                listData.Add(dataObj);
            }
            var result = DownloadDrivePlan(listData);
            var fileStream = new MemoryStream(result);
            return File(fileStream, "application/ms-excel", "QLXe.xlsx");
        }
        public byte[] DownloadDrivePlan(List<CarExport> models)
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLXe.xlsx", HostingEnvironment.MapPath("/Uploads")));

            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var productWorksheet = p.Workbook.Worksheets[0];
                    productWorksheet.Select();
                    int i = 0;
                    foreach (var item in models)
                    {
                        productWorksheet.Cells[i + 2, 1].Value = item.CarOwerName;
                        productWorksheet.Cells[i + 2, 2].Value = item.NumberPlate;
                        productWorksheet.Cells[i + 2, 3].Value = item.WeightName;
                        productWorksheet.Cells[i + 2, 4].Value = item.Mobile;
                        productWorksheet.Cells[i + 2, 5].Value = item.PartnerName;

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
            var result = Session["UploadCarProgress"] == null ? 0 : (int)Session["UploadCarProgress"];

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
                var fileTemp = new FileInfo(string.Format(@"{0}\QLXe.xlsx", HostingEnvironment.MapPath("/Uploads")));
                var check = ExcelHelper.CheckHeaderFile(fileTemp, file,1);
                if (check == 1)
                {
                    var result = new UploadCarFromExcel().UploadProducts(file, Session["UploadCarProgress"]);
                    if (result == null)
                    {
                        ViewBag.check = "Upload xe thành công!";
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
                var result = new UploadCarFromExcel().DownloadTemplate();
                var fileStream = new MemoryStream(result);
                return File(fileStream, "application/ms-excel", "QLXe_Template.xlsx");
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

            return File(dateGet, "application/ms-excel", "QLXe_UploadResult.xlsx");

        }
    }
}