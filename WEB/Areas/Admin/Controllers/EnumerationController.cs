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

    public class EnumerationController : Controller
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
            var firstDay = date.AddDays(-7);
            var lastDay = date.AddDays(7);

            string day = firstDay.Day.ToString() + "-" + firstDay.Month.ToString() + "-" + firstDay.Year.ToString() + " to " + lastDay.Day.ToString() + "-" + lastDay.Month.ToString() + "-" + lastDay.Year.ToString();
            ViewBag.Date = day;
            ViewBag.DateNow = date;

            return View();
        }
        [AllowAnonymous]
        public JsonResult Get_Vehicles()
        {
            var vehicles = from x in db.Vehicle.AsNoTracking() select x;
            return Json(vehicles.Select(x => new
            {
                x.ID,
                x.NumberPlate
            }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult TransportActualLists_Read([DataSourceRequest] DataSourceRequest request, string datetime, string status)
        {
            var enumarationList = new List<TransportActualViewModel>();
            DateTime date = DateTime.Now;
            string[] arrListStr = datetime.Split(new char[] { 't', 'o' });
            var firstDay = DateTime.Parse(arrListStr[0].Format("{0:dd-MM-yyyy}"));
            var lastDay = DateTime.Parse(arrListStr[2].Format("{0:dd-MM-yyyy}"));

            enumarationList = (from a in db.TransportActuals.Where(x => firstDay <= x.ActualDate && x.ActualDate <= lastDay && x.SourcePartner.PartnerCode == "ST" && x.IsRemove != true)
                               join b in db.Route on a.RouteID equals b.ID into group1
                               from b in group1.DefaultIfEmpty()
                               join c in db.VehicleWeight on a.ActualWeightID equals c.ID into group2
                               from c in group2.DefaultIfEmpty()
                               join d in db.Vehicle on a.VehicleID equals d.ID into group3
                               from d in group3.DefaultIfEmpty()
                               join e in db.Partner on a.SourcePartnerID equals e.ID into group4
                               from e in group4.DefaultIfEmpty()
                               join f in db.Partner on a.DestinationPartnerID equals f.ID into group5
                               from f in group5.DefaultIfEmpty()
                               select new
                               {
                                   a.ID,
                                   a.ActualDate,
                                   a.Note,
                                   a.TripCount,
                                   a.Status,
                                   b.RouteCode,
                                   a.TrackingCode,
                                   a.CreatedDate,
                                   a.ModifiedDate,
                                   StartLocation = b.StartLocation.LocationName,
                                   EndLocation = b.EndLocation.LocationName,
                                   ActualWeight = c.WeightName,
                                   d.NumberPlate,
                                   Weight = d.VehicleWeight.WeightName,
                                   SourcePartnerName = e.PartnerName,
                                   DestinationPartnerName = f.PartnerName,
                                   a.UnitPrice
                               }).AsEnumerable()
                                        .Select(B => new TransportActualViewModel()
                                        {
                                            ID = B.ID,
                                            ActualDate = B.ActualDate,
                                            Note = B.Note,
                                            TripCount = B.TripCount,
                                            Status = B.Status,
                                            RouteCode = B.RouteCode,
                                            TrackingCode = B.TrackingCode,
                                            CreatedDate = B.CreatedDate,
                                            ModifiedDate = B.ModifiedDate,
                                            StartLocationName = B.StartLocation,
                                            EndLocationName = B.EndLocation,
                                            ActualWeightName = B.ActualWeight,
                                            NumberPlate = B.NumberPlate,
                                            VehicleWeightName = B.Weight,
                                            SourcePartnerName = B.SourcePartnerName,
                                            DestinationPartnerName = B.DestinationPartnerName,
                                            UnitPrice = B.UnitPrice

                                        }).ToList();

            if (status != "false")
            {
                var listForMerge = new List<TransportActualViewModel>();

                var queryPlan = from a in enumarationList
                                group a by new { a.NumberPlate, a.ActualDate, a.VehicleWeightName, a.ActualWeightName, a.RouteCode, a.SourcePartnerName, a.DestinationPartnerName };

                foreach (var listPlan in queryPlan)
                {
                    var plan = new TransportActualViewModel();
                    plan.ID = listPlan.First().ID;
                    plan.CreatedDate = listPlan.First().CreatedDate;
                    plan.ModifiedDate = listPlan.First().ModifiedDate;
                    plan.Note = listPlan.First().Note;
                    plan.ActualDate = listPlan.First().ActualDate;
                    plan.TrackingCode = listPlan.First().TrackingCode;
                    plan.UnitPrice = listPlan.First().UnitPrice;
                    plan.Status = listPlan.First().Status;
                    plan.RouteCode = listPlan.First().RouteCode;
                    plan.StartLocationName = listPlan.First().StartLocationName;
                    plan.EndLocationName = listPlan.First().EndLocationName;
                    plan.NumberPlate = listPlan.First().NumberPlate;
                    plan.VehicleWeightName = listPlan.First().VehicleWeightName;
                    plan.ActualWeightName = listPlan.First().ActualWeightName;
                    plan.SourcePartnerName = listPlan.First().SourcePartnerName;
                    plan.DestinationPartnerName = listPlan.First().DestinationPartnerName;

                    Double? countforTrip = 0;

                    foreach (var item in listPlan)
                    {
                        countforTrip += item.TripCount;
                    }
                    plan.TripCount = countforTrip;
                    listForMerge.Add(plan);
                }

                var jsonResultMerge = Json(listForMerge.OrderBy(x => x.ActualDate).ThenByDescending(x => x.CreatedDate).ToDataSourceResult(request));
                jsonResultMerge.MaxJsonLength = int.MaxValue;
                return jsonResultMerge;
            }

            var jsonResult = Json(enumarationList.OrderBy(x => x.ActualDate).ThenByDescending(x => x.CreatedDate).ToDataSourceResult(request));
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult Add()
        {
            var model = new TransportActual();
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add([Bind(Exclude = "")] TransportActual model, DateTime? date)
        {
            if (ModelState.IsValid)
            {
                if (date == null)
                {
                    ModelState.AddModelError("", AccountResources.RequiredPlanDate);
                    return View(model);
                }
                if (model.TripCountViewModel == null)
                {
                    ModelState.AddModelError("", AccountResources.RequiredTripCount);
                    return View(model);
                }
                String tripCount = (model.TripCountViewModel).Replace('.', ',');
                model.TripCount = Double.Parse(tripCount);

                var Partner = db.Partner.ToList();
                var PricingTable = db.PricingTable.ToList();
                var Vehicle = db.Vehicle.ToList();
                Double unitPrice = 0;

                if (model.UnitPrice == null)
                {
                    if (model.ActualWeightID != null)
                    {
                        unitPrice = CheckPrice(model.ActualWeightID, model.RouteID, model.SourcePartnerID, PricingTable);
                    }
                    else
                    {
                        var weightID = db.Vehicle.Where(x => x.ID == model.VehicleID).Select(x => x.WeightID).FirstOrDefault();
                        unitPrice = CheckPrice(weightID, model.RouteID, model.SourcePartnerID, PricingTable);
                    }

                    if (unitPrice == 0)
                    {
                        ModelState.AddModelError("", "Không tồn tại giá, vui lòng kiểm tra lại!");
                        return View(model);
                    }

                    model.UnitPrice = unitPrice;
                }

                //Check price for AT Price Table
                var ATId = Partner.Where(x => x.PartnerCode == "AT").Select(x => x.ID).FirstOrDefault();

                if (model.ActualWeightID != null)
                {
                    unitPrice = CheckPrice(model.ActualWeightID, model.RouteID, ATId, PricingTable);
                }
                else
                {
                    var weightID = Vehicle.Where(x => x.ID == model.VehicleID).Select(x => x.WeightID).FirstOrDefault();
                    unitPrice = CheckPrice(weightID, model.RouteID, ATId, PricingTable);
                }
                model.UnitPriceAT = unitPrice;

                //Check price for HPC Price Table
                var HPCId = Partner.Where(x => x.PartnerCode == "HPC").Select(x => x.ID).FirstOrDefault();
                if (model.ActualWeightID != null)
                {
                    unitPrice = CheckPrice(model.ActualWeightID, model.RouteID, HPCId, PricingTable);
                }
                else
                {
                    var weightID = Vehicle.Where(x => x.ID == model.VehicleID).Select(x => x.WeightID).FirstOrDefault();
                    unitPrice = CheckPrice(weightID, model.RouteID, HPCId, PricingTable);
                }
                model.UnitPriceHPC = unitPrice;

                model.ActualDate = DateTime.Parse(date.ToString());
                model.Status = false;
                var routeCode = db.Route.Where(x => x.ID == model.RouteID).Select(x => x.RouteCode).FirstOrDefault();
                string weightName;
                if (model.ActualWeightID != null)
                {
                    weightName = db.VehicleWeight.Where(x => x.ID == model.ActualWeightID).Select(x => x.WeightName).FirstOrDefault();
                }
                else
                {
                    var vehicle = db.Vehicle.Where(x => x.ID == model.VehicleID).Select(x => x.WeightID).FirstOrDefault();
                    weightName = db.VehicleWeight.Where(x => x.ID == vehicle).Select(x => x.WeightName).FirstOrDefault();
                }
                model.TrackingCode = routeCode + "-" + weightName;

                model.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                model.CreatedDate = DateTime.Now;

                db.Set<TransportActual>().Add(model);
                db.SaveChanges();
                ViewBag.StartupScript = "create_success();";
                return View(model);
            }
            else
            {
                return View(model);
            }
        }
        private Double CheckPrice(int? weightID, int? routeID, int? sourcePartnerID, List<PricingTable> prices)
        {
            Double unitPrice = 0;

            var price = prices.Where(x => x.RouteID == routeID &&
                (x.WeightID == weightID) && x.SourcePartnerID == sourcePartnerID)
                .Select(x => x.Price).FirstOrDefault();

            if (price != null)
            {
                unitPrice = (double)price;
            }
            return unitPrice;
        }

        public ActionResult Edit(int id)
        {
            var model = db.Set<TransportActual>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            model.TripCountViewModel = model.TripCount.ToString().Replace(',', '.');
            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "")] TransportActual model, DateTime? date)
        {
            if (ModelState.IsValid)
            {
                var oldPlan = db.TransportActuals.Where(x => x.ID == model.ID).FirstOrDefault();

                if (date == null)
                {
                    ModelState.AddModelError("", AccountResources.RequiredPlanDate);
                    return View(model);
                }
                if (model.UnitPrice == null)
                {
                    ModelState.AddModelError("", AccountResources.RequiredUnitPrice);
                    return View(model);
                }
                if (model.TripCountViewModel == null)
                {
                    ModelState.AddModelError("", AccountResources.RequiredTripCount);
                    return View(model);
                }

                oldPlan.UnitPrice = model.UnitPrice;

              
                if (oldPlan.RouteID != model.RouteID || oldPlan.VehicleID != model.VehicleID || oldPlan.ActualWeightID != model.ActualWeightID)
                {
                    var routeCode = db.Route.Where(x => x.ID == model.RouteID).Select(x => x.RouteCode).FirstOrDefault();
                    string weightName;
                    if (model.ActualWeightID != null)
                    {
                        weightName = db.VehicleWeight.Where(x => x.ID == model.ActualWeightID).Select(x => x.WeightName).FirstOrDefault();
                    }
                    else
                    {
                        var vehicle = db.Vehicle.Where(x => x.ID == model.VehicleID).Select(x => x.WeightID).FirstOrDefault();
                        weightName = db.VehicleWeight.Where(x => x.ID == vehicle).Select(x => x.WeightName).FirstOrDefault();
                    }
                    oldPlan.TrackingCode = routeCode + "-" + weightName;
                }
                String tripCount = (model.TripCountViewModel).Replace('.', ',');
                oldPlan.TripCount = Double.Parse(tripCount);

                oldPlan.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                oldPlan.ModifiedDate = DateTime.Now;
                oldPlan.RouteID = model.RouteID;
                oldPlan.SourcePartnerID = model.SourcePartnerID;
                oldPlan.DestinationPartnerID = model.DestinationPartnerID;
                oldPlan.ActualWeightID = model.ActualWeightID;
                oldPlan.VehicleID = model.VehicleID;
                //oldPlan.TripCount = model.TripCount;
                oldPlan.ActualDate = DateTime.Parse(date.ToString());
                oldPlan.Note = model.Note;
                db.SaveChanges();
                ViewBag.StartupScript = "edit_success("+oldPlan.ID+");";

                return View(model);
            }
            else
            {
                return View(model);
            }
        }

        public ActionResult Duplicate(int id)
        {
            var model = db.Set<TransportActual>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            model.TripCountViewModel = model.TripCount.ToString().Replace(',', '.');
            return View("Duplicate", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Duplicate([Bind(Exclude = "")] TransportActual model, DateTime? date)
        {
            if (ModelState.IsValid)
            {
                var dateDayNow = DateTime.Now;
                if (date == null)
                {
                    ModelState.AddModelError("", AccountResources.RequiredPlanDate);
                    return View(model);
                }
                if (model.UnitPrice == null)
                {
                    ModelState.AddModelError("", AccountResources.RequiredUnitPrice);
                    return View(model);
                }

                if (model.TripCountViewModel == null)
                {
                    ModelState.AddModelError("", AccountResources.RequiredTripCount);
                    return View(model);
                }
                String tripCount = (model.TripCountViewModel).Replace('.', ',');
                model.TripCount = Double.Parse(tripCount);

                var Partner = db.Partner.ToList();
                var PricingTable = db.PricingTable.ToList();
                var Vehicle = db.Vehicle.ToList();
                Double unitPrice = 0;

                if (model.UnitPrice == null)
                {
                    if (model.ActualWeightID != null)
                    {
                        unitPrice = CheckPrice(model.ActualWeightID, model.RouteID, model.SourcePartnerID, PricingTable);
                    }
                    else
                    {
                        var weightID = db.Vehicle.Where(x => x.ID == model.VehicleID).Select(x => x.WeightID).FirstOrDefault();
                        unitPrice = CheckPrice(weightID, model.RouteID, model.SourcePartnerID, PricingTable);
                    }

                    if (unitPrice == 0)
                    {
                        ModelState.AddModelError("", "Không tồn tại giá, vui lòng kiểm tra lại!");
                        return View(model);
                    }

                    model.UnitPrice = unitPrice;
                }

                //Check price for AT Price Table
                var ATId = Partner.Where(x => x.PartnerCode == "AT").Select(x => x.ID).FirstOrDefault();

                if (model.ActualWeightID != null)
                {
                    unitPrice = CheckPrice(model.ActualWeightID, model.RouteID, ATId, PricingTable);
                }
                else
                {
                    var weightID = Vehicle.Where(x => x.ID == model.VehicleID).Select(x => x.WeightID).FirstOrDefault();
                    unitPrice = CheckPrice(weightID, model.RouteID, ATId, PricingTable);
                }
                model.UnitPriceAT = unitPrice;

                //Check price for HPC Price Table
                var HPCId = Partner.Where(x => x.PartnerCode == "HPC").Select(x => x.ID).FirstOrDefault();
                if (model.ActualWeightID != null)
                {
                    unitPrice = CheckPrice(model.ActualWeightID, model.RouteID, HPCId, PricingTable);
                }
                else
                {
                    var weightID = Vehicle.Where(x => x.ID == model.VehicleID).Select(x => x.WeightID).FirstOrDefault();
                    unitPrice = CheckPrice(weightID, model.RouteID, HPCId, PricingTable);
                }
                model.UnitPriceHPC = unitPrice;

                model.ActualDate = DateTime.Parse(date.ToString());
                model.Status = false;

                var routeCode = db.Route.Where(x => x.ID == model.RouteID).Select(x => x.RouteCode).FirstOrDefault();
                string weightName;
                if (model.ActualWeightID != null)
                {
                    weightName = db.VehicleWeight.Where(x => x.ID == model.ActualWeightID).Select(x => x.WeightName).FirstOrDefault();
                }
                else
                {
                    var vehicle = db.Vehicle.Where(x => x.ID == model.VehicleID).Select(x => x.WeightID).FirstOrDefault();
                    weightName = db.VehicleWeight.Where(x => x.ID == vehicle).Select(x => x.WeightName).FirstOrDefault();
                }
                model.TrackingCode = routeCode + "-" + weightName;

                model.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                model.CreatedDate = DateTime.Now;
                db.Set<TransportActual>().Add(model);
                db.SaveChanges();
                ViewBag.StartupScript = "create_success("+ model.ID+ ");";
                return View(model);
            }
            else
            {
                return View(model);
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult FindUnitPrice(TransportActual model)
        {
            if (model.ActualWeightID != null)
            {
                model.UnitPrice = db.PricingTable.Where(x => x.RouteID == model.RouteID && x.WeightID == model.ActualWeightID
                                                     && x.SourcePartnerID == model.SourcePartnerID).Select(x => x.Price).FirstOrDefault();
            }
            else
            {
                model.UnitPrice = db.PricingTable.Where(x => x.RouteID == model.RouteID && x.WeightID == (db.Vehicle.Where(y => y.ID == model.VehicleID).Select(y => y.WeightID).FirstOrDefault())
                                                    && x.SourcePartnerID == model.SourcePartnerID).Select(x => x.Price).FirstOrDefault();
            }


            return Json(new { ErrorMessage = string.Empty, price = model.UnitPrice }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ChangeStatus(GetSelected model)
        {
            var temp = from p in db.Set<TransportActual>()
                       where model.ID.Contains(p.ID)
                       select p;
            foreach (var item in temp)
            {
                item.Status = true;
                db.TransportActuals.Attach(item);
                db.Entry(item).Property(a => a.Status).IsModified = true;
            }
            db.SaveChanges();
            return Json(new { ErrorMessage = string.Empty }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult UnChangeStatus(GetSelected model)
        {
            var temp = from p in db.Set<TransportActual>()
                       where model.ID.Contains(p.ID)
                       select p;
            foreach (var item in temp)
            {
                item.Status = false;
                db.TransportActuals.Attach(item);
                db.Entry(item).Property(a => a.Status).IsModified = true;
            }
            db.SaveChanges();
            return Json(new { ErrorMessage = string.Empty }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult ExportExcel(string dataString)
        {
            List<TransportActualExport> listData = new List<TransportActualExport>();
            var dataListJson = dataString.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                TransportActualExport dataObj = null;
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
                dataObj = JsonConvert.DeserializeObject<TransportActualExport>(dataObjString);
                dataObj.ActualDate = dataObj.ActualDate.ToLocalTime();
                listData.Add(dataObj);
            }
            var result = DownloadTransportActual(listData);
            var fileStream = new MemoryStream(result);
            return File(fileStream, "application/ms-excel", "QLBangKe.xlsx");
        }
        public byte[] DownloadTransportActual(List<TransportActualExport> models)
        {
            DateTime MaxDate = models.Max(x => x.ActualDate);
            DateTime MinDate = models.Min(x => x.ActualDate);


            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLBangKe.xlsx", HostingEnvironment.MapPath("/Uploads")));

            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var productWorksheet = p.Workbook.Worksheets[0];
                    productWorksheet.Select();
              

                    //time
                    var dateNow = DateTime.Now;
                    productWorksheet.Cells["J2:M2"].Merge = true;
                    productWorksheet.Cells[2, 10].Value = "Hà Nội, ngày " + dateNow.Day.ToString() + " tháng " + dateNow.Month.ToString() + " năm " + dateNow.Year.ToString();

                    //max and min time
                    productWorksheet.Cells["A6:N6"].Merge = true;
                    if (MaxDate == MinDate)
                    {
                        productWorksheet.Cells[6, 1].Value = "Ngày " + MaxDate.Day + "/" + MaxDate.Month + "/" + MaxDate.Year;

                    }
                    else
                    {
                        productWorksheet.Cells[6, 1].Value = "Từ ngày " + MinDate.Day + "/" + MinDate.Month + "/" + MinDate.Year + " đến " + MaxDate.Day + "/" + MaxDate.Month + "/" + MaxDate.Year;
                    }
                    productWorksheet.Cells[6, 1].Style.Font.Bold = true;
                    productWorksheet.Cells[6, 1].Style.Font.Italic = true;
           

                    int i = 14;
                    int stt = 1;
                    Double sumTotal = 0;
                    foreach (var price in models)
                    {
                        productWorksheet.Cells[i, 1].Value = stt;
                        productWorksheet.Cells[i, 2].Value = price.ActualDate;
                        productWorksheet.Cells[i, 3].Value = price.TrackingCode;
                        productWorksheet.Cells[i, 4].Value = price.NumberPlate;
                        productWorksheet.Cells[i, 5].Value = price.TripCount;
                        productWorksheet.Cells[i, 6].Value = price.StartLocationName;
                        productWorksheet.Cells[i, 7].Value = price.EndLocationName;
                        productWorksheet.Cells[i, 8].Value = price.NumberPlate;

                        Double weightNumber;
                        if (price.ActualWeightName == null)
                        {
                            if(Double.TryParse(price.VehicleWeightName.Replace(".",",").ToString(), out weightNumber))
                            {
                                productWorksheet.Cells[i, 9].Value = weightNumber;
                            }  
                            else
                            {
                                productWorksheet.Cells[i, 9].Value = price.VehicleWeightName;
                            }
                        }
                        else
                        {
                            if (Double.TryParse(price.ActualWeightName.Replace(".", ",").ToString(), out weightNumber))
                            {
                                productWorksheet.Cells[i, 9].Value = weightNumber;
                            }
                            else
                            {
                                productWorksheet.Cells[i, 9].Value = price.ActualWeightName;
                            }

                        }

                        productWorksheet.Cells[i, 10].Value = " '' ";
                        productWorksheet.Cells[i, 11].Value = price.TripCount;
                        productWorksheet.Cells[i, 12].Value = price.UnitPrice;
                        productWorksheet.Cells[i, 12].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i, 13].Value = price.TotalMoney;
                       
                        productWorksheet.Cells[i, 13].Formula = " = ROUND((K" + (i).ToString() + "* L" + i.ToString()+"), -3)";
                        productWorksheet.Cells[i, 13].Style.Numberformat.Format = "#,##0";
                        productWorksheet.Cells[i, 14].Value = price.Note;
                        i++;
                        stt++;
                        sumTotal = (double)(sumTotal + price.TotalMoney);
                    }

                    ////sumtotal
                    productWorksheet.Cells["A" + i + ":C" + i].Merge = true;
                    productWorksheet.Cells[i, 1].Value = "Tổng cộng:";
                    productWorksheet.Cells[i, 1].Style.Font.Bold = true;
                    productWorksheet.Cells[i, 13].Formula = "= SUBTOTAL(9,M14:M" + (i - 1).ToString() + ")";
                    productWorksheet.Cells[i, 13].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[i, 13].Style.Font.Bold = true;

                    productWorksheet.Cells["A" + (i + 1) + ":C" + (i + 1)].Merge = true;
                    productWorksheet.Cells[(i + 1), 1].Value = "Thuế VAT 10%";
                    productWorksheet.Cells[(i + 1), 1].Style.Font.Bold = true;
                    productWorksheet.Cells[(i + 1), 13].Formula = " = ROUND((M" + (i).ToString() + "*0.1), 0)";
                    productWorksheet.Cells[(i + 1), 13].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[(i + 1), 13].Style.Font.Bold = true;


                    productWorksheet.Cells["A" + (i + 2) + ":C" + (i + 2)].Merge = true;
                    productWorksheet.Cells[(i + 2), 1].Value = "Tổng thanh toán:";
                    productWorksheet.Cells[(i + 2), 1].Style.Font.Bold = true;
                    productWorksheet.Cells[(i + 2), 13].Formula = "= M" + (i).ToString() + "+ M" + (i + 1).ToString();
                    productWorksheet.Cells[(i + 2), 13].Style.Numberformat.Format = "#,##0";
                    productWorksheet.Cells[(i + 2), 13].Style.Font.Bold = true;

                    var test = "A13:M" + (i + 2).ToString();
                    var range = productWorksheet.Cells[test.ToString()];
                    range.AutoFilter = true;

                    string test1 = "A14:N" + (i+2).ToString();
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
        [HttpPost]
        [AllowAnonymous]
        public ActionResult CheckPartner(GetSelected model)
        {
            var Partner = db.Set<TransportActual>().Where(x => model.ID.Contains(x.ID)).ToList();
            var checkPartner = true;
            int i = 0;
            var sourceID = 0;
            var destinationID = 0;
            foreach (var item in Partner)
            {
                if (i == 0)
                {
                    sourceID = int.Parse(item.SourcePartnerID.ToString());
                    destinationID = int.Parse(item.DestinationPartnerID.ToString());
                    i++;
                }
                else
                {
                    if (item.SourcePartnerID != sourceID)
                    {
                        checkPartner = false;
                    }
                    if (item.DestinationPartnerID != destinationID)
                    {
                        checkPartner = false;
                    }
                }

            }
            return Json(new { ErrorMessage = string.Empty, checkPartner = checkPartner }, JsonRequestBehavior.AllowGet);
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

        //[HttpPost]
        //[ValidateInput(false)]
        //public ActionResult UploadExcel(string submit)
        //{
        //    if (submit == "Upload")
        //    {
        //        HttpPostedFileBase file = Request.Files[0];
        //        if (file == null || file.ContentLength == 0)
        //        {
        //            return View();
        //        }
        //        var fileTemp = new FileInfo(string.Format(@"{0}\QLKeHoachChay.xlsx", HostingEnvironment.MapPath("/Uploads")));
        //        var check = ExcelHelper.CheckHeaderFile(fileTemp, file);
        //        if (check)
        //        {
        //            var result = new UploadDrivePlanFromExcel().UploadProducts(file, Session["UploadPlanProgress"]);
        //            var fileStream = new MemoryStream(result);
        //            ViewBag.StartupScript = "upload_success();";
        //            return File(fileStream, "application/ms-excel", "QLKeHoachChay_UploadResult.xlsx");
        //        }
        //        else
        //        {
        //            ViewBag.check = "File không đúng định dạng Template";
        //            return View();
        //        }
        //    }
        //    else
        //    {
        //        var result = new UploadDrivePlanFromExcel().DownloadTemplate();
        //        var fileStream = new MemoryStream(result);
        //        return File(fileStream, "application/ms-excel", "QLKeHoachChay_Template.xlsx");
        //    }
        //}
    }
}