using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WEB.Models
{
    public class ExcelHelper : Controller
    {

        class CastCheckResult<T>
        {
            public T Value { get; set; }

            public bool IsSuccess { get; set; }
        }
        public class ErrorCheckModel
        {
            public string ErrorMessage { get; set; }

            public int RowNumber { get; set; }

            public int ColumnNumber { get; set; }
        }

        private static List<ErrorCheckModel> errorCheckModel = new List<ErrorCheckModel>();
        private static CastCheckResult<T> CastToProperty<T>(ExcelWorksheet worksheet, int rowNumber, int columnNuber, string property, bool isRequired = true)
        {
            if (isRequired && (worksheet.Cells[rowNumber, columnNuber].Value == null || string.IsNullOrWhiteSpace(worksheet.Cells[rowNumber, columnNuber].Value.ToString())))
            {
                errorCheckModel.Add(new ErrorCheckModel()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không được để trống"
                });

                return new CastCheckResult<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
            try
            {
                return new CastCheckResult<T>()
                {
                    Value = (T)Convert.ChangeType(worksheet.Cells[rowNumber, columnNuber].Value, typeof(T)),
                    IsSuccess = true
                };
            }
            catch (FormatException ex)
            {
                errorCheckModel.Add(new ErrorCheckModel()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không đúng định dạng"
                });

                return new CastCheckResult<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
        }
        // GET: ExcelHelper
        public static int CheckHeaderFile(FileInfo fileTemp, HttpPostedFileBase file, int row)
        {
            List<string> headerTemp = new List<string>();
            List<string> headerFile = new List<string>();
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            try 
            {
                if (fileTemp.Exists)
                {
                    using (var p = new ExcelPackage(fileTemp))
                    {
                        var ws1 = p.Workbook.Worksheets.First();
                        var noOfCol = ws1.Dimension.End.Column;
                        for (int j = 1; j <= noOfCol; j++)
                        {
                            var temp = CastToProperty<string>(ws1, row, j, "", false);
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
                            var temp = CastToProperty<string>(ws2, row, j, "", false);
                            headerFile.Add(temp.Value);
                        }
                    }
                }

                int check = 1;
                if (headerFile.Count() > 35)
                {
                    for (var i = 0; i < 35; i++)
                    {
                        if (i == 1 || i == 13 || i == 25)
                        {
                            if (headerTemp[i] != headerFile[i])
                            {
                                check = 0;
                            }
                        }
                    }
                }
                else
                {
                    if (headerTemp.Count() > headerFile.Count())
                    {
                        check = 0;
                    }
                    else
                    {
                        for (var i = 0; i < headerTemp.Count(); i++)
                        {
                            if (headerTemp[i] != headerFile[i])
                            {
                                check = 0;
                            }
                        }
                    }
                }
                return check;

            }
            catch (Exception e)
            {
                return -1;
            }
        }
    }
}