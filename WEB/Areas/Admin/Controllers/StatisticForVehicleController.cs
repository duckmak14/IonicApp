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

    public class StatisticForVehicleController : Controller
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
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
                var dataOfParking = QueryForParkingCost(firstDayOfMonth, lastDayOfMonth);
                var dataOfDayTicket = QueryForDayTicket(firstDayOfMonth, lastDayOfMonth);
                var dataOfMonthTicket = QueryForMonthTicket(firstDayOfMonth, lastDayOfMonth);
                var dataOfTransportActual = QueryForEnumeration(firstDayOfMonth, lastDayOfMonth);
                var manageOil = QueryForManageOil(firstDayOfMonth, lastDayOfMonth);

                // add driverID for left join 4 table
                AddIDNotFoundDriverPay(dataOfDriverPay, manageOil);
                AddIDNotFoundSalary(dataOfSalary, manageOil);
                AddIDNotFoundParkingCost(dataOfParking, manageOil);
                AddIDNotFoundDayTicket(dataOfDayTicket, manageOil);
                AddIDNotFoundMonthTicket(dataOfMonthTicket, manageOil);
                AddIDNotFoundEnumeration(dataOfTransportActual, manageOil);

                // check for join
                var dataJoin = (from a in manageOil
                                join b in dataOfDriverPay on a.VehicleID equals b.VehicleID into group1
                                from b in group1.DefaultIfEmpty()
                                join c in dataOfSalary on a.VehicleID equals c.VehicleID into group2
                                from c in group2.DefaultIfEmpty()
                                join d in dataOfParking on a.VehicleID equals d.VehicleID into group3
                                from d in group3.DefaultIfEmpty()
                                join e in dataOfDayTicket on a.VehicleID equals e.VehicleID into group4
                                from e in group4.DefaultIfEmpty()
                                join f in dataOfMonthTicket on a.VehicleID equals f.VehicleID into group5
                                from f in group5.DefaultIfEmpty()
                                join g in dataOfTransportActual on a.VehicleID equals g.VehicleID into group6
                                from g in group6.DefaultIfEmpty()
                                select new
                                {
                                    a.NumberPlate,
                                    a.CarOwerName,
                                    TotalOil = a.TotalOil,
                                    b.TotalDriverPay,
                                    c.TotalSalary,
                                    d.TotalParking,
                                    e.TotalDayTicket,
                                    f.TotalMonthTicket,
                                    g.TotalTransportActual

                                }).AsEnumerable()
                                   .Select(B => new StatisticVehicleViewModel()
                                   {
                                       StatisticVehicleDate = firstDayOfMonth,
                                       TotalOil = B.TotalOil,
                                       TotalDriverPay = B.TotalDriverPay,
                                       TotalSalary = B.TotalSalary,
                                       TotalParking = B.TotalParking,
                                       TotalMonthTicket = B.TotalMonthTicket,
                                       TotalDateTicket = B.TotalDayTicket,
                                       TotalTransportActual = B.TotalTransportActual,
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
                    .Select(k => new { k.Price, k.VehicleID })
                    .GroupBy(x => new { x.VehicleID }, (key, group) => new
                    {
                        VehicleID = (int)key.VehicleID,
                        TotalDriverPay = group.Sum(k => k.Price)
                    })).AsEnumerable()
                                       .Select(B => new DataJsonDriverPay()
                                       {
                                           VehicleID = B.VehicleID,
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
                    if (item.VehicleID == i.VehicleID)
                    {
                        check = true;
                    }
                }
                if (!check)
                {
                    var addData = new DataJsonDriverPay();
                    addData.VehicleID = (int)item.VehicleID;
                    dataJsonDriverPays.Add(addData);
                }
            }

            return dataJsonDriverPays;
        }

        public List<DataJsonSalary> QueryForSalary(DateTime firstDayOfMonth, DateTime lastDayOfMonth)
        {
            var dataOfSalary = (db.ManageSalarys.Where(x => DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.SalaryDate) && DbFunctions.TruncateTime(x.SalaryDate) <= DbFunctions.TruncateTime(lastDayOfMonth) && x.IsRemove != true)
                  .Select(k => new { k.Total, k.VehicleID })
                  .GroupBy(x => new { x.VehicleID }, (key, group) => new
                  {
                      VehicleID = (int)key.VehicleID,
                      TotalSalary = group.Sum(k => k.Total)
                  })).AsEnumerable()
                                      .Select(B => new DataJsonSalary()
                                      {
                                          VehicleID = B.VehicleID,
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
                    if (item.VehicleID == i.VehicleID)
                    {
                        check = true;
                    }
                }
                if (!check)
                {
                    var addData = new DataJsonSalary();
                    addData.VehicleID = (int)item.VehicleID;
                    dataJsonSalaries.Add(addData);
                }
            }

            return dataJsonSalaries;
        }


        public List<DataJsonParking> QueryForParkingCost(DateTime firstDayOfMonth, DateTime lastDayOfMonth)
        {
            var dataOfParking = (db.ParkingCosts.Where(x => DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.ParkingDate) && DbFunctions.TruncateTime(x.ParkingDate) <= DbFunctions.TruncateTime(lastDayOfMonth) && x.IsRemove != true)
                  .Select(k => new { k.Price, k.VehicleID })
                  .GroupBy(x => new { x.VehicleID }, (key, group) => new
                  {
                      VehicleID = (int)key.VehicleID,
                      TotalParking = group.Sum(k => k.Price)
                  })).AsEnumerable()
                                      .Select(B => new DataJsonParking()
                                      {
                                          VehicleID = B.VehicleID,
                                          TotalParking = (Double)B.TotalParking
                                      }).ToList();

            return dataOfParking;
        }

        public List<DataJsonParking> AddIDNotFoundParkingCost(List<DataJsonParking> dataJsonParkings, List<DataJsonManageOil> dataJsonManageOils)
        {
            foreach (var item in dataJsonManageOils)
            {
                var check = false;
                foreach (var i in dataJsonParkings)
                {
                    if (item.VehicleID == i.VehicleID)
                    {
                        check = true;
                    }
                }
                if (!check)
                {
                    var addData = new DataJsonParking();
                    addData.VehicleID = (int)item.VehicleID;
                    dataJsonParkings.Add(addData);
                }
            }

            return dataJsonParkings;
        }

        public List<DataJsonMonthTicket> QueryForMonthTicket(DateTime firstDayOfMonth, DateTime lastDayOfMonth)
        {
            var dataOfTicket = (db.ManageTickets.Where(x => DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.TicketDate) && DbFunctions.TruncateTime(x.TicketDate) <= DbFunctions.TruncateTime(lastDayOfMonth) && x.IsRemove != true && x.TicketType == (int)TypeOfTicket.MonthTicket)
                  .Select(k => new { k.Price, k.VehicleID })
                  .GroupBy(x => new { x.VehicleID }, (key, group) => new
                  {
                      VehicleID = (int)key.VehicleID,
                      TotalMonthTicket = group.Sum(k => k.Price)
                  })).AsEnumerable()
                                      .Select(B => new DataJsonMonthTicket()
                                      {
                                          VehicleID = B.VehicleID,
                                          TotalMonthTicket = (Double)B.TotalMonthTicket
                                      }).ToList();

            return dataOfTicket;
        }

        public List<DataJsonMonthTicket> AddIDNotFoundMonthTicket(List<DataJsonMonthTicket> dataJsonMonthTickets, List<DataJsonManageOil> dataJsonManageOils)
        {
            foreach (var item in dataJsonManageOils)
            {
                var check = false;
                foreach (var i in dataJsonMonthTickets)
                {
                    if (item.VehicleID == i.VehicleID)
                    {
                        check = true;
                    }
                }
                if (!check)
                {
                    var addData = new DataJsonMonthTicket();
                    addData.VehicleID = (int)item.VehicleID;
                    dataJsonMonthTickets.Add(addData);
                }
            }

            return dataJsonMonthTickets;
        }


        public List<DataJsonDayTicket> QueryForDayTicket(DateTime firstDayOfMonth, DateTime lastDayOfMonth)
        {
            var dataOfTicket = (db.ManageTickets.Where(x => DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.TicketDate) && DbFunctions.TruncateTime(x.TicketDate) <= DbFunctions.TruncateTime(lastDayOfMonth) && x.IsRemove != true && x.TicketType == (int)TypeOfTicket.DayTicket)
                  .Select(k => new { k.Price, k.VehicleID })
                  .GroupBy(x => new { x.VehicleID }, (key, group) => new
                  {
                      VehicleID = (int)key.VehicleID,
                      TotalDayTicket = group.Sum(k => k.Price)
                  })).AsEnumerable()
                                      .Select(B => new DataJsonDayTicket()
                                      {
                                          VehicleID = B.VehicleID,
                                          TotalDayTicket = (Double)B.TotalDayTicket
                                      }).ToList();

            return dataOfTicket;
        }

        public List<DataJsonDayTicket> AddIDNotFoundDayTicket(List<DataJsonDayTicket> dataJsonDayTickets, List<DataJsonManageOil> dataJsonManageOils)
        {
            foreach (var item in dataJsonManageOils)
            {
                var check = false;
                foreach (var i in dataJsonDayTickets)
                {
                    if (item.VehicleID == i.VehicleID)
                    {
                        check = true;
                    }
                }
                if (!check)
                {
                    var addData = new DataJsonDayTicket();
                    addData.VehicleID = (int)item.VehicleID;
                    dataJsonDayTickets.Add(addData);
                }
            }
            return dataJsonDayTickets;
        }

        public List<DataJsonTransportActual> QueryForEnumeration(DateTime firstDayOfMonth, DateTime lastDayOfMonth)
        {

            var dataOfTransportActual = (db.TransportActuals.Where(x => DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.ActualDate) && DbFunctions.TruncateTime(x.ActualDate) <= DbFunctions.TruncateTime(lastDayOfMonth) && x.IsRemove != true)
                  .Select(k => new { k.UnitPrice,k.TripCount, k.VehicleID })
                  .GroupBy(x => new { x.VehicleID }, (key, group) => new
                  {
                      VehicleID = (int)key.VehicleID,
                      TotalTransportActual = group.Sum(k => ((Double)k.UnitPrice * (Double)k.TripCount))
                  })).AsEnumerable()
                                      .Select(B => new DataJsonTransportActual()
                                      {
                                          VehicleID = B.VehicleID,
                                          TotalTransportActual = (Double)B.TotalTransportActual
                                      }).ToList();

            return dataOfTransportActual;
        }

        public List<DataJsonTransportActual> AddIDNotFoundEnumeration(List<DataJsonTransportActual> dataJsonTransportActuals, List<DataJsonManageOil> dataJsonManageOils)
        {
            foreach (var item in dataJsonManageOils)
            {
                var check = false;
                foreach (var i in dataJsonTransportActuals)
                {
                    if (item.VehicleID == i.VehicleID)
                    {
                        check = true;
                    }
                }
                if (!check)
                {
                    var addData = new DataJsonTransportActual();
                    addData.VehicleID = (int)item.VehicleID;
                    dataJsonTransportActuals.Add(addData);
                }
            }
            return dataJsonTransportActuals;
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
                                 VehicleID = (int)a.VehicleID,
                                 TotalOil = a.Total,
                                 e.NumberPlate,
                                 f.CarOwerName
                             }).AsEnumerable()
                                 .Select(B => new DataJsonManageOil()
                                 {
                                     DriverID = B.DriverID,
                                     TotalOil = B.TotalOil,
                                     VehicleID = B.VehicleID,
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