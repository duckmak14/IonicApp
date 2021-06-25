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

    public class UploadOtherCostFromExcel
    {
        WebContext db = new WebContext();
        private List<ErrorModelOtherCostList> ErrorModels = new List<ErrorModelOtherCostList>();

        public byte[] DownloadTemplate()
        {
            var fileinfo = new FileInfo(string.Format(@"{0}\ThongKeChiPhiKhac.xlsx", HostingEnvironment.MapPath("/Uploads")));
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
                    var otherCostList = db.OtherCosts.Where(x => x.IsRemove != true).ToList();
                    DateTime dateBegin = new DateTime();
                    var Vehicle = db.Vehicle.ToList();

                    var otherCostAdd = new List<OtherCost>();
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

                    for (int rowIterator = 3; rowIterator <= noOfRow; rowIterator++)
                    {
                        if (IsRowEmpty(workSheet, rowIterator))
                        {
                            continue; // ket thuc
                        }

                        var otherCost = new OtherCost();
                        Boolean isValid = true;

                        if (checkDate)
                        {
                            otherCost.OtherCostDate = dateBegin;
                        }

                        var driver = CastToProperty<string>(workSheet, rowIterator, 1, "Lái xe");
                        if (driver.IsSuccess)
                        {
                            var findDriver = Vehicle.Where(x => x.CarOwerName == driver.Value).FirstOrDefault();
                            if (findDriver != null)
                            {
                                otherCost.DriverID = findDriver.ID;
                            }
                            else
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 1, "Không tồn tại lái xe!");
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
                                    otherCost.VehicleID = numberPlateExists.ID;
                                }
                                else
                                {
                                    isValid = false;
                                    AddErrorModels(rowIterator, 2, "Xe không tồn tại");
                                }
                            }
                            else if (otherCost.DriverID.HasValue)
                            {
                                otherCost.VehicleID = otherCost.DriverID;
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        if (checkDate && otherCost.VehicleID.HasValue && otherCost.DriverID.HasValue)
                        {
                            var firstDayOfMonth = new DateTime(otherCost.OtherCostDate.Year, otherCost.OtherCostDate.Month, 1);
                            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                            var check = otherCostList.Where(x => x.VehicleID == otherCost.VehicleID && x.DriverID == otherCost.DriverID && x.IsRemove != true 
                            && firstDayOfMonth.Day <= x.OtherCostDate.Day && x.OtherCostDate.Day <= lastDayOfMonth.Day).Any();
                            if (check)
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 1, "Đã tồn tại bản ghi trùng tên lái xe và biển kiểm soát trên hệ thống!");
                            }
                        }

                        var content = CastToProperty<string>(workSheet, rowIterator, 7, "Ghi chú", false);
                        if (content.IsSuccess)
                        {
                            otherCost.Note = content.Value;
                        }
                        else
                        {
                            isValid = false;
                        }

                        var mortgage= CastToProperty<string>(workSheet, rowIterator, 3, "Đóng thế chấp theo nội quy",false);
                        if (mortgage.IsSuccess)
                        {
                            if(mortgage.Value != null)
                            {
                                try
                                {
                                    var mortgageCheck = Double.Parse(mortgage.Value);
                                    if (mortgageCheck != 0)
                                    {
                                        otherCost.MortgageCosts = mortgageCheck *1000;
                                    }
                                }
                                catch (Exception e)
                                {
                                    isValid = false;
                                    AddErrorModels(rowIterator, 3, "Kiểu dữ liệu không đúng");
                                }
                            }   
                        }
                        else
                        {
                            isValid = false;
                        }

                        var advance = CastToProperty<string>(workSheet, rowIterator, 4, "Tạm ứng",false);
                        if (advance.IsSuccess)
                        {
                            if (advance.Value != null)
                            {
                                try
                                {
                                    var advanceCheck = Double.Parse(advance.Value);
                                    if (advanceCheck != 0)
                                    {
                                        otherCost.AdvanceCosts = advanceCheck *1000;
                                    }
                                }
                                catch (Exception e)
                                {
                                    isValid = false;
                                    AddErrorModels(rowIterator, 4, "Kiểu dữ liệu không đúng");
                                }
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        var other = CastToProperty<string>(workSheet, rowIterator, 5, "Khác",false);
                        if (other.IsSuccess)
                        {
                            if (other.Value != null)
                            {
                                try
                                {
                                    var otherCheck = Double.Parse(other.Value);
                                    if (otherCheck != 0)
                                    {
                                        otherCost.OtherCosts = otherCheck *1000;
                                    }
                                }
                                catch (Exception e)
                                {
                                    isValid = false;
                                    AddErrorModels(rowIterator, 5, "Kiểu dữ liệu không đúng");
                                }
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        if (!otherCost.OtherCosts.HasValue && !otherCost.AdvanceCosts.HasValue && !otherCost.MortgageCosts.HasValue)
                        {
                            isValid = false;
                            AddErrorModels(rowIterator, 3, "Vui lòng không để trống 1 trong 3 mục chi phí!");
                        }

                        //if conditions was success
                        if (isValid && checkDate)
                        {
                            Double otherCosts = 0; Double advanceCosts = 0; Double mortgageCosts = 0;
                            if (otherCost.OtherCosts != null)
                            {
                                otherCosts = (Double)otherCost.OtherCosts;
                            }

                            if (otherCost.AdvanceCosts != null)
                            {
                                advanceCosts = (Double)otherCost.AdvanceCosts;
                            }

                            if (otherCost.MortgageCosts != null)
                            {
                                mortgageCosts = (Double)otherCost.MortgageCosts;
                            }

                            otherCost.Total = otherCosts + advanceCosts + mortgageCosts;

                            otherCost.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                            otherCost.CreatedDate = DateTime.Now;
                            otherCostAdd.Add(otherCost);
                            otherCostList.Add(otherCost);
                        }
                    }
                    var checkSave = true;
                    try
                    {
                        if (otherCostAdd.Count > 0)
                        {
                            otherCostAdd.Reverse();
                            db.OtherCosts.AddRange(otherCostAdd);
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
            ErrorModels.Add(new ErrorModelOtherCostList()
            {
                ColumnNumber = columnNuber,
                RowNumber = rowNumber,
                ErrorMessage = error
            });
        }

        private CastResultOtherCostList<T> CastToProperty<T>(ExcelWorksheet worksheet, int rowNumber, int columnNuber, string property, bool isRequired = true)
        {
            if (isRequired && (worksheet.Cells[rowNumber, columnNuber].Value == null || string.IsNullOrWhiteSpace(worksheet.Cells[rowNumber, columnNuber].Value.ToString())))
            {
                ErrorModels.Add(new ErrorModelOtherCostList()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không được để trống"
                });

                return new CastResultOtherCostList<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
            try
            {
                return new CastResultOtherCostList<T>()
                {
                    Value = (T)Convert.ChangeType(worksheet.Cells[rowNumber, columnNuber].Value, typeof(T)),
                    IsSuccess = true
                };
            }
            catch (FormatException ex)
            {
                ErrorModels.Add(new ErrorModelOtherCostList()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không đúng định dạng"
                });

                return new CastResultOtherCostList<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
        }

        private byte[] LogErrorsToFile(ExcelWorksheet worksheet)
        {
            var noOfCol = worksheet.Dimension.End.Column;
            var fileinfo = new FileInfo(string.Format(@"{0}\ThongKeChiPhiKhac.xlsx", HostingEnvironment.MapPath("/Uploads")));
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

    public class CastResultOtherCostList<T>
    {
        public T Value { get; set; }

        public bool IsSuccess { get; set; }
    }

    public class ErrorModelOtherCostList
    {
        public string ErrorMessage { get; set; }

        public int RowNumber { get; set; }

        public int ColumnNumber { get; set; }
    }
}

