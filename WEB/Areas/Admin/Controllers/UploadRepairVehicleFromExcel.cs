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

    public class UploadRepairVehicleFromExcel
    {
        WebContext db = new WebContext();
        private List<ErrorModelRepairList> ErrorModels = new List<ErrorModelRepairList>();

        public byte[] DownloadTemplate()
        {
            var fileinfo = new FileInfo(string.Format(@"{0}\QLSuaChua.xlsx", HostingEnvironment.MapPath("/Uploads")));
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

                    DateTime dateBegin = DateTime.Now;
                    var Vehicle = db.Vehicle.ToList();
                    var RepairCategory = db.RepairCategorys.Where(x => x.IsRemove != true).ToList();
                    var repairVehicles = db.RepairVehicles.Where(x => x.IsRemove != true).ToList();
                    var listAdd = new List<RepairVehicle>();
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

                        var repairInfo = new RepairVehicle();
                        Boolean isValid = true;

                        if (checkDate)
                        {
                            repairInfo.RepairDate = dateBegin;
                        }

                        var numberPlate = CastToProperty<string>(workSheet, rowIterator, 1, "BKS");
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
                                repairInfo.VehicleID = numberPlateExists.ID;
                            }
                            else
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 1, "Xe không tồn tại");
                            }
                        }
                        else
                        {
                            isValid = false;
                        }


                        var driver = CastToProperty<string>(workSheet, rowIterator, 2, "Lái xe", false);
                        if (driver.IsSuccess )
                        {
                            if(driver.Value != null)
                            {
                                var findDriver = Vehicle.Where(x => x.CarOwerName == driver.Value).FirstOrDefault();
                                if (findDriver != null)
                                {
                                    repairInfo.DriverID = findDriver.ID;
                                }
                                else 
                                {
                                    isValid = false;
                                    AddErrorModels(rowIterator, 2, "Không tồn tại lái xe!");
                                }
                            }
                            else if (repairInfo.VehicleID.HasValue)
                            {
                                repairInfo.DriverID = repairInfo.VehicleID;
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        if (checkDate && repairInfo.VehicleID.HasValue && repairInfo.DriverID.HasValue)
                        {
                            var firstDayOfMonth = new DateTime(repairInfo.RepairDate.Year, repairInfo.RepairDate.Month, 1);
                            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                            var check = repairVehicles.Where(x => x.VehicleID == repairInfo.VehicleID && x.DriverID == repairInfo.DriverID && x.IsRemove != true
                            && firstDayOfMonth.Day <= x.RepairDate.Day && x.RepairDate.Day <= lastDayOfMonth.Day).Any();
                            if (check)
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 1, "Đã tồn tại bản ghi trùng tên lái xe và biển kiểm soát trên hệ thống!");
                            }
                        }

                        var category = CastToProperty<string>(workSheet, rowIterator, 3, "Danh mục sửa chữa");
                        if (category.IsSuccess)
                        {
                            var repairCategory = RepairCategory.Where(x => x.Category == category.Value).FirstOrDefault();
                            if(repairCategory != null)
                            {
                                repairInfo.CategoryID = repairCategory.ID;
                            }
                            else
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 3, "Không tồn tại danh mục sửa chữa!");
                            }
                        }
                        else
                        {
                            isValid = false;
                        }



                        var content = CastToProperty<string>(workSheet, rowIterator, 4, "Nội dung sửa chữa", false);
                        if (content.IsSuccess)
                        {
                            repairInfo.Content = content.Value;
                        }
                        else
                        {
                            isValid = false;
                        }

                        var price = CastToProperty<string>(workSheet, rowIterator, 5, "Số tiền");
                        if (price.IsSuccess)
                        {
                            try
                            {
                                var priceCheck = Double.Parse(price.Value);
                                if (priceCheck != 0)
                                {
                                    repairInfo.Price = priceCheck * 1000;
                                }
                            }
                            catch (Exception e)
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 5, "Kiểu dữ liệu không đúng");
                            }
                        }
                        else
                        {
                            isValid = false;
                        }
                        
                        //if conditions was success
                        if (isValid && checkDate)
                        {
                            repairInfo.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                            repairInfo.CreatedDate = DateTime.Now;
                            listAdd.Add(repairInfo);
                            repairVehicles.Add(repairInfo);
                        }
                    }

                    var checkSave = true;
                    try
                    {
                        if (listAdd.Count > 0)
                        {
                            listAdd.Reverse();
                            db.RepairVehicles.AddRange(listAdd);
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
            ErrorModels.Add(new ErrorModelRepairList()
            {
                ColumnNumber = columnNuber,
                RowNumber = rowNumber,
                ErrorMessage = error
            });
        }

        private CastResultRepairList<T> CastToProperty<T>(ExcelWorksheet worksheet, int rowNumber, int columnNuber, string property, bool isRequired = true)
        {
            if (isRequired && (worksheet.Cells[rowNumber, columnNuber].Value == null || string.IsNullOrWhiteSpace(worksheet.Cells[rowNumber, columnNuber].Value.ToString())))
            {
                ErrorModels.Add(new ErrorModelRepairList()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không được để trống"
                });

                return new CastResultRepairList<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
            try
            {
                return new CastResultRepairList<T>()
                {
                    Value = (T)Convert.ChangeType(worksheet.Cells[rowNumber, columnNuber].Value, typeof(T)),
                    IsSuccess = true
                };
            }
            catch (FormatException ex)
            {
                ErrorModels.Add(new ErrorModelRepairList()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không đúng định dạng"
                });

                return new CastResultRepairList<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
        }

        private byte[] LogErrorsToFile(ExcelWorksheet worksheet)
        {
            var noOfCol = worksheet.Dimension.End.Column;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLSuaChua.xlsx", HostingEnvironment.MapPath("/Uploads")));
            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var ws = p.Workbook.Worksheets.First();
                    // set rows
                    if (!ErrorModels.Any())
                    {
                        ws.Cells["A2:J2"].Merge = true;
                        ws.Cells[2, 1].Value = "Tất cả mục sửa chữa đã được nhập thành công!";
                    }

                    var allRowNumbers = ErrorModels.Where(x => x.RowNumber != 1).Select(x => x.RowNumber).Distinct().OrderBy(x => x).ToList();

                    var errorModelRow1 = ErrorModels.FirstOrDefault(x => x.RowNumber == 1 && x.ColumnNumber == 1);

                    if (errorModelRow1 != null)
                    {
                        ws.Cells[1, 1].Style.Font.Color.SetColor(Color.Red);
                        ws.Cells[1, 1].Style.Font.Bold = true;
                        ws.Cells[1, 1].AddComment(errorModelRow1.ErrorMessage, "Vận tải sức trẻ Website");
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

    public class CastResultRepairList<T>
    {
        public T Value { get; set; }

        public bool IsSuccess { get; set; }
    }

    public class ErrorModelRepairList
    {
        public string ErrorMessage { get; set; }

        public int RowNumber { get; set; }

        public int ColumnNumber { get; set; }
    }
}

