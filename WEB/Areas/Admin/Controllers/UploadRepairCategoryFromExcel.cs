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

    public class UploadRepairCategoryFromExcel
    {
        WebContext db = new WebContext();
        private List<ErrorRepairCategoryModel> ErrorRepairCategoryModels = new List<ErrorRepairCategoryModel>();

        public byte[] DownloadTemplate()
        {
            var fileinfo = new FileInfo(string.Format(@"{0}\QLDanhMucSuaChua.xlsx", HostingEnvironment.MapPath("/Uploads")));
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

                        var categoryInfo = new RepairCategory();
                        var isValid = true;

                        var category = CastToProperty<string>(workSheet, rowIterator, 2, "Danh mục sửa chữa");
                        if (category.IsSuccess)
                        {
                            categoryInfo.Category = category.Value;
                        }
                        else
                        {
                            isValid = false;
                        }

                        if (isValid)
                        {
                            categoryInfo.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                            categoryInfo.CreatedDate = DateTime.Now;
                            db.Set<RepairCategory>().Add(categoryInfo);
                            db.SaveChanges();
                        }
                    }

                    //if (!checkSave)
                    //{
                    //    byte[] err = { 1 };
                    //    return err;
                    //}

                    if (!ErrorRepairCategoryModels.Any())
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



        private void AddErrorRepairCategoryModels(int rowNumber, int columnNuber, string error)
        {
            ErrorRepairCategoryModels.Add(new ErrorRepairCategoryModel()
            {
                ColumnNumber = columnNuber,
                RowNumber = rowNumber,
                ErrorMessage = error
            });
        }

        private CastRepairCategoryResult<T> CastToProperty<T>(ExcelWorksheet worksheet, int rowNumber, int columnNuber, string property, bool isRequired = true)
        {
            if (isRequired && (worksheet.Cells[rowNumber, columnNuber].Value == null || string.IsNullOrWhiteSpace(worksheet.Cells[rowNumber, columnNuber].Value.ToString())))
            {
                ErrorRepairCategoryModels.Add(new ErrorRepairCategoryModel()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không được để trống"
                });

                return new CastRepairCategoryResult<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
            try
            {
                return new CastRepairCategoryResult<T>()
                {
                    Value = (T)Convert.ChangeType(worksheet.Cells[rowNumber, columnNuber].Value, typeof(T)),
                    IsSuccess = true
                };
            }
            catch (FormatException ex)
            {
                ErrorRepairCategoryModels.Add(new ErrorRepairCategoryModel()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không đúng định dạng"
                });

                return new CastRepairCategoryResult<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
        }

        private byte[] LogErrorsToFile(ExcelWorksheet worksheet)
        {
            var noOfCol = worksheet.Dimension.End.Column;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLDanhMucSuaChua.xlsx", HostingEnvironment.MapPath("/Uploads")));
            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var ws = p.Workbook.Worksheets.First();
                    // set rows
                    if (!ErrorRepairCategoryModels.Any())
                    {
                        ws.Cells["A2:J2"].Merge = true;
                        ws.Cells[2, 1].Value = "Tất cả danh mục đã được nhập thành công!";
                    }

                    var allRowNumbers = ErrorRepairCategoryModels.Select(x => x.RowNumber).Distinct().OrderBy(x => x).ToList();

                    for (int i = 0; i < allRowNumbers.Count(); i++)
                    {
                        for (int j = 1; j <= noOfCol; j++)
                        {
                            ws.Cells[i + 2, j].Value = worksheet.Cells[allRowNumbers[i], j].Value;

                            var ErrorRepairCategoryModel = ErrorRepairCategoryModels.FirstOrDefault(x => x.RowNumber == allRowNumbers[i] && x.ColumnNumber == j);
                            if (ErrorRepairCategoryModel != null)
                            {
                                ws.Cells[i + 2, j].Style.Font.Color.SetColor(Color.Red);
                                ws.Cells[i + 2, j].Style.Font.Bold = true;
                                ws.Cells[i + 2, j].AddComment(ErrorRepairCategoryModel.ErrorMessage, "Vận tải sức trẻ Website");
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

    class CastRepairCategoryResult<T>
    {
        public T Value { get; set; }

        public bool IsSuccess { get; set; }
    }

    public class ErrorRepairCategoryModel
    {
        public string ErrorMessage { get; set; }

        public int RowNumber { get; set; }

        public int ColumnNumber { get; set; }
    }
}

