using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using WEB.Areas.ContentType.Controllers;
using WEB.Models;
using WebMatrix.WebData;
using WebModels;

namespace WEB.Areas.Admin.Controllers
{
    [VanTaiAuthorize]

    public class ManagePriceListController : Controller
    {

        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        public ActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        public JsonResult Get_RouteCodes()
        {
            var routes = from x in db.Route.AsNoTracking() select x;
            return Json(routes.Select(x => new
            {
                RouteID = x.ID,
                RouteName = x.RouteCode
            }), JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public JsonResult Get_End_Locations(int startID)
        {
            var locations = db.Location.Where(x => x.ID != startID).ToList();

            return Json(locations.Select(x => new
            {
                endID = x.ID,
                endName = x.LocationName
            }), JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public JsonResult Get_Start_Locations()
        {
            var locations = from x in db.Location.AsNoTracking() select x;
            return Json(locations.Select(x => new
            {
                startID = x.ID,
                startName = x.LocationName
            }), JsonRequestBehavior.AllowGet);
        }
       
        [AllowAnonymous]
        public JsonResult Get_DestinationPartners(int sourceID)
        {
            var partners = db.Partner.Where(x => x.ID != 0).ToList();

            return Json(partners.Select(x => new
            {
                x.ID,
                x.PartnerName
            }), JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public JsonResult Get_SourcePartners()
        {
            var partners = from x in db.Partner.AsNoTracking() select x;
            return Json(partners.Select(x => new
            {
                x.ID,
                x.PartnerName
            }), JsonRequestBehavior.AllowGet);
        }
        public ActionResult PriceLists_Read([DataSourceRequest] DataSourceRequest request)
        {
            List<PricingTable> PriceList = new List<PricingTable>();

            var routeList = from a in db.PricingTable
                            join b in db.Route on a.RouteID equals b.ID into group1
                            from b in group1.DefaultIfEmpty()
                            join c in db.VehicleWeight on a.WeightID equals c.ID into group2
                            from c in group2.DefaultIfEmpty()
                            join d in db.Partner on a.SourcePartnerID equals d.ID into group3
                            from d in group3.DefaultIfEmpty()
                            join e in db.Partner on a.DestinationPartnerID equals e.ID into group4
                            from e in group4.DefaultIfEmpty()
                                //join d in db.VehicleWeight on a.ActualWeightID equals d.ID into group3
                                //from d in group3.DefaultIfEmpty()
                            select new
                            {
                                a.ID,
                                a.Price,
                                a.Note,
                                b.RouteCode,
                                StartLocation = b.StartLocation.LocationName,
                                EndLocation = b.EndLocation.LocationName,
                                c.WeightName,
                                SourcePartnerName = d.PartnerName,
                                DestinationPartnerName = e.PartnerName,
                                d.PartnerCode,
                                a.WeightID

                            };

            foreach (var item in routeList.Where(x => x.PartnerCode == "ST").ToList())
            {
                var price = new PricingTable();
                price.ID = item.ID;
                price.WeightID = item.WeightID;
                price.Route = new Route()
                {
                    RouteCode = item.RouteCode,
                    StartLocation = new Location()
                    {
                        LocationName = item.StartLocation
                    },
                    EndLocation = new Location()
                    {
                        LocationName = item.EndLocation
                    }

                };
                price.Weight = new VehicleWeight()
                {
                    WeightName = item.WeightName
                };
                price.Price = item.Price;
                price.Note = item.Note;
                price.SourcePartner = new Partner()
                {
                    PartnerName = item.SourcePartnerName,
                    PartnerCode = item.PartnerCode
                    
                };
                price.DestinationPartner = new Partner()
                {
                    PartnerName = item.DestinationPartnerName
                };
                PriceList.Add(price);
            }

            var jsonResult = Json(PriceList.OrderByDescending(x => x.ID).ToDataSourceResult(request));
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }
        public ActionResult Add()
        {
            var model = new PricingTable();
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add([Bind(Exclude = "")] PricingTable model)
        {
            
            if (ModelState.IsValid)
            {
                var temp = db.PricingTable.Where(p => p.RouteID == model.RouteID && p.WeightID == model.WeightID && p.SourcePartnerID == model.SourcePartnerID && p.DestinationPartnerID == model.DestinationPartnerID).FirstOrDefault();
                if (temp != null)
                {
                    ModelState.AddModelError("", AccountResources.PriceIsExists);
                    return View(model);
                }
                else
                {
                    try
                    {
                        model.CreatedDate = DateTime.Now;
                        var user = db.Set<UserProfile>().Find(WebSecurity.GetUserId(User.Identity.Name));
                        model.CreatedBy = user.UserId;
                        model.ModifiedDate = null;
                        db.Set<PricingTable>().Add(model);
                        db.SaveChanges();
                        ViewBag.StartupScript = "create_success();";
                        return View(model);

                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                        return View(model);
                    }
                }
            }
            else
            {
                return View(model);
            }
        }

        public ActionResult Edit(int id)
        {
            var model = db.Set<PricingTable>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "")] PricingTable model)
        {
            if (ModelState.IsValid)
            {
                var list = db.PricingTable.Where(x => x.ID != model.ID).ToList();
                var temp = list.Where(p => p.RouteID == model.RouteID && p.WeightID == model.WeightID && p.SourcePartnerID == model.SourcePartnerID && p.DestinationPartnerID == model.DestinationPartnerID).FirstOrDefault();

                if (temp != null)
                {
                    ModelState.AddModelError("", AccountResources.PriceIsExists);
                    return View(model);
                }
                else
                {
                    using (DbContextTransaction transaction = db.Database.BeginTransaction())
                    {

                        try
                        {
                            var user = db.Set<UserProfile>().Find(WebSecurity.GetUserId(User.Identity.Name));
                            var price = db.PricingTable.Where(m => m.ID == model.ID).Select(x => new { x.RouteID, x.Price, x.SourcePartnerID, x.DestinationPartnerID, x.WeightID }).FirstOrDefault();

                            if (model.Price != price.Price || model.RouteID != price.RouteID || model.SourcePartnerID != price.SourcePartnerID || model.DestinationPartnerID != price.DestinationPartnerID || model.WeightID != price.WeightID)
                            {
                                var priceChangeLog = new PriceChangeLog();
                                priceChangeLog.Price = (double)price.Price;
                                priceChangeLog.ChangePrice = Double.Parse(model.Price.ToString());
                                priceChangeLog.SourcePartnerID = price.SourcePartnerID;
                                if (model.SourcePartnerID != price.SourcePartnerID)
                                {
                                    priceChangeLog.ChangeSourceID = model.SourcePartnerID;
                                }
                                else
                                {
                                    priceChangeLog.ChangeSourceID = null;

                                }
                                priceChangeLog.DestinationPartnerID = price.DestinationPartnerID;
                                if (model.DestinationPartnerID != price.DestinationPartnerID)
                                {
                                    priceChangeLog.ChangeDestinationID = model.DestinationPartnerID;
                                }
                                else
                                {
                                    priceChangeLog.ChangeDestinationID = null;

                                }
                                priceChangeLog.RouteID = price.RouteID;
                                if (model.RouteID != price.RouteID)
                                {
                                    priceChangeLog.ChangeRouteID = model.RouteID;
                                }
                                else
                                {
                                    priceChangeLog.ChangeRouteID = null;

                                }
                                priceChangeLog.WeightID = price.WeightID;
                                if (model.WeightID != price.WeightID)
                                {
                                    priceChangeLog.ChangeWeightID = model.WeightID;
                                }
                                else
                                {
                                    priceChangeLog.ChangeWeightID = null;

                                }
                                priceChangeLog.CreatedDate = DateTime.Now;
                                priceChangeLog.ModifiedDate = null;
                                priceChangeLog.CreatedBy = user.UserId;
                                priceChangeLog.PricingTableID = model.ID;
                                db.Set<PriceChangeLog>().Add(priceChangeLog);
                                db.SaveChanges();
                            }
                            model.ModifiedDate = DateTime.Now;
                            model.ModifiedBy = user.UserId;
                            model.CreatedDate = null;
                            db.PricingTable.Attach(model);
                            db.Entry(model).Property(a => a.RouteID).IsModified = true;
                            db.Entry(model).Property(a => a.SourcePartnerID).IsModified = true;
                            db.Entry(model).Property(a => a.WeightID).IsModified = true;
                            db.Entry(model).Property(a => a.Price).IsModified = true;
                            db.Entry(model).Property(a => a.Note).IsModified = true;
                            db.Entry(model).Property(a => a.DestinationPartnerID).IsModified = true;
                            db.Entry(model).Property(a => a.ModifiedBy).IsModified = true;
                            db.Entry(model).Property(a => a.ModifiedDate).IsModified = true;
                            db.SaveChanges();
                            transaction.Commit();
                            ViewBag.StartupScript = "edit_success();";
                            return View(model);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            ModelState.AddModelError("", AccountResources.PriceIsNotSaved);
                            return View(model);
                        }
                    }
                }
            }
            else
            {
                return View(model);
            }
        }
        public ActionResult ChangePrice()
        {
            var model = new PercentPriceViewModel();
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult ChangePrice([Bind(Exclude = "")] PercentPriceViewModel model)
        {
            if (ModelState.IsValid)
            {
                String percentString = (model.Percent).Replace('.', ',');
                double percent = Double.Parse(percentString);
                var user = db.Set<UserProfile>().Find(WebSecurity.GetUserId(User.Identity.Name));
                var totalCount = db.PricingTable.Count();
                try
                {
                    var percentParam = new SqlParameter("@Percent", percent);
                    var createdByParam = new SqlParameter("@CreatedBy", user.UserId);

                    var result = db.Database.ExecuteSqlCommand("ChangePriceList @Percent,@CreatedBy", percentParam, createdByParam);

                    if (result != totalCount + 1)
                    {
                        throw new Exception();
                    }

                    ViewBag.StartupScript = "change_success();";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", AccountResources.PriceIsNotSaved);
                }

                return View(model);
            }
            else
            {
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult ExportExcel(string dataString)
        {
            List<PricingTableExport> listData = new List<PricingTableExport>();
            var dataListJson = dataString.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                PricingTableExport dataObj = null;
                var dataObjString = string.Empty;
                if (i == 0)
                {
                    dataObjString = dataObjSplit1[i] + "}";
                }
                else
                {
                    var dataObjString0 = dataObjSplit1[i].Substring(1);
                    dataObjString = dataObjString0 + "}";
                }
                dataObj = JsonConvert.DeserializeObject<PricingTableExport>(dataObjString);
                listData.Add(dataObj);
            }
            var result = DownloadPrice(listData);
            var fileStream = new MemoryStream(result);
            return File(fileStream, "application/ms-excel", "QLBangGia.xlsx");
        }
        public byte[] DownloadPrice(List<PricingTableExport> models)
        {
            var queryPrice = from a in models
                             group a by a.RouteCode;

            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLBangGia.xlsx", HostingEnvironment.MapPath("/Uploads")));

            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var productWorksheet = p.Workbook.Worksheets[0];
                    productWorksheet.Select();
                    int i = 2;
                    foreach (var price in queryPrice)
                    {
                        foreach(var subPrice in price)
                        {
                            //var routeCode = db.Route.Where(m => m.ID == price.Key).FirstOrDefault();
                            productWorksheet.Cells[i + 2, 2].Value = subPrice.RouteCode;
                            //var StartLocation = db.Location.Where(m => m.ID == routeCode.StartLocationID).Select(x => x.LocationName);
                            productWorksheet.Cells[i + 2, 3].Value = subPrice.StartLocationName;
                            //var EndLocation = db.Location.Where(m => m.ID == routeCode.EndLocationID).Select(x => x.LocationName);
                            productWorksheet.Cells[i + 2, 4].Value = subPrice.EndLocationName;

                            //ST-AT
                            productWorksheet.Cells[i + 2, 14].Value = subPrice.RouteCode; 
                            productWorksheet.Cells[i + 2, 15].Value = subPrice.StartLocationName;
                            productWorksheet.Cells[i + 2, 16].Value = subPrice.EndLocationName;

                            //AT-HPC
                            productWorksheet.Cells[i + 2, 26].Value = subPrice.RouteCode;
                            productWorksheet.Cells[i + 2, 27].Value = subPrice.StartLocationName;
                            productWorksheet.Cells[i + 2, 28].Value = subPrice.EndLocationName;
                            break;
                        }

                        productWorksheet.Cells[i + 2, 1].Value = i - 1;
                        productWorksheet.Cells[i + 2, 13].Value = i - 1;
                        productWorksheet.Cells[i + 2, 25].Value = i - 1;

                        if (i==2)
                        {
                            productWorksheet.Cells[i + 2, 5].Value = "";
                            productWorksheet.Cells[i + 2, 6].Value = "";
                            productWorksheet.Cells[i + 2, 7].Value = "";
                            productWorksheet.Cells[i + 2, 8].Value = "";
                            productWorksheet.Cells[i + 2, 9].Value = "";
                            productWorksheet.Cells[i + 2, 10].Value = "";
                            productWorksheet.Cells[i + 2, 11].Value = "";
                            productWorksheet.Cells[i + 2, 29].Value = "";
                            productWorksheet.Cells[i + 2, 30].Value = "";
                            productWorksheet.Cells[i + 2, 31].Value = "";
                            productWorksheet.Cells[i + 2, 32].Value = "";
                            productWorksheet.Cells[i + 2, 33].Value = "";
                            productWorksheet.Cells[i + 2, 34].Value = "";
                            productWorksheet.Cells[i + 2, 35].Value = "";
                            productWorksheet.Cells[i + 2, 17].Value = "";
                            productWorksheet.Cells[i + 2, 18].Value = "";
                            productWorksheet.Cells[i + 2, 19].Value = "";
                            productWorksheet.Cells[i + 2, 20].Value = "";
                            productWorksheet.Cells[i + 2, 21].Value = "";
                            productWorksheet.Cells[i + 2, 22].Value = "";
                            productWorksheet.Cells[i + 2, 23].Value = "";
                        }
                       

                     
                        // Export price KT-ST
                        foreach (var subPrice in price.Where(x => x.PartnerCode == "ST"))
                        {
                            if (subPrice.WeightID == 1)
                            {
                                productWorksheet.Cells[i + 2, 5].Value = subPrice.Price;
                                productWorksheet.Cells[i + 2, 5].Style.Numberformat.Format = "#,##0";
                            }
                            if (subPrice.WeightID == 11)
                            {
                                productWorksheet.Cells[i + 2, 6].Value = subPrice.Price;
                                productWorksheet.Cells[i + 2, 6].Style.Numberformat.Format = "#,##0";
                            }
                            if (subPrice.WeightID == 2)
                            {
                                productWorksheet.Cells[i + 2, 7].Value = subPrice.Price;
                                productWorksheet.Cells[i + 2, 7].Style.Numberformat.Format = "#,##0";
                            }
                            if (subPrice.WeightID == 3)
                            {
                                productWorksheet.Cells[i + 2, 8].Value = subPrice.Price;
                                productWorksheet.Cells[i + 2, 8].Style.Numberformat.Format = "#,##0";
                            }
                            if (subPrice.WeightID == 12)
                            {
                                productWorksheet.Cells[i + 2, 9].Value = subPrice.Price;
                                productWorksheet.Cells[i + 2, 9].Style.Numberformat.Format = "#,##0";
                            }

                            if (subPrice.WeightID == 8)
                            {
                                productWorksheet.Cells[i + 2, 10].Value = subPrice.Price;
                                productWorksheet.Cells[i + 2, 10].Style.Numberformat.Format = "#,##0";
                            }
                            if (subPrice.WeightID == 9)
                            {
                                productWorksheet.Cells[i + 2, 11].Value = subPrice.Price;
                                productWorksheet.Cells[i + 2, 11].Style.Numberformat.Format = "#,##0";
                            }

                        }

                        // Export price ST-AT
                        foreach (var subPrice in price.Where(x => x.PartnerCode == "AT").ToList())

                        {
                            
                            //if (subPrice.WeightID == 1)
                            //{
                            //    productWorksheet.Cells[i + 2, 17].Value = subPrice.Price;
                            //    productWorksheet.Cells[i + 2, 17].Style.Numberformat.Format = "#,##0";
                            //}
                            //if (subPrice.WeightID == 11)
                            //{
                            //    productWorksheet.Cells[i + 2, 18].Value = subPrice.Price;
                            //    productWorksheet.Cells[i + 2, 18].Style.Numberformat.Format = "#,##0";
                            //}
                            //if (subPrice.WeightID == 2)
                            //{
                            //    productWorksheet.Cells[i + 2, 19].Value = subPrice.Price;
                            //    productWorksheet.Cells[i + 2, 19].Style.Numberformat.Format = "#,##0";
                            //}
                            //if (subPrice.WeightID == 3)
                            //{
                            //    productWorksheet.Cells[i + 2, 20].Value = subPrice.Price;
                            //    productWorksheet.Cells[i + 2, 20].Style.Numberformat.Format = "#,##0";
                            //}
                            //if (subPrice.WeightID == 12)
                            //{
                            //    productWorksheet.Cells[i + 2, 21].Value = subPrice.Price;
                            //    productWorksheet.Cells[i + 2, 21].Style.Numberformat.Format = "#,##0";
                            //}

                            //if (subPrice.WeightID == 8)
                            //{
                            //    productWorksheet.Cells[i + 2, 22].Value = subPrice.Price;
                            //    productWorksheet.Cells[i + 2, 22].Style.Numberformat.Format = "#,##0";
                            //}
                            //if (subPrice.WeightID == 9)
                            //{
                            //    productWorksheet.Cells[i + 2, 23].Value = subPrice.Price;
                            //    productWorksheet.Cells[i + 2, 23].Style.Numberformat.Format = "#,##0";
                            //}

                        }

                        // Export price AT-HPC
                        foreach (var subPrice in price.Where(x => x.PartnerCode == "HPC").ToList())

                        {
                            //if (subPrice.WeightID == 1)
                            //{
                            //    productWorksheet.Cells[i + 2, 29].Value = subPrice.Price;
                            //    productWorksheet.Cells[i + 2, 29].Style.Numberformat.Format = "#,##0";
                            //}
                            //if (subPrice.WeightID == 11)
                            //{
                            //    productWorksheet.Cells[i + 2, 30].Value = subPrice.Price;
                            //    productWorksheet.Cells[i + 2, 30].Style.Numberformat.Format = "#,##0";
                            //}
                            //if (subPrice.WeightID == 2)
                            //{
                            //    productWorksheet.Cells[i + 2, 31].Value = subPrice.Price;
                            //    productWorksheet.Cells[i + 2, 31].Style.Numberformat.Format = "#,##0";
                            //}
                            //if (subPrice.WeightID == 3)
                            //{
                            //    productWorksheet.Cells[i + 2, 32].Value = subPrice.Price;
                            //    productWorksheet.Cells[i + 2, 32].Style.Numberformat.Format = "#,##0";
                            //}
                            //if (subPrice.WeightID == 12)
                            //{
                            //    productWorksheet.Cells[i + 2, 33].Value = subPrice.Price;
                            //    productWorksheet.Cells[i + 2, 33].Style.Numberformat.Format = "#,##0";
                            //}

                            //if (subPrice.WeightID == 8)
                            //{
                            //    productWorksheet.Cells[i + 2, 34].Value = subPrice.Price;
                            //    productWorksheet.Cells[i + 2, 34].Style.Numberformat.Format = "#,##0";
                            //}
                            //if (subPrice.WeightID == 9)
                            //{
                            //    productWorksheet.Cells[i + 2, 35].Value = subPrice.Price;
                            //    productWorksheet.Cells[i + 2, 35].Style.Numberformat.Format = "#,##0";
                            //}
                        }

                        i++;
                    }
                    
                    var modelTable = productWorksheet.Cells["A4:K" + (i + 1).ToString()];
                    modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    modelTable.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    modelTable.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                    var modelTableST_AT = productWorksheet.Cells["M4:W" + (i + 1).ToString()];

                    modelTableST_AT.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    modelTableST_AT.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    modelTableST_AT.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    modelTableST_AT.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                    var modelTableAT_HPC = productWorksheet.Cells["Y4:AI" + (i + 1).ToString()];

                    modelTableAT_HPC.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    modelTableAT_HPC.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    modelTableAT_HPC.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    modelTableAT_HPC.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                    return p.GetAsByteArray();
                }
            }
            else
            {
                return null;
            }
        }

        public ActionResult UploadExcel()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetUploadProgress()
        {
            var result = Session["UploadPriceProgress"] == null ? 0 : (int)Session["UploadPriceProgress"];

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UploadExcel(string submit)
        {
            if (submit == "Upload")
            {
                HttpPostedFileBase file = Request.Files[0];
                if (file == null || file.ContentLength == 0)
                {
                    return View();
                }
                var fileTemp = new FileInfo(string.Format(@"{0}\QLBangGia.xlsx", HostingEnvironment.MapPath("/Uploads")));
                var check = ExcelHelper.CheckHeaderFile(fileTemp, file, 2);
                if (check == 1)
                {
                    var result = new UploadPricingListFromExcel().UploadProducts(file, Session["UploadPriceProgress"]);
                    if (result == null)
                    {
                        ViewBag.check = "Upload giá thành công!";
                        ViewBag.StartupScript = "upload_success();";
                        return View();
                    }
                    else
                    {
                        String path = Server.MapPath("~/Uploads/FileResult/"); //Path

                        //Check if directory exist
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path); //Create directory if it doesn't exist
                        }

                        var randomFileName = Guid.NewGuid().ToString().Substring(0, 4) + ".xlsx";
                        string filePath = Path.Combine(path, randomFileName);

                        System.IO.File.WriteAllBytes(filePath, result);
                        ViewBag.check = "Vui lòng kiểm tra file được trả về!";
                        ViewBag.pathFileName = "/Uploads/FileResult/" + randomFileName;
                        ViewBag.StartupScript = "downLoadFile();";
                        return View();
                    }
                }
                else if (check == 0)
                {
                    ViewBag.check = "File không đúng định dạng Template";
                    ViewBag.StartupScript = "hideLoading();";
                    return View();
                }
                else
                {
                    ViewBag.check = "Đã có lỗi xảy ra! Vui lòng liên hệ quản trị viên.";
                    ViewBag.StartupScript = "hideLoading();";
                    return View();
                }
            }
            else
            {
                var result = new UploadPricingListFromExcel().DownloadTemplate();
                var fileStream = new MemoryStream(result);
                return File(fileStream, "application/ms-excel", "QLBangGia_Template.xlsx");
            }
        }
        [HttpPost]
        public ActionResult DeleteFile(string dataString)
        {
            string path = Server.MapPath(dataString);
            FileInfo file = new FileInfo(path);

            var dateGet = System.IO.File.ReadAllBytes(path);
            if (file.Exists)//check file exsit or not  
            {
                file.Delete();
            }

            return File(dateGet, "application/ms-excel", "QLBangGia_UploadResult.xlsx");

        }
    }
}