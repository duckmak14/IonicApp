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
using System.Data.Entity;
using System.Data.Entity.Validation;

namespace WEB.Areas.ContentType.Controllers
{
    [VanTaiAuthorize]

    public class UploadPricingListFromExcel
    {
        WebContext db = new WebContext();
        private List<ErrorModelPricingList> ErrorModels = new List<ErrorModelPricingList>();

        public byte[] DownloadTemplate()
        {
            var fileinfo = new FileInfo(string.Format(@"{0}\QLBangGia.xlsx", HostingEnvironment.MapPath("/Uploads")));
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
            var localCategories = db.WebModules.ToList();
            ExcelPackage.LicenseContext = LicenseContext.Commercial;

            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                using (var package = new ExcelPackage(file.InputStream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfRow = workSheet.Dimension.End.Row;
                    var Locations = db.Location.ToList();
                    var Routes = db.Route.ToList();
                    var Partners = db.Partner.ToList();
                    var PricingTables = db.PricingTable.ToList();

                    var listPriceAddDB = new List<PricingTable>();
                    for (int rowIterator = 4; rowIterator <= noOfRow; rowIterator++)
                    {
                        if (IsRowEmpty(workSheet, rowIterator))
                        {
                            break; // ket thuc
                        }
                        var pricingList = new List<PricingTable>();

                        var priceInfo = new PricingTable();
                        var isValid = true;

                        var routeCode = CastToProperty<string>(workSheet, rowIterator, 2, "Mã lộ trình");
                        if (routeCode.IsSuccess)
                        {
                            priceInfo.RouteID = 0;
                            var RouteCodeIsExist = Routes.Where(y => y.RouteCode == routeCode.Value).FirstOrDefault();
                            if (RouteCodeIsExist != null)
                            {
                                priceInfo.RouteID = RouteCodeIsExist.ID;
                            }
                            else
                            {
                                var startLocation = CastToProperty<string>(workSheet, rowIterator, 3, "Nơi nhận");
                                var endLocation = CastToProperty<string>(workSheet, rowIterator, 4, "Nơi trả");
                                if (startLocation.IsSuccess && endLocation.IsSuccess)
                                {
                                    var findStartLocation = Locations.Where(x => x.LocationName.ToLower() == startLocation.Value.ToLower()).FirstOrDefault();
                                    var findEndLocation = Locations.Where(x => x.LocationName.ToLower() == endLocation.Value.ToLower()).FirstOrDefault();
                                    using (DbContextTransaction transaction = db.Database.BeginTransaction())
                                    {
                                        var newStartLocation = new Location();
                                        var newEndLocation = new Location();
                                        var newRoute = new Route();

                                        try
                                        {
                                            var startLocationID = 0;

                                            if (findStartLocation == null)
                                            {
                                                newStartLocation.LocationName = startLocation.Value;
                                                newStartLocation.CreatedDate = DateTime.Now;
                                                newStartLocation.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                                                db.Set<Location>().Add(newStartLocation);
                                                db.SaveChanges();
                                                Locations.Add(newStartLocation);
                                                startLocationID = newStartLocation.ID;
                                            }
                                            else
                                            {
                                                startLocationID = findStartLocation.ID;
                                            }

                                            var endLocationID = 0;

                                            if (findEndLocation == null)
                                            {
                                                newEndLocation.LocationName = endLocation.Value;
                                                newEndLocation.CreatedDate = DateTime.Now;
                                                newEndLocation.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                                                db.Set<Location>().Add(newEndLocation);
                                                db.SaveChanges();
                                                Locations.Add(newEndLocation);
                                                endLocationID = newEndLocation.ID;
                                            }
                                            else
                                            {
                                                endLocationID = findEndLocation.ID;

                                            }

                                            if (endLocationID != 0 && startLocationID != 0)
                                            {
                                                newRoute.RouteCode = routeCode.Value;
                                                newRoute.StartLocationID = startLocationID;
                                                newRoute.EndLocationID = endLocationID;
                                                db.Set<Route>().Add(newRoute);
                                                db.SaveChanges();
                                                Routes.Add(newRoute);
                                            }
                                            else
                                            {
                                                throw new Exception();
                                            }

                                            transaction.Commit();

                                        }
                                        catch (Exception e)
                                        {
                                            transaction.Rollback();
                                            if (newStartLocation.ID != 0)
                                            {
                                                Locations.Remove(newStartLocation);
                                            }
                                            if (newEndLocation.ID != 0)
                                            {
                                                Locations.Remove(newEndLocation);
                                            }
                                            if (newRoute.ID != 0)
                                            {
                                                Routes.Remove(newRoute);
                                            }

                                            AddErrorModels(rowIterator, 2, "Không thể thực hiện thêm địa điểm và lộ trình! Vui lòng thử lại");
                                            isValid = false;
                                        }
                                    }
                                }
                                priceInfo.RouteID = Routes.Where(x => x.RouteCode == routeCode.Value).Select(x => x.ID).FirstOrDefault();
                            }
                            if (priceInfo.RouteID == 0)
                            {
                                AddErrorModels(rowIterator, 2, "Không thể thực hiện thêm địa điểm và lộ trình! Vui lòng thử lại");
                                isValid = false;
                            }
                            var sourceKT_ST = Partners.Where(x => x.PartnerCode == "ST").Select(x => x.ID).FirstOrDefault();

                            var sourceST_AT = Partners.Where(x => x.PartnerCode == "AT").Select(x => x.ID).FirstOrDefault();
                            var destinationST_AT = sourceKT_ST;

                            var sourceAT_HPC = Partners.Where(x => x.PartnerCode == "HPC").Select(x => x.ID).FirstOrDefault();
                            var destinationAT_HPC = destinationST_AT;


                            // Import price for KT-ST
                            if (sourceKT_ST == 0)
                            {
                                AddErrorModels(rowIterator, 2, "Không tồn tại đối tác ST trên hệ thống!");
                                isValid = false;
                            }
                            else
                            {
                                var price_1_25 = CastToProperty<string>(workSheet, rowIterator, 5, "1.25", false);
                                var Weight_1_25_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 1 && x.SourcePartnerID == sourceKT_ST).Any();
                                if (price_1_25.IsSuccess && !Weight_1_25_IsExist)
                                {
                                    if (price_1_25.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceKT_ST;
                                        Price.WeightID = 1;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_1_25.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 5, "Kiểu dữ liệu không đúng");
                                        }
                                    }
                                }
                                else
                                {
                                    if (price_1_25.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 5, "Đã tồn tại bản ghi");
                                        isValid = false;
                                    }
                                }

                                var price_2_5 = CastToProperty<string>(workSheet, rowIterator, 6, "2.5", false);
                                var Weight_2_5_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 11 && x.SourcePartnerID == sourceKT_ST).Any();
                                if (price_2_5.IsSuccess && !Weight_2_5_IsExist)
                                {
                                    if (price_2_5.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceKT_ST;
                                        Price.WeightID = 11;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_2_5.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 6, "Kiểu dữ liệu không đúng");
                                        }

                                    }
                                }
                                else
                                {
                                    if (price_2_5.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 6, "Đã tồn tại bản ghi");
                                        isValid = false;
                                    }
                                }

                                var price_3_5 = CastToProperty<string>(workSheet, rowIterator, 7, "3.5", false);
                                var Weight_3_5_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 2 && x.SourcePartnerID == sourceKT_ST).Any();
                                if (price_3_5.IsSuccess && !Weight_3_5_IsExist)
                                {
                                    if (price_3_5.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceKT_ST;
                                        Price.WeightID = 2;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_3_5.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }

                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 7, "Kiểu dữ liệu không đúng");
                                        }
                                    }
                                }
                                else
                                {
                                    if (price_3_5.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 7, "Đã tồn tại bản ghi");
                                        isValid = false;
                                    }
                                }
                                var price_5 = CastToProperty<string>(workSheet, rowIterator, 8, "5", false);
                                var Weight_5_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 3 && x.SourcePartnerID == sourceKT_ST).Any();
                                if (price_5.IsSuccess && !Weight_5_IsExist)
                                {
                                    if (price_5.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceKT_ST;
                                        Price.WeightID = 3;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_5.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 8, "Kiểu dữ liệu không đúng");
                                        }

                                    }
                                }
                                else
                                {
                                    if (price_5.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 8, "Đã tồn tại bản ghi");
                                        isValid = false;
                                    }
                                }

                                var price_10 = CastToProperty<string>(workSheet, rowIterator, 9, "10", false);
                                var Weight_10_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 12 && x.SourcePartnerID == sourceKT_ST).Any();
                                if (price_10.IsSuccess && !Weight_10_IsExist)
                                {
                                    if (price_10.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceKT_ST;
                                        Price.WeightID = 12;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_10.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }

                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 9, "Kiểu dữ liệu không đúng");
                                        }

                                    }
                                }
                                else
                                {
                                    if (price_10.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 9, "Đã tồn tại bản ghi");
                                        isValid = false;
                                    }
                                }

                                var price_CA05 = CastToProperty<string>(workSheet, rowIterator, 10, "CA05", false);
                                var Weight_CA05_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 8 && x.SourcePartnerID == sourceKT_ST).Any();
                                if (price_CA05.IsSuccess && !Weight_CA05_IsExist)
                                {
                                    if (price_CA05.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceKT_ST;
                                        Price.WeightID = 8;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_CA05.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 10, "Kiểu dữ liệu không đúng");
                                        }

                                    }
                                }
                                else
                                {
                                    if (price_CA05.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 10, "Đã tồn tại bản ghi");
                                        isValid = false;

                                    }

                                }

                                var price_CA10 = CastToProperty<string>(workSheet, rowIterator, 11, "CA10", false);
                                var Weight_CA10_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 9 && x.SourcePartnerID == sourceKT_ST).Any();
                                if (price_CA10.IsSuccess && !Weight_CA10_IsExist)
                                {
                                    if (price_CA10.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceKT_ST;
                                        Price.WeightID = 9;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_CA10.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }

                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 11, "Kiểu dữ liệu không đúng");
                                        }

                                    }
                                }
                                else
                                {
                                    if (price_CA10.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 11, "Đã tồn tại bản ghi");
                                        isValid = false;
                                    }

                                }

                            }

                            // Import price for ST_AT
                            if (sourceST_AT == 0)
                            {
                                if (destinationST_AT == 0)
                                {
                                    AddErrorModels(rowIterator, 14, "Không tồn tại đối tác ST-AT trên hệ thống!");
                                    isValid = false;
                                }
                                else
                                {
                                    AddErrorModels(rowIterator, 14, "Không tồn tại đối tác AT trên hệ thống!");
                                    isValid = false;
                                }
                            }
                            else if (destinationST_AT == 0)
                            {
                                AddErrorModels(rowIterator, 14, "Không tồn tại đối tác ST trên hệ thống!");
                                isValid = false;
                            }
                            else
                            {
                                var price_1_25 = CastToProperty<string>(workSheet, rowIterator, 17, "1.25", false);
                                var Weight_1_25_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 1 && x.SourcePartnerID == sourceST_AT && x.DestinationPartnerID == destinationST_AT).Any();
                                if (price_1_25.IsSuccess && !Weight_1_25_IsExist)
                                {
                                    if (price_1_25.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.DestinationPartnerID = destinationST_AT;
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceST_AT;
                                        Price.WeightID = 1;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_1_25.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }

                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 17, "Kiểu dữ liệu không đúng");
                                        }

                                    }
                                }
                                else
                                {
                                    if (price_1_25.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 17, "Đã tồn tại bản ghi");
                                        isValid = false;
                                    }
                                }

                                var price_2_5 = CastToProperty<string>(workSheet, rowIterator, 18, "2.5", false);
                                var Weight_2_5_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 11 && x.SourcePartnerID == sourceST_AT && x.DestinationPartnerID == destinationST_AT).Any();
                                if (price_2_5.IsSuccess && !Weight_2_5_IsExist)
                                {
                                    if (price_2_5.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.DestinationPartnerID = destinationST_AT;
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceST_AT;
                                        Price.WeightID = 11;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_2_5.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 18, "Kiểu dữ liệu không đúng");
                                        }

                                    }
                                }
                                else
                                {
                                    if (price_2_5.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 18, "Đã tồn tại bản ghi");
                                        isValid = false;
                                    }
                                }

                                var price_3_5 = CastToProperty<string>(workSheet, rowIterator, 19, "3.5", false);
                                var Weight_3_5_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 2 && x.SourcePartnerID == sourceST_AT && x.DestinationPartnerID == destinationST_AT).Any();
                                if (price_3_5.IsSuccess && !Weight_3_5_IsExist)
                                {
                                    if (price_3_5.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.DestinationPartnerID = destinationST_AT;
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceST_AT;
                                        Price.WeightID = 2;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_3_5.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 19, "Kiểu dữ liệu không đúng");
                                        }
                                    }
                                }
                                else
                                {
                                    if (price_3_5.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 19, "Đã tồn tại bản ghi");
                                        isValid = false;
                                    }
                                }
                                var price_5 = CastToProperty<string>(workSheet, rowIterator, 20, "5", false);
                                var Weight_5_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 3 && x.SourcePartnerID == sourceST_AT && x.DestinationPartnerID == destinationST_AT).Any();
                                if (price_5.IsSuccess && !Weight_5_IsExist)
                                {
                                    if (price_5.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.DestinationPartnerID = destinationST_AT;
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceST_AT;
                                        Price.WeightID = 3;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_5.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 20, "Kiểu dữ liệu không đúng");
                                        }

                                    }
                                }
                                else
                                {
                                    if (price_5.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 20, "Đã tồn tại bản ghi");
                                        isValid = false;
                                    }
                                }

                                var price_10 = CastToProperty<string>(workSheet, rowIterator, 21, "10", false);
                                var Weight_10_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 12 && x.SourcePartnerID == sourceST_AT && x.DestinationPartnerID == destinationST_AT).Any();
                                if (price_10.IsSuccess && !Weight_10_IsExist)
                                {
                                    if (price_10.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.DestinationPartnerID = destinationST_AT;
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceST_AT;
                                        Price.WeightID = 12;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_10.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 21, "Kiểu dữ liệu không đúng");
                                        }

                                    }
                                }
                                else
                                {
                                    if (price_10.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 21, "Đã tồn tại bản ghi");
                                        isValid = false;
                                    }
                                }

                                var price_CA05 = CastToProperty<string>(workSheet, rowIterator, 22, "CA05", false);
                                var Weight_CA05_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 8 && x.SourcePartnerID == sourceST_AT && x.DestinationPartnerID == destinationST_AT).Any();
                                if (price_CA05.IsSuccess && !Weight_CA05_IsExist)
                                {
                                    if (price_CA05.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.DestinationPartnerID = destinationST_AT;
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceST_AT;
                                        Price.WeightID = 8;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_CA05.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }

                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 22, "Kiểu dữ liệu không đúng");
                                        }

                                    }
                                }
                                else
                                {
                                    if (price_CA05.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 22, "Đã tồn tại bản ghi");
                                        isValid = false;

                                    }

                                }

                                var price_CA10 = CastToProperty<string>(workSheet, rowIterator, 23, "CA10", false);
                                var Weight_CA10_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 9 && x.SourcePartnerID == sourceST_AT && x.DestinationPartnerID == destinationST_AT).Any();
                                if (price_CA10.IsSuccess && !Weight_CA10_IsExist)
                                {
                                    if (price_CA10.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.DestinationPartnerID = destinationST_AT;
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceST_AT;
                                        Price.WeightID = 9;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_CA10.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 23, "Kiểu dữ liệu không đúng");
                                        }

                                    }
                                }
                                else
                                {
                                    if (price_CA10.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 23, "Đã tồn tại bản ghi");
                                        isValid = false;
                                    }
                                }
                            }

                            // Import price for AT-HPC
                            if (sourceAT_HPC == 0)
                            {
                                if (destinationAT_HPC == 0)
                                {
                                    AddErrorModels(rowIterator, 26, "Không tồn tại đối tác AT-HPC trên hệ thống!");
                                    isValid = false;
                                }
                                else
                                {
                                    AddErrorModels(rowIterator, 26, "Không tồn tại đối tác HPC trên hệ thống!");
                                    isValid = false;
                                }
                            }
                            else if (destinationAT_HPC == 0)
                            {
                                AddErrorModels(rowIterator, 26, "Không tồn tại đối tác AT trên hệ thống!");
                                isValid = false;
                            }
                            else
                            {
                                var price_1_25 = CastToProperty<string>(workSheet, rowIterator, 29, "1.25", false);
                                var Weight_1_25_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 1 && x.SourcePartnerID == sourceAT_HPC && x.DestinationPartnerID == destinationAT_HPC).Any();
                                if (price_1_25.IsSuccess && !Weight_1_25_IsExist)
                                {
                                    if (price_1_25.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.DestinationPartnerID = destinationAT_HPC;
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceAT_HPC;
                                        Price.WeightID = 1;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_1_25.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }

                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 29, "Kiểu dữ liệu không đúng");
                                        }

                                    }
                                }
                                else
                                {
                                    if (price_1_25.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 29, "Đã tồn tại bản ghi");
                                        isValid = false;
                                    }
                                }

                                var price_2_5 = CastToProperty<string>(workSheet, rowIterator, 30, "2.5", false);
                                var Weight_2_5_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 11 && x.SourcePartnerID == sourceAT_HPC && x.DestinationPartnerID == destinationAT_HPC).Any();
                                if (price_2_5.IsSuccess && !Weight_2_5_IsExist)
                                {
                                    if (price_2_5.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.DestinationPartnerID = destinationAT_HPC;
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceAT_HPC;
                                        Price.WeightID = 11;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_2_5.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 30, "Kiểu dữ liệu không đúng");
                                        }

                                    }
                                }
                                else
                                {
                                    if (price_2_5.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 30, "Đã tồn tại bản ghi");
                                        isValid = false;
                                    }
                                }

                                var price_3_5 = CastToProperty<string>(workSheet, rowIterator, 31, "3.5", false);
                                var Weight_3_5_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 2 && x.SourcePartnerID == sourceAT_HPC && x.DestinationPartnerID == destinationAT_HPC).Any();
                                if (price_3_5.IsSuccess && !Weight_3_5_IsExist)
                                {
                                    if (price_3_5.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.DestinationPartnerID = destinationAT_HPC;
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceAT_HPC;
                                        Price.WeightID = 2;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_3_5.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }

                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 31, "Kiểu dữ liệu không đúng");
                                        }
                                    }
                                }
                                else
                                {
                                    if (price_3_5.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 31, "Đã tồn tại bản ghi");
                                        isValid = false;
                                    }
                                }
                                var price_5 = CastToProperty<string>(workSheet, rowIterator, 32, "5", false);
                                var Weight_5_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 3 && x.SourcePartnerID == sourceAT_HPC && x.DestinationPartnerID == destinationAT_HPC).Any();
                                if (price_5.IsSuccess && !Weight_5_IsExist)
                                {
                                    if (price_5.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.DestinationPartnerID = destinationAT_HPC;
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceAT_HPC;
                                        Price.WeightID = 3;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_5.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 32, "Kiểu dữ liệu không đúng");
                                        }

                                    }
                                }
                                else
                                {
                                    if (price_5.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 32, "Đã tồn tại bản ghi");
                                        isValid = false;
                                    }
                                }

                                var price_10 = CastToProperty<string>(workSheet, rowIterator, 33, "10", false);
                                var Weight_10_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 12 && x.SourcePartnerID == sourceAT_HPC && x.DestinationPartnerID == destinationAT_HPC).Any();
                                if (price_10.IsSuccess && !Weight_10_IsExist)
                                {
                                    if (price_10.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.DestinationPartnerID = destinationAT_HPC;
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceAT_HPC;
                                        Price.WeightID = 12;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_10.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 33, "Kiểu dữ liệu không đúng");
                                        }

                                    }
                                }
                                else
                                {
                                    if (price_10.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 33, "Đã tồn tại bản ghi");
                                        isValid = false;
                                    }
                                }

                                var price_CA05 = CastToProperty<string>(workSheet, rowIterator, 34, "CA05", false);
                                var Weight_CA05_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 8 && x.SourcePartnerID == sourceAT_HPC && x.DestinationPartnerID == destinationAT_HPC).Any();
                                if (price_CA05.IsSuccess && !Weight_CA05_IsExist)
                                {
                                    if (price_CA05.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.DestinationPartnerID = destinationAT_HPC;
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceAT_HPC;
                                        Price.WeightID = 8;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_CA05.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }

                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 34, "Kiểu dữ liệu không đúng");
                                        }

                                    }
                                }
                                else
                                {
                                    if (price_CA05.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 34, "Đã tồn tại bản ghi");
                                        isValid = false;

                                    }

                                }

                                var price_CA10 = CastToProperty<string>(workSheet, rowIterator, 35, "CA10", false);
                                var Weight_CA10_IsExist = PricingTables.Where(x => x.RouteID == priceInfo.RouteID && x.WeightID == 9 && x.SourcePartnerID == sourceAT_HPC && x.DestinationPartnerID == destinationAT_HPC).Any();
                                if (price_CA10.IsSuccess && !Weight_CA10_IsExist)
                                {
                                    if (price_CA10.Value != null)
                                    {
                                        PricingTable Price = new PricingTable();
                                        Price.DestinationPartnerID = destinationAT_HPC;
                                        Price.RouteID = priceInfo.RouteID;
                                        Price.SourcePartnerID = sourceAT_HPC;
                                        Price.WeightID = 9;
                                        try
                                        {
                                            var priceCheck = Double.Parse(price_CA10.Value);
                                            if (priceCheck != 0)
                                            {
                                                Price.Price = priceCheck;
                                                pricingList.Add(Price);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            AddErrorModels(rowIterator, 35, "Kiểu dữ liệu không đúng");
                                        }

                                    }
                                }
                                else
                                {
                                    if (price_CA10.Value != null)
                                    {
                                        AddErrorModels(rowIterator, 35, "Đã tồn tại bản ghi");
                                        isValid = false;
                                    }
                                }

                            }
                        }
                        else
                        {
                            isValid = false;
                        }

                        try
                        {
                            if (isValid)
                            {
                                foreach (var price in pricingList)
                                {
                                    price.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                                    price.CreatedDate = DateTime.Now;
                                }
                                PricingTables.AddRange(pricingList);
                                listPriceAddDB.AddRange(pricingList);
                            }
                        }
                        catch (DbEntityValidationException ex)
                        {
                            // Retrieve the error messages as a list of strings.
                            var errorMessages = ex.EntityValidationErrors
                                    .SelectMany(x => x.ValidationErrors)
                                    .Select(x => x.ErrorMessage);

                            // Join the list to a single string.
                            var fullErrorMessage = string.Join("; ", errorMessages);

                            // Combine the original exception message with the new one.
                            var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);
                        }
                    }
                    var checkSave = true;
                    try
                    {
                        listPriceAddDB.Reverse();
                        db.PricingTable.AddRange(listPriceAddDB);
                        db.SaveChanges();
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
            ErrorModels.Add(new ErrorModelPricingList()
            {
                ColumnNumber = columnNuber,
                RowNumber = rowNumber,
                ErrorMessage = error
            });
        }

        private CastResultPricingList<T> CastToProperty<T>(ExcelWorksheet worksheet, int rowNumber, int columnNuber, string property, bool isRequired = true)
        {
            if (isRequired && (worksheet.Cells[rowNumber, columnNuber].Value == null || string.IsNullOrWhiteSpace(worksheet.Cells[rowNumber, columnNuber].Value.ToString())))
            {
                ErrorModels.Add(new ErrorModelPricingList()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không được để trống"
                });

                return new CastResultPricingList<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
            try
            {
                return new CastResultPricingList<T>()
                {
                    Value = (T)Convert.ChangeType(worksheet.Cells[rowNumber, columnNuber].Value, typeof(T)),
                    IsSuccess = true
                };
            }
            catch (FormatException ex)
            {
                ErrorModels.Add(new ErrorModelPricingList()
                {
                    ColumnNumber = columnNuber,
                    RowNumber = rowNumber,
                    ErrorMessage = "Không đúng định dạng"
                });

                return new CastResultPricingList<T>()
                {
                    Value = default(T),
                    IsSuccess = false
                };
            }
        }

        private byte[] LogErrorsToFile(ExcelWorksheet worksheet)
        {
            var noOfCol = worksheet.Dimension.End.Column;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLBangGia.xlsx", HostingEnvironment.MapPath("/Uploads")));
            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var ws = p.Workbook.Worksheets.First();
                    // set rows
                    if (!ErrorModels.Any())
                    {
                        ws.Cells["A4:J4"].Merge = true;
                        ws.Cells[4, 1].Value = "Tất cả giá đã được nhập thành công!";
                    }

                    var allRowNumbers = ErrorModels.Select(x => x.RowNumber).Distinct().OrderBy(x => x).ToList();

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

    public class CastResultPricingList<T>
    {
        public T Value { get; set; }

        public bool IsSuccess { get; set; }
    }

    public class ErrorModelPricingList
    {
        public string ErrorMessage { get; set; }

        public int RowNumber { get; set; }

        public int ColumnNumber { get; set; }
    }
}