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

namespace WEB.Areas.ContentType.Controllers
{
    [VanTaiAuthorize]

    public class UploadCarFromExcel
    {
        WebContext db = new WebContext();
        private List<ErrorCarModel> ErrorCarModels = new List<ErrorCarModel>();

        public byte[] DownloadTemplate()
        {
            var fileinfo = new FileInfo(string.Format(@"{0}\QLXe.xlsx", HostingEnvironment.MapPath("/Uploads")));
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
            //var fileTemp = new FileInfo(string.Format(@"{0}\QLXe.xlsx", HostingEnvironment.MapPath("/Uploads")));
            //bool cheched = checkHeaderFile(fileTemp, file);

            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                using (var package = new ExcelPackage(file.InputStream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfRow = workSheet.Dimension.End.Row;

                    for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                    {
                        if (IsRowEmpty(workSheet, rowIterator))
                        {
                            break; // ket thuc
                        }

                        var carInfo = new Vehicle();
                        var isValid = true;

                        var carOwerName = CastToProperty<string>(workSheet, rowIterator, 1, "Tên chủ xe");
                        if (carOwerName.IsSuccess)
                        {
                            carInfo.CarOwerName = carOwerName.Value;
                        }
                        else
                        {
                            isValid = false;
                        }

                        var numberPlate = CastToProperty<string>(workSheet, rowIterator, 2, "Biển số xe");
                        if (numberPlate.IsSuccess)
                        {
                            var numberPlateIsExist = db.Vehicle.Where(x => x.NumberPlate.ToUpper() == numberPlate.Value.ToUpper()).Any();

                            if (!numberPlateIsExist)
                            {
                                carInfo.NumberPlate = numberPlate.Value;
                            }
                            else
                            {
                                AddErrorCarModels(rowIterator, 2, "Biển số xe đã tồn tại");
                                isValid = false;
                            }

                        }
                        else
                        {
                            isValid = false;
                        }

                        var weight = CastToProperty<string>(workSheet, rowIterator, 3, "Tải trọng");
                        if (weight.IsSuccess)
                        {
                            var weightIsExist = new VehicleWeight();
                            try
                            {
                                var weightTest = weight.Value;
                                double weightValue;
                                double weightValueExists;

                                var weightIsExists = db.VehicleWeight.ToList();
                                foreach (var item in weightIsExists)
                                {
                                    var itemString = item.WeightName.Replace(".", ",");
                                    if (Double.TryParse(weight.Value, out weightValue))
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
                                        weightIsExist = db.VehicleWeight.Where(x => x.WeightName == weight.Value).FirstOrDefault();
                                    }
                                }

                            }
                            catch (Exception e)
                            {
                                //weightIsExist = db.VehicleWeight.Where(x => x.WeightName == weight.Value).FirstOrDefault();
                            }


                            if (weightIsExist != null)
                            {
                                var id = weightIsExist;
                                carInfo.WeightID = id.ID;
                            }
                            else
                            {
                                isValid = false;
                                AddErrorCarModels(rowIterator, 3, "Tải trọng không tồn tại");

                            }

                        }
                        else
                        {
                            isValid = false;
                        }

                        var mobile = CastToProperty<string>(workSheet, rowIterator, 4, "Di động", false);
                        if (mobile.IsSuccess)
                        {
                            carInfo.Mobile = mobile.Value;
                        }
                        else
                        {
                            isValid = false;
                        }

                        var partner = CastToProperty<string>(workSheet, rowIterator, 5, "Đơn vị chủ quản", false);
                        if (partner.IsSuccess)
                        {
                            var partnerIsExist = db.Partner.Where(x => x.PartnerName.ToUpper().Trim() == partner.Value.ToUpper().Trim()).FirstOrDefault();
                            if (partnerIsExist != null)
                            {
                                carInfo.PartnerID = partnerIsExist.ID;
                            }
                            else if (partner.Value != null)
                            {
                                AddErrorCarModels(rowIterator, 5, "Đơn vị chủ quản không tồn tại");
                                isValid = false;
                            }

                        }
                        else
                        {
                            isValid = false;
                        }

                        if (isValid)
                        {
                            carInfo.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                            carInfo.CreatedDate = DateTime.Now;
                            db.Set<Vehicle>().Add(carInfo);
                            db.SaveChanges();
                        }
                    }
                    if (!ErrorCarModels.Any())
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



        private void AddErrorCarModels(int rowNumber, int columnNuber, string error)
        {
            ErrorCarModels.Add(new ErrorCarModel()
            {
                ColumnNumber = columnNuber,
                RowNumber = rowNumber,
                ErrorMessage = error
            });
        }

        private CastCarResult<T> CastToProperty<T>(ExcelWorksheet worksheet, int rowNumber, int columnNuber, string property, bool isRequired = true)
        {
            if (isRequired && (worksheet.Cells[rowNumber, columnNuber].Value == null || string.IsNullOrWhiteSpace(worksheet.Cells[rowNumber, columnNuber].Value.ToString())))
            {
                ErrorCarModels.Add(new ErrorCarModel()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không được để trống"
                });

                return new CastCarResult<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
            try
            {
                return new CastCarResult<T>()
                {
                    Value = (T)Convert.ChangeType(worksheet.Cells[rowNumber, columnNuber].Value, typeof(T)),
                    IsSuccess = true
                };
            }
            catch (FormatException ex)
            {
                ErrorCarModels.Add(new ErrorCarModel()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không đúng định dạng"
                });

                return new CastCarResult<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
        }

        private byte[] LogErrorsToFile(ExcelWorksheet worksheet)
        {
            var noOfCol = worksheet.Dimension.End.Column;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLXe.xlsx", HostingEnvironment.MapPath("/Uploads")));
            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var ws = p.Workbook.Worksheets.First();
                    // set rows
                    if (!ErrorCarModels.Any())
                    {
                        ws.Cells["A2:J2"].Merge = true;
                        ws.Cells[2, 1].Value = "Tất cả xe đã được nhập thành công!";
                    }

                    var allRowNumbers = ErrorCarModels.Select(x => x.RowNumber).Distinct().OrderBy(x => x).ToList();

                    for (int i = 0; i < allRowNumbers.Count(); i++)
                    {
                        for (int j = 1; j <= noOfCol; j++)
                        {
                            ws.Cells[i + 2, j].Value = worksheet.Cells[allRowNumbers[i], j].Value;

                            var ErrorCarModel = ErrorCarModels.FirstOrDefault(x => x.RowNumber == allRowNumbers[i] && x.ColumnNumber == j);
                            if (ErrorCarModel != null)
                            {
                                ws.Cells[i + 2, j].Style.Font.Color.SetColor(Color.Red);
                                ws.Cells[i + 2, j].Style.Font.Bold = true;
                                ws.Cells[i + 2, j].AddComment(ErrorCarModel.ErrorMessage, "Vận tải sức trẻ Website");
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

    class CastCarResult<T>
    {
        public T Value { get; set; }

        public bool IsSuccess { get; set; }
    }

    public class ErrorCarModel
    {
        public string ErrorMessage { get; set; }

        public int RowNumber { get; set; }

        public int ColumnNumber { get; set; }
    }
}

