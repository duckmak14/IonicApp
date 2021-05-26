using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using OfficeOpenXml;
using WEB.Models;
using WebModels;

namespace WEB.Areas.ContentType.Controllers
{
    [VanTaiAuthorize]

    public class UploadAddressFromExcel
    {
        WebContext db = new WebContext();
        private List<ErrorLocationModel> ErrorLocationModels = new List<ErrorLocationModel>();

        public byte[] DownloadTemplate()
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLDiaDiem.xlsx", HostingEnvironment.MapPath("/Uploads")));
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
            var localCategories = db.WebModules.ToList();
            ExcelPackage.LicenseContext = LicenseContext.Commercial;

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

                        var locationInfo = new Location();
                        var isValid = true;

                        var locationName = CastToProperty<string>(workSheet, rowIterator, 1, "Tên địa điểm");
                        if (locationName.IsSuccess )
                        {
                            var locationNameIsExist = db.Location.Where(x => x.LocationName == locationName.Value).Any();

                            if (!locationNameIsExist)
                            {
                                locationInfo.LocationName = locationName.Value;
                            }
                            else
                            {
                                AddErrorLocationModels(rowIterator, 1, "Tên địa điểm đã tồn tại");
                                isValid = false;
                            }
                                
                        }
                        else
                        {
                            isValid = false;
                        }

                        var locationAddress = CastToProperty<string>(workSheet, rowIterator, 2, "Địa chỉ");
                        if (locationAddress.IsSuccess)
                        {
                            locationInfo.AddressName = locationAddress.Value;
                        }
                        else
                        {
                            isValid = false;
                        }

                        var district = CastToProperty<string>(workSheet, rowIterator, 3, "Quận/Huyện", false);
                        if (district.IsSuccess )
                        {
                            var districtIsExist = db.District.Where(x => x.DistrictName.ToUpper() == district.Value.ToUpper()).FirstOrDefault();
                            if (districtIsExist != null)
                            {
                                
                                locationInfo.DistrictID = districtIsExist.ID;
                            }
                            else if(district.Value != null)
                            {
                                AddErrorLocationModels(rowIterator, 3, "Giá trị quận/huyện không tồn tại");
                                isValid = false;
                            }                        }
                        else
                        {
                            isValid = false;
                        }

                        var province = CastToProperty<string>(workSheet, rowIterator, 4, "Tỉnh",false);
                        if (province.IsSuccess)
                        {
                            var provinceIsExist = db.Province.Where(x => x.ProvinceName.ToUpper() == province.Value.ToUpper()).FirstOrDefault();
                            if (provinceIsExist != null)
                            {
                                locationInfo.ProvinceID = provinceIsExist.ID;
                            }
                            else if(province.Value != null)
                            {
                                AddErrorLocationModels(rowIterator, 4, "Giá trị tỉnh/thành phố không tồn tại");
                                isValid = false;
                            }    
                        }
                        else
                        {
                            isValid = false;
                        }

                        if (isValid)
                        {
                            locationInfo.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                            locationInfo.CreatedDate = DateTime.Now;
                            db.Set<Location>().Add(locationInfo);
                            db.SaveChanges();
                        }
                    }
                    if (!ErrorLocationModels.Any())
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



        private void AddErrorLocationModels(int rowNumber, int columnNuber, string error)
        {
            ErrorLocationModels.Add(new ErrorLocationModel()
            {
                ColumnNumber = columnNuber,
                RowNumber = rowNumber,
                ErrorMessage = error
            });
        }

        private CastLocationResult<T> CastToProperty<T>(ExcelWorksheet worksheet, int rowNumber, int columnNuber, string property, bool isRequired = true)
        {
            if (isRequired && (worksheet.Cells[rowNumber, columnNuber].Value == null || string.IsNullOrWhiteSpace(worksheet.Cells[rowNumber, columnNuber].Value.ToString())))
            {
                ErrorLocationModels.Add(new ErrorLocationModel()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không được để trống"
                });

                return new CastLocationResult<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
            try
            {
                return new CastLocationResult<T>()
                {
                    Value = (T)Convert.ChangeType(worksheet.Cells[rowNumber, columnNuber].Value, typeof(T)),
                    IsSuccess = true
                };
            }
            catch (FormatException ex)
            {
                ErrorLocationModels.Add(new ErrorLocationModel()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không đúng định dạng"
                });

                return new CastLocationResult<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
        }

        private byte[] LogErrorsToFile(ExcelWorksheet worksheet)
        {
            var noOfCol = worksheet.Dimension.End.Column;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLDiaDiem.xlsx", HostingEnvironment.MapPath("/Uploads")));
            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var ws = p.Workbook.Worksheets.First();
                    // set rows
                    if (!ErrorLocationModels.Any())
                    {
                        ws.Cells["A2:J2"].Merge = true;
                        ws.Cells[2, 1].Value = "Tất cả địa điểm đã được nhập thành công!";
                    }

                    var allRowNumbers = ErrorLocationModels.Select(x => x.RowNumber).Distinct().OrderBy(x => x).ToList();
                    
                    for (int i = 0; i < allRowNumbers.Count(); i++)
                    {
                        for (int j = 1; j <= noOfCol; j++)
                        {
                            ws.Cells[i + 2, j].Value = worksheet.Cells[allRowNumbers[i], j].Value;

                            var ErrorLocationModel = ErrorLocationModels.FirstOrDefault(x => x.RowNumber == allRowNumbers[i] && x.ColumnNumber == j);
                            if (ErrorLocationModel != null)
                            {
                                ws.Cells[i + 2, j].Style.Font.Color.SetColor(Color.Red);
                                ws.Cells[i + 2, j].Style.Font.Bold = true;
                                ws.Cells[i + 2, j].AddComment(ErrorLocationModel.ErrorMessage, "Vận tải sức trẻ Website");
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

    class CastLocationResult<T>
    {
        public T Value { get; set; }

        public bool IsSuccess { get; set; }
    }

    public class ErrorLocationModel
    {
        public string ErrorMessage { get; set; }

        public int RowNumber { get; set; }

        public int ColumnNumber { get; set; }
    }
}

