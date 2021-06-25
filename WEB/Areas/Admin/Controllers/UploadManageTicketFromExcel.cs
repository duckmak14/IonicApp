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
using System.Text.RegularExpressions;
using System.Globalization;
using System.Data.Entity;

namespace WEB.Areas.ContentType.Controllers
{
    [VanTaiAuthorize]

    public class UploadManageTicketFromExcel
    {
        WebContext db = new WebContext();
        private List<ErrorManageTicketModel> ErrorManageTicketModels = new List<ErrorManageTicketModel>();

        public byte[] DownloadTemplate()
        {
            var fileinfo = new FileInfo(string.Format(@"{0}\QLVeVETC.xlsx", HostingEnvironment.MapPath("/Uploads")));
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


        public byte[] UploadProducts(HttpPostedFileBase file, object uploadProgressSession, int ticketType)
        {

            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            //var fileTemp = new FileInfo(string.Format(@"{0}\QLXe.xlsx", HostingEnvironment.MapPath("/Uploads")));
            //bool cheched = checkHeaderFile(fileTemp, file);

            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                using (var package = new ExcelPackage(file.InputStream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfRow = workSheet.Dimension.End.Row;

                    DateTime dateBegin = DateTime.Now;
                    var Vehicle = db.Vehicle.ToList();
                    var manageTickets = db.ManageTickets.ToList();
                    var manageTicketsAdd = new List<ManageTicket>();
                    for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                    {
                        if (IsRowEmpty(workSheet, rowIterator))
                        {
                            continue; // ket thuc
                        }

                        var ticketInfo = new ManageTicket();
                        Boolean isValid = true;

                        var checkDate = true;

                        var date = CastToProperty<string>(workSheet, rowIterator, 1, "Ngày tháng", true);
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
                                ticketInfo.TicketDate = dateBegin;

                            }
                            catch (Exception ex)
                            {
                                checkDate = false;
                                AddErrorModels(rowIterator, 1, "Vui lòng nhập đúng định dạng thời gian");
                                isValid = false;
                            }
                        }
                        else
                        {
                            checkDate = false;
                            isValid = false;
                        }

                        var numberPlate = CastToProperty<string>(workSheet, rowIterator, 3, "BKS");
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
                                ticketInfo.VehicleID = numberPlateExists.ID;
                            }
                            else
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 3, "Xe không tồn tại");
                            }
                        }
                        else
                        {
                            isValid = false;
                        }


                        if (ticketInfo.VehicleID.HasValue)
                        {
                            ticketInfo.DriverID = ticketInfo.VehicleID;
                        }

                        if (checkDate && ticketInfo.VehicleID.HasValue && ticketInfo.DriverID.HasValue)
                        {
                            var check = manageTickets.Where(x => x.VehicleID == ticketInfo.VehicleID && x.DriverID == ticketInfo.DriverID && x.IsRemove != true && x.TicketType == ticketType
                            && ticketInfo.TicketDate == x.TicketDate).Any();
                            if (check)
                            {
                                isValid = false;
                                AddErrorModels(rowIterator, 3, "Đã tồn tại bản ghi trùng tên lái xe và biển kiểm soát trên hệ thống!");
                            }
                        }

                        var category = CastToProperty<string>(workSheet, rowIterator, 2, "Danh mục");
                        if (category.IsSuccess)
                        {
                            ticketInfo.Category = category.Value;
                        }
                        else
                        {
                            isValid = false;
                        }


