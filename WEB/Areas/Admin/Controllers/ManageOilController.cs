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

    public class ManageOilController : Controller
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        // GET: Admin/DrivePlan
        public ActionResult Index()
        {
            var model = db.ManageOils.Where(x => x.IsRemove != true).ToList();

            var data = model.Select(k => new { k.OilDate.Year, k.OilDate.Month, k.Total }).GroupBy(x => new { x.Year, x.Month }, (key, group) => new
            {
                year = key.Year,
                month = key.Month,
                total = group.Sum(k => k.Total)
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
                            newTotal.Total = month.total;
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

        public ActionResult OilDetail(string dataString)
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
        public ActionResult Oil_Read([DataSourceRequest] DataSourceRequest request, string datetime)
        {
            string[] arrListStr = datetime.Split(new char[] { 't', 'o' });
            var firstDay = DateTime.Parse(arrListStr[0].Format("{0:dd-MM-yyyy}"));
            var lastDay = DateTime.Parse(arrListStr[2].Format("{0:dd-MM-yyyy}"));

            var firstDayOfMonth = new DateTime(firstDay.Year, firstDay.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var driverPayQuery = from a in db.ManageOils.Where(x => DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.OilDate) && DbFunctions.TruncateTime(x.OilDate) <= DbFunctions.TruncateTime(lastDayOfMonth) && x.IsRemove != true)
                                 join b in db.Vehicle on a.VehicleID equals b.ID into group1
                                 from b in group1.DefaultIfEmpty()
                                 join c in db.Vehicle on a.DriverID equals c.ID into group2
                                 from c in group2.DefaultIfEmpty()
                                 select new
                                 {
                                     a.ID,
                                     a.Note,
                                     a.OilDate,
                                     a.Total,
                                     a.Distance,
                                     a.OilLevel,
                                     a.SuppliedFromLevel,
                                     a.OtherRun,
                                     a.AmountOil,
                                     a.SuppliedOil,
                                     a.DifferenceOil,
                                     a.VehicleID,
                                     c.CarOwerName,
                                     b.NumberPlate
                                 };
            var test = driverPayQuery.ToList();

            var jsonResult = Json(driverPayQuery.ToList().OrderByDescending(x => x.ID).ToDataSourceResult(request));
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult Add()
        {
            var model = new ManageOilViewModel();
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add([Bind(Exclude = "")] ManageOilViewModel model, DateTime? date)
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
                model.OilDate = DateTime.Parse(date.ToString());

                var firstDayOfMonth = new DateTime(model.OilDate.Year, model.OilDate.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                var check = db.ManageOils.Where(x => x.VehicleID == model.VehicleID && x.DriverID == model.DriverID && x.IsRemove != true 
                && DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.OilDate) && DbFunctions.TruncateTime(x.OilDate) <= DbFunctions.TruncateTime(lastDayOfMonth)).Any();
                if (check)
                {
                    ModelState.AddModelError("", "Đã tồn tại bản ghi trùng tên lái xe và biển kiểm soát trên hệ thống!");
                    return View(model);
                }
                else
                {

                    var manageOil = new ManageOil();
                    manageOil.DriverID = model.DriverID;
                    manageOil.VehicleID = model.VehicleID;

                    String oilLevelString = (model.OilLevelString).Replace('.', ',');
                    double oilLevel = Double.Parse(oilLevelString);
                    manageOil.OilLevel = oilLevel;

                    String distanceString = (model.DistanceString).Replace('.', ',');
                    double distance = Double.Parse(distanceString);
                    manageOil.Distance = distance;

                    if (model.OtherRunString != null)
                    {
                        String otherRunString = (model.OtherRunString).Replace('.', ',');
                        double otherRun = Double.Parse(otherRunString);
                        manageOil.OtherRun = otherRun;
                    }


                    String suppliedOilString = (model.SuppliedOilString).Replace('.', ',');
                    double suppliedOil = Double.Parse(suppliedOilString);
                    manageOil.SuppliedOil = suppliedOil;

                    var price = db.OilPrices.Where(x => DbFunctions.TruncateTime(x.StartDate) <= DbFunctions.TruncateTime(model.OilDate) && DbFunctions.TruncateTime(x.EndDate) >= DbFunctions.TruncateTime(model.OilDate)).FirstOrDefault();
                    if (price != null)
                    {
                        manageOil.OilPrice = price.Price;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không tồn tại giá dầu trên hệ thống!");
                        return View(model);
                    }
                    manageOil.SuppliedFromLevel = (manageOil.Distance * manageOil.OilLevel) / 100;
                    manageOil.AmountOil = manageOil.SuppliedFromLevel + manageOil.OtherRun;
                    manageOil.DifferenceOil = manageOil.AmountOil - manageOil.SuppliedOil;
                    manageOil.Total = manageOil.DifferenceOil * manageOil.OilPrice;
                    manageOil.OilDate = model.OilDate;
                    manageOil.Note = model.Note;
                    manageOil.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    manageOil.CreatedDate = DateTime.Now;
                    db.Set<ManageOil>().Add(manageOil);
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
            var model = db.Set<ManageOil>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            var manageOilView = new ManageOilViewModel();
            manageOilView.ID = model.ID;
            manageOilView.OilDate = model.OilDate;
            manageOilView.Note = model.Note;
            manageOilView.DriverID = model.DriverID;
            if (model.DriverID != model.VehicleID)
            {
                manageOilView.VehicleID = model.VehicleID;
            }
            manageOilView.DistanceString = model.Distance.ToString().Replace(",", ".");
            manageOilView.OilLevelString = model.OilLevel.ToString().Replace(",", ".");
            if (model.OtherRun != 0)
            {
                manageOilView.OtherRunString = model.OtherRun.ToString().Replace(",", ".");
            }
            manageOilView.SuppliedOilString = model.SuppliedOil.ToString().Replace(",", ".");
            return View("Edit", manageOilView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "")] ManageOilViewModel model, DateTime? date)
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
                model.OilDate = DateTime.Parse(date.ToString());

                var firstDayOfMonth = new DateTime(model.OilDate.Year, model.OilDate.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                var check = db.ManageOils.Where(x => x.VehicleID == model.VehicleID && x.DriverID == model.DriverID && x.IsRemove != true && x.ID != model.ID
                && DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.OilDate) && DbFunctions.TruncateTime(x.OilDate) <= DbFunctions.TruncateTime(lastDayOfMonth)).Any();
                if (check)
                {
                    ModelState.AddModelError("", "Đã tồn tại bản ghi trùng tên lái xe và biển kiểm soát trên hệ thống!");
                    return View(model);
                }
                else
                {
                    var manageOil = new ManageOil();

                    manageOil.ID = model.ID;
                    manageOil.DriverID = model.DriverID;
                    manageOil.VehicleID = model.VehicleID;

                    String oilLevelString = (model.OilLevelString).Replace('.', ',');
                    double oilLevel = Double.Parse(oilLevelString);
                    manageOil.OilLevel = oilLevel;

                    String distanceString = (model.DistanceString).Replace('.', ',');
                    double distance = Double.Parse(distanceString);
                    manageOil.Distance = distance;

                    if (model.OtherRunString != null)
                    {
                        String otherRunString = (model.OtherRunString).Replace('.', ',');
                        double otherRun = Double.Parse(otherRunString);
                        manageOil.OtherRun = otherRun;
                    }

                    String suppliedOilString = (model.SuppliedOilString).Replace('.', ',');
                    double suppliedOil = Double.Parse(suppliedOilString);
                    manageOil.SuppliedOil = suppliedOil;

                    var price = db.OilPrices.Where(x => DbFunctions.TruncateTime(x.StartDate) <= DbFunctions.TruncateTime(model.OilDate) && DbFunctions.TruncateTime(x.EndDate) >= DbFunctions.TruncateTime(model.OilDate)).FirstOrDefault();
                    if (price != null)
                    {
                        manageOil.OilPrice = price.Price;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không tồn tại giá dầu trên hệ thống!");
                        return View(model);
                    }

                    manageOil.SuppliedFromLevel = (manageOil.Distance * manageOil.OilLevel) / 100;
                    manageOil.AmountOil = manageOil.SuppliedFromLevel + manageOil.OtherRun;
                    manageOil.DifferenceOil = manageOil.AmountOil - manageOil.SuppliedOil;
                    manageOil.Total = manageOil.DifferenceOil * manageOil.OilPrice;
                    manageOil.OilDate = model.OilDate;
                    manageOil.Note = model.Note;
                    manageOil.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    manageOil.ModifiedDate = DateTime.Now;

                    db.ManageOils.Attach(manageOil);
                    db.Entry(manageOil).Property(a => a.OilDate).IsModified = true;
                    db.Entry(manageOil).Property(a => a.Note).IsModified = true;
                    db.Entry(manageOil).Property(a => a.VehicleID).IsModified = true;
                    db.Entry(manageOil).Property(a => a.DriverID).IsModified = true;
                    db.Entry(manageOil).Property(a => a.Distance).IsModified = true;
                    db.Entry(manageOil).Property(a => a.OilLevel).IsModified = true;
                    db.Entry(manageOil).Property(a => a.SuppliedFromLevel).IsModified = true;
                    db.Entry(manageOil).Property(a => a.OtherRun).IsModified = true;
                    db.Entry(manageOil).Property(a => a.AmountOil).IsModified = true;
                    db.Entry(manageOil).Property(a => a.SuppliedOil).IsModified = true;
                    db.Entry(manageOil).Property(a => a.DifferenceOil).IsModified = true;
                    db.Entry(manageOil).Property(a => a.Total).IsModified = true;
                    db.Entry(manageOil).Property(a => a.ModifiedDate).IsModified = true;
                    db.Entry(manageOil).Property(a => a.ModifiedBy).IsModified = true;
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
            List<ManageOilViewModel> listData = new List<ManageOilViewModel>();
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

                var driverPayQuery = (from a in db.ManageOils.Where(x => DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.OilDate) && DbFunctions.TruncateTime(x.OilDate) <= DbFunctions.TruncateTime(lastDayOfMonth) && x.IsRemove != true)
                                      join b in db.Vehicle on a.VehicleID equals b.ID into group1
                                      from b in group1.DefaultIfEmpty()
                                      join c in db.Vehicle on a.DriverID equals c.ID into group2
                                      from c in group2.DefaultIfEmpty()
                                      select new
                                      {
                                          a.ID,
                                          a.Note,
                                          a.OilDate,
                                          a.Total,
                                          a.Distance,
                                          a.OilLevel,
                                          a.SuppliedFromLevel,
                                          a.OtherRun,
                                          a.AmountOil,
                                          a.SuppliedOil,
                                          a.DifferenceOil,
                                          a.VehicleID,
                                          c.CarOwerName,
                                          b.NumberPlate
                                      }).AsEnumerable()
                                        .Select(B => new ManageOilViewModel()
                                        {
                                            ID = B.ID,
                                            Note = B.Note,
                                            OilDate = B.OilDate,
                                            Total = B.Total,
                                            Distance = B.Distance,
                                            OilLevel = B.OilLevel,
                                            SuppliedFromLevel = B.SuppliedFromLevel,
                                            OtherRun = B.OtherRun,
                                            AmountOil = B.AmountOil,
                                            SuppliedOil = B.SuppliedOil,
                                            DifferenceOil = B.DifferenceOil,
                                            VehicleID = B.VehicleID,
                                            CarOwerName = B.CarOwerName,
                                            NumberPlate = B.NumberPlate
                                        }).ToList();

                listData.AddRange(driverPayQuery);
            }

            var result = DownloadManageOil(listData.OrderByDescending(x => x.ID).ToList());
            var fileStream = new MemoryStream(result);
            return File(fileStream, "application/ms-excel", "Thong_Ke_Dau.xlsx");
        }

        [HttpPost]
        public ActionResult ExportExcel(string dataString)
        {
            List<ManageOilViewModel> listData = new List<ManageOilViewModel>();
            var dataListJson = dataString.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                ManageOilViewModel dataObj = null;
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
                dataObj = JsonConvert.DeserializeObject<ManageOilViewModel>(dataObjString);
                dataObj.OilDate = dataObj.OilDate.ToLocalTime();
                listData.Add(dataObj);
            }
            var result = DownloadManageOil(listData);
            var fileStream = new MemoryStream(result);
            return File(fileStream, "application/ms-excel", "Thong_Ke_Dau.xlsx");
        }
        public byte[] DownloadManageOil(List<ManageOilViewModel> models)
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\ThongKeDau.xlsx", HostingEnvironment.MapPath("/Uploads")));

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

                    productWorksheet.Cells[1, 1].Value = models.Select(x => x.OilDate).Min().Month.ToString() + "/" + models.Select(x => x.OilDate).Min().Year.ToString();

                    foreach (var listItem in queryPlan)
                    {

                        foreach (var item in listItem)
                        {
                            productWorksheet.Cells[i, 1].Value = item.NumberPlate;
                            productWorksheet.Cells[i, 2].Value = item.CarOwerName;
                            productWorksheet.Cells[i, 3].Value = item.Distance;
                            productWorksheet.Cells[i, 3].Style.Numberformat.Format = "#,##0.00";
                            productWorksheet.Cells[i, 4].Value = item.OilLevel;
                            productWorksheet.Cells[i, 4].Style.Numberformat.Format = "#,##0.00";
                            productWorksheet.Cells[i, 5].Value = item.SuppliedFromLevel;
                            productWorksheet.Cells[i, 5].Style.Numberformat.Format = "#,##0.00";
                            if (item.OtherRun != 0)
                            {
                                productWorksheet.Cells[i, 6].Value = item.OtherRun;
                                productWorksheet.Cells[i, 6].Style.Numberformat.Format = "#,##0.00";
                            }
                            productWorksheet.Cells[i, 7].Value = item.AmountOil;
                            productWorksheet.Cells[i, 7].Style.Numberformat.Format = "#,##0.00";
                            productWorksheet.Cells[i, 8].Value = item.SuppliedOil;
                            productWorksheet.Cells[i, 8].Style.Numberformat.Format = "#,##0.00";
                            productWorksheet.Cells[i, 9].Value = item.DifferenceOil;
                            productWorksheet.Cells[i, 9].Style.Numberformat.Format = "#,##0.00";
                            productWorksheet.Cells[i, 9].Style.Font.Bold = true;
                            productWorksheet.Cells[i, 10].Value = item.Total;
                            productWorksheet.Cells[i, 10].Style.Numberformat.Format = "#,##0.00";
                            productWorksheet.Cells[i, 10].Style.Font.Bold = true;
                            i++;
                        }
                    }

                    var addressColumn = "";
                    ////sumtotal
                    for (int k = 3; k <= 10; k++)
                    {
                        addressColumn = productWorksheet.Cells[i, k].Address.Substring(0, 1);
                        productWorksheet.Cells[i, k].Style.Font.Bold = true;
                        productWorksheet.Cells[i, k].Formula = "= SUBTOTAL(9," + addressColumn + "4:" + addressColumn + (i - 1).ToString() + ")";
                        productWorksheet.Cells[i, k].Style.Numberformat.Format = "#,##0.00";
                    }

                    var setFilter = "A3:J" + (i).ToString();
                    var range = productWorksheet.Cells[setFilter.ToString()];
                    range.AutoFilter = true;


                    string test1 = "A1:K" + (i).ToString();
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
                var fileTemp = new FileInfo(string.Format(@"{0}\ThongKeDau.xlsx", HostingEnvironment.MapPath("/Uploads")));
                var check = ExcelHelper.CheckHeaderFile(fileTemp, file, 2);
                if (check == 1)
                {
                    var result = new UploadManageOilFromExcel().UploadProducts(file, Session["UploadDriverPayProgress"]);

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
                var result = new UploadManageOilFromExcel().DownloadTemplate();
                var fileStream = new MemoryStream(result);
                return File(fileStream, "application/ms-excel", "ThongKeDau_Template.xlsx");
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

            return File(dateGet, "application/ms-excel", "ThongKeDau_UploadResult.xlsx");

        }

        [HttpPost]
        public ActionResult Deletes(string stringSubmit)
        {
            List<ManageOilViewModel> listData = new List<ManageOilViewModel>();
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
                var listManageOil = db.ManageOils.Where(x => DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.OilDate) && DbFunctions.TruncateTime(x.OilDate) <= DbFunctions.TruncateTime(lastDayOfMonth) && x.IsRemove != true).ToList();
                foreach (var item in listManageOil)
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
            List<ManageOil> listData = new List<ManageOil>();
            var dataListJson = dataStringDelete.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                ManageOil dataObj = null;
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
                dataObj = JsonConvert.DeserializeObject<ManageOil>(dataObjString);
                dataObj.OilDate = dataObj.OilDate.ToLocalTime();
                listData.Add(dataObj);
            }

            var temp = new List<ManageOil>();
            foreach (var item in listData)
            {
                try
                {
                    var oilChange = db.ManageOils.Find(item.ID);
                    oilChange.IsRemove = true;
                    oilChange.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    oilChange.ModifiedDate = DateTime.Now;
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
                return RedirectToAction("OilDetail", new { dataString = dataStringDate });
            }
            else
            {
                ViewBag.StartupScript = "deletes_success();";
                return RedirectToAction("OilDetail", new { dataString = dataStringDate });
            }

        }
    }
}