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

    public class ManageSalaryController : Controller
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
            var model = db.ManageSalarys.Where(x => x.IsRemove != true).ToList();

            var data = model.Select(k => new { k.SalaryDate.Year, k.SalaryDate.Month, k.Total }).GroupBy(x => new { x.Year, x.Month }, (key, group) => new
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

        public ActionResult SalaryDetail(string dataString)
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
        public ActionResult Salary_Read([DataSourceRequest] DataSourceRequest request, string datetime)
        {
            string[] arrListStr = datetime.Split(new char[] { 't', 'o' });
            var firstDay = DateTime.Parse(arrListStr[0].Format("{0:dd-MM-yyyy}"));
            var lastDay = DateTime.Parse(arrListStr[2].Format("{0:dd-MM-yyyy}"));

            var firstDayOfMonth = new DateTime(firstDay.Year, firstDay.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var salaryQuery = from a in db.ManageSalarys.Where(x => DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.SalaryDate) && DbFunctions.TruncateTime(x.SalaryDate) <= DbFunctions.TruncateTime(lastDayOfMonth) && x.IsRemove != true)
                              join b in db.Vehicle on a.VehicleID equals b.ID into group1
                              from b in group1.DefaultIfEmpty()
                              join c in db.Vehicle on a.DriverID equals c.ID into group2
                              from c in group2.DefaultIfEmpty()
                              select new
                              {
                                  a.ID,
                                  a.Note,
                                  a.SalaryDate,
                                  a.Distance,
                                  a.WorkDay,
                                  a.WorkDayPrice,
                                  a.DistancePrice,
                                  a.PhoneCosts,
                                  a.SupportCosts,
                                  a.BonusCosts,
                                  a.InsuranceCosts,
                                  a.VehicleID,
                                  c.CarOwerName,
                                  b.NumberPlate,
                                  a.Total,
                                  a.WorkDayTotal,
                                  a.DistanceTotal,
                                  a.SalaryTotal
                              };

            var test = salaryQuery.ToList();

            var jsonResult = Json(salaryQuery.ToList().OrderByDescending(x => x.ID).ToDataSourceResult(request));
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult Add()
        {
            var model = new ManageSalaryViewModel();
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add([Bind(Exclude = "")] ManageSalaryViewModel model, DateTime? date)
        {
            if (ModelState.IsValid)
            {
                if (date == null)
                {
                    ModelState.AddModelError("CustomErrorDate", AccountResources.RequiredPlanDate);
                    return View(model);
                }
                if (model.VehicleID == null)
                {
                    model.VehicleID = model.DriverID;
                }
                model.SalaryDate = DateTime.Parse(date.ToString());

                var firstDayOfMonth = new DateTime(model.SalaryDate.Year, model.SalaryDate.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                var check = db.ManageSalarys.Where(x => x.VehicleID == model.VehicleID && x.DriverID == model.DriverID && x.IsRemove != true
                && DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.SalaryDate) && DbFunctions.TruncateTime(x.SalaryDate) <= DbFunctions.TruncateTime(lastDayOfMonth)).Any();
                if (check)
                {
                    ModelState.AddModelError("", "Đã tồn tại bản ghi trùng tên lái xe và biển kiểm soát trên hệ thống!");
                    return View(model);
                }
                else
                {
                    var manageSalary = new ManageSalary();
                    manageSalary.DriverID = model.DriverID;
                    manageSalary.VehicleID = model.VehicleID;
                    manageSalary.SalaryDate = model.SalaryDate;


                    String workDayString = (model.WorkDayString).Replace('.', ',');
                    double workDay = Double.Parse(workDayString);
                    manageSalary.WorkDay = workDay;


                    var distance = db.ManageOils.Where(x => x.VehicleID == manageSalary.VehicleID && x.DriverID == manageSalary.DriverID && x.IsRemove != true
                    && DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.OilDate) && DbFunctions.TruncateTime(x.OilDate) <= DbFunctions.TruncateTime(lastDayOfMonth)).FirstOrDefault();
                    if (distance == null)
                    {
                        manageSalary.Distance = 0;
                    }
                    else
                    {
                        manageSalary.Distance = distance.Distance;
                    }

                    String distancePriceString = (model.DistancePriceString).Replace('.', ',');
                    double distancePrice = Double.Parse(distancePriceString);
                    manageSalary.DistancePrice = distancePrice;


                    String workDayPriceString = (model.WorkDayPriceString).Replace('.', ',');
                    double workDayPrice = Double.Parse(workDayPriceString);
                    manageSalary.WorkDayPrice = workDayPrice;

                    if (model.PhoneCostsString != null)
                    {
                        String phoneCostsString = (model.PhoneCostsString).Replace('.', ',');
                        double phoneCost = Double.Parse(phoneCostsString);
                        manageSalary.PhoneCosts = phoneCost;
                    }

                    if (model.SupportCostsString != null)
                    {
                        String supportCostsString = (model.SupportCostsString).Replace('.', ',');
                        double supportCosts = Double.Parse(supportCostsString);
                        manageSalary.SupportCosts = supportCosts;
                    }

                    if (model.BonusCostsString != null)
                    {
                        String bonusCostsString = (model.BonusCostsString).Replace('.', ',');
                        double bonusCosts = Double.Parse(bonusCostsString);
                        manageSalary.BonusCosts = bonusCosts;
                    }
                    if (model.InsuranceCostsString != null)
                    {
                        String insuranceCostsString = (model.InsuranceCostsString).Replace('.', ',');
                        double insuranceCosts = Double.Parse(insuranceCostsString);
                        manageSalary.InsuranceCosts = insuranceCosts;
                    }

                    Double phone = 0; Double bonus = 0; Double support = 0; Double insurance = 0;
                    if (manageSalary.PhoneCosts != null)
                    {
                        phone = (Double)manageSalary.PhoneCosts;
                    }

                    if (manageSalary.BonusCosts != null)
                    {
                        bonus = (Double)manageSalary.BonusCosts;
                    }

                    if (manageSalary.SupportCosts != null)
                    {
                        support = (Double)manageSalary.SupportCosts;
                    }

                    if (manageSalary.InsuranceCosts != null)
                    {
                        insurance = (Double)manageSalary.InsuranceCosts;
                    }

                    manageSalary.WorkDayTotal = manageSalary.WorkDay * manageSalary.WorkDayPrice;
                    manageSalary.DistanceTotal = manageSalary.Distance * manageSalary.DistancePrice;
                    manageSalary.SalaryTotal = manageSalary.WorkDayTotal + manageSalary.DistanceTotal + phone + bonus + support;
                    manageSalary.Total = manageSalary.SalaryTotal + insurance;

                    manageSalary.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    manageSalary.CreatedDate = DateTime.Now;

                    db.Set<ManageSalary>().Add(manageSalary);
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
            var model = db.Set<ManageSalary>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            var manageOilView = new ManageSalaryViewModel();
            manageOilView.ID = model.ID;
            manageOilView.SalaryDate = model.SalaryDate;
            manageOilView.Note = model.Note;
            manageOilView.DriverID = model.DriverID;

            if (model.DriverID != model.VehicleID)
            {
                manageOilView.VehicleID = model.VehicleID;
            }

            manageOilView.WorkDayString = model.WorkDay.ToString().Replace(",", ".");
            manageOilView.DistancePriceString = model.DistancePrice.ToString().Replace(",", ".");
            manageOilView.WorkDayPriceString = model.WorkDayPrice.ToString().Replace(",", ".");

            if (model.PhoneCosts != null)
            {
                manageOilView.PhoneCostsString = model.PhoneCosts.ToString().Replace(",", ".");
            }

            if (model.BonusCosts != null)
            {
                manageOilView.BonusCostsString = model.BonusCosts.ToString().Replace(",", ".");
            }

            if (model.SupportCosts != null)
            {
                manageOilView.SupportCostsString = model.SupportCosts.ToString().Replace(",", ".");
            }

            if (model.InsuranceCosts != null)
            {
                manageOilView.InsuranceCostsString = model.InsuranceCosts.ToString().Replace(",", ".");
            }

            return View("Edit", manageOilView);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "")] ManageSalaryViewModel model, DateTime? date)
        {
            if (ModelState.IsValid)
            {
                if (date == null)
                {
                    ModelState.AddModelError("CustomErrorDate", AccountResources.RequiredPlanDate);
                    return View(model);
                }
                if (model.VehicleID == null)
                {
                    model.VehicleID = model.DriverID;
                }

                model.SalaryDate = DateTime.Parse(date.ToString());

                var manageSalary = new ManageSalary();

                manageSalary.ID = model.ID;
                manageSalary.DriverID = model.DriverID;
                manageSalary.VehicleID = model.VehicleID;
                manageSalary.SalaryDate = model.SalaryDate;

                var firstDayOfMonth = new DateTime(manageSalary.SalaryDate.Year, manageSalary.SalaryDate.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                var check = db.ManageSalarys.Where(x => x.VehicleID == manageSalary.VehicleID && x.DriverID == manageSalary.DriverID && x.IsRemove != true && x.ID != manageSalary.ID
                && DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.SalaryDate) && DbFunctions.TruncateTime(x.SalaryDate) <= DbFunctions.TruncateTime(lastDayOfMonth)).Any();
                if (check)
                {
                    ModelState.AddModelError("", "Đã tồn tại bản ghi trùng tên lái xe và biển kiểm soát trên hệ thống!");
                    return View(model);
                }
                else
                {
                    String workDayString = (model.WorkDayString).Replace('.', ',');
                    double workDay = Double.Parse(workDayString);
                    manageSalary.WorkDay = workDay;

                    var distance = db.ManageOils.Where(x => x.VehicleID == manageSalary.VehicleID && x.DriverID == manageSalary.DriverID && x.IsRemove != true
                    && DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.OilDate) && DbFunctions.TruncateTime(x.OilDate) <= DbFunctions.TruncateTime(lastDayOfMonth)).FirstOrDefault();
                    if (distance == null)
                    {
                        manageSalary.Distance = 0;
                    }
                    else
                    {
                        manageSalary.Distance = distance.Distance;
                    }

                    String distancePriceString = (model.DistancePriceString).Replace('.', ',');
                    double distancePrice = Double.Parse(distancePriceString);
                    manageSalary.DistancePrice = distancePrice;


                    String workDayPriceString = (model.WorkDayPriceString).Replace('.', ',');
                    double workDayPrice = Double.Parse(workDayPriceString);
                    manageSalary.WorkDayPrice = workDayPrice;

                    if (model.PhoneCostsString != null)
                    {
                        String phoneCostsString = (model.PhoneCostsString).Replace('.', ',');
                        double phoneCost = Double.Parse(phoneCostsString);
                        manageSalary.PhoneCosts = phoneCost;
                    }

                    if (model.SupportCostsString != null)
                    {
                        String supportCostsString = (model.SupportCostsString).Replace('.', ',');
                        double supportCosts = Double.Parse(supportCostsString);
                        manageSalary.SupportCosts = supportCosts;
                    }

                    if (model.BonusCostsString != null)
                    {
                        String bonusCostsString = (model.BonusCostsString).Replace('.', ',');
                        double bonusCosts = Double.Parse(bonusCostsString);
                        manageSalary.BonusCosts = bonusCosts;
                    }
                    if (model.InsuranceCostsString != null)
                    {
                        String insuranceCostsString = (model.InsuranceCostsString).Replace('.', ',');
                        double insuranceCosts = Double.Parse(insuranceCostsString);
                        manageSalary.InsuranceCosts = insuranceCosts;
                    }

                    Double phone = 0; Double bonus = 0; Double support = 0; Double insurance = 0;
                    if (manageSalary.PhoneCosts != null)
                    {
                        phone = (Double)manageSalary.PhoneCosts;
                    }

                    if (manageSalary.BonusCosts != null)
                    {
                        bonus = (Double)manageSalary.BonusCosts;
                    }

                    if (manageSalary.SupportCosts != null)
                    {
                        support = (Double)manageSalary.SupportCosts;
                    }

                    if (manageSalary.InsuranceCosts != null)
                    {
                        insurance = (Double)manageSalary.InsuranceCosts;
                    }
                    manageSalary.WorkDayTotal = manageSalary.WorkDay * manageSalary.WorkDayPrice;
                    manageSalary.DistanceTotal = manageSalary.Distance * manageSalary.DistancePrice;
                    manageSalary.SalaryTotal = manageSalary.WorkDayTotal + manageSalary.DistanceTotal + phone + bonus + support;
                    manageSalary.Total = manageSalary.SalaryTotal + insurance;

                    manageSalary.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    manageSalary.ModifiedDate = DateTime.Now;

                    db.ManageSalarys.Attach(manageSalary);
                    db.Entry(manageSalary).Property(a => a.SalaryDate).IsModified = true;
                    db.Entry(manageSalary).Property(a => a.Note).IsModified = true;
                    db.Entry(manageSalary).Property(a => a.VehicleID).IsModified = true;
                    db.Entry(manageSalary).Property(a => a.DriverID).IsModified = true;
                    db.Entry(manageSalary).Property(a => a.Distance).IsModified = true;
                    db.Entry(manageSalary).Property(a => a.WorkDay).IsModified = true;
                    db.Entry(manageSalary).Property(a => a.WorkDayPrice).IsModified = true;
                    db.Entry(manageSalary).Property(a => a.DistancePrice).IsModified = true;
                    db.Entry(manageSalary).Property(a => a.WorkDayTotal).IsModified = true;
                    db.Entry(manageSalary).Property(a => a.DistanceTotal).IsModified = true;
                    db.Entry(manageSalary).Property(a => a.PhoneCosts).IsModified = true;
                    db.Entry(manageSalary).Property(a => a.SalaryTotal).IsModified = true;
                    db.Entry(manageSalary).Property(a => a.SupportCosts).IsModified = true;
                    db.Entry(manageSalary).Property(a => a.BonusCosts).IsModified = true;
                    db.Entry(manageSalary).Property(a => a.InsuranceCosts).IsModified = true;
                    db.Entry(manageSalary).Property(a => a.Total).IsModified = true;
                    db.Entry(manageSalary).Property(a => a.ModifiedBy).IsModified = true;
                    db.Entry(manageSalary).Property(a => a.ModifiedDate).IsModified = true;
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
            List<ManageSalaryViewModel> listData = new List<ManageSalaryViewModel>();
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

                var driverPayQuery = (from a in db.ManageSalarys.Where(x => DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.SalaryDate) && DbFunctions.TruncateTime(x.SalaryDate) <= DbFunctions.TruncateTime(lastDayOfMonth) && x.IsRemove != true)
                                      join b in db.Vehicle on a.VehicleID equals b.ID into group1
                                      from b in group1.DefaultIfEmpty()
                                      join c in db.Vehicle on a.DriverID equals c.ID into group2
                                      from c in group2.DefaultIfEmpty()
                                      select new
                                      {
                                          a.ID,
                                          a.Note,
                                          a.SalaryDate,
                                          a.Distance,
                                          a.WorkDay,
                                          a.WorkDayPrice,
                                          a.DistancePrice,
                                          a.PhoneCosts,
                                          a.SupportCosts,
                                          a.BonusCosts,
                                          a.InsuranceCosts,
                                          a.VehicleID,
                                          c.CarOwerName,
                                          b.NumberPlate,
                                          a.Total,
                                          a.WorkDayTotal,
                                          a.DistanceTotal,
                                          a.SalaryTotal

                                      }).AsEnumerable()
                                        .Select(B => new ManageSalaryViewModel()
                                        {
                                            ID = B.ID,
                                            Note = B.Note,
                                            SalaryDate = B.SalaryDate,
                                            Distance = B.Distance,
                                            WorkDay = B.WorkDay,
                                            DistancePrice = B.DistancePrice,
                                            WorkDayPrice = B.WorkDayPrice,
                                            PhoneCosts = B.PhoneCosts,
                                            SupportCosts = B.SupportCosts,
                                            BonusCosts = B.BonusCosts,
                                            InsuranceCosts = B.InsuranceCosts,
                                            VehicleID = B.VehicleID,
                                            CarOwerName = B.CarOwerName,
                                            NumberPlate = B.NumberPlate,
                                            Total = B.Total,
                                            SalaryTotal = B.SalaryTotal,
                                            WorkDayTotal = B.WorkDayTotal,
                                            DistanceTotal = B.DistanceTotal
                                        }).ToList();

                listData.AddRange(driverPayQuery);
            }

            var result = DownloadManageOil(listData.OrderByDescending(x => x.ID).ToList());
            var fileStream = new MemoryStream(result);
            return File(fileStream, "application/ms-excel", "Thống_Kê_Bảng_Lương.xlsx");
        }

        [HttpPost]
        public ActionResult ExportExcel(string dataString)
        {
            List<ManageSalaryViewModel> listData = new List<ManageSalaryViewModel>();
            var dataListJson = dataString.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                ManageSalaryViewModel dataObj = null;
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
                dataObj = JsonConvert.DeserializeObject<ManageSalaryViewModel>(dataObjString);
                dataObj.SalaryDate = dataObj.SalaryDate.ToLocalTime();
                listData.Add(dataObj);
            }
            var result = DownloadManageOil(listData.OrderByDescending(x => x.ID).ToList());
            var fileStream = new MemoryStream(result);
            return File(fileStream, "application/ms-excel", "Thống_Kê_Bảng_Lương.xlsx");
        }
        public byte[] DownloadManageOil(List<ManageSalaryViewModel> models)
        {

            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\ThongKeBangLuong.xlsx", HostingEnvironment.MapPath("/Uploads")));

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

                    int i = 5;
                    List<String> listNumber = new List<String>();
                    var queryPlan = from a in models
                                    group a by new { a.NumberPlate };

                    var queryPlanForDriver = from a in models
                                             group a by new { a.CarOwerName };

                    foreach (var listPlan in queryPlan)
                    {
                        listNumber.Add(listPlan.First().NumberPlate);
                    }

                    productWorksheet.Cells[1, 1].Value = models.Select(x => x.SalaryDate).Min().Month.ToString() + "/" + models.Select(x => x.SalaryDate).Min().Year.ToString();

                    foreach (var listItem in queryPlan)
                    {
                        foreach (var item in listItem)
                        {

                            productWorksheet.Cells[i, 1].Value = i - 4;
                            productWorksheet.Cells[i, 2].Value = item.NumberPlate;
                            productWorksheet.Cells[i, 3].Value = item.CarOwerName;
                            productWorksheet.Cells[i, 4].Value = item.WorkDay;
                            productWorksheet.Cells[i, 4].Style.Numberformat.Format = "#,##0";
                            productWorksheet.Cells[i, 5].Value = item.Distance;
                            productWorksheet.Cells[i, 5].Style.Numberformat.Format = "#,##0.00";
                            productWorksheet.Cells[i, 6].Value = item.WorkDayPrice;
                            productWorksheet.Cells[i, 6].Style.Numberformat.Format = "#,##0";
                            productWorksheet.Cells[i, 7].Value = item.DistancePrice;
                            productWorksheet.Cells[i, 7].Style.Numberformat.Format = "#,##0";
                            productWorksheet.Cells[i, 8].Value = item.WorkDayTotal;
                            productWorksheet.Cells[i, 8].Style.Numberformat.Format = "#,##0";
                            productWorksheet.Cells[i, 9].Value = item.DistanceTotal;
                            productWorksheet.Cells[i, 9].Style.Numberformat.Format = "#,##0";
                            if (item.PhoneCosts != null)
                            {
                                productWorksheet.Cells[i, 10].Value = item.PhoneCosts;
                                productWorksheet.Cells[i, 10].Style.Numberformat.Format = "#,##0";
                            }
                            if (item.SupportCosts != null)
                            {
                                productWorksheet.Cells[i, 11].Value = item.SupportCosts;
                                productWorksheet.Cells[i, 11].Style.Numberformat.Format = "#,##0";
                            }
                            if (item.BonusCosts != null)
                            {
                                productWorksheet.Cells[i, 12].Value = item.BonusCosts;
                                productWorksheet.Cells[i, 12].Style.Numberformat.Format = "#,##0";
                            }

                            productWorksheet.Cells[i, 13].Value = item.SalaryTotal;
                            productWorksheet.Cells[i, 13].Style.Numberformat.Format = "#,##0";

                            if (item.InsuranceCosts != null)
                            {
                                productWorksheet.Cells[i, 14].Value = item.InsuranceCosts;
                                productWorksheet.Cells[i, 14].Style.Numberformat.Format = "#,##0";
                            }

                            productWorksheet.Cells[i, 15].Formula = "=ROUND(M" + i + "+N" + i + ",0)";
                            productWorksheet.Cells[i, 15].Style.Numberformat.Format = "#,##0";
                            productWorksheet.Cells[i, 15].Style.Font.Bold = true;

                            productWorksheet.Cells[i, 16].Value = item.Note;
                            i++;
                        }
                    }

                    var addressColumn = "";
                    ////sumtotal
                    for (int k = 4; k <= 15; k++)
                    {
                        addressColumn = productWorksheet.Cells[i, k].Address.Substring(0, 1);
                        productWorksheet.Cells[i, k].Style.Font.Bold = true;
                        productWorksheet.Cells[i, k].Formula = "= SUBTOTAL(9," + addressColumn + "5:" + addressColumn + (i - 1).ToString() + ")";
                        productWorksheet.Cells[i, k].Style.Numberformat.Format = "#,##0";
                    }

                    var setFilter = "A4:O" + (i).ToString();
                    var range = productWorksheet.Cells[setFilter.ToString()];
                    range.AutoFilter = true;


                    string test1 = "A1:P" + (i).ToString();
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
                var fileTemp = new FileInfo(string.Format(@"{0}\ThongKeBangLuong.xlsx", HostingEnvironment.MapPath("/Uploads")));
                var check = ExcelHelper.CheckHeaderFile(fileTemp, file, 2);
                if (check == 1)
                {
                    var result = new UploadManageSalaryFromExcel().UploadProducts(file, Session["UploadDriverPayProgress"]);

                    if (result == null)
                    {
                        ViewBag.check = "Upload bảng lương thành công!";
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
                var result = new UploadManageSalaryFromExcel().DownloadTemplate();
                var fileStream = new MemoryStream(result);
                return File(fileStream, "application/ms-excel", "Thong_Ke_Bang_Lương_Template.xlsx");
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

            return File(dateGet, "application/ms-excel", "Thống_kê_bảng_lương_UploadResult.xlsx");

        }

        [HttpPost]
        public ActionResult Deletes(string stringSubmit)
        {
            List<ManageSalaryViewModel> listData = new List<ManageSalaryViewModel>();
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
                var listManageSalary = db.ManageSalarys.Where(x => DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.SalaryDate) && DbFunctions.TruncateTime(x.SalaryDate) <= DbFunctions.TruncateTime(lastDayOfMonth) && x.IsRemove != true).ToList();
                foreach (var item in listManageSalary)
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
            List<ManageSalary> listData = new List<ManageSalary>();
            var dataListJson = dataStringDelete.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                ManageSalary dataObj = null;
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
                dataObj = JsonConvert.DeserializeObject<ManageSalary>(dataObjString);
                dataObj.SalaryDate = dataObj.SalaryDate.ToLocalTime();
                listData.Add(dataObj);
            }

            var temp = new List<ManageSalary>();
            foreach (var item in listData)
            {
                try
                {
                    var salaryChange = db.ManageSalarys.Find(item.ID);
                    salaryChange.IsRemove = true;
                    salaryChange.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                    salaryChange.ModifiedDate = DateTime.Now;
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
                return RedirectToAction("SalaryDetail", new { dataString = dataStringDate });
            }
            else
            {
                ViewBag.StartupScript = "deletes_success();";
                return RedirectToAction("SalaryDetail", new { dataString = dataStringDate });
            }

        }
    }
}