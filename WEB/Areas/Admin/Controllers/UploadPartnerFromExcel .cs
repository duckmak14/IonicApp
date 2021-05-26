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

    public class UploadPartnerFromExcel
    {
        WebContext db = new WebContext();
        private List<ErrorModelPartner> ErrorModels = new List<ErrorModelPartner>();

        public byte[] DownloadTemplate()
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLDoiTac.xlsx", HostingEnvironment.MapPath("/Uploads")));
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

                        var partnerInfo = new Partner();
                        var isValid = true;

                        var partnerName = CastToProperty<string>(workSheet, rowIterator, 1, "Tên đối tác");
                        if (partnerName.IsSuccess)
                        {
                            var partnerNameIsExist = db.Partner.Where(x => x.PartnerName == partnerName.Value).Any();
                            if (!partnerNameIsExist)
                            {
                                partnerInfo.PartnerName = partnerName.Value;
                            }
                            else
                            {
                                AddErrorModels(rowIterator, 1, "Tên đối tác đã tồn tại");
                                isValid = false;
                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        var addressPartner = CastToProperty<string>(workSheet, rowIterator, 2, "Địa chỉ", false);

                        if (addressPartner.IsSuccess)
                        {
                            partnerInfo.Address = addressPartner.Value;
                        }
                        else
                        {
                            isValid = false;
                        }

                        var emailPartner = CastToProperty<string>(workSheet, rowIterator, 3, "Email", false);
                        if (emailPartner.IsSuccess)
                        {
                            if (emailPartner.Value != null)
                            {
                                try
                                {
                                    var addr = new System.Net.Mail.MailAddress(emailPartner.Value);
                                    partnerInfo.Email = emailPartner.Value;
                                }
                                catch
                                {
                                    AddErrorModels(rowIterator, 1, "Email không đúng định dạng!");
                                    isValid = false;
                                }
                            }
                            else
                            {
                                partnerInfo.Email = emailPartner.Value;
                            }    

                        }
                        else
                        {
                            isValid = false;
                        }

                        var mobilePartner = CastToProperty<string>(workSheet, rowIterator, 4, "Di động", false);
                        if (mobilePartner.IsSuccess)
                        {
                            partnerInfo.Mobile = mobilePartner.Value;
                        }
                        else
                        {
                            isValid = false;
                        }

                        var partnerCode = CastToProperty<string>(workSheet, rowIterator, 5, "Mã đối tác", false);
                        if (partnerCode.IsSuccess)
                        {
                            partnerInfo.PartnerCode = partnerCode.Value;
                        }
                        else
                        {
                            isValid = false;
                        }

                        if (isValid)
                        {
                            partnerInfo.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                            partnerInfo.CreatedDate = DateTime.Now;
                            db.Set<Partner>().Add(partnerInfo);
                            db.SaveChanges();
                        }
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
            ErrorModels.Add(new ErrorModelPartner()
            {
                ColumnNumber = columnNuber,
                RowNumber = rowNumber,
                ErrorMessage = error
            });
        }

        private CastResultPartner<T> CastToProperty<T>(ExcelWorksheet worksheet, int rowNumber, int columnNuber, string property, bool isRequired = true)
        {
            if (isRequired && (worksheet.Cells[rowNumber, columnNuber].Value == null || string.IsNullOrWhiteSpace(worksheet.Cells[rowNumber, columnNuber].Value.ToString())))
            {
                ErrorModels.Add(new ErrorModelPartner()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không được để trống"
                });

                return new CastResultPartner<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
            try
            {
                return new CastResultPartner<T>()
                {
                    Value = (T)Convert.ChangeType(worksheet.Cells[rowNumber, columnNuber].Value, typeof(T)),
                    IsSuccess = true
                };
            }
            catch (FormatException ex)
            {
                ErrorModels.Add(new ErrorModelPartner()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không đúng định dạng"
                });

                return new CastResultPartner<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
        }

        private byte[] LogErrorsToFile(ExcelWorksheet worksheet)
        {
            var noOfCol = worksheet.Dimension.End.Column;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLDoiTac.xlsx", HostingEnvironment.MapPath("/Uploads")));
            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var ws = p.Workbook.Worksheets.First();
                    // set rows
                    if (!ErrorModels.Any())
                    {
                        ws.Cells["A2:J2"].Merge = true;
                        ws.Cells[2, 1].Value = "Tất cả đối tác đã được nhập thành công!";
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
                    if (!ErrorModels.Any())
                    {
                        return null;
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

    public class CastResultPartner<T>
    {
        public T Value { get; set; }

        public bool IsSuccess { get; set; }
    }

    public class ErrorModelPartner
    {
        public string ErrorMessage { get; set; }

        public int RowNumber { get; set; }

        public int ColumnNumber { get; set; }
    }
}

