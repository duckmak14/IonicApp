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

    public class ManageOtherCostController : Controller
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
            var model = db.OtherCosts.Where(x => x.IsRemove != true).ToList();

            var data = model.Select(k => new { k.OtherCostDate.Year, k.OtherCostDate.Month, k.Total }).GroupBy(x => new { x.Year, x.Month }, (key, group) => new
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

        public ActionResult OtherCostDetail(string dataString)
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
        public ActionResult OtherCosts_Read([DataSourceRequest] DataSourceRequest request, string datetime)
        {
            string[] arrListStr = datetime.Split(new char[] { 't', 'o' });
            var firstDay = DateTime.Parse(arrListStr[0].Format("{0:dd-MM-yyyy}"));
            var lastDay = DateTime.Parse(arrListStr[2].Format("{0:dd-MM-yyyy}"));

            var firstDayOfMonth = new DateTime(firstDay.Year, firstDay.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var salaryQuery = from a in db.OtherCosts.Where(x => DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.OtherCostDate) && DbFunctions.TruncateTime(x.OtherCostDate) <= DbFunctions.TruncateTime(lastDayOfMonth) && x.IsRemove != true)
                              join b in db.Vehicle on a.VehicleID equals b.ID into group1
                              from b in group1.DefaultIfEmpty()
                              join c in db.Vehicle on a.DriverID equals c.ID into group2
                              from c in group2.DefaultIfEmpty()
                              select new
                              {
                                  a.ID,
                                  a.Note,
                                  a.OtherCosts,
                                  a.AdvanceCosts,
                                  a.MortgageCosts,
                                  a.OtherCostDate,
                                  a.VehicleID,
                                  c.CarOwerName,
                                  b.NumberPlate,
                                  a.Total
                              };

            var test = salaryQuery.ToList();

            var jsonResult = Json(salaryQuery.ToList().OrderByDescending(x => x.ID).ToDataSourceResult(request));
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult Add()
        {
            var model = new OtherCostViewModel();
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add([Bind(Exclude = "")] OtherCostViewModel model, DateTime? date)
        {
            if (ModelState.IsValid)
            {
                if (date == null)
                {
                    ModelState.AddModelError("", AccountResources.RequiredPlanDate);
                    return View(model);
                }

                if (model.OtherCostsString != null && model.AdvanceCostsString != null && model.MortgageCostsString != null)
                {
                    ModelState.AddModelError("", "Vui lòng điền 1 trong 3 mục chi phí!");
                    return View(model);
                }

                if (model.VehicleID == null)
                {
                    model.VehicleID = model.DriverID;
                }
                var otherCost = new OtherCost();
                otherCost.DriverID = model.DriverID;
                otherCost.VehicleID = model.VehicleID;
                otherCost.OtherCostDate = DateTime.Parse(date.ToString());

                var firstDayOfMonth = new DateTime(otherCost.OtherCostDate.Year, otherCost.OtherCostDate.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                var check = db.OtherCosts.Where(x => x.VehicleID == otherCost.VehicleID && x.DriverID == otherCost.DriverID && x.IsRemove != true
                && DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.OtherCostDate) && DbFunctions.TruncateTime(x.OtherCostDate) <= DbFunctions.TruncateTime(lastDayOfMonth)).Any();
                if (check)
                {
                    ModelState.AddModelError("", "Đã tồn tại bản ghi trùng tên lái xe và biển kiểm soát trên hệ thống!");
                    return View(model);
                }
                else
                {
                   

                    if (model.AdvanceCostsString != null)
                    {
                        String advanceCostsString = (model.AdvanceCostsString).Replace('.', ',');
                        double advanceCosts = Double.Parse(advanceCostsString);
                        otherCost.AdvanceCosts = advanceCosts;
                    }

                    if (model.MortgageCostsString != null)
                    {
                        String mortgageCostsString = (model.MortgageCostsString).Replace('.', ',');
                        double mortgageCosts = Double.Parse(mortgageCostsString);
                        otherCost.MortgageCosts = mortgageCosts;
                    }

                    if (model.OtherCostsString != null)
                    {
                        String otherCostsString = (model.OtherCostsString).Replace('.', ',');
                        double otherCosts = Double.Parse(otherCostsString);
                        otherCost.OtherCosts = otherCosts;
                    }

                    Double other = 0; Double advance = 0; Double mortgage = 0;
                    if (otherCost.OtherCosts != null)
                    {
                        other = (Double)otherCost.OtherCosts;
                    }

                    if (otherCost.AdvanceCosts != null)
                    {
                        advance = (Double)otherCost.AdvanceCosts;
                    }

                    if (otherCost.MortgageCosts != null)
                    {
                        mortgage = (Double)otherCost.MortgageCosts;
                    }

                    otherCost.Total = other + advance + mortgage;
                    otherCost.Note = model.Note;

                    otherCost.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    otherCost.CreatedDate = DateTime.Now;

                    db.Set<OtherCost>().Add(otherCost);
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
            var model = db.Set<OtherCost>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            var manageOilView = new OtherCostViewModel();
            manageOilView.ID = model.ID;
            manageOilView.OtherCostDate = model.OtherCostDate;
            manageOilView.Note = model.Note;
            manageOilView.DriverID = model.DriverID;

            if (model.DriverID != model.VehicleID)
            {
                manageOilView.VehicleID = model.VehicleID;
            }

            if (model.AdvanceCosts != null)
            {
                manageOilView.AdvanceCostsString = model.AdvanceCosts.ToString().Replace(",", ".");
            }

            if (model.MortgageCosts != null)
            {
                manageOilView.MortgageCostsString = model.MortgageCosts.ToString().Replace(",", ".");
            }

            if (model.OtherCosts != null)
            {
                manageOilView.OtherCostsString = model.OtherCosts.ToString().Replace(",", ".");
            }

            return View("Edit", manageOilView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "")] OtherCostViewModel model, DateTime? date)
        {
            if (ModelState.IsValid)
            {
                if (date == null)
                {
                    ModelState.AddModelError("", AccountResources.RequiredPlanDate);
                    return View(model);
                }

                if (model.OtherCostsString != null && model.AdvanceCostsString != null && model.MortgageCostsString != null)
                {
                    ModelState.AddModelError("", "Vui lòng điền 1 trong 3 mục chi phí!");
                    return View(model);
                }

                if (model.VehicleID == null)
                {
                    model.VehicleID = model.DriverID;
                }
                var otherCost = new OtherCost();
                otherCost.ID = model.ID;
                otherCost.DriverID = model.DriverID;
                otherCost.VehicleID = model.VehicleID;
                otherCost.OtherCostDate = DateTime.Parse(date.ToString());
                 
                var firstDayOfMonth = new DateTime(otherCost.OtherCostDate.Year, otherCost.OtherCostDate.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                var check = db.OtherCosts.Where(x => x.VehicleID == otherCost.VehicleID && x.DriverID == otherCost.DriverID && x.IsRemove != true && x.ID != otherCost.ID
                && DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.OtherCostDate) && DbFunctions.TruncateTime(x.OtherCostDate) <= DbFunctions.TruncateTime(lastDayOfMonth)).Any();
                if (check)
                {
                    ModelState.AddModelError("", "Đã tồn tại bản ghi trùng tên lái xe và biển kiểm soát trên hệ thống!");
                    return View(model);
                }
                else
                {

                    if (model.AdvanceCostsString != null)
                    {
                        String advanceCostsString = (model.AdvanceCostsString).Replace('.', ',');
                        double advanceCosts = Double.Parse(advanceCostsString);
                        otherCost.AdvanceCosts = advanceCosts;
                    }

                    if (model.MortgageCostsString != null)
                    {
                        String mortgageCostsString = (model.MortgageCostsString).Replace('.', ',');
                        double mortgageCosts = Double.Parse(mortgageCostsString);
                        otherCost.MortgageCosts = mortgageCosts;
                    }

                    if (model.OtherCostsString != null)
                    {
                        String otherCostsString = (model.OtherCostsString).Replace('.', ',');
                        double otherCosts = Double.Parse(otherCostsString);
                        otherCost.OtherCosts = otherCosts;
                    }

                    Double other = 0; Double advance = 0; Double mortgage = 0;
                    if (otherCost.OtherCosts != null)
                    {
                        other = (Double)otherCost.OtherCosts;
                    }

                    if (otherCost.AdvanceCosts != null)
                    {
                        advance = (Double)otherCost.AdvanceCosts;
                    }

                    if (otherCost.MortgageCosts != null)
                    {
                        mortgage = (Double)otherCost.MortgageCosts;
                    }

                    otherCost.Total = other + advance + mortgage;
                    otherCost.Note = model.Note;

                    otherCost.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    otherCost.ModifiedDate = DateTime.Now;

                    db.OtherCosts.Attach(otherCost);
                    db.Entry(otherCost).Property(a => a.OtherCostDate).IsModified = true;
                    db.Entry(otherCost).Property(a => a.Note).IsModified = true;
                    db.Entry(otherCost).Property(a => a.VehicleID).IsModified = true;
                    db.Entry(otherCost).Property(a => a.DriverID).IsModified = true;
                    db.Entry(otherCost).Property(a => a.AdvanceCosts).IsModified = true;
                    db.Entry(otherCost).Property(a => a.MortgageCosts).IsModified = true;
                    db.Entry(otherCost).Property(a => a.OtherCosts).IsModified = true;
                    db.Entry(otherCost).Property(a => a.Total).IsModified = true;
                    db.Entry(otherCost).Property(a => a.ModifiedDate).IsModified = true;
                    db.Entry(otherCost).Property(a => a.ModifiedBy).IsModified = true;
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
            List<OtherCostViewModel> listData = new List<OtherCostViewModel>();
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

                var salaryQuery = (from a in db.OtherCosts.Where(x => DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.OtherCostDate) && DbFunctions.TruncateTime(x.OtherCostDate) <= DbFunctions.TruncateTime(lastDayOfMonth) && x.IsRemove != true)
                                   join b in db.Vehicle on a.VehicleID equals b.ID into group1
                                   from b in group1.DefaultIfEmpty()
                                   join c in db.Vehicle on a.DriverID equals c.ID into group2
                                   from c in group2.DefaultIfEmpty()
                                   select new
                                   {
                                       a.ID,
                                       a.Note,
                                       a.OtherCosts,
                                       a.AdvanceCosts,
                                       a.MortgageCosts,
                                       a.OtherCostDate,
                                       a.VehicleID,
                                       c.CarOwerName,
                                       b.NumberPlate,
                                       a.Total
                                   }).AsEnumerable()
                                        .Select(B => new OtherCostViewModel()
                                        {
                                            ID = B.ID,
                                            Note = B.Note,
                                            OtherCosts = B.OtherCosts,
                                            OtherCostDate = B.OtherCostDate,
                                            AdvanceCosts = B.AdvanceCosts,
                                            MortgageCosts = B.MortgageCosts,
                                            VehicleID = B.VehicleID,
                                            CarOwerName = B.CarOwerName,
                                            NumberPlate = B.NumberPlate,
                                            Total = B.Total
                                        }).ToList();

                listData.AddRange(salaryQuery);
            }

            var result = DownloadManageOil(listData.OrderByDescending(x => x.ID).ToList());
            var fileStream = new MemoryStream(result);
            return File(fileStream, "application/ms-excel", "Thống_Kê_Chi_Phí_Khác.xlsx");
        }

        [HttpPost]
        public ActionResult ExportExcel(string dataString)
        {
            List<OtherCostViewModel> listData = new List<OtherCostViewModel>();
            var dataListJson = dataString.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                OtherCostViewModel dataObj = null;
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
                dataObj = JsonConvert.DeserializeObject<OtherCostViewModel>(dataObjString);
                dataObj.OtherCostDate = dataObj.OtherCostDate.ToLocalTime();
                listData.Add(dataObj);
            }
            var result = DownloadManageOil(listData);
            var fileStream = new MemoryStream(result);
            return File(fileStream, "application/ms-excel", "Thống_Kê_Chi_Phí_Khác.xlsx");
        }
        public byte[] DownloadManageOil(List<OtherCostViewModel> models)
        {

            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\ThongKeChiPhiKhac.xlsx", HostingEnvironment.MapPath("/Uploads")));

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

                    int i = 3;
                    List<String> listNumber = new List<String>();
                    var queryPlan = from a in models
                                    group a by new { a.NumberPlate };

                    var queryPlanForDriver = from a in models
                                             group a by new { a.CarOwerName };

                    foreach (var listPlan in queryPlan)
                    {
                        listNumber.Add(listPlan.First().NumberPlate);
                    }

                    productWorksheet.Cells[1, 1].Value = models.Select(x => x.OtherCostDate).Min().Month.ToString() + "/" + models.Select(x => x.OtherCostDate).Min().Year.ToString();

                    foreach (var listItem in queryPlan)
                    {
                        foreach (var item in listItem)
                        {
                            productWorksheet.Cells[i, 2].Value = item.NumberPlate;
                            productWorksheet.Cells[i, 1].Value = item.CarOwerName;

                            if (item.MortgageCosts != null)
                            {
                                productWorksheet.Cells[i, 3].Value = item.MortgageCosts;
                                productWorksheet.Cells[i, 3].Style.Numberformat.Format = "#,##0";
                            }
                            if (item.AdvanceCosts != null)
                            {
                                productWorksheet.Cells[i, 4].Value = item.AdvanceCosts;
                                productWorksheet.Cells[i, 4].Style.Numberformat.Format = "#,##0";
                            }
                            if (item.OtherCosts != null)
                            {
                                productWorksheet.Cells[i, 5].Value = item.OtherCosts;
                                productWorksheet.Cells[i, 5].Style.Numberformat.Format = "#,##0";
                            }

                            productWorksheet.Cells[i, 6].Formula = "=ROUND(C" + i + "+E" + i + "+D" + i + ",0)";
                            productWorksheet.Cells[i, 6].Style.Numberformat.Format = "#,##0";
                            productWorksheet.Cells[i, 6].Style.Font.Bold = true;

                            productWorksheet.Cells[i, 7].Value = item.Note;
                            i++;
                        }
                    }

                    var addressColumn = "";
                    ////sumtotal
                    for (int k = 3; k <= 6; k++)
                    {
                        addressColumn = productWorksheet.Cells[i, k].Address.Substring(0, 1);
                        productWorksheet.Cells[i, k].Style.Font.Bold = true;
                        productWorksheet.Cells[i, k].Formula = "= SUBTOTAL(9," + addressColumn + "3:" + addressColumn + (i - 1).ToString() + ")";
                        productWorksheet.Cells[i, k].Style.Numberformat.Format = "#,##0";
                    }

                    var setFilter = "A2:F" + (i).ToString();
                    var range = productWorksheet.Cells[setFilter.ToString()];
                    range.AutoFilter = true;


                    string test1 = "A1:G" + (i).ToString();
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
            var result = Session["UploadOtherCostProgress"] == null ? 0 : (int)Session["UploadOtherCostProgress"];

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
                var fileTemp = new FileInfo(string.Format(@"{0}\ThongKeChiPhiKhac.xlsx", HostingEnvironment.MapPath("/Uploads")));
                var check = ExcelHelper.CheckHeaderFile(fileTemp, file, 2);
                if (check == 1)
                {
                    var result = new UploadOtherCostFromExcel().UploadProducts(file, Session["UploadOtherCostProgress"]);


                    if (result == null)
                    {
                        ViewBag.check = "Upload chi phí khác thành công!";
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
                var result = new UploadOtherCostFromExcel().DownloadTemplate();
                var fileStream = new MemoryStream(result);
                return File(fileStream, "application/ms-excel", "Thong_Ke_Chi_Phi_Khac_Template.xlsx");
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
            List<OtherCostViewModel> listData = new List<OtherCostViewModel>();
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
                var listOtherCost = db.OtherCosts.Where(x => DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.OtherCostDate) && DbFunctions.TruncateTime(x.OtherCostDate) <= DbFunctions.TruncateTime(lastDayOfMonth) && x.IsRemove != true).ToList();
                foreach (var item in listOtherCost)
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
            List<OtherCost> listData = new List<OtherCost>();
            var dataListJson = dataStringDelete.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                OtherCost dataObj = null;
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
                dataObj = JsonConvert.DeserializeObject<OtherCost>(dataObjString);
                dataObj.OtherCostDate = dataObj.OtherCostDate.ToLocalTime();
                listData.Add(dataObj);
            }

            var temp = new List<OtherCost>();
            foreach (var item in listData)
            {
                try
                {
                    var otherCostChange = db.OtherCosts.Find(item.ID);
                    otherCostChange.IsRemove = true;
                    otherCostChange.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    otherCostChange.ModifiedDate = DateTime.Now;
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
                return RedirectToAction("OtherCostDetail", new { dataString = dataStringDate });
            }
            else
            {
                ViewBag.StartupScript = "deletes_success();";
                return RedirectToAction("OtherCostDetail", new { dataString = dataStringDate });
            }

        }
    }
}