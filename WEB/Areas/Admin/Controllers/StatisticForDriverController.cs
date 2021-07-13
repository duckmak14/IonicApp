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

    public class StatisticForDriverController : Controller
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
            DateTime date = DateTime.Now;
            string day = "Tháng " + date.Month.ToString() + " / " + date.Year.ToString();

            ViewBag.Date = day;

            return View();
        }

        public ActionResult Statistic_Read([DataSourceRequest] DataSourceRequest request, string datetime, bool checkForm)
        {
            if (checkForm)
            {
                string[] arrListStr = datetime.Split(' ');
                string dateString = arrListStr[1] + arrListStr[2] + arrListStr[3];
                var monthOfYear = DateTime.Parse(dateString.Format("{0:M/yyyy}"));
                var firstDayOfMonth = new DateTime(monthOfYear.Year, monthOfYear.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                //Query get data 4 table
                var dataOfDriverPay = QueryForDriverPay(firstDayOfMonth, lastDayOfMonth);
                var dataOfSalary = QueryForSalary(firstDayOfMonth, lastDayOfMonth);
                var dataOfOtherCost = QueryForOtherCost(firstDayOfMonth, lastDayOfMonth);
                var manageOil = QueryForManageOil(firstDayOfMonth, lastDayOfMonth);

                // add driverID for left join 4 table
                AddIDNotFoundDriverPay(dataOfDriverPay, manageOil);
                AddIDNotFoundSalary(dataOfSalary, manageOil);
                AddIDNotFoundOtherCost(dataOfOtherCost, manageOil);

                // check for join
                var dataJoin = (from a in manageOil
                                join b in dataOfDriverPay on a.DriverID equals b.DriverID into group1
                                from b in group1.DefaultIfEmpty()
                                join c in dataOfSalary on a.DriverID equals c.DriverID into group2
                                from c in group2.DefaultIfEmpty()
                                join d in dataOfOtherCost on a.DriverID equals d.DriverID into group3
                                from d in group3.DefaultIfEmpty()
                                select new
                                {
                                    a.NumberPlate,
                                    a.CarOwerName,
                                    TotalOil = a.TotalOil,
                                    b.TotalDriverPay,
                                    c.TotalSalary,
                                    d.TotalAdvanceCost,
                                    d.TotalMortgageCost,
                                    d.TotalOtherCost
                                }).AsEnumerable()
                                   .Select(B => new StatisticDriverViewModel()
                                   {
                                       StatisticDriverDate = firstDayOfMonth,
                                       TotalOil = B.TotalOil,
                                       TotalDriverPay = B.TotalDriverPay,
                                       TotalSalary = B.TotalSalary,
                                       TotalAdvanceCost = B.TotalAdvanceCost,
                                       TotalMortgageCost = B.TotalMortgageCost,
                                       TotalOtherCost = B.TotalOtherCost,
                                       CarOwerName = B.CarOwerName,
                                       NumberPlate = B.NumberPlate
                                   }).ToList();


                var jsonResult = Json(dataJoin.ToList().OrderBy(x => x.ID).ToDataSourceResult(request));
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }

            return null;
        }

        public List<DataJsonDriverPay> QueryForDriverPay(DateTime firstDayOfMonth, DateTime lastDayOfMonth)
        {
            var dataOfDriverPay = (db.DriverPays.Where(x => DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.PayDate) && DbFunctions.TruncateTime(x.PayDate) <= DbFunctions.TruncateTime(lastDayOfMonth) && x.IsRemove != true)
                    .Select(k => new { k.Price, k.DriverID })
                    .GroupBy(x => new { x.DriverID }, (key, group) => new
                    {
                        DriverID = (int)key.DriverID,
                        TotalDriverPay = group.Sum(k => k.Price)
                    })).AsEnumerable()
                                       .Select(B => new DataJsonDriverPay()
                                       {
                                           DriverID = B.DriverID,
                                           TotalDriverPay = (Double)B.TotalDriverPay
                                       }).ToList();

            return dataOfDriverPay;
        }

        public List<DataJsonDriverPay> AddIDNotFoundDriverPay(List<DataJsonDriverPay> dataJsonDriverPays, List<DataJsonManageOil> dataJsonManageOils)
        {
            foreach (var item in dataJsonManageOils)
            {
                var check = false;
                foreach (var i in dataJsonDriverPays)
                {
                    if (item.DriverID == i.DriverID)
                    {
                        check = true;
                    }
                }
                if (!check)
                {
                    var addData = new DataJsonDriverPay();
                    addData.DriverID = (int)item.DriverID;
                    dataJsonDriverPays.Add(addData);
                }
            }

            return dataJsonDriverPays;
        }

        public List<DataJsonSalary> QueryForSalary(DateTime firstDayOfMonth, DateTime lastDayOfMonth)
        {
            var dataOfSalary = (db.ManageSalarys.Where(x => DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.SalaryDate) && DbFunctions.TruncateTime(x.SalaryDate) <= DbFunctions.TruncateTime(lastDayOfMonth) && x.IsRemove != true)
                  .Select(k => new { k.Total, k.DriverID })
                  .GroupBy(x => new { x.DriverID }, (key, group) => new
                  {
                      DriverID = (int)key.DriverID,
                      TotalSalary = group.Sum(k => k.Total)
                  })).AsEnumerable()
                                      .Select(B => new DataJsonSalary()
                                      {
                                          DriverID = B.DriverID,
                                          TotalSalary = B.TotalSalary
                                      }).ToList();

            return dataOfSalary;
        }

        public List<DataJsonSalary> AddIDNotFoundSalary(List<DataJsonSalary> dataJsonSalaries, List<DataJsonManageOil> dataJsonManageOils)
        {
            foreach (var item in dataJsonManageOils)
            {
                var check = false;
                foreach (var i in dataJsonSalaries)
                {
                    if (item.DriverID == i.DriverID)
                    {
                        check = true;
                    }
                }
                if (!check)
                {
                    var addData = new DataJsonSalary();
                    addData.DriverID = (int)item.DriverID;
                    dataJsonSalaries.Add(addData);
                }
            }

            return dataJsonSalaries;
        }

        public List<DataJsonOtherCost> QueryForOtherCost(DateTime firstDayOfMonth, DateTime lastDayOfMonth)
        {
            var dataOfOtherCost = db.OtherCosts.Where(x => DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.OtherCostDate) && DbFunctions.TruncateTime(x.OtherCostDate) <= DbFunctions.TruncateTime(lastDayOfMonth) && x.IsRemove != true)
                .Select(k => new { k.AdvanceCosts, k.DriverID, k.MortgageCosts, k.OtherCosts })
                .GroupBy(x => new { x.DriverID }, (key, group) => new
                {
                    DriverID = (int)key.DriverID,
                    TotalOtherCost = group.Sum(k => k.OtherCosts),
                    TotalAdvanceCost = group.Sum(k => k.AdvanceCosts),
                    TotalMortgageCost = group.Sum(k => k.MortgageCosts)

                }).AsEnumerable()
                                      .Select(B => new DataJsonOtherCost()
                                      {
                                          DriverID = B.DriverID,
                                          TotalOtherCost = B.TotalOtherCost,
                                          TotalAdvanceCost = B.TotalAdvanceCost,
                                          TotalMortgageCost = B.TotalMortgageCost,
                                      }).ToList();
            return dataOfOtherCost;
        }

        public List<DataJsonOtherCost> AddIDNotFoundOtherCost(List<DataJsonOtherCost> dataJsonOtherCosts, List<DataJsonManageOil> dataJsonManageOils)
        {
            foreach (var item in dataJsonManageOils)
            {
                var check = false;
                foreach (var i in dataJsonOtherCosts)
                {
                    if (item.DriverID == i.DriverID)
                    {
                        check = true;
                    }
                }
                if (!check)
                {
                    var addData = new DataJsonOtherCost();
                    addData.DriverID = (int)item.DriverID;
                    dataJsonOtherCosts.Add(addData);
                }
            }

            return dataJsonOtherCosts;
        }

        public List<DataJsonManageOil> QueryForManageOil(DateTime firstDayOfMonth, DateTime lastDayOfMonth)
        {
            var manageOil = (from a in db.ManageOils.Where(x => DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.OilDate) && DbFunctions.TruncateTime(x.OilDate) <= DbFunctions.TruncateTime(lastDayOfMonth) && x.IsRemove != true)
                             join e in db.Vehicle on a.VehicleID equals e.ID into group4
                             from e in group4.DefaultIfEmpty()
                             join f in db.Vehicle on a.DriverID equals f.ID into group5
                             from f in group5.DefaultIfEmpty()
                             select new
                             {
                                 DriverID = (int)a.DriverID,
                                 TotalOil = a.Total,
                                 e.NumberPlate,
                                 f.CarOwerName
                             }).AsEnumerable()
                                 .Select(B => new DataJsonManageOil()
                                 {
                                     DriverID = B.DriverID,
                                     TotalOil = B.TotalOil,
                                     NumberPlate = B.NumberPlate,
                                     CarOwerName = B.CarOwerName,
                                 }).ToList();
            return manageOil;
        }


        [HttpPost]
        public ActionResult ExportExcel(string dataString)
        {
            List<StatisticDriverViewModel> listData = new List<StatisticDriverViewModel>();
            var dataListJson = dataString.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                StatisticDriverViewModel dataObj = null;
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
                dataObj = JsonConvert.DeserializeObject<StatisticDriverViewModel>(dataObjString);
                dataObj.StatisticDriverDate = dataObj.StatisticDriverDate.ToLocalTime();
                listData.Add(dataObj);
            }
            var result = DownloadStatistic(listData);
            var fileStream = new MemoryStream(result);
            return File(fileStream, "application/ms-excel", "Thống_Kê_Chi_Phí_Theo_Lái_Xe.xlsx");
        }
        public byte[] DownloadStatistic(List<StatisticDriverViewModel> models)
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\ThongKeTheoLaiXe.xlsx", HostingEnvironment.MapPath("/Uploads")));

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

                    productWorksheet.Cells[2, 1].Value = models.Select(x => x.StatisticDriverDate).Min().Month.ToString() + "/" + models.Select(x => x.StatisticDriverDate).Min().Year.ToString();

                    foreach (var listItem in queryPlan)
                    {

                        foreach (var item in listItem)
                        {
                            productWorksheet.Cells[i, 1].Value = i - 3;
                            productWorksheet.Cells[i, 2].Value = item.NumberPlate;
                            productWorksheet.Cells[i, 3].Value = item.CarOwerName;
                            productWorksheet.Cells[i, 4].Value = item.TotalDriverPay;
                            productWorksheet.Cells[i, 4].Style.Numberformat.Format = "#,##0";
                            productWorksheet.Cells[i, 5].Value = item.TotalOil;
                            productWorksheet.Cells[i, 5].Style.Numberformat.Format = "#,##0.00";
                            productWorksheet.Cells[i, 6].Value = item.TotalSalary;
                            productWorksheet.Cells[i, 6].Style.Numberformat.Format = "#,##0";
                            productWorksheet.Cells[i, 7].Value = item.TotalMortgageCost;
                            productWorksheet.Cells[i, 7].Style.Numberformat.Format = "#,##0";
                            productWorksheet.Cells[i, 8].Value = item.TotalAdvanceCost;
                            productWorksheet.Cells[i, 8].Style.Numberformat.Format = "#,##0";
                            productWorksheet.Cells[i, 9].Value = item.TotalOtherCost;
                            productWorksheet.Cells[i, 9].Style.Numberformat.Format = "#,##0";

                            productWorksheet.Cells[i, 10].Formula = "=ROUND(D" + i + "+I" + i + ",0)";
                            productWorksheet.Cells[i, 10].Style.Numberformat.Format = "#,##0";
                            productWorksheet.Cells[i, 10].Style.Font.Bold = true;
                            i++;
                        }
                    }

                    var addressColumn = "";
                    ////sumtotal
                    for (int k = 4; k <= 10; k++)
                    {
                        addressColumn = productWorksheet.Cells[i, k].Address.Substring(0, 1);
                        productWorksheet.Cells[i, k].Style.Font.Bold = true;
                        productWorksheet.Cells[i, k].Formula = "= SUBTOTAL(9," + addressColumn + "4:" + addressColumn + (i - 1).ToString() + ")";
                        productWorksheet.Cells[i, k].Style.Numberformat.Format = "#,##0";
                    }

                    var setFilter = "A3:J" + (i).ToString();
                    var range = productWorksheet.Cells[setFilter.ToString()];
                    range.AutoFilter = true;


                    string test1 = "A1:J" + (i).ToString();
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
    }
}