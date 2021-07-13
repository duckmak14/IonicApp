using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
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

    public class DrivePlanController : Controller
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        // GET: Admin/DrivePlan
        public ActionResult Index(string dataString)
        {
            DateTime date = DateTime.Now;
            ViewBag.DateNow = date;
            var firstDay = new DateTime();
            var lastDay = new DateTime();
            var user = WebHelpers.UserInfoHelper.GetUserData().UserName;
            List<string> role = Roles.GetRolesForUser(user).ToList();
            var checkDriver = 0;
            if (role.Contains("Lái xe"))
            {
                checkDriver = 1;
                firstDay = date.AddDays(-1);
                lastDay = date.AddDays(3);
            }
            else
            {
                firstDay = date.AddDays(-7);
                lastDay = date.AddDays(7);
            }    
            ViewBag.CheckDriver = checkDriver;

            string day = "";

            if (dataString == null)
            {
                day = firstDay.Day.ToString() + "-" + firstDay.Month.ToString() + "-" + firstDay.Year.ToString() + " to " + lastDay.Day.ToString() + "-" + lastDay.Month.ToString() + "-" + lastDay.Year.ToString();
                ViewBag.Date = day;
            } 
            else
            {
                day = dataString;
            }

            ViewBag.Date = day;

            return View();
        }

        [AllowAnonymous]
        public JsonResult Get_Vehicles()
        {
            var vehicles = from x in db.Vehicle.AsNoTracking() select x;
            return Json(vehicles.Select(x => new
            {
                x.ID,
                x.NumberPlate,
                x.CarOwerName
            }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult TransportPlanLists_Read([DataSourceRequest] DataSourceRequest request, string datetime)
        {
            var TransportPlanList = new List<TransportPlanViewModel>();
            var user = db.Set<UserProfile>().Find(WebHelpers.UserInfoHelper.GetUserData().UserId);
            List<string> role = Roles.GetRolesForUser(user.UserName).ToList();
            DateTime date = DateTime.Now;
            string[] arrListStr = datetime.Split(new char[] { 't', 'o' });
            var firstDay = DateTime.Parse(arrListStr[0].Format("{0:dd-MM-yyyy}"));
            var lastDay = DateTime.Parse(arrListStr[2].Format("{0:dd-MM-yyyy}"));
            if (role.Contains("Lái xe"))
            {
                var vehicleOfDriver = db.Vehicle.Where(x => x.Mobile == user.Mobile).Select(x => x.NumberPlate).ToList();
                TransportPlanList = (from a in db.TransportPlans.Where(x => vehicleOfDriver.Contains(x.Vehicle.NumberPlate)).Where(x => DbFunctions.TruncateTime(firstDay) <= DbFunctions.TruncateTime(x.PlanDate) && DbFunctions.TruncateTime(x.PlanDate) <= DbFunctions.TruncateTime(lastDay) && x.IsRemove != true)
                                     join b in db.Route on a.RouteID equals b.ID into group1
                                     from b in group1.DefaultIfEmpty()
                                     join d in db.Vehicle on a.VehicleID equals d.ID into group3
                                     from d in group3.DefaultIfEmpty()
                                     join e in db.Partner on a.SourcePartnerID equals e.ID into group4
                                     from e in group4.DefaultIfEmpty()
                                     join f in db.Partner on a.DestinationPartnerID equals f.ID into group5
                                     from f in group5.DefaultIfEmpty()
                                     join g in db.VehicleWeight on a.ActualWeightID equals g.ID into group6
                                     from g in group6.DefaultIfEmpty()
                                     join h in db.Location on a.EndLocationID equals h.ID into group7
                                     from h in group7.DefaultIfEmpty()
                                     join j in db.Location on a.StartLocationID equals j.ID into group8
                                     from j in group8.DefaultIfEmpty()
                                         //join d in db.VehicleWeight on a.ActualWeightID equals d.ID into group3
                                         //from d in group3.DefaultIfEmpty()
                                     select new
                                     {
                                         a.ID,
                                         a.PlanDate,
                                         a.Note,
                                         a.StartTime,
                                         a.EndTime,
                                         a.DetailCode,
                                         a.Amount,
                                         a.Status,
                                         b.RouteCode,
                                         a.TrackingCode,
                                         a.ActualWeightID,
                                         a.CreatedDate,
                                         a.ModifiedDate,
                                         a.TripBack,
                                         d.NumberPlate,
                                         d.CarOwerName,
                                         Weight = d.VehicleWeight.WeightName,
                                         SourcePartnerName = e.PartnerName,
                                         DestinationPartnerName = f.PartnerName,
                                         SourcePartnerID = e.ID,
                                         DestinationPartnerID = f.ID,
                                         VehicleID = d.ID,
                                         RouteID = b.ID,
                                         ActualWeightName = g.WeightName,
                                         endLocation = h.LocationName,
                                         startLocation = j.LocationName

                                     }).AsEnumerable()
                                        .Select(B => new TransportPlanViewModel()
                                        {
                                            ID = B.ID,
                                            SourcePartnerID = B.SourcePartnerID,
                                            DestinationPartnerID = B.DestinationPartnerID,
                                            VehicleID = B.VehicleID,
                                            RouteID = B.RouteID,
                                            TripBack = B.TripBack,
                                            Note = B.Note,
                                            PlanDate = B.PlanDate,
                                            TrackingCode = B.TrackingCode,
                                            StartTime = B.StartTime,
                                            EndTime = B.EndTime,
                                            DetailCode = B.DetailCode,
                                            Amount = B.Amount,
                                            ActualWeightID = B.ActualWeightID,
                                            CreatedDate = B.CreatedDate,
                                            ModifiedDate = B.ModifiedDate,
                                            Status = B.Status,
                                            RouteCode = B.RouteCode,
                                            NumberPlate = B.NumberPlate,
                                            CarOwerName = B.CarOwerName,
                                            VehicleWeightName = B.Weight,
                                            SourcePartnerName = B.SourcePartnerName,
                                            DestinationPartnerName = B.DestinationPartnerName,
                                            ActualWeightName = B.ActualWeightName,
                                            EndLocationName = B.endLocation,
                                            StartLocationName = B.startLocation

                                        }).ToList();

            }
            else
            {
                TransportPlanList = (from a in db.TransportPlans.Where(x => DbFunctions.TruncateTime(firstDay) <= DbFunctions.TruncateTime(x.PlanDate) && DbFunctions.TruncateTime(x.PlanDate) <= DbFunctions.TruncateTime(lastDay) && x.IsRemove != true)
                                     join b in db.Route on a.RouteID equals b.ID into group1
                                     from b in group1.DefaultIfEmpty()
                                     join d in db.Vehicle on a.VehicleID equals d.ID into group3
                                     from d in group3.DefaultIfEmpty()
                                     join e in db.Partner on a.SourcePartnerID equals e.ID into group4
                                     from e in group4.DefaultIfEmpty()
                                     join f in db.Partner on a.DestinationPartnerID equals f.ID into group5
                                     from f in group5.DefaultIfEmpty()
                                     join g in db.VehicleWeight on a.ActualWeightID equals g.ID into group6
                                     from g in group6.DefaultIfEmpty()
                                     join h in db.Location on a.EndLocationID equals h.ID into group7
                                     from h in group7.DefaultIfEmpty()
                                     join j in db.Location on a.StartLocationID equals j.ID into group8
                                     from j in group8.DefaultIfEmpty()
                                     select new
                                     {
                                         a.ID,
                                         a.PlanDate,
                                         a.Note,
                                         a.StartTime,
                                         a.EndTime,
                                         a.DetailCode,
                                         a.Amount,
                                         a.Status,
                                         b.RouteCode,
                                         a.TrackingCode,
                                         a.ActualWeightID,
                                         a.CreatedDate,
                                         a.ModifiedDate,
                                         a.TripBack,
                                         StartLocation = b.StartLocation.LocationName,
                                         EndLocation = b.EndLocation.LocationName,
                                         d.NumberPlate,
                                         d.CarOwerName,
                                         Weight = d.VehicleWeight.WeightName,
                                         SourcePartnerName = e.PartnerName,
                                         DestinationPartnerName = f.PartnerName,
                                         SourcePartnerID = e.ID,
                                         DestinationPartnerID = f.ID,
                                         VehicleID = d.ID,
                                         RouteID = b.ID,
                                         ActualWeightName = g.WeightName,
                                         endLocation = h.LocationName,
                                         startLocation = j.LocationName
                                     }).AsEnumerable()
                                        .Select(B => new TransportPlanViewModel()
                                        {
                                            ID = B.ID,
                                            SourcePartnerID = B.SourcePartnerID,
                                            DestinationPartnerID = B.DestinationPartnerID,
                                            VehicleID = B.VehicleID,
                                            RouteID = B.RouteID,
                                            TripBack = B.TripBack,
                                            Note = B.Note,
                                            PlanDate = B.PlanDate,
                                            TrackingCode = B.TrackingCode,
                                            StartTime = B.StartTime,
                                            EndTime = B.EndTime,
                                            DetailCode = B.DetailCode,
                                            Amount = B.Amount,
                                            ActualWeightID = B.ActualWeightID,
                                            CreatedDate = B.CreatedDate,
                                            ModifiedDate = B.ModifiedDate,
                                            Status = B.Status,
                                            RouteCode = B.RouteCode,
                                            NumberPlate = B.NumberPlate,
                                            CarOwerName = B.CarOwerName,
                                            VehicleWeightName = B.Weight,
                                            SourcePartnerName = B.SourcePartnerName,
                                            DestinationPartnerName = B.DestinationPartnerName,
                                            ActualWeightName = B.ActualWeightName,
                                            EndLocationName = B.endLocation,
                                            StartLocationName = B.startLocation

                                        }).ToList();

            }
            var jsonResult = Json(TransportPlanList.OrderBy(x => x.PlanDate).ThenByDescending(x => x.CreatedDate).ToDataSourceResult(request));
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult Add()
        {
            var model = new TransportPlan();
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add([Bind(Exclude = "")] TransportPlan model, DateTime? date, DateTime? start, DateTime? end)
        {
            if (ModelState.IsValid)
            {
                //var dateDayNow = DateTime.Now.AddDays(-3);
                if (date == null)
                {
                    ModelState.AddModelError("CustomErrorDate", AccountResources.RequiredPlanDate);
                    return View(model);
                }
                if (start == null)
                {
                    ModelState.AddModelError("", AccountResources.RequiredStartTime);
                    return View(model);
                }
                if (end == null)
                {
                    ModelState.AddModelError("", AccountResources.RequiredEndTime);
                    return View(model);
                }
                if (model.StartLocationID == null)
                {
                    ModelState.AddModelError("CustomErrorStart", AccountResources.RequiredStartLocation);
                    return View(model);
                }
                if (model.EndLocationID == null)
                {
                    ModelState.AddModelError("CustomErrorEnd", AccountResources.RequiredEndLocation);
                    return View(model);
                }
                var Location = db.Location.ToList();
                var startLocationID = FindParentLocation(model.StartLocationID, Location);
                var endLocationID = FindParentLocation(model.EndLocationID, Location);

                var route = db.Route.Where(x => x.StartLocationID == startLocationID && x.EndLocationID == endLocationID).FirstOrDefault();

                if (route != null)
                {
                    model.RouteID = route.ID;
                }
                else
                {
                    ModelState.AddModelError("CustomErrorRoute", "Không tồn tại mã lộ trình cho nơi nhận và nơi trả ");
                    return View(model);
                }

                if (model.TripBack.HasValue() && model.TripBack.Trim().ToUpper() != "X")
                {
                    ModelState.AddModelError("CustomErrorTripBack", "Vui lòng nhập X hoặc x !");
                    return View(model);
                }

                //if (date.Value.Date <= dateDayNow.Date)
                //{
                //    ModelState.AddModelError("", AccountResources.NotAdd);
                //    return View(model);
                //}
                //else
                //{
                if (start.Value.TimeOfDay == end.Value.TimeOfDay)
                {
                    ModelState.AddModelError("CustomError", "Vui lòng nhập giờ đến không trùng giờ đi !");
                    return View(model);
                }

                var checkPlan = db.TransportPlans.Where(x => x.PlanDate == date && x.RouteID == model.RouteID && x.VehicleID == model.VehicleID
                && x.SourcePartnerID == model.SourcePartnerID && x.DestinationPartnerID == model.DestinationPartnerID && x.IsRemove != true).ToList();
                Boolean checkTime = true;
                string err = "Thời gian trùng:   ";
                if (checkPlan.Count() > 0)
                {
                    foreach (var item in checkPlan)
                    {
                        if (item.StartTime != null && item.EndTime != null)
                        {
                            var startDateTime = new DateTime(item.StartTime.Value);
                            var endDateTime = new DateTime(item.EndTime.Value);
                            if (startDateTime.TimeOfDay == start.Value.TimeOfDay && end.Value.TimeOfDay == endDateTime.TimeOfDay)
                            {
                                checkTime = false;
                                err += startDateTime.TimeOfDay.ToString() + " - " + endDateTime.TimeOfDay.ToString();
                            }

                        }
                    }
                    if (!checkTime)
                    {
                        ModelState.AddModelError("CustomError", err);
                        return View(model);
                    }
                }

                using (DbContextTransaction transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        model.PlanDate = DateTime.Parse(date.ToString());
                        model.StartTime = start.Value.Ticks;
                        model.EndTime = end.Value.Ticks;
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
                        db.Set<TransportPlan>().Add(model);
                        db.SaveChanges();

                        var Partner = db.Partner.ToList();
                        var PricingTable = db.PricingTable.ToList();
                        var Vehicle = db.Vehicle.ToList();
                        Double unitPrice = 0;
                        if (model.ActualWeightID != null)
                        {
                            unitPrice = CheckPrice(model.ActualWeightID, model.RouteID, model.SourcePartnerID, PricingTable);
                        }
                        else
                        {
                            var weightID = Vehicle.Where(x => x.ID == model.VehicleID).Select(x => x.WeightID).FirstOrDefault();
                            unitPrice = CheckPrice(weightID, model.RouteID, model.SourcePartnerID, PricingTable);
                        }
                        if (unitPrice == 0)
                        {
                            ModelState.AddModelError("", "Không tồn tại giá, vui lòng kiểm tra lại!");
                            return View(model);
                        }
                        //Create TransportActual for ST Partner Source
                        var transportActualST = new TransportActual();
                        transportActualST.ActualDate = model.PlanDate;
                        transportActualST.StartTime = model.StartTime;
                        transportActualST.EndTime = model.EndTime;
                        transportActualST.TrackingCode = model.TrackingCode;
                        transportActualST.RouteID = model.RouteID;
                        transportActualST.VehicleID = model.VehicleID;
                        transportActualST.SourcePartnerID = model.SourcePartnerID;
                        transportActualST.DestinationPartnerID = model.DestinationPartnerID;
                        transportActualST.ActualWeightID = model.ActualWeightID;
                        transportActualST.UnitPrice = unitPrice;
                        transportActualST.CreatedBy = model.CreatedBy;
                        transportActualST.CreatedDate = model.CreatedDate;
                        if(model.TripBack.HasValue())
                        {
                            transportActualST.TripCount = 0.5;
                        }
                        else
                        {
                            transportActualST.TripCount = 1;
                        }

                        transportActualST.Status = false;

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
                        transportActualST.UnitPriceAT = unitPrice;

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
                        transportActualST.UnitPriceHPC = unitPrice;

                        db.Set<TransportActual>().Add(transportActualST);
                        db.SaveChanges();


                        transaction.Commit();
                        //model.CreatedDate = DateTime.Now;
                        //model.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                        //model.ModifiedDate = null;
                        ViewBag.StartupScript = "create_success();";
                        return View(model);

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", AccountResources.PriceIsNotSaved);
                        return View(model);
                    }
                }
                //}
            }
            else
            {
                return View(model);
            }
        }

        private int FindParentLocation(int? id, List<Location> locations)
        {
            var parentLocation = locations.Where(x => x.ID == id).FirstOrDefault();
            if (parentLocation != null)
            {
                if (parentLocation.ParentID == null)
                {
                    return parentLocation.ID;
                }
                else
                {
                    return FindParentLocation(parentLocation.ParentID, locations);
                }
            }
            else
            {
                return 0;
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
            var model = db.Set<TransportPlan>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            ViewBag.RouteCode = model.Route.RouteCode;
            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "")] TransportPlan model, DateTime? date, DateTime? start, DateTime? end)
        {
            if (ModelState.IsValid)
            {

                var dateDayNow = DateTime.Now.AddDays(-10);
                if (date == null)
                {
                    ModelState.AddModelError("CustomErrorDate", AccountResources.RequiredPlanDate);
                    return View(model);
                }
                if (start == null)
                {
                    ModelState.AddModelError("", AccountResources.RequiredStartTime);
                    return View(model);
                }
                if (end == null)
                {
                    ModelState.AddModelError("", AccountResources.RequiredEndTime);
                    return View(model);
                }

                if (start.Value.TimeOfDay == end.Value.TimeOfDay)
                {
                    ModelState.AddModelError("CustomError", "Vui lòng nhập giờ đến không trùng giờ đi !");
                    return View(model);
                }
                if (model.StartLocationID == null)
                {
                    ModelState.AddModelError("CustomErrorStart", AccountResources.RequiredStartLocation);
                    return View(model);
                }
                if (model.EndLocationID == null)
                {
                    ModelState.AddModelError("CustomErrorEnd", AccountResources.RequiredEndLocation);
                    return View(model);
                }

                //if (date.Value.Date <= dateDayNow.Date)
                //{
                //    ModelState.AddModelError("", AccountResources.NotAdd);
                //    return View(model);
                //}
                //else
                //{

                var Location = db.Location.ToList();
                var startLocationID = FindParentLocation(model.StartLocationID, Location);
                var endLocationID = FindParentLocation(model.EndLocationID, Location);

                var route = db.Route.Where(x => x.StartLocationID == startLocationID && x.EndLocationID == endLocationID).FirstOrDefault();

                if (route != null)
                {
                    model.RouteID = route.ID;
                }
                else
                {
                    ModelState.AddModelError("CustomErrorRoute", "Không tồn tại mã lộ trình cho nơi nhận và nơi trả ");
                    return View(model);

                }

                model.PlanDate = DateTime.Parse(date.ToString());
                model.StartTime = start.Value.Ticks;
                model.EndTime = end.Value.Ticks;

                if (model.TripBack.HasValue() && model.TripBack.Trim().ToUpper() != "X")
                {
                    ModelState.AddModelError("CustomErrorTripBack", "Vui lòng nhập X hoặc x !");
                    return View(model);
                }

                Boolean CheckChange = false;
                Boolean CheckChangeActual = false;
                Boolean checkChangePrice = false;


                var oldPlan = db.TransportPlans.Where(x => x.ID == model.ID && x.IsRemove != true).FirstOrDefault();

                if (oldPlan.Note != model.Note || oldPlan.Amount != model.Amount || oldPlan.DetailCode != model.DetailCode)
                {
                    CheckChange = true;
                }

                if (oldPlan.StartTime != start.Value.Ticks || oldPlan.EndTime != end.Value.Ticks || oldPlan.PlanDate != model.PlanDate || oldPlan.RouteID != model.RouteID
                    || oldPlan.VehicleID != model.VehicleID || oldPlan.SourcePartnerID != model.SourcePartnerID
                    || oldPlan.DestinationPartnerID != model.DestinationPartnerID || oldPlan.ActualWeightID != model.ActualWeightID || oldPlan.StartLocationID != model
                    .StartLocationID || oldPlan.EndLocationID != model.EndLocationID ||oldPlan.TripBack != model.TripBack)
                {
                    if (oldPlan.RouteID != model.RouteID || oldPlan.VehicleID != model.VehicleID ||
                        oldPlan.SourcePartnerID != model.SourcePartnerID || oldPlan.ActualWeightID != model.ActualWeightID)
                    {
                        checkChangePrice = true;
                    }

                    CheckChangeActual = true;
                }


                if (CheckChange || CheckChangeActual)
                {

                    if (!CheckChangeActual)
                    {
                        oldPlan.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                        oldPlan.ModifiedDate = DateTime.Now;
                        oldPlan.Note = model.Note;
                        oldPlan.Amount = model.Amount;
                        oldPlan.DetailCode = model.DetailCode;
                        db.SaveChanges();
                        ViewBag.StartupScript = "edit_success();";
                        return View(oldPlan);

                    }
                    else
                    {
                        var Partner = db.Partner.ToList();
                        var PricingTable = db.PricingTable.ToList();
                        var Vehicle = db.Vehicle.ToList();

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
                                var vehicle = Vehicle.Where(x => x.ID == model.VehicleID).Select(x => x.WeightID).FirstOrDefault();
                                weightName = db.VehicleWeight.Where(x => x.ID == vehicle).Select(x => x.WeightName).FirstOrDefault();
                            }

                            oldPlan.TrackingCode = routeCode + "-" + weightName;
                        }

                        using (DbContextTransaction transaction = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var findTranActualST = db.TransportActuals.Where(x => x.ActualDate == oldPlan.PlanDate && x.StartTime == oldPlan.StartTime && x.EndTime == oldPlan.EndTime &&
                                x.RouteID == oldPlan.RouteID && x.VehicleID == oldPlan.VehicleID && x.ActualWeightID == oldPlan.ActualWeightID && x.SourcePartnerID == oldPlan.SourcePartnerID
                                && x.DestinationPartnerID == oldPlan.DestinationPartnerID && x.IsRemove != true).FirstOrDefault();

                                //edit transportPlan
                                oldPlan.RouteID = model.RouteID;
                                oldPlan.StartTime = model.StartTime;
                                oldPlan.EndTime = model.EndTime;
                                oldPlan.SourcePartnerID = model.SourcePartnerID;
                                oldPlan.DestinationPartnerID = model.DestinationPartnerID;
                                oldPlan.ActualWeightID = model.ActualWeightID;
                                oldPlan.VehicleID = model.VehicleID;
                                oldPlan.Note = model.Note;
                                oldPlan.Amount = model.Amount;
                                oldPlan.DetailCode = model.DetailCode;
                                oldPlan.PlanDate = model.PlanDate;
                                oldPlan.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                                oldPlan.ModifiedDate = DateTime.Now;
                                oldPlan.StartLocationID = model.StartLocationID;
                                oldPlan.EndLocationID = model.EndLocationID;
                                oldPlan.TripBack = model.TripBack;
                                db.SaveChanges();

                                Double unitPrice = 0;
                                // compare TransportActual for Partner ST

                                if (findTranActualST != null)
                                {
                                    var enumaration = new EnumerationChangeLog();
                                    enumaration.Time = findTranActualST.ActualDate;
                                    enumaration.StartTime = findTranActualST.StartTime;
                                    enumaration.EndTime = findTranActualST.EndTime;
                                    enumaration.RouteID = findTranActualST.RouteID;
                                    enumaration.VehicleID = findTranActualST.VehicleID;
                                    enumaration.ActualWeightID = findTranActualST.ActualWeightID;
                                    enumaration.TripCount = findTranActualST.TripCount;
                                    enumaration.DestinationPartner = findTranActualST.DestinationPartnerID;
                                    enumaration.SourcePartner = findTranActualST.SourcePartnerID;
                                    enumaration.CreatedDate = oldPlan.ModifiedDate;
                                    enumaration.CreatedBy = oldPlan.ModifiedBy;
                                    enumaration.UnitPrice = findTranActualST.UnitPrice;
                                    enumaration.UnitPriceAT = findTranActualST.UnitPriceAT;
                                    enumaration.UnitPriceHPC = findTranActualST.UnitPriceHPC;

                                    findTranActualST.RouteID = oldPlan.RouteID;
                                    findTranActualST.VehicleID = oldPlan.VehicleID;
                                    findTranActualST.ActualDate = oldPlan.PlanDate;
                                    findTranActualST.StartTime = oldPlan.StartTime;
                                    findTranActualST.EndTime = oldPlan.EndTime;
                                    findTranActualST.SourcePartnerID = oldPlan.SourcePartnerID;
                                    findTranActualST.DestinationPartnerID = oldPlan.DestinationPartnerID;
                                    findTranActualST.ModifiedBy = oldPlan.ModifiedBy;
                                    findTranActualST.ModifiedDate = oldPlan.ModifiedDate;
                                    findTranActualST.ActualWeightID = oldPlan.ActualWeightID;
                                    findTranActualST.TrackingCode = oldPlan.TrackingCode;
                                    findTranActualST.Status = false;

                                    if(oldPlan.TripBack.HasValue())
                                    {
                                        findTranActualST.TripCount = 0.5;
                                    }
                                    else
                                    {
                                        findTranActualST.TripCount = 1;
                                    }

                                    if (checkChangePrice)
                                    {
                                        if (model.ActualWeightID != null)
                                        {
                                            unitPrice = CheckPrice(model.ActualWeightID, model.RouteID, model.SourcePartnerID, PricingTable);
                                        }
                                        else
                                        {
                                            var weightID = Vehicle.Where(x => x.ID == model.VehicleID).Select(x => x.WeightID).FirstOrDefault();
                                            unitPrice = CheckPrice(weightID, model.RouteID, model.SourcePartnerID, PricingTable);
                                        }

                                        if (unitPrice == 0)
                                        {
                                            transaction.Rollback();
                                            ModelState.AddModelError("", "Không tồn tại giá!");
                                            return View(model);
                                        }
                                        else
                                        {
                                            findTranActualST.UnitPrice = unitPrice;
                                        }
                                    }

                                    //check price for AT Partner
                                    var ATId = Partner.Where(x => x.PartnerCode == "AT").Select(x => x.ID).FirstOrDefault();
                                    if (checkChangePrice)
                                    {
                                        if (model.ActualWeightID != null)
                                        {
                                            unitPrice = CheckPrice(model.ActualWeightID, model.RouteID, ATId, PricingTable);
                                        }
                                        else
                                        {
                                            var weightID = Vehicle.Where(x => x.ID == model.VehicleID).Select(x => x.WeightID).FirstOrDefault();
                                            unitPrice = CheckPrice(weightID, model.RouteID, ATId, PricingTable);
                                        }
                                        findTranActualST.UnitPriceAT = unitPrice;
                                    }

                                    //check price for HPC Partner
                                    var HPCId = Partner.Where(x => x.PartnerCode == "HPC").Select(x => x.ID).FirstOrDefault();
                                    if (checkChangePrice)
                                    {
                                        if (model.ActualWeightID != null)
                                        {
                                            unitPrice = CheckPrice(model.ActualWeightID, model.RouteID, HPCId, PricingTable);
                                        }
                                        else
                                        {
                                            var weightID = Vehicle.Where(x => x.ID == model.VehicleID).Select(x => x.WeightID).FirstOrDefault();
                                            unitPrice = CheckPrice(weightID, model.RouteID, HPCId, PricingTable);
                                        }

                                        findTranActualST.UnitPriceHPC = unitPrice;
                                    }

                                    db.SaveChanges();
                                    enumaration.ChangeTime = findTranActualST.ActualDate;
                                    enumaration.ChangeStartTime = findTranActualST.StartTime;
                                    enumaration.ChangeEndTime = findTranActualST.EndTime;
                                    enumaration.ChangeRouteID = findTranActualST.RouteID;
                                    enumaration.ChangeVehicleID = findTranActualST.VehicleID;
                                    enumaration.ChangeActualWeight = findTranActualST.ActualWeightID;
                                    enumaration.ChangeTripCount = findTranActualST.TripCount;
                                    enumaration.ChangeDestinationPartner = findTranActualST.DestinationPartnerID;
                                    enumaration.ChangeSourcePartner = findTranActualST.SourcePartnerID;
                                    enumaration.ChangeUnitPrice = findTranActualST.UnitPrice;
                                    enumaration.ChangeUnitPriceAT = findTranActualST.UnitPriceAT;
                                    enumaration.ChangeUnitPriceHPC = findTranActualST.UnitPriceHPC;

                                    db.Set<EnumerationChangeLog>().Add(enumaration);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    transaction.Rollback();
                                    ModelState.AddModelError("", "Không tồn tại trong bảng kê tương ứng!");
                                    return View(model);
                                }

                                transaction.Commit();

                                ViewBag.StartupScript = "edit_success();";
                                return View(model);
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                ModelState.AddModelError("", AccountResources.PriceIsNotSaved);
                                return View(model);
                            }
                        }
                    }
                }
                else
                {
                    ViewBag.StartupScript = "edit_success();";
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }

        public ActionResult Show(int id)
        {
            var model = db.Set<TransportPlan>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            return View("Show", model);
        }

        [HttpPost]
        public ActionResult Deletes(string dataStringDelete, string dataStringDate)
        {
            List<TransportPlan> listData = new List<TransportPlan>();
            var dataListJson = dataStringDelete.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                TransportPlan dataObj = null;
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
                dataObj = JsonConvert.DeserializeObject<TransportPlan>(dataObjString);
                dataObj.PlanDate = dataObj.PlanDate.ToLocalTime();
                listData.Add(dataObj);
            }

            var temp = new List<TransportPlan>();
            foreach (var item in listData)
            {
                using (DbContextTransaction transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var findTransportActual = db.TransportActuals.Where(x => x.ActualDate == item.PlanDate && x.RouteID == item.RouteID && x.VehicleID == item.VehicleID
                        && x.ActualWeightID == item.ActualWeightID && x.SourcePartnerID == item.SourcePartnerID && x.DestinationPartnerID == item.DestinationPartnerID &&
                        x.StartTime == item.StartTime && x.EndTime == item.EndTime && x.IsRemove != true).FirstOrDefault();
                        if (findTransportActual != null)
                        {
                            var enumarationDelete = new EnumerationChangeLog();
                            enumarationDelete.Time = findTransportActual.ActualDate;
                            enumarationDelete.RouteID = findTransportActual.RouteID;
                            enumarationDelete.VehicleID = findTransportActual.VehicleID;
                            enumarationDelete.ActualWeightID = findTransportActual.ActualWeightID;
                            enumarationDelete.TripCount = findTransportActual.TripCount;
                            enumarationDelete.DestinationPartner = findTransportActual.DestinationPartnerID;
                            enumarationDelete.SourcePartner = findTransportActual.SourcePartnerID;
                            enumarationDelete.CreatedDate = DateTime.Now;
                            enumarationDelete.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                            enumarationDelete.UnitPrice = findTransportActual.UnitPrice;
                            enumarationDelete.IsRemove = true;
                            db.Set<EnumerationChangeLog>().Add(enumarationDelete);
                            db.SaveChanges();

                            findTransportActual.ModifiedBy = enumarationDelete.CreatedBy;
                            findTransportActual.ModifiedDate = enumarationDelete.CreatedDate;
                            findTransportActual.IsRemove = true;
                            db.SaveChanges();
                        }
                        else
                        {
                            transaction.Rollback();
                            temp.Add(item);
                            continue;
                        }

                        var transportPlanChange = db.TransportPlans.Find(item.ID);
                        transportPlanChange.IsRemove = true;
                        db.SaveChanges();

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
                return RedirectToAction("Index", new { dataString = dataStringDate });
            }
            else if (temp.Count > 0)
            {
                ViewBag.StartupScript = "deletes_unsuccess();";
                return RedirectToAction("Index", new { dataString = dataStringDate });
            }
            else
            {
                ViewBag.StartupScript = "deletes_success();";
                return RedirectToAction("Index", new { dataString = dataStringDate });
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

            return File(dateGet, "application/ms-excel", "QLKeHoachChay_UploadResult.xlsx");

        }

        [HttpPost]
        public ActionResult ExportExcel(string dataString)
        {
            List<TransportPlanExport> listData = new List<TransportPlanExport>();
            var dataListJson = dataString.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                TransportPlanExport dataObj = null;
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
                dataObj = JsonConvert.DeserializeObject<TransportPlanExport>(dataObjString);
                dataObj.PlanDate = dataObj.PlanDate.ToLocalTime();
                listData.Add(dataObj);
            }
            var result = DownloadDrivePlan(listData);
            var fileStream = new MemoryStream(result);
            return File(fileStream, "application/ms-excel", "QLKeHoachChay.xlsx");
        }
        public byte[] DownloadDrivePlan(List<TransportPlanExport> models)
        {
            var queryPlan = from a in models
                            group a by new { a.PlanDate };
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLKeHoachChay.xlsx", HostingEnvironment.MapPath("/Uploads")));

            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var productWorksheet = p.Workbook.Worksheets[0];
                    productWorksheet.Select();
                    int i = 0;

                    foreach (var listPlan in queryPlan)
                    {
                        int start = i + 3;

                        var queryVehicle = from a in listPlan
                                           group a by new { a.NumberPlate };

                        foreach (var listVehice in queryVehicle)
                        {
                            productWorksheet.Cells["B" + (i + 3).ToString() + ":N" + (i + 3).ToString()].Merge = true;
                            productWorksheet.Cells[i + 3, 2].Value = listVehice.First().NumberPlate;
                            Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#cfd8dc");
                            productWorksheet.Cells[i + 3, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            productWorksheet.Cells[i + 3, 2].Style.Fill.BackgroundColor.SetColor(colFromHex);
                            productWorksheet.Cells[i + 3, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            i++;

                            foreach (var item in listVehice)
                            {
                                productWorksheet.Cells[i + 3, 2].Value = item.NumberPlate;
                                productWorksheet.Cells[i + 3, 3].Value = item.CarOwerName;
                                productWorksheet.Cells[i + 3, 4].Value = item.DestinationPartnerName;

                                if (item.VehicleWeightName != null)
                                {
                                    Double weightNumber;

                                    if (Double.TryParse(item.VehicleWeightName.Replace(".", ",").ToString(), out weightNumber))
                                    {
                                        productWorksheet.Cells[i + 3, 5].Value = weightNumber;

                                    }
                                    else
                                    {
                                        productWorksheet.Cells[i + 3, 5].Value = item.VehicleWeightName;
                                    }
                                }

                                productWorksheet.Cells[i + 3, 6].Value = item.DetailCode;
                                productWorksheet.Cells[i + 3, 7].Value = item.Amount;
                                productWorksheet.Cells[i + 3, 8].Value = item.StartLocationnName;
                                if (item.StartTime != null)
                                {
                                    DateTime StartDateTime = new DateTime(item.StartTime.Value);
                                    productWorksheet.Cells[i + 3, 9].Value = StartDateTime.ToString("HH:mm");
                                }
                                productWorksheet.Cells[i + 3, 10].Value = item.EndLocationName;

                                if (item.EndTime != null)
                                {
                                    DateTime EndDateTime = new DateTime(item.EndTime.Value);
                                    productWorksheet.Cells[i + 3, 11].Value = EndDateTime.ToString("HH:mm");
                                }
                                productWorksheet.Cells[i + 3, 12].Value = item.Note;

                                if (item.ActualWeightName != null)
                                {
                                    Double weightNumber;

                                    if (Double.TryParse(item.ActualWeightName.Replace(".", ",").ToString(), out weightNumber))
                                    {
                                        productWorksheet.Cells[i + 3, 13].Value = weightNumber;

                                    }
                                    else
                                    {
                                        productWorksheet.Cells[i + 3, 13].Value = item.ActualWeightName;
                                    }
                                }

                                i++;
                            }
                            var count = listPlan.Count();
                            productWorksheet.Cells["A" + start.ToString() + ":A" + (i + 2).ToString()].Merge = true;
                            productWorksheet.Cells[start, 1].Value = listPlan.First().PlanDate;
                            i++;
                        }
                    }
                    string test1 = "A3:N" + (i + 1).ToString();
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
                var fileTemp = new FileInfo(string.Format(@"{0}\QLKeHoachChay.xlsx", HostingEnvironment.MapPath("/Uploads")));
                var check = ExcelHelper.CheckHeaderFile(fileTemp, file, 1);
                if (check == 1)
                {
                    var result = new UploadDrivePlanFromExcel().UploadProducts(file, Session["UploadPlanProgress"]);

                    if (result == null)
                    {
                        ViewBag.check = "Upload kế hoạch thành công!";
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
                var result = new UploadDrivePlanFromExcel().DownloadTemplate();
                var fileStream = new MemoryStream(result);
                return File(fileStream, "application/ms-excel", "QLKeHoachChay_Template.xlsx");
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult FindRoute(TransportPlan model)
        {
            var Location = db.Location.ToList();
            var startLocation = FindParentLocation(model.StartLocationID, Location);
            var endLocation = FindParentLocation(model.EndLocationID, Location);
            model.Route = db.Route.Where(x => x.StartLocationID == startLocation && x.EndLocationID == endLocation).FirstOrDefault();
            var routeCode = "";
            if (model.Route != null)
            {
                routeCode = model.Route.RouteCode;
            }
            return Json(new { ErrorMessage = string.Empty, routeID = routeCode }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ChangeStatus(GetSelected model)
        {
            var temp = from p in db.Set<TransportPlan>()
                       where model.ID.Contains(p.ID)
                       select p;
            foreach (var item in temp)
            {
                item.Status = true;
                item.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                item.ModifiedDate = DateTime.Now;
            }
            db.SaveChanges();
            return Json(new { ErrorMessage = string.Empty }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult UnChangeStatus(GetSelected model)
        {
            var temp = from p in db.Set<TransportPlan>()
                       where model.ID.Contains(p.ID)
                       select p;

            foreach (var item in temp)
            {
                item.Status = false;
                item.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                item.ModifiedDate = DateTime.Now;
            }
            db.SaveChanges();
            return Json(new { ErrorMessage = string.Empty }, JsonRequestBehavior.AllowGet);
        }
    }
}


