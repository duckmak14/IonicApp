using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using WebMatrix.WebData;
using WebModels;
using WEB.Areas.Admin.Controllers;
using WEB.Models;
using System.Globalization;
using System.Data.Entity;

namespace WEB.Areas.ContentType.Controllers
{
    [VanTaiAuthorize]

    public class UploadDrivePlanFromExcel
    {
        WebContext db = new WebContext();
        private List<ErrorModelPlanList> ErrorModels = new List<ErrorModelPlanList>();

        public byte[] DownloadTemplate()
        {
            var fileinfo = new FileInfo(string.Format(@"{0}\QLKeHoachChay.xlsx", HostingEnvironment.MapPath("/Uploads")));
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    return p.GetAsByteArray();
                }
            }

            return null;
        }


        private List<WebContent> GetListContents(int webModuleId, List<WebContent> results)
        {
            var webContents = db.WebContents.Where(x => x.WebModuleID == webModuleId && x.Status.HasValue && x.Status.Value.Equals((int)Status.Public));
            results.AddRange(webContents);

            var childWebModules = db.WebModules.Where(x => x.ParentID == webModuleId);

            foreach (var childWebModule in childWebModules)
            {
                GetListContents(childWebModule.ID, results);
            }

            return results;
        }

        public byte[] UploadProducts(HttpPostedFileBase file, object uploadProgressSession)
        {


            ExcelPackage.LicenseContext = LicenseContext.Commercial;

            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                using (var package = new ExcelPackage(file.InputStream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfRow = workSheet.Dimension.End.Row;

                    DateTime dateBegin = DateTime.Now;
                    var Vehicle = db.Vehicle.ToList();
                    var Route = db.Route.ToList();
                    var Partner = db.Partner.ToList();
                    var Location = db.Location.ToList();
                    var VehicleWeight = db.VehicleWeight.ToList();
                    var TransportPlans = db.TransportPlans.ToList();
                    var TransportActuals = db.TransportActuals.ToList();
                    var PricingTable = db.PricingTable.ToList();

                    for (int rowIterator = 3; rowIterator <= noOfRow; rowIterator++)
                    {
                        if (IsRowEmpty(workSheet, rowIterator))
                        {
                            continue; // ket thuc
                        }

                        var planInfo = new TransportPlan();
                        var isValid = true;

                        workSheet.Cells[rowIterator, 1].Style.Numberformat.Format = "dd/MM/yyyy";
                        var planDate = CastToProperty<string>(workSheet, rowIterator, 1, "Thời gian", true);
                        if (planDate.IsSuccess)
                        {
                            try
                            {
                                int dateParse;
                                if (Int32.TryParse(planDate.Value, out dateParse))
                                {
                                    dateBegin = DateTime.FromOADate(double.Parse(dateParse.ToString()));
                                }
                                else
                                {
                                    workSheet.Cells[rowIterator, 1].Style.Numberformat.Format = "dd/MM/yyyy";
                                    dateBegin = DateTime.Parse(planDate.Value.ToString(), new CultureInfo("vi-VN", false));
                                }
                                planInfo.PlanDate = dateBegin;

                            }
                            catch (Exception ex)
                            {
                                AddErrorModels(rowIterator, 1, "Vui lòng nhập đúng định dạng thời gian");
                                isValid = false;
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        var StartTime = CastToProperty<string>(workSheet, rowIterator, 9, "Giờ đi");
                        if (StartTime.IsSuccess && isValid)
                        {
                            try
                            {
                                DateTime date = DateTime.Parse(StartTime.Value.Format("{0:dd/mm/yyyy}"));
                                DateTime dateTime = new DateTime(planInfo.PlanDate.Year, planInfo.PlanDate.Month, planInfo.PlanDate.Day, date.Hour, date.Minute, date.Second);
                                planInfo.StartTime = dateTime.Ticks;
                            }
                            catch (Exception ex)
                            {
                                AddErrorModels(rowIterator, 9, "Không đúng định dạng thời gian");
                                isValid = false;
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        var EndTime = CastToProperty<string>(workSheet, rowIterator, 11, "Giờ đến");
                        if (EndTime.IsSuccess && isValid)
                        {
                            try
                            {
                                DateTime date = DateTime.Parse(EndTime.Value.Format("{0:dd/mm/yyyy}"));
                                DateTime dateTime = new DateTime(planInfo.PlanDate.Year, planInfo.PlanDate.Month, planInfo.PlanDate.Day, date.Hour, date.Minute, date.Second);
                                planInfo.EndTime = dateTime.Ticks;
                            }
                            catch (Exception ex)
                            {
                                AddErrorModels(rowIterator, 11, "Không đúng định dạng thời gian");
                                isValid = false;
                            }
                        }
                        else
                        {
                            isValid = false;
                        }


                        if (planInfo.StartTime.HasValue && planInfo.EndTime.HasValue)
                        {
                            var PlanStartTime = new DateTime(planInfo.StartTime.Value);
                            var PlanEndTime = new DateTime(planInfo.EndTime.Value);
                            if (PlanStartTime.TimeOfDay == PlanEndTime.TimeOfDay)
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 9, "Vui lòng nhập giờ đến không trùng giờ đi !");
                            }
                        }

                        var souceLocation = CastToProperty<string>(workSheet, rowIterator, 8, "Nơi nhận");

                        if (souceLocation.IsSuccess)
                        {
                            var sourceLocationIsExist = Location.Where(x => x.LocationName.ToUpper() == souceLocation.Value.ToUpper()).Select(x => x.ID).FirstOrDefault();

                            if (sourceLocationIsExist > 0)
                            {
                                planInfo.StartLocationID = sourceLocationIsExist;
                            }
                            else
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 8, "Nơi nhận không tồn tại");
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        var destinationLocation = CastToProperty<string>(workSheet, rowIterator, 10, "Nơi trả");
                        if (destinationLocation.IsSuccess)
                        {
                            var desLocationIsExist = Location.Where(x => x.LocationName.ToUpper() == destinationLocation.Value.ToUpper()).Select(x => x.ID).FirstOrDefault();

                            if (desLocationIsExist > 0)
                            {
                                planInfo.EndLocationID = desLocationIsExist;
                            }
                            else
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 10, "Nơi giao không tồn tại");
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        var startLocationID = FindParentLocation(planInfo.StartLocationID, Location);
                        var endLocationID = FindParentLocation(planInfo.EndLocationID, Location);

                        var routeCode = Route.Where(x => x.StartLocationID == startLocationID && x.EndLocationID == endLocationID).FirstOrDefault();
                        if (routeCode != null)
                        {
                            planInfo.RouteID = routeCode.ID;
                        }
                        else
                        {
                            isValid = false;
                            AddErrorModels(rowIterator, 8, "Không tồn tại mã lộ trình cho nơi nhận và nơi trả ");
                        }

                        var actualWeight = CastToProperty<string>(workSheet, rowIterator, 13, "Tải trọng chạy", false);
                        if (actualWeight.IsSuccess)
                        {
                            if (actualWeight.Value != null)
                            {
                                var weightIsExist = new VehicleWeight();
                                try
                                {
                                    double weightValue;
                                    double weightValueExists;

                                    foreach (var item in VehicleWeight)
                                    {
                                        var itemString = item.WeightName.Replace(".", ",");
                                        if (Double.TryParse(actualWeight.Value, out weightValue))
                                        {
                                            if (Double.TryParse(itemString, out weightValueExists))
                                            {
                                                if (weightValue == weightValueExists)
                                                {
                                                    weightIsExist = item;
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            weightIsExist = VehicleWeight.Where(x => x.WeightName == actualWeight.Value).FirstOrDefault();
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    isValid = false;
                                    AddErrorModels(rowIterator, 13, "Không thể kiểm tra tải trọng");
                                }

                                if (weightIsExist != null)
                                {
                                    var id = weightIsExist;
                                    planInfo.ActualWeightID = id.ID;
                                }
                                else
                                {
                                    isValid = false;
                                    AddErrorModels(rowIterator, 13, "Tải trọng không tồn tại");
                                }
                            }
                        }
                        else
                        {
                            isValid = false;
                        }



                        var numberPlate = CastToProperty<string>(workSheet, rowIterator, 2, "BKS");
                        if (numberPlate.IsSuccess)
                        {
                            var countCharacter = numberPlate.Value.Length;
                            if (countCharacter < 5)
                            {
                                numberPlate.Value = "0" + numberPlate.Value;
                            }

                            Vehicle numberPlateExists = null;
                            foreach (var item in Vehicle)
                            {
                                if (item.NumberPlate.Length == 4)
                                {
                                    item.NumberPlate = "0" + item.NumberPlate;
                                }
                                var test = item.NumberPlate.Substring(item.NumberPlate.Length - 5, 5);
                                if (item.NumberPlate.Substring(item.NumberPlate.Length - 5, 5) == numberPlate.Value.Substring(numberPlate.Value.Length - 5, 5) && item.NumberPlate.Length >= 5)
                                {
                                    numberPlateExists = item;
                                }

                            }

                            if (numberPlateExists != null)
                            {
                                planInfo.VehicleID = numberPlateExists.ID;
                            }
                            else
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 2, "Xe không tồn tại");
                            }
                        }
                        else
                        {
                            isValid = false;
                        }


                        var destinationPartner = CastToProperty<string>(workSheet, rowIterator, 4, "Đơn vị thuê");
                        if (destinationPartner.IsSuccess)
                        {
                            var destinationPartnerIsExist = Partner.Where(x => x.PartnerCode == destinationPartner.Value).Select(x => x.ID).FirstOrDefault();

                            if (destinationPartnerIsExist > 0)
                            {
                                planInfo.DestinationPartnerID = destinationPartnerIsExist;
                            }
                            else
                            {
                                destinationPartnerIsExist = Partner.Where(x => x.PartnerName == destinationPartner.Value).Select(x => x.ID).FirstOrDefault();
                                if (destinationPartnerIsExist > 0)
                                {
                                    planInfo.DestinationPartnerID = destinationPartnerIsExist;
                                }
                                else
                                {
                                    isValid = false;
                                    AddErrorModels(rowIterator, 4, "Đơn vị thuê không tồn tại!");
                                }

                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        var sourcePartner = Partner.Where(x => x.PartnerCode == "ST").Select(x => x.ID).FirstOrDefault();
                        planInfo.SourcePartnerID = sourcePartner;

                        var note = CastToProperty<string>(workSheet, rowIterator, 12, "Ghi chú", false);
                        if (note.IsSuccess)
                        {
                            planInfo.Note = note.Value;
                        }
                        else
                        {
                            isValid = false;
                        }
                        var amount = CastToProperty<string>(workSheet, rowIterator, 7, "Số lượng", false);
                        if (amount.IsSuccess)
                        {
                            planInfo.Amount = amount.Value;
                        }
                        else
                        {
                            isValid = false;
                        }

                        var detailCode = CastToProperty<string>(workSheet, rowIterator, 6, "Mã chi tiết", false);
                        if (detailCode.IsSuccess)
                        {
                            planInfo.DetailCode = detailCode.Value;
                        }
                        else
                        {
                            isValid = false;
                        }

                        Double tripCount = 1;
                        var checkTripCount = CastToProperty<string>(workSheet, rowIterator, 14, "Chiều về", false);
                        if (checkTripCount.IsSuccess)
                        {
                            if (checkTripCount.Value != null)
                            {
                                if (checkTripCount.Value.Trim().ToUpper() == "X")
                                {
                                    tripCount = 0.5;
                                    planInfo.TripBack = "X";
                                }
                                else
                                {
                                    isValid = false;
                                    AddErrorModels(rowIterator, 14, "Vui lòng kiểm tra lại kí tự đã điền!");
                                }
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        Double unitPrice;
                        if (planInfo.ActualWeightID != null)
                        {
                            unitPrice = CheckPrice(planInfo.ActualWeightID, planInfo.RouteID, planInfo.SourcePartnerID, PricingTable);
                        }
                        else
                        {
                            var weightID = Vehicle.Where(x => x.ID == planInfo.VehicleID).Select(x => x.WeightID).FirstOrDefault();
                            unitPrice = CheckPrice(weightID, planInfo.RouteID, planInfo.SourcePartnerID, PricingTable);
                        }

                        if (planInfo.VehicleID.HasValue && planInfo.VehicleID.Value != 0 && unitPrice == 0 && isValid)
                        {
                            isValid = false;
                            AddErrorModels(rowIterator, 1, "Không tồn tại giá !");
                        }


                        // Check Date > Date.Now(-3)
                        //var dateDayNow = DateTime.Now.AddDays(-3); ;
                        //if (planInfo.PlanDate.Date <= dateDayNow.Date && isValid)
                        //{
                        //    isValid = false;
                        //    AddErrorModels(rowIterator, 1, "Vui lòng nhập ngày lớn hơn!");

                        //}

                        //if conditions was success
                        if (isValid)
                        {
                            // Check overlap datetime
                            var checkPlan = TransportPlans.Where(x => x.PlanDate == planInfo.PlanDate && x.RouteID == planInfo.RouteID && x.VehicleID == planInfo.VehicleID
                                 && x.SourcePartnerID == planInfo.SourcePartnerID && x.DestinationPartnerID == planInfo.DestinationPartnerID && x.ActualWeightID == planInfo.ActualWeightID && x.IsRemove != true).ToList();

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
                                        var PlanStartTime = new DateTime(planInfo.StartTime.Value);
                                        var PlanEndTime = new DateTime(planInfo.EndTime.Value);

                                        if (startDateTime.TimeOfDay == PlanStartTime.TimeOfDay && PlanEndTime.TimeOfDay == endDateTime.TimeOfDay)
                                        {
                                            checkTime = false;
                                            err += startDateTime.TimeOfDay.ToString() + " - " + endDateTime.TimeOfDay.ToString();
                                        }
                                    }
                                }
                                if (!checkTime)
                                {
                                    isValid = false;
                                    AddErrorModels(rowIterator, 1, err);

                                }
                            }
                            if (!isValid)
                            {
                                continue;
                            }

                            using (DbContextTransaction transaction = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    string weightName;
                                    var routeCodeTracking = db.Route.Where(x => x.ID == planInfo.RouteID).Select(x => x.RouteCode).FirstOrDefault();
                                    if (planInfo.ActualWeightID != null)
                                    {
                                        weightName = db.VehicleWeight.Where(x => x.ID == planInfo.ActualWeightID).Select(x => x.WeightName).FirstOrDefault();
                                    }
                                    else
                                    {
                                        var vehicle = db.Vehicle.Where(x => x.ID == planInfo.VehicleID).Select(x => x.WeightID).FirstOrDefault();
                                        weightName = db.VehicleWeight.Where(x => x.ID == vehicle).Select(x => x.WeightName).FirstOrDefault();
                                    }
                                    planInfo.TrackingCode = routeCodeTracking + "-" + weightName;

                                    planInfo.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                                    planInfo.CreatedDate = DateTime.Now;
                                    db.Set<TransportPlan>().Add(planInfo);
                                    db.SaveChanges();

                                    //check TransportActual is valid
                                    var checkActualValid = TransportActuals.Where(x => x.ActualDate == planInfo.PlanDate &&
                                    x.StartTime == planInfo.StartTime && x.EndTime == planInfo.EndTime && x.SourcePartnerID == planInfo.SourcePartnerID
                                    && x.DestinationPartnerID == planInfo.DestinationPartnerID && x.RouteID == planInfo.RouteID &&
                                    x.VehicleID == planInfo.VehicleID && x.ActualWeightID == planInfo.ActualWeightID && x.IsRemove != true).FirstOrDefault();
                                    if (checkActualValid != null)
                                    {
                                        AddErrorModels(rowIterator, 1, "Đã xảy ra lỗi, không đồng bộ bảng kê và bảng kế hoạch! Vui lòng liên hệ quản trị viên");
                                        transaction.Rollback();
                                        continue;
                                    }

                                    //Create TransportActual for ST Partner Source
                                    var transportActualST = new TransportActual();
                                    transportActualST.ActualDate = planInfo.PlanDate;
                                    transportActualST.StartTime = planInfo.StartTime;
                                    transportActualST.EndTime = planInfo.EndTime;
                                    transportActualST.TrackingCode = planInfo.TrackingCode;
                                    transportActualST.RouteID = planInfo.RouteID;
                                    transportActualST.VehicleID = planInfo.VehicleID;
                                    transportActualST.SourcePartnerID = planInfo.SourcePartnerID;
                                    transportActualST.DestinationPartnerID = planInfo.DestinationPartnerID;
                                    transportActualST.ActualWeightID = planInfo.ActualWeightID;
                                    transportActualST.UnitPrice = unitPrice;
                                    transportActualST.CreatedBy = planInfo.CreatedBy;
                                    transportActualST.CreatedDate = planInfo.CreatedDate;
                                    transportActualST.TripCount = tripCount;
                                    transportActualST.Status = false;

                                    //Check price for AT Price Table
                                    var ATId = Partner.Where(x => x.PartnerCode == "AT").Select(x => x.ID).FirstOrDefault();

                                    if (planInfo.ActualWeightID != null)
                                    {
                                        unitPrice = CheckPrice(planInfo.ActualWeightID, planInfo.RouteID, ATId, PricingTable);
                                    }
                                    else
                                    {
                                        var weightID = Vehicle.Where(x => x.ID == planInfo.VehicleID).Select(x => x.WeightID).FirstOrDefault();
                                        unitPrice = CheckPrice(weightID, planInfo.RouteID, ATId, PricingTable);
                                    }
                                    transportActualST.UnitPriceAT = unitPrice;

                                    //Check price for HPC Price Table
                                    var HPCId = Partner.Where(x => x.PartnerCode == "HPC").Select(x => x.ID).FirstOrDefault();
                                    if (planInfo.ActualWeightID != null)
                                    {
                                        unitPrice = CheckPrice(planInfo.ActualWeightID, planInfo.RouteID, HPCId, PricingTable);
                                    }
                                    else
                                    {
                                        var weightID = Vehicle.Where(x => x.ID == planInfo.VehicleID).Select(x => x.WeightID).FirstOrDefault();
                                        unitPrice = CheckPrice(weightID, planInfo.RouteID, HPCId, PricingTable);
                                    }
                                    transportActualST.UnitPriceHPC = unitPrice;

                                    db.Set<TransportActual>().Add(transportActualST);
                                    db.SaveChanges();

                                    transaction.Commit();
                                }
                                catch (Exception e)
                                {
                                    AddErrorModels(rowIterator, 1, "Đã xảy ra lỗi, vui lòng liên hệ quản trị viên!");
                                    transaction.Rollback();

                                }
                            }
                        }
                    }
                    if (!ErrorModels.Any())
                    {
                        return null;
                    }
                    return LogErrorsToFile(workSheet);
                }
            }

            return null;
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

        private string GetImageName(string linkImage)
        {
            int index = linkImage.LastIndexOf('/');
            int index2 = linkImage.LastIndexOf('_');
            var name = linkImage.Substring(index + 1, (index2 - 1) - index);

            return name;
        }

        private WebConfig Getconfig(string key)
        {
            var config = (from c in db.WebConfigs
                          where c.Key.Equals(key)
                          select c);

            return config.FirstOrDefault();
        }


        private void AddErrorModels(int rowNumber, int columnNuber, string error)
        {
            ErrorModels.Add(new ErrorModelPlanList()
            {
                ColumnNumber = columnNuber,
                RowNumber = rowNumber,
                ErrorMessage = error
            });
        }

        private CastResultPlanList<T> CastToProperty<T>(ExcelWorksheet worksheet, int rowNumber, int columnNuber, string property, bool isRequired = true)
        {
            if (isRequired && (worksheet.Cells[rowNumber, columnNuber].Value == null || string.IsNullOrWhiteSpace(worksheet.Cells[rowNumber, columnNuber].Value.ToString())))
            {
                ErrorModels.Add(new ErrorModelPlanList()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không được để trống"
                });

                return new CastResultPlanList<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
            try
            {
                return new CastResultPlanList<T>()
                {
                    Value = (T)Convert.ChangeType(worksheet.Cells[rowNumber, columnNuber].Value, typeof(T)),
                    IsSuccess = true
                };
            }
            catch (FormatException ex)
            {
                ErrorModels.Add(new ErrorModelPlanList()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không đúng định dạng"
                });

                return new CastResultPlanList<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
        }

        private byte[] LogErrorsToFile(ExcelWorksheet worksheet)
        {
            var noOfCol = worksheet.Dimension.End.Column;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLKeHoachChay.xlsx", HostingEnvironment.MapPath("/Uploads")));
            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var ws = p.Workbook.Worksheets.First();
                    // set rows
                    if (!ErrorModels.Any())
                    {
                        ws.Cells["A2:J2"].Merge = true;
                        ws.Cells[2, 1].Value = "Tất cả kế hoạch đã được nhập thành công!";
                    }

                    var allRowNumbers = ErrorModels.Select(x => x.RowNumber).Distinct().OrderBy(x => x).ToList();

                    for (int i = 0; i < allRowNumbers.Count(); i++)
                    {
                        for (int j = 1; j <= noOfCol; j++)
                        {
                            ws.Cells[i + 3, j].Value = worksheet.Cells[allRowNumbers[i], j].Value;

                            var errorModel = ErrorModels.FirstOrDefault(x => x.RowNumber == allRowNumbers[i] && x.ColumnNumber == j);
                            if (errorModel != null)
                            {
                                ws.Cells[i + 3, j].Style.Font.Color.SetColor(Color.Red);
                                ws.Cells[i + 3, j].Style.Font.Bold = true;
                                ws.Cells[i + 3, j].AddComment(errorModel.ErrorMessage, "Vận tải sức trẻ Website");
                            }
                        }
                    }


                    return p.GetAsByteArray();
                }
            }
            return null;
        }

        private bool IsRowEmpty(ExcelWorksheet worksheet, int rowNumber)
        {
            var empties = new List<bool>();

            for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
            {
                var rowEmpty = worksheet.Cells[rowNumber, i].Value == null || string.IsNullOrWhiteSpace(worksheet.Cells[rowNumber, i].Value.ToString()) ? true : false;
                empties.Add(rowEmpty);
            }

            return empties.All(e => e);
        }
    }

    public class CastResultPlanList<T>
    {
        public T Value { get; set; }

        public bool IsSuccess { get; set; }
    }

    public class ErrorModelPlanList
    {
        public string ErrorMessage { get; set; }

        public int RowNumber { get; set; }

        public int ColumnNumber { get; set; }
    }
}

