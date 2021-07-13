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

    public class UploadManageSalaryFromExcel
    {
        WebContext db = new WebContext();
        private List<ErrorModelManageSalaryList> ErrorModels = new List<ErrorModelManageSalaryList>();

        public byte[] DownloadTemplate()
        {
            var fileinfo = new FileInfo(string.Format(@"{0}\ThongKeBangLuong.xlsx", HostingEnvironment.MapPath("/Uploads")));
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

                    DateTime dateBegin = new DateTime();
                    var Vehicle = db.Vehicle.ToList();
                    var manageSalarys = db.ManageSalarys.ToList();

                    var manageSalaryAdd = new List<ManageSalary>();

                    Boolean checkDate = true;

                    var date = CastToProperty<string>(workSheet, 1, 1, "Ngày tháng", true);
                    if (date.IsSuccess)
                    {
                        try
                        {
                            Double dateParse;
                            if (Double.TryParse(date.Value, out dateParse))
                            {
                                dateBegin = DateTime.FromOADate(dateParse);
                            }
                            else
                            {
                                dateBegin = DateTime.Parse(date.Value.ToString(), new CultureInfo("vi-VN", false));
                            }
                        }
                        catch (Exception ex)
                        {
                            AddErrorModels(1, 1, "Vui lòng nhập đúng định dạng thời gian");
                            checkDate = false;
                        }
                    }
                    else
                    {
                        checkDate = false;
                    }

                    for (int rowIterator = 5; rowIterator <= noOfRow; rowIterator++)
                    {
                        if (IsRowEmpty(workSheet, rowIterator))
                        {
                            continue; // ket thuc
                        }

                        var manageSalary = new ManageSalary();
                        Boolean isValid = true;

                        if (checkDate)
                        {
                            manageSalary.SalaryDate = dateBegin;
                        }
                       

                        var driver = CastToProperty<string>(workSheet, rowIterator, 3, "Lái xe");
                        if (driver.IsSuccess)
                        {
                            var findDriver = Vehicle.Where(x => x.CarOwerName == driver.Value).FirstOrDefault();
                            if (findDriver != null)
                            {
                                manageSalary.DriverID = findDriver.ID;
                            }
                            else
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 3, "Không tồn tại lái xe!");
                            }
                        }
                        else
                        {
                            isValid = false;
                        }


                        var numberPlate = CastToProperty<string>(workSheet, rowIterator, 2, "BKS", false);
                        if (numberPlate.IsSuccess)
                        {
                            if (numberPlate.Value != null)
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
                                    manageSalary.VehicleID = numberPlateExists.ID;
                                }
                                else
                                {
                                    isValid = false;
                                    AddErrorModels(rowIterator, 2, "Xe không tồn tại");
                                }
                            }
                            else if (manageSalary.DriverID.HasValue)
                            {
                                manageSalary.VehicleID = manageSalary.DriverID;
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        if (checkDate && manageSalary.VehicleID.HasValue && manageSalary.DriverID.HasValue)
                        {
                            var firstDayOfMonth = new DateTime(manageSalary.SalaryDate.Year, manageSalary.SalaryDate.Month, 1);
                            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                            var check = manageSalarys.Where(x => x.VehicleID == manageSalary.VehicleID && x.DriverID == manageSalary.DriverID && x.IsRemove != true
                            && firstDayOfMonth.Date <= x.SalaryDate.Date && x.SalaryDate.Date <= lastDayOfMonth.Date).Any();
                            if (check)
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 2, "Đã tồn tại bản ghi trùng tên lái xe và biển kiểm soát trên hệ thống!");
                            }
                        }


                        var content = CastToProperty<string>(workSheet, rowIterator, 16, "Ghi chú", false);
                        if (content.IsSuccess)
                        {
                            manageSalary.Note = content.Value;
                        }
                        else
                        {
                            isValid = false;
                        }


                        if (manageSalary.VehicleID.HasValue && manageSalary.DriverID.HasValue)
                        {
                            var firstDayOfMonth = new DateTime(manageSalary.SalaryDate.Year, manageSalary.SalaryDate.Month, 1);
                            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                            var distance = db.ManageOils.Where(x => x.VehicleID == manageSalary.VehicleID && x.DriverID == manageSalary.DriverID && x.IsRemove != true
                            && DbFunctions.TruncateTime(firstDayOfMonth) <= DbFunctions.TruncateTime(x.OilDate) && DbFunctions.TruncateTime(x.OilDate) <= DbFunctions.TruncateTime(lastDayOfMonth)).FirstOrDefault();
                            if (distance != null)
                            {
                                manageSalary.Distance = distance.Distance;
                            }
                            else
                            {
                                manageSalary.Distance = 0;
                            }
                        }


                        var workDay = CastToProperty<string>(workSheet, rowIterator, 4, "Ngày công");
                        if (workDay.IsSuccess)
                        {
                            try
                            {
                                var workDayCheck = Double.Parse(workDay.Value);
                                if (workDayCheck != 0)
                                {
                                    manageSalary.WorkDay = workDayCheck;
                                }
                            }
                            catch (Exception e)
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 4, "Kiểu dữ liệu không đúng");
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        var workDayPrice = CastToProperty<string>(workSheet, rowIterator, 6, "Đơn giá ngày công");
                        if (workDayPrice.IsSuccess)
                        {
                            try
                            {
                                var workDayPriceCheck = Double.Parse(workDayPrice.Value);
                                if (workDayPriceCheck != 0)
                                {
                                    manageSalary.WorkDayPrice = workDayPriceCheck * 1000;
                                }
                            }
                            catch (Exception e)
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 6, "Kiểu dữ liệu không đúng");
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        var distancePrice = CastToProperty<string>(workSheet, rowIterator, 7, "Đơn giá Km");
                        if (distancePrice.IsSuccess)
                        {
                            try
                            {
                                var distancePriceCheck = Double.Parse(distancePrice.Value);
                                if (distancePriceCheck != 0)
                                {
                                    manageSalary.DistancePrice = distancePriceCheck * 1000;
                                }
                            }
                            catch (Exception e)
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 7, "Kiểu dữ liệu không đúng");
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        var phoneCost = CastToProperty<string>(workSheet, rowIterator, 10, "Hỗ trợ điện thoại",false);
                        if (phoneCost.IsSuccess)
                        {
                            if(phoneCost.Value != null)
                            {
                                try
                                {
                                    var phoneCostCheck = Double.Parse(phoneCost.Value);
                                    if (phoneCostCheck != 0)
                                    {
                                        manageSalary.PhoneCosts = phoneCostCheck * 1000;
                                    }
                                }
                                catch (Exception e)
                                {
                                    isValid = false;
                                    AddErrorModels(rowIterator, 10, "Kiểu dữ liệu không đúng");
                                }
                            }   
                        }
                        else
                        {
                            isValid = false;
                        }

                        var supportCost = CastToProperty<string>(workSheet, rowIterator, 11, "Hỗ trợ",false);
                        if (supportCost.IsSuccess)
                        {
                            if (supportCost.Value != null)
                            {
                                try
                                {
                                    var supportCostCheck = Double.Parse(supportCost.Value);
                                    if (supportCostCheck != 0)
                                    {
                                        manageSalary.SupportCosts = supportCostCheck * 1000;
                                    }
                                }
                                catch (Exception e)
                                {
                                    isValid = false;
                                    AddErrorModels(rowIterator, 11, "Kiểu dữ liệu không đúng");
                                }
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        var bonusCost = CastToProperty<string>(workSheet, rowIterator, 12, "Thưởng",false);
                        if (bonusCost.IsSuccess)
                        {
                            if (bonusCost.Value != null)
                            {
                                try
                                {
                                    var bonusCostCheck = Double.Parse(bonusCost.Value);
                                    if (bonusCostCheck != 0)
                                    {
                                        manageSalary.BonusCosts = bonusCostCheck * 1000;
                                    }
                                }
                                catch (Exception e)
                                {
                                    isValid = false;
                                    AddErrorModels(rowIterator, 12, "Kiểu dữ liệu không đúng");
                                }
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        var insuranceCost = CastToProperty<string>(workSheet, rowIterator, 14, "Trừ BHXH",false);
                        if (insuranceCost.IsSuccess)
                        {
                            if (insuranceCost.Value != null)
                            {
                                try
                                {
                                    var insuranceCostCheck = Double.Parse(insuranceCost.Value);
                                    if (insuranceCostCheck != 0)
                                    {
                                        manageSalary.InsuranceCosts = insuranceCostCheck * 1000;
                                    }
                                }
                                catch (Exception e)
                                {
                                    isValid = false;
                                    AddErrorModels(rowIterator, 14, "Kiểu dữ liệu không đúng");
                                }
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        //if conditions was success
                        if (isValid && checkDate)
                        {
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

                            manageSalaryAdd.Add(manageSalary);
                            manageSalarys.Add(manageSalary);
                        }
                    }
                    var checkSave = true;
                    try
                    {
                        if (manageSalaryAdd.Count > 0)
                        {
                            manageSalaryAdd.Reverse();
                            db.ManageSalarys.AddRange(manageSalaryAdd);
                            db.SaveChanges();
                        }
                    }
                    catch (Exception e)
                    {
                         checkSave = false;
                    }

                    if (!checkSave)
                    {
                        byte[] err = { 1 };
                        return err;
                    }

                    if (!ErrorModels.Any())
                    {
                        return null;
                    }
                    return LogErrorsToFile(workSheet, dateBegin);
                }
            }

            return null;
        }


        private void AddErrorModels(int rowNumber, int columnNuber, string error)
        {
            ErrorModels.Add(new ErrorModelManageSalaryList()
            {
                ColumnNumber = columnNuber,
                RowNumber = rowNumber,
                ErrorMessage = error
            });
        }

        private CastResultManageSalaryList<T> CastToProperty<T>(ExcelWorksheet worksheet, int rowNumber, int columnNuber, string property, bool isRequired = true)
        {
            if (isRequired && (worksheet.Cells[rowNumber, columnNuber].Value == null || string.IsNullOrWhiteSpace(worksheet.Cells[rowNumber, columnNuber].Value.ToString())))
            {
                ErrorModels.Add(new ErrorModelManageSalaryList()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không được để trống"
                });

                return new CastResultManageSalaryList<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
            try
            {
                return new CastResultManageSalaryList<T>()
                {
                    Value = (T)Convert.ChangeType(worksheet.Cells[rowNumber, columnNuber].Value, typeof(T)),
                    IsSuccess = true
                };
            }
            catch (FormatException ex)
            {
                ErrorModels.Add(new ErrorModelManageSalaryList()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không đúng định dạng"
                });

                return new CastResultManageSalaryList<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
        }

        private byte[] LogErrorsToFile(ExcelWorksheet worksheet, DateTime date)
        {
            var noOfCol = worksheet.Dimension.End.Column;
            var fileinfo = new FileInfo(string.Format(@"{0}\ThongKeBangLuong.xlsx", HostingEnvironment.MapPath("/Uploads")));
            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var ws = p.Workbook.Worksheets.First();
                    // set rows
                    if (!ErrorModels.Any())
                    {
                        ws.Cells["A2:J2"].Merge = true;
                        ws.Cells[2, 1].Value = "Tất cả thống kê dầu đã được nhập thành công!";
                    }

                    var allRowNumbers = ErrorModels.Where(x => x.RowNumber != 1 ).Select(x => x.RowNumber).Distinct().OrderBy(x => x).ToList();

                    var errorModelRow1 = ErrorModels.FirstOrDefault(x => x.RowNumber == 1 && x.ColumnNumber == 1);

                    if (errorModelRow1 != null)
                    {
                        ws.Cells[1, 1].Style.Font.Color.SetColor(Color.Red);
                        ws.Cells[1, 1].Style.Font.Bold = true;
                        ws.Cells[1, 1].AddComment(errorModelRow1.ErrorMessage, "Vận tải sức trẻ Website");
                    }
                    else
                    {
                        ws.Cells[1, 1].Value = date.Month.ToString() + "/" + date.Year.ToString();
                    }

                    for (int i = 0; i < allRowNumbers.Count(); i++)
                    {
                        for (int j = 1; j <= noOfCol; j++)
                        {
                            ws.Cells[i + 5, j].Value = worksheet.Cells[allRowNumbers[i], j].Value;

                            var errorModel = ErrorModels.FirstOrDefault(x => x.RowNumber == allRowNumbers[i] && x.ColumnNumber == j);

                            if (errorModel != null)
                            {
                                ws.Cells[i + 5, j].Style.Font.Color.SetColor(Color.Red);
                                ws.Cells[i + 5, j].Style.Font.Bold = true;
                                ws.Cells[i + 5, j].AddComment(errorModel.ErrorMessage, "Vận tải sức trẻ Website");
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

    public class CastResultManageSalaryList<T>
    {
        public T Value { get; set; }

        public bool IsSuccess { get; set; }
    }

    public class ErrorModelManageSalaryList
    {
        public string ErrorMessage { get; set; }

        public int RowNumber { get; set; }

        public int ColumnNumber { get; set; }
    }
}

