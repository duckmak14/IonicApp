using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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

    public class ManageDriverPayController : Controller
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            var model = db.DriverPays.Where(x => x.IsRemove != true).ToList();

            var data = model.Select(k => new { k.PayDate.Year, k.PayDate.Month, k.Price }).GroupBy(x => new { x.Year, x.Month }, (key, group) => new
            {
                year = key.Year,
                month = key.Month,
                total = group.Sum(k => k.Price)
            }).ToList();

            var groupByYear = from a in data
                              group a by new { a.year };

            var listTotal = new List<MonthInYearTotal>();

            foreach (var listYear in groupByYear)
            {
                for (int i = 1; i < 13; i++)
                {
                    var check = false;
                    var newTotal = new MonthInYearTotal();

                    foreach (var month in listYear)
                    {
                        if (month.month == i)
                        {
                            check = true;
                            newTotal.Month = month.month;
                            newTotal.Year = month.year;
                            newTotal.Total = (Double)month.total;
                            listTotal.Add(newTotal);
                        }
                    }

                    if (!check)
                    {
                        newTotal.Month = i;
                        newTotal.Year = listYear.First().year;
                        newTotal.Total = 0;
                        listTotal.Add(newTotal);
                    }
                }
            }
            ViewBag.listTotal = listTotal.OrderByDescending(x => x.Year).ThenBy(x => x.Month).ToList();
            return View();
        }

        public ActionResult DriverPayDetail(string dataString)
        {
            if (dataString != null)
            {
                DateTime date = new DateTime();
                try
                {
                    date = DateTime.Parse(dataString.Format("{0:dd-MM-yyyy}"));

                }
                catch
                {
                    string[] arrListStr = dataString.Split(new char[] { 't', 'o' });
                    date = DateTime.Parse(arrListStr[0].Format("{0:dd-MM-yyyy}"));
                }

                var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                string day = firstDayOfMonth.Day.ToString() + "-" + firstDayOfMonth.Month.ToString() + "-" + firstDayOfMonth.Year.ToString()
                    + " to " + lastDayOfMonth.Day.ToString() + "-" + lastDayOfMonth.Month.ToString() + "-" + lastDayOfMonth.Year.ToString();

                ViewBag.Date = day;
                ViewBag.DateNow = "Tháng " + date.Month + " - " + date.Year;
            }

            return View();
        }

        public ActionResult DriverPay_Read([DataSourceRequest] DataSourceRequest request, string datetime)
        {
            string[] arrListStr = datetime.Split(new char[] { 't', 'o' });
            var firstDay = DateTime.Parse(arrListStr[0].Format("{0:dd-MM-yyyy}"));
            var lastDay = DateTime.Parse(arrListStr[2].Format("{0:dd-MM-yyyy}"));

            var driverPayQuery = from a in db.DriverPays.Where(x => DbFunctions.TruncateTime(firstDay) <= DbFunctions.TruncateTime(x.PayDate) && DbFunctions.TruncateTime(x.PayDate) <= DbFunctions.TruncateTime(lastDay) && x.IsRemove != true)
                                 join b in db.Vehicle on a.VehicleID equals b.ID into group1
                                 from b in group1.DefaultIfEmpty()
                                 join c in db.Vehicle on a.DriverID equals c.ID into group2
                                 from c in group2.DefaultIfEmpty()
                                 select new
                                 {
                                     a.ID,
                                     a.Content,
                                     a.PayDate,
                                     a.Price,
                                     a.VehicleID,
                                     c.CarOwerName,
                                     b.NumberPlate
                                 };
            var test = driverPayQuery.ToList();

            var jsonResult = Json(driverPayQuery.ToList().OrderBy(x => x.PayDate).ToDataSourceResult(request));
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult Add()
        {
            var model = new DriverPay();
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add([Bind(Exclude = "")] DriverPay model, DateTime? date)
        {
            if (ModelState.IsValid)
            {
                if (date == null)
                {
                    ModelState.AddModelError("", AccountResources.RequiredPlanDate);
                    return View(model);
                }
                if (model.VehicleID == null)
                {
                    model.VehicleID = model.DriverID;
                }
                model.PayDate = DateTime.Parse(date.ToString());

                var firstDayOfMonth = new DateTime(model.PayDate.Year, model.PayDate.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                var check = db.DriverPays.Where(x => x.VehicleID == model.VehicleID && x.DriverID == model.DriverID && x.IsRemove != true
                && DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.PayDate) && DbFunctions.TruncateTime(x.PayDate) <= DbFunctions.TruncateTime(lastDayOfMonth)).Any();
                if (check)
                {
                    ModelState.AddModelError("", "Đã tồn tại bản ghi trùng tên lái xe và biển kiểm soát trên hệ thống!");
                    return View(model);
                }
                else
                {
                    model.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    model.CreatedDate = DateTime.Now;
                    db.Set<DriverPay>().Add(model);
                    db.SaveChanges();
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
            var model = db.Set<DriverPay>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "")] DriverPay model, DateTime? date)
        {
            if (ModelState.IsValid)
            {
                if (date == null)
                {
                    ModelState.AddModelError("", AccountResources.RequiredPlanDate);
                    return View(model);
                }
                if (model.VehicleID == null)
                {
                    model.VehicleID = model.DriverID;
                }
                model.PayDate = DateTime.Parse(date.ToString());

                var firstDayOfMonth = new DateTime(model.PayDate.Year, model.PayDate.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                var check = db.DriverPays.Where(x => x.VehicleID == model.VehicleID && x.DriverID == model.DriverID && x.IsRemove != true && x.ID != model.ID
                && DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.PayDate) && DbFunctions.TruncateTime(x.PayDate) <= DbFunctions.TruncateTime(lastDayOfMonth)).Any();

                if (check)
                {
                    ModelState.AddModelError("", "Đã tồn tại bản ghi trùng tên lái xe và biển kiểm soát trên hệ thống!");
                    return View(model);
                }
                else
                {
                    model.PayDate = DateTime.Parse(date.ToString());
                    model.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    model.ModifiedDate = DateTime.Now;
                    db.DriverPays.Attach(model);
                    db.Entry(model).Property(a => a.PayDate).IsModified = true;
                    db.Entry(model).Property(a => a.Content).IsModified = true;
                    db.Entry(model).Property(a => a.VehicleID).IsModified = true;
                    db.Entry(model).Property(a => a.DriverID).IsModified = true;
                    db.Entry(model).Property(a => a.Price).IsModified = true;
                    db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                    db.Entry(model).Property(a => a.ModifiedDate).IsModified = true;
                    db.SaveChanges();
                    ViewBag.StartupScript = "edit_success();";
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult ExportExcelFromIndex(string data)
        {
            List<DriverPayViewModel> listData = new List<DriverPayViewModel>();
            var objects = data.Split(',');
            var lstDate = new List<DateTime>();
            foreach (var obj in objects)
            {
                try
                {
                    lstDate.Add(DateTime.Parse(obj.Format("{0:dd-MM-yyyy}")));
                }
                catch (Exception)
                { }
            }

            foreach (var item in lstDate)
            {
                var firstDayOfMonth = new DateTime(item.Year, item.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                var driverPayQuery = (from a in db.DriverPays.Where(x => DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.PayDate) && DbFunctions.TruncateTime(x.PayDate) <= DbFunctions.TruncateTime(lastDayOfMonth) && x.IsRemove != true)
                                      join b in db.Vehicle on a.VehicleID equals b.ID into group1
                                      from b in group1.DefaultIfEmpty()
                                      join c in db.Vehicle on a.DriverID equals c.ID into group2
                                      from c in group2.DefaultIfEmpty()
                                      select new
                                      {
                                          a.ID,
                                          a.Content,
                                          a.PayDate,
                                          a.Price,
                                          a.VehicleID,
                                          c.CarOwerName,
                                          b.NumberPlate
                                      }).AsEnumerable()
                                        .Select(B => new DriverPayViewModel()
                                        {
                                            ID = B.ID,
                                            Content = B.Content,
                                            PayDate = B.PayDate,
                                            Price = B.Price,
                                            VehicleID = B.VehicleID,
                                            CarOwerName = B.CarOwerName,
                                            NumberPlate = B.NumberPlate
                                        }).ToList();

                listData.AddRange(driverPayQuery);
            }

            var result = DownloadDriverPay(listData.OrderByDescending(x => x.ID).ToList());
            var fileStream = new MemoryStream(result);
            return File(fileStream, "application/ms-excel", "QL_Chi_Phi_Lai_Xe.xlsx");
        }

        [HttpPost]
        public ActionResult ExportExcel(string dataString)
        {
            List<DriverPayViewModel> listData = new List<DriverPayViewModel>();
            var dataListJson = dataString.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                DriverPayViewModel dataObj = null;
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
                dataObj = JsonConvert.DeserializeObject<DriverPayViewModel>(dataObjString);
                dataObj.PayDate = dataObj.PayDate.ToLocalTime();
                listData.Add(dataObj);
            }
            var result = DownloadDriverPay(listData);
            var fileStream = new MemoryStream(result);
            return File(fileStream, "application/ms-excel", "QL_Chi_Phi_Lai_Xe.xlsx");
        }
        public byte[] DownloadDriverPay(List<DriverPayViewModel> models)
        {

            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLChiPhiLaiXe.xlsx", HostingEnvironment.MapPath("/Uploads")));

            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var productWorksheet = p.Workbook.Worksheets[0];
                    productWorksheet.Select();

                    if (models.Count == 0)
                    {
                        return p.GetAsByteArray();
                    }

                    int i = 4;
                    List<String> listNumber = new List<String>();
                    var queryPlan = from a in models
                                    group a by new { a.NumberPlate };

                    var queryPlanForDriver = from a in models
                                             group a by new { a.CarOwerName };

                    foreach (var listPlan in queryPlan)
                    {
                        listNumber.Add(listPlan.First().NumberPlate);
                    }

                    productWorksheet.Cells[1, 1].Value = models.Select(x => x.PayDate).Min().Month.ToString() + "/" + models.Select(x => x.PayDate).Min().Year.ToString();
                    foreach (var listItem in queryPlanForDriver)
                    {
                        int start = i;

                        foreach (var item in listItem)
                        {
                            productWorksheet.Cells[i, 3].Value = item.NumberPlate;
                            productWorksheet.Cells[i, 2].Value = item.Content;
                            productWorksheet.Cells[i, 4].Value = item.Price;
                            productWorksheet.Cells[i, 4].Style.Numberformat.Format = "#,##0";
                            i++;
                        }
                        var count = listItem.Count();
                        productWorksheet.Cells["A" + start.ToString() + ":A" + (i - 1).ToString()].Merge = true;
                        productWorksheet.Cells[start, 1].Value = listItem.First().CarOwerName;

                    }
                    string addressColumn = "";
                    var countColumn = 4;

                    foreach (var item in listNumber)
                    {
                        productWorksheet.Cells[3, countColumn + 1].Value = item;
                        addressColumn = productWorksheet.Cells[i, countColumn + 1].Address.Substring(0, 1);
                        for (int k = 4; k < i; k++)
                        {
                            productWorksheet.Cells[k, countColumn + 1].Formula = "=IF($C" + (k).ToString() + "=" + addressColumn + "$3,$D" + (k).ToString() + ",0)";
                            productWorksheet.Cells[k, countColumn + 1].Style.Numberformat.Format = "#,##0";
                            productWorksheet.Column(countColumn + 1).Width = 18;
                        }
                        countColumn++;
                    }

                    ////sumtotal
                    for (int k = 5; k <= countColumn; k++)
                    {
                        addressColumn = productWorksheet.Cells[i, k].Address.Substring(0, 1);
                        productWorksheet.Cells[i, k].Style.Font.Bold = true;
                        productWorksheet.Cells[i, k].Formula = "= SUBTOTAL(9," + addressColumn + "4:" + addressColumn + (i - 1).ToString() + ")";
                        productWorksheet.Cells[i, k].Style.Numberformat.Format = "#,##0";
                    }

                    var setFilter = "A3:" + addressColumn + (i).ToString();
                    var range = productWorksheet.Cells[setFilter.ToString()];
                    range.AutoFilter = true;


                    string test1 = "A1:" + addressColumn + (i).ToString();
                    var modelTable = productWorksheet.Cells[test1];
                    modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    modelTable.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    modelTable.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
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
            var result = Session["UploadPlanProgress"] == null ? 0 : (int)Session["UploadPlanProgress"];

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
                var fileTemp = new FileInfo(string.Format(@"{0}\QLChiPhiLaiXe.xlsx", HostingEnvironment.MapPath("/Uploads")));
                var check = ExcelHelper.CheckHeaderFile(fileTemp, file, 2);
                if (check == 1)
                {
                    var result = new UploadDriverPayFromExcel().UploadProducts(file, Session["UploadDriverPayProgress"]);

                    if (result == null)
                    {
                        ViewBag.check = "Upload mục chi phí thành công!";
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
                var result = new UploadDriverPayFromExcel().DownloadTemplate();
                var fileStream = new MemoryStream(result);
                return File(fileStream, "application/ms-excel", "QLChiPhiLaiXe_Template.xlsx");
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

            return File(dateGet, "application/ms-excel", "QLChiPhi_UploadResult.xlsx");
        }

        [HttpPost]
        public ActionResult Deletes(string stringSubmit)
        {
            List<DriverPayViewModel> listData = new List<DriverPayViewModel>();
            var objects = stringSubmit.Split(',');
            var lstDate = new List<DateTime>();
            foreach (var obj in objects)
            {
                try
                {
                    lstDate.Add(DateTime.Parse(obj.Format("{0:dd-MM-yyyy}")));
                }
                catch (Exception)
                { }
            }

            foreach (var date in lstDate)
            {
                var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                var listDriverPay = db.DriverPays.Where(x => DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.PayDate) && DbFunctions.TruncateTime(x.PayDate) <= DbFunctions.TruncateTime(lastDayOfMonth) && x.IsRemove != true).ToList();
                foreach (var item in listDriverPay)
                {
                    item.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    item.ModifiedDate = DateTime.Now;
                    item.IsRemove = true;
                }

                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DeletesFromDetail(string dataStringDelete, string dataStringDate)
        {
            List<DriverPay> listData = new List<DriverPay>();
            var dataListJson = dataStringDelete.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                DriverPay dataObj = null;
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
                dataObj = JsonConvert.DeserializeObject<DriverPay>(dataObjString);
                dataObj.PayDate = dataObj.PayDate.ToLocalTime();
                listData.Add(dataObj);
            }

            var temp = new List<DriverPay>();
            foreach (var item in listData)
            {
                try
                {
                    var driverPayChange = db.DriverPays.Find(item.ID);
                    driverPayChange.IsRemove = true;
                    driverPayChange.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    driverPayChange.ModifiedDate = DateTime.Now;
                    db.SaveChanges();

                }
                catch (Exception)
                {
                    temp.Add(item);
                }
            }

            if (temp.Count > 0)
            {
                ViewBag.StartupScript = "deletes_unsuccess();";
                return RedirectToAction("DriverPayDetail", new { dataString = dataStringDate });
            }
            else
            {
                ViewBag.StartupScript = "deletes_success();";
                return RedirectToAction("DriverPayDetail", new { dataString = dataStringDate });
            }
        }
    }
}