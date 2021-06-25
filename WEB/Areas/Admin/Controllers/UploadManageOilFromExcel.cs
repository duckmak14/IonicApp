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

    public class UploadManageOilFromExcel
    {
        WebContext db = new WebContext();
        private List<ErrorModelManageOilList> ErrorModels = new List<ErrorModelManageOilList>();

        public byte[] DownloadTemplate()
        {
            var fileinfo = new FileInfo(string.Format(@"{0}\ThongKeDau.xlsx", HostingEnvironment.MapPath("/Uploads")));
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
                    var manageOils = db.ManageOils.ToList();
                    var manageOilAdd = new List<ManageOil>();

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

                    for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                    {
                        if (IsRowEmpty(workSheet, rowIterator))
                        {
                            continue; // ket thuc
                        }

                        var manageOil = new ManageOil();
                        Boolean isValid = true;

                        if (checkDate)
                        {
                            manageOil.OilDate = dateBegin;
                        }

                        var driver = CastToProperty<string>(workSheet, rowIterator, 2, "Lái xe");
                        if (driver.IsSuccess)
                        {
                            var findDriver = Vehicle.Where(x => x.CarOwerName == driver.Value).FirstOrDefault();
                            if (findDriver != null)
                            {
                                manageOil.DriverID = findDriver.ID;
                            }
                            else
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 2, "Không tồn tại lái xe!");
                            }
                        }
                        else
                        {
                            isValid = false;
                        }


                        var numberPlate = CastToProperty<string>(workSheet, rowIterator, 1, "BKS", false);
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
                                    manageOil.VehicleID = numberPlateExists.ID;
                                }
                                else
                                {
                                    isValid = false;
                                    AddErrorModels(rowIterator, 1, "Xe không tồn tại");
                                }
                            }
                            else if (manageOil.DriverID.HasValue)
                            {
                                manageOil.VehicleID = manageOil.DriverID;
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        if (checkDate && manageOil.VehicleID.HasValue && manageOil.DriverID.HasValue)
                        {
                            var firstDayOfMonth = new DateTime(manageOil.OilDate.Year, manageOil.OilDate.Month, 1);
                            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                            var check = manageOils.Where(x => x.VehicleID == manageOil.VehicleID && x.DriverID == manageOil.DriverID && x.IsRemove != true
                            && firstDayOfMonth.Day <=x.OilDate.Day && x.OilDate.Day <= lastDayOfMonth.Day).Any();
                            if (check)
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 1, "Đã tồn tại bản ghi trùng tên lái xe và biển kiểm soát trên hệ thống!");
                            }
                        }

                        var content = CastToProperty<string>(workSheet, rowIterator, 11, "Ghi chú", false);
                        if (content.IsSuccess)
                        {
                            manageOil.Note = content.Value;
                        }
                        else
                        {
                            isValid = false;
                        }

                        var distance = CastToProperty<string>(workSheet, rowIterator, 3, "Km");
                        if (distance.IsSuccess)
                        {
                            try
                            {
                                var distanceCheck = Double.Parse(distance.Value);
                                if (distanceCheck != 0)
                                {
                                    manageOil.Distance = distanceCheck;
                                }
                            }
                            catch (Exception e)
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 3, "Kiểu dữ liệu không đúng");
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        var oilLevel = CastToProperty<string>(workSheet, rowIterator, 4, "Định mức dầu");
                        if (oilLevel.IsSuccess)
                        {
                            try
                            {
                                var oilLevelCheck = Double.Parse(oilLevel.Value);
                                if (oilLevelCheck != 0)
                                {
                                    manageOil.OilLevel = oilLevelCheck;
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

                        var otherRun = CastToProperty<string>(workSheet, rowIterator, 6, "Chạy khác",false);
                        if (otherRun.IsSuccess)
                        {
                            if(otherRun.Value != null)
                            {
                                try
                                {
                                    var otherRunCheck = Double.Parse(otherRun.Value);
                                    if (otherRunCheck != 0)
                                    {
                                        manageOil.OtherRun = otherRunCheck;
                                    }
                                }
                                catch (Exception e)
                                {
                                    isValid = false;
                                    AddErrorModels(rowIterator, 6, "Kiểu dữ liệu không đúng");
                                }
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        var suppliedOil = CastToProperty<string>(workSheet, rowIterator, 8, "Số dầu đã cấp");
                        if (suppliedOil.IsSuccess)
                        {
                            try
                            {
                                var suppliedOilCheck = Double.Parse(suppliedOil.Value);
                                if (suppliedOilCheck != 0)
                                {
                                    manageOil.SuppliedOil = suppliedOilCheck;
                                }
                            }
                            catch (Exception e)
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 8, "Kiểu dữ liệu không đúng");
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        var price = db.OilPrices.Where(x => DbFunctions.TruncateTime(x.StartDate) <= DbFunctions.TruncateTime(manageOil.OilDate) && DbFunctions.TruncateTime(x.EndDate) >= DbFunctions.TruncateTime(manageOil.OilDate)).FirstOrDefault();
                        if (price != null)
                        {
                            manageOil.OilPrice = price.Price;
                        }
                        else
                        {
                            isValid = false;
                            AddErrorModels(rowIterator,10,"Không tồn tại giá dầu trên hệ thống!");
                        }

                        //if conditions was success
                        if (isValid && checkDate)
                        {
                            manageOil.SuppliedFromLevel = (manageOil.Distance * manageOil.OilLevel) / 100;
                            manageOil.AmountOil = manageOil.SuppliedFromLevel + (Double)manageOil.OtherRun;
                            manageOil.DifferenceOil = manageOil.AmountOil - manageOil.SuppliedOil;
                            manageOil.Total = manageOil.DifferenceOil * manageOil.OilPrice;
                            manageOil.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                            manageOil.CreatedDate = DateTime.Now;

                            manageOilAdd.Add(manageOil);
                            manageOils.Add(manageOil); 
                        }
                    }
                    var checkSave = true;
                    try
                    {
                        if (manageOilAdd.Count > 0)
                        {
                            manageOilAdd.Reverse();
                            db.ManageOils.AddRange(manageOilAdd);
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
                    return LogErrorsToFile(workSheet);
                }
            }

            return null;
        }


        private void AddErrorModels(int rowNumber, int columnNuber, string error)
        {
            ErrorModels.Add(new ErrorModelManageOilList()
            {
                ColumnNumber = columnNuber,
                RowNumber = rowNumber,
                ErrorMessage = error
            });
        }

        private CastResultManageOilList<T> CastToProperty<T>(ExcelWorksheet worksheet, int rowNumber, int columnNuber, string property, bool isRequired = true)
        {
            if (isRequired && (worksheet.Cells[rowNumber, columnNuber].Value == null || string.IsNullOrWhiteSpace(worksheet.Cells[rowNumber, columnNuber].Value.ToString())))
            {
                ErrorModels.Add(new ErrorModelManageOilList()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không được để trống"
                });

                return new CastResultManageOilList<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
            try
            {
                return new CastResultManageOilList<T>()
                {
                    Value = (T)Convert.ChangeType(worksheet.Cells[rowNumber, columnNuber].Value, typeof(T)),
                    IsSuccess = true
                };
            }
            catch (FormatException ex)
            {
                ErrorModels.Add(new ErrorModelManageOilList()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không đúng định dạng"
                });

                return new CastResultManageOilList<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
        }

        private byte[] LogErrorsToFile(ExcelWorksheet worksheet)
        {
            var noOfCol = worksheet.Dimension.End.Column;
            var fileinfo = new FileInfo(string.Format(@"{0}\ThongKeDau.xlsx", HostingEnvironment.MapPath("/Uploads")));
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
                        ws.Cells[1,1].Style.Font.Color.SetColor(Color.Red);
                        ws.Cells[1,1].Style.Font.Bold = true;
                        ws.Cells[1,1].AddComment(errorModelRow1.ErrorMessage, "Vận tải sức trẻ Website");
                    }
                    
                    for (int i = 0; i < allRowNumbers.Count(); i++)
                    {
                        for (int j = 1; j <= noOfCol; j++)
                        {
                            ws.Cells[i + 4, j].Value = worksheet.Cells[allRowNumbers[i], j].Value;

                            var errorModel = ErrorModels.FirstOrDefault(x => x.RowNumber == allRowNumbers[i] && x.ColumnNumber == j);

                            if (errorModel != null)
                            {
                                ws.Cells[i + 4, j].Style.Font.Color.SetColor(Color.Red);
                                ws.Cells[i + 4, j].Style.Font.Bold = true;
                                ws.Cells[i + 4, j].AddComment(errorModel.ErrorMessage, "Vận tải sức trẻ Website");
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

    public class CastResultManageOilList<T>
    {
        public T Value { get; set; }

        public bool IsSuccess { get; set; }
    }

    public class ErrorModelManageOilList
    {
        public string ErrorMessage { get; set; }

        public int RowNumber { get; set; }

        public int ColumnNumber { get; set; }
    }
}

