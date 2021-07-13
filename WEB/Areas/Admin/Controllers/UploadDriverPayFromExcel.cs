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

    public class UploadDriverPayFromExcel
    {
        WebContext db = new WebContext();
        private List<ErrorModelPayList> ErrorModels = new List<ErrorModelPayList>();

        public byte[] DownloadTemplate()
        {
            var fileinfo = new FileInfo(string.Format(@"{0}\QLChiPhiLaiXe.xlsx", HostingEnvironment.MapPath("/Uploads")));
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
                    var driverPays = db.DriverPays.ToList();
                    var driverPayAdd = new List<DriverPay>();
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

                        var payInfo = new DriverPay();
                        Boolean isValid = true;

                        if (checkDate)
                        {
                            payInfo.PayDate = dateBegin;
                        }

                        var driver = CastToProperty<string>(workSheet, rowIterator, 1, "Lái xe");
                        if (driver.IsSuccess)
                        {
                            var findDriver = Vehicle.Where(x => x.CarOwerName == driver.Value).FirstOrDefault();
                            if (findDriver != null)
                            {
                                payInfo.DriverID = findDriver.ID;
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


                        var numberPlate = CastToProperty<string>(workSheet, rowIterator, 3, "BKS", false);
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
                                    payInfo.VehicleID = numberPlateExists.ID;
                                }
                                else
                                {
                                    isValid = false;
                                    AddErrorModels(rowIterator, 3, "Xe không tồn tại");
                                }
                            }
                            else if (payInfo.DriverID.HasValue)
                            {
                                payInfo.VehicleID = payInfo.DriverID;
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        var content = CastToProperty<string>(workSheet, rowIterator, 2, "Nội dung chi");
                        if (content.IsSuccess)
                        {
                            payInfo.Content = content.Value;
                        }
                        else
                        {
                            isValid = false;
                        }

                        if (checkDate && payInfo.VehicleID.HasValue && payInfo.DriverID.HasValue && payInfo.Content != "")
                        {
                            var firstDayOfMonth = new DateTime(payInfo.PayDate.Year, payInfo.PayDate.Month, 1);
                            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                            var check = driverPays.Where(x => x.VehicleID == payInfo.VehicleID && x.DriverID == payInfo.DriverID && x.IsRemove != true && x.Content == payInfo.Content
                            && firstDayOfMonth.Date <= x.PayDate.Date && x.PayDate.Date <= lastDayOfMonth.Date).Any();
                            if(check)
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 1, "Đã tồn tại bản ghi trùng tên lái xe, BKS và nội dung trên hệ thống!");
                            }    
                        }    

                      

                        var price = CastToProperty<string>(workSheet, rowIterator, 4, "Số tiền");
                        if (price.IsSuccess)
                        {
                            try
                            {
                                var priceCheck = Double.Parse(price.Value);
                                if (priceCheck != 0)
                                {
                                    payInfo.Price = priceCheck * 1000;
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

                        //if conditions was success
                        if (isValid && checkDate)
                        {
                            payInfo.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                            payInfo.CreatedDate = DateTime.Now;
                            driverPayAdd.Add(payInfo);
                            driverPays.Add(payInfo);
                        }
                    }
                    var checkSave = true;
                    try
                    {
                        if (driverPayAdd.Count > 0)
                        {
                            driverPayAdd.Reverse();
                            db.DriverPays.AddRange(driverPayAdd);
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
            ErrorModels.Add(new ErrorModelPayList()
            {
                ColumnNumber = columnNuber,
                RowNumber = rowNumber,
                ErrorMessage = error
            });
        }

        private CastResultPayList<T> CastToProperty<T>(ExcelWorksheet worksheet, int rowNumber, int columnNuber, string property, bool isRequired = true)
        {
            if (isRequired && (worksheet.Cells[rowNumber, columnNuber].Value == null || string.IsNullOrWhiteSpace(worksheet.Cells[rowNumber, columnNuber].Value.ToString())))
            {
                ErrorModels.Add(new ErrorModelPayList()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không được để trống"
                });

                return new CastResultPayList<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
            try
            {
                return new CastResultPayList<T>()
                {
                    Value = (T)Convert.ChangeType(worksheet.Cells[rowNumber, columnNuber].Value, typeof(T)),
                    IsSuccess = true
                };
            }
            catch (FormatException ex)
            {
                ErrorModels.Add(new ErrorModelPayList()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không đúng định dạng"
                });

                return new CastResultPayList<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
        }

        private byte[] LogErrorsToFile(ExcelWorksheet worksheet, DateTime date)
        {
            var noOfCol = worksheet.Dimension.End.Column;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLChiPhiLaiXe.xlsx", HostingEnvironment.MapPath("/Uploads")));
            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var ws = p.Workbook.Worksheets.First();
                    // set rows
                    if (!ErrorModels.Any())
                    {
                        ws.Cells["A2:J2"].Merge = true;
                        ws.Cells[2, 1].Value = "Tất cả mục lái xe chi đã được nhập thành công!";
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

    public class CastResultPayList<T>
    {
        public T Value { get; set; }

        public bool IsSuccess { get; set; }
    }

    public class ErrorModelPayList
    {
        public string ErrorMessage { get; set; }

        public int RowNumber { get; set; }

        public int ColumnNumber { get; set; }
    }
}

