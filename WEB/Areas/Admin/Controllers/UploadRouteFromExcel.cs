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

namespace WEB.Areas.ContentType.Controllers
{
    [VanTaiAuthorize]

    public class UploadRouteFromExcel
    {
        WebContext db = new WebContext();
        private List<ErrorModel> ErrorModels = new List<ErrorModel>();

        public byte[] DownloadTemplate()
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLLoTrinh.xlsx", HostingEnvironment.MapPath("/Uploads")));
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
                    var routes = db.Route.ToList();
                    var locations = db.Location.ToList();
                    var routeListAdd = new List<Route>();

                    for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                    {
                        if (IsRowEmpty(workSheet, rowIterator))
                        {
                            break; // ket thuc
                        }

                        var routetInfo = new Route();
                        var isValid = true;

                        var routeCode = CastToProperty<string>(workSheet, rowIterator, 1, "Mã lộ trình");
                        if (routeCode.IsSuccess)
                        {
                            var routeCodeIsExist = routes.Where(x => x.RouteCode == routeCode.Value).Any();
                            if (!routeCodeIsExist)
                            {
                                routetInfo.RouteCode = routeCode.Value;

                            }
                            else
                            {
                                AddErrorModels(rowIterator, 1, "Mã lộ trình đã tồn tại");
                                isValid = false;
                            }

                        }
                        else
                        {
                            isValid = false;
                        }

                        var startLocation = CastToProperty<string>(workSheet, rowIterator, 2, "Nơi nhận");

                        if (startLocation.IsSuccess)
                        {
                            var startLocationIsExist = locations.Where(x => x.LocationName == startLocation.Value).FirstOrDefault();
                            if (startLocationIsExist != null)
                            {
                                routetInfo.StartLocationID = startLocationIsExist.ID;
                            }
                            else
                            {
                                AddErrorModels(rowIterator, 2, "Giá trị nơi nhận không tồn tại");
                                isValid = false;
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        var endLocation = CastToProperty<string>(workSheet, rowIterator, 3, "Nơi trả");
                        if (endLocation.IsSuccess)
                        {
                            var endLocationIsExist = locations.Where(x => x.LocationName == endLocation.Value).FirstOrDefault();
                            if (endLocationIsExist != null)
                            {
                                routetInfo.EndLocationID = endLocationIsExist.ID;

                            }
                            else
                            {
                                AddErrorModels(rowIterator, 3, "Giá trị nơi trả không tồn tại");
                                isValid = false;
                            }

                        }
                        else
                        {
                            isValid = false;
                        }

                        var distance = CastToProperty<string>(workSheet, rowIterator, 4, "Khoảng cách", false);
                        if (distance.IsSuccess)
                        {
                            try
                            {
                                var checkDpubleParse = Double.Parse(distance.Value);
                                routetInfo.Distance = distance.Value;
                            }
                            catch
                            {

                                AddErrorModels(rowIterator, 4, "Vui lòng nhập đúng định dạng!");
                                isValid = false;
                            }
                            
                        }
                        else
                        {
                            isValid = false;
                        }

                        if (isValid)
                        {
                            routetInfo.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                            routetInfo.CreatedDate = DateTime.Now;
                            routeListAdd.Add(routetInfo);
                            routes.Add(routetInfo);
                        }
                    }
                    var checkSave = true;
                    try
                    {
                        if(routeListAdd.Count()>0)
                        {
                            routeListAdd.Reverse();
                            db.Route.AddRange(routeListAdd);
                            db.SaveChanges();
                        }    
                    }
                    catch
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
            ErrorModels.Add(new ErrorModel()
            {
                ColumnNumber = columnNuber,
                RowNumber = rowNumber,
                ErrorMessage = error
            });
        }

        private CastResult<T> CastToProperty<T>(ExcelWorksheet worksheet, int rowNumber, int columnNuber, string property, bool isRequired = true)
        {
            if (isRequired && (worksheet.Cells[rowNumber, columnNuber].Value == null || string.IsNullOrWhiteSpace(worksheet.Cells[rowNumber, columnNuber].Value.ToString())))
            {
                ErrorModels.Add(new ErrorModel()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không được để trống"
                });

                return new CastResult<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
            try
            {
                return new CastResult<T>()
                {
                    Value = (T)Convert.ChangeType(worksheet.Cells[rowNumber, columnNuber].Value, typeof(T)),
                    IsSuccess = true
                };
            }
            catch (FormatException ex)
            {
                ErrorModels.Add(new ErrorModel()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không đúng định dạng"
                });

                return new CastResult<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
        }

        private byte[] LogErrorsToFile(ExcelWorksheet worksheet)
        {
            var noOfCol = worksheet.Dimension.End.Column;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLLoTrinh.xlsx", HostingEnvironment.MapPath("/Uploads")));
            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var ws = p.Workbook.Worksheets.First();
                    // set rows
                    if (!ErrorModels.Any())
                    {
                        ws.Cells["A2:J2"].Merge = true;
                        ws.Cells[2, 1].Value = "Tất cả lộ trình đã được nhập thành công!";
                    }

                    var allRowNumbers = ErrorModels.Select(x => x.RowNumber).Distinct().OrderBy(x => x).ToList();

                    for (int i = 0; i < allRowNumbers.Count(); i++)
                    {
                        for (int j = 1; j <= noOfCol; j++)
                        {
                            ws.Cells[i + 2, j].Value = worksheet.Cells[allRowNumbers[i], j].Value;

                            var errorModel = ErrorModels.FirstOrDefault(x => x.RowNumber == allRowNumbers[i] && x.ColumnNumber == j);
                            if (errorModel != null)
                            {
                                ws.Cells[i + 2, j].Style.Font.Color.SetColor(Color.Red);
                                ws.Cells[i + 2, j].Style.Font.Bold = true;
                                ws.Cells[i + 2, j].AddComment(errorModel.ErrorMessage, "Vận tải sức trẻ Website");
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

    class CastResult<T>
    {
        public T Value { get; set; }

        public bool IsSuccess { get; set; }
    }

    public class ErrorModel
    {
        public string ErrorMessage { get; set; }

        public int RowNumber { get; set; }

        public int ColumnNumber { get; set; }
    }
}