                        var price = CastToProperty<string>(workSheet, rowIterator, 4, "Số tiền");
                        if (price.IsSuccess)
                        {
                            try
                            {
                                var priceCheck = Double.Parse(price.Value);
                                if (priceCheck != 0)
                                {
                                    ticketInfo.Price = priceCheck * 1000;
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
                        if (isValid)
                        {
                            ticketInfo.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                            ticketInfo.CreatedDate = DateTime.Now;
                            ticketInfo.TicketType = ticketType;
                            manageTicketsAdd.Add(ticketInfo);
                            manageTickets.Add(ticketInfo);

                        }
                    }

                    var checkSave = true;
                    try
                    {
                        if (manageTicketsAdd.Count > 0)
                        {
                            manageTicketsAdd.Reverse();
                            db.ManageTickets.AddRange(manageTicketsAdd);
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

                    if (!ErrorManageTicketModels.Any())
                    {
                        return null;
                    }
                    return LogErrorsToFile(workSheet);

                }
            }
            return null;
        }
        public bool IsNumber(string pText)
        {
            Regex regex = new Regex(@"^[0-9]*$");
            return regex.IsMatch(pText);
        }


        public bool checkHeaderFile(FileInfo fileTemp, HttpPostedFileBase file)
        {
            List<string> headerTemp = new List<string>();
            List<string> headerFile = new List<string>();
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            if (fileTemp.Exists)
            {
                using (var p = new ExcelPackage(fileTemp))
                {
                    var ws1 = p.Workbook.Worksheets.First();
                    var noOfCol = ws1.Dimension.End.Column;
                    for (int j = 1; j <= noOfCol; j++)
                    {
                        var temp = CastToProperty<string>(ws1, 1, j, "", false);
                        headerTemp.Add(temp.Value);
                    }
                }
            }
            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                using (var p = new ExcelPackage(file.InputStream))
                {
                    var ws2 = p.Workbook.Worksheets.First();
                    var noOfCol = ws2.Dimension.End.Column;
                    for (int j = 1; j <= noOfCol; j++)
                    {
                        var temp = CastToProperty<string>(ws2, 1, j, "", false);
                        headerFile.Add(temp.Value);
                    }
                }
            }

            bool check = true;
            if (headerTemp.Count() != headerFile.Count())
            {
                check = false;

                return check;
            }
            else
            {
                for (var i = 0; i < headerTemp.Count(); i++)
                {
                    if (headerTemp[i] != headerFile[i])
                    {
                        check = false;
                    }
                }
            }
            return check;
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
            ErrorManageTicketModels.Add(new ErrorManageTicketModel()
            {
                ColumnNumber = columnNuber,
                RowNumber = rowNumber,
                ErrorMessage = error
            });
        }

        private CastManageTicketResult<T> CastToProperty<T>(ExcelWorksheet worksheet, int rowNumber, int columnNuber, string property, bool isRequired = true)
        {
            if (isRequired && (worksheet.Cells[rowNumber, columnNuber].Value == null || string.IsNullOrWhiteSpace(worksheet.Cells[rowNumber, columnNuber].Value.ToString())))
            {
                ErrorManageTicketModels.Add(new ErrorManageTicketModel()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không được để trống"
                });

                return new CastManageTicketResult<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
            try
            {
                return new CastManageTicketResult<T>()
                {
                    Value = (T)Convert.ChangeType(worksheet.Cells[rowNumber, columnNuber].Value, typeof(T)),
                    IsSuccess = true
                };
            }
            catch (FormatException ex)
            {
                ErrorManageTicketModels.Add(new ErrorManageTicketModel()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không đúng định dạng"
                });

                return new CastManageTicketResult<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
        }

        private byte[] LogErrorsToFile(ExcelWorksheet worksheet)
        {
            var noOfCol = worksheet.Dimension.End.Column;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLVeVETC.xlsx", HostingEnvironment.MapPath("/Uploads")));
            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var ws = p.Workbook.Worksheets.First();
                    // set rows
                    if (!ErrorManageTicketModels.Any())
                    {
                        ws.Cells["A2:J2"].Merge = true;
                        ws.Cells[2, 1].Value = "Tất cả danh mục đã được nhập thành công!";
                    }

                    var allRowNumbers = ErrorManageTicketModels.Select(x => x.RowNumber).Distinct().OrderBy(x => x).ToList();

                    for (int i = 0; i < allRowNumbers.Count(); i++)
                    {
                        for (int j = 1; j <= noOfCol; j++)
                        {
                            ws.Cells[i + 4, j].Value = worksheet.Cells[allRowNumbers[i], j].Value;

                            var ErrorManageTicketModel = ErrorManageTicketModels.FirstOrDefault(x => x.RowNumber == allRowNumbers[i] && x.ColumnNumber == j);
                            if (ErrorManageTicketModel != null)
                            {
                                ws.Cells[i + 4, j].Style.Font.Color.SetColor(Color.Red);
                                ws.Cells[i + 4, j].Style.Font.Bold = true;
                                ws.Cells[i + 4, j].AddComment(ErrorManageTicketModel.ErrorMessage, "Vận tải sức trẻ Website");
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

    class CastManageTicketResult<T>
    {
        public T Value { get; set; }

        public bool IsSuccess { get; set; }
    }

    public class ErrorManageTicketModel
    {
        public string ErrorMessage { get; set; }

        public int RowNumber { get; set; }

        public int ColumnNumber { get; set; }
    }
}

