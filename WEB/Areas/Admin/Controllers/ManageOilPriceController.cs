using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Security;
using WEB.Areas.ContentType.Controllers;
using WEB.Models;
using WebMatrix.WebData;
using WebModels;

namespace WEB.Areas.Admin.Controllers
{
    [VanTaiAuthorize]

    public class ManageOilPriceController : Controller
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        // GET: Admin/DrivePlan
        public ActionResult Index()
        {
           

            DateTime date = DateTime.Now.AddMonths(-6);
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(12).AddDays(-1);

            string day = firstDayOfMonth.Day.ToString() + "-" + firstDayOfMonth.Month.ToString() + "-" + firstDayOfMonth.Year.ToString()
                + " to " + lastDayOfMonth.Day.ToString() + "-" + lastDayOfMonth.Month.ToString() + "-" + lastDayOfMonth.Year.ToString();

           
            ViewBag.Date = day;
            ViewBag.DateNow = date;

            return View();
        }
        public ActionResult OilPrices_Read([DataSourceRequest] DataSourceRequest request, string datetime)
        {
            string[] arrListStr = datetime.Split(new char[] { 't', 'o' });
            var firstDay = DateTime.Parse(arrListStr[0].Format("{0:dd-MM-yyyy}"));
            var lastDay = DateTime.Parse(arrListStr[2].Format("{0:dd-MM-yyyy}"));
            var oilPriceyQuery = from a in db.OilPrices.Where(x => DbFunctions.TruncateTime(firstDay) <= DbFunctions.TruncateTime(x.EndDate) && DbFunctions.TruncateTime(x.StartDate) <= DbFunctions.TruncateTime(lastDay))
                                 select a;
       
            var jsonResult = Json(oilPriceyQuery.ToList().OrderByDescending(x => x.StartDate).ThenByDescending(x=>x.EndDate).ToDataSourceResult(request));
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult Add()
        {
            var model = new OilPriceViewModel();
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add([Bind(Exclude = "")] OilPriceViewModel model, DateTime? datestart, DateTime? date)
        {
            if (ModelState.IsValid)
            {
                if (datestart == null)
                {
                    ModelState.AddModelError("", AccountResources.RequiredStartDate);
                    return View(model);
                }
                if (date == null)
                {
                    ModelState.AddModelError("", AccountResources.RequiredEndDate);
                    return View(model);
                }
                var oilPrice = new OilPrice();

                oilPrice.StartDate = DateTime.Parse(datestart.ToString());
                oilPrice.EndDate = DateTime.Parse(date.ToString());

                //Check overlap
                var checkOilPrice = db.OilPrices.ToList();
                Boolean checkDate = true;
                string err = "Thời gian trùng trong khoảng:   ";
                if (checkOilPrice.Count() > 0)
                {
                    foreach (var item in checkOilPrice)
                    {
                        if (item.StartDate != null && item.EndDate != null)
                        {

                            if (item.StartDate.Date <= date.Value.Date && datestart.Value.Date <= item.EndDate.Date)
                            {
                                if(!checkDate)
                                {
                                    err += ", ";
                                }    
                                checkDate = false;
                                err += item.StartDate.Day.ToString() + "/" + item.StartDate.Month.ToString() + "/" + item.StartDate.Year.ToString() + " - " + item.EndDate.Day.ToString() + "/" + item.EndDate.Month.ToString() + "/" + item.EndDate.Year.ToString() + "   ";

                            }
                        }
                    }
                    if (!checkDate)
                    {
                        ModelState.AddModelError("CustomError", err);
                        return View(model);
                    }
                }
                String priceString = (model.OilPrice).Replace('.', ',');
                double price = Double.Parse(priceString);

                oilPrice.Price = price;
                oilPrice.CreatedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                oilPrice.CreatedDate = DateTime.Now;

                db.Set<OilPrice>().Add(oilPrice);
                db.SaveChanges();
                ViewBag.StartupScript = "create_success();";
                return View(model);
            }
            else
            {
                return View(model);
            }
        }

        public ActionResult Edit(int id)
        {
            var model = db.Set<OilPrice>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            var oil = new OilPriceViewModel();
            oil.StartDate = model.StartDate;
            oil.EndDate = model.EndDate;
            oil.OilPrice = model.Price.ToString().Replace(',', '.');
            oil.ID = model.ID;
            return View("Edit", oil);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "")] OilPriceViewModel model, DateTime? datestart, DateTime? date)
        {
            if (ModelState.IsValid)
            {
                if (datestart == null)
                {
                    ModelState.AddModelError("", AccountResources.RequiredStartDate);
                    return View(model);
                }
                if (date == null)
                {
                    ModelState.AddModelError("", AccountResources.RequiredEndDate);
                    return View(model);
                }
                if(datestart.Value.Date > date.Value.Date)
                {
                    ModelState.AddModelError("","Vui lòng chọn thời gian bắt đầu nhỏ hơn thời gian kết thúc!");
                    return View(model);
                }
                var oldPrice = db.OilPrices.Where(x => x.ID == model.ID).Select(x => new { x.StartDate, x.EndDate, x.Price }).FirstOrDefault();
                var oilPrice = new OilPrice();

                oilPrice.StartDate = DateTime.Parse(datestart.ToString());
                oilPrice.EndDate = DateTime.Parse(date.ToString());

                //Check overlap
                var checkOilPrice = db.OilPrices.Where(x => x.ID != model.ID).ToList();
                Boolean checkDate = true;
                string err = "Thời gian trùng trong khoảng:   ";
                if (checkOilPrice.Count() > 0)
                {
                    foreach (var item in checkOilPrice)
                    {
                        if (item.StartDate != null && item.EndDate != null)
                        {

                            if (item.StartDate.Date <= date.Value.Date && datestart.Value.Date <= item.EndDate.Date)
                            {
                                if (!checkDate)
                                {
                                    err += ", ";
                                }
                                checkDate = false;
                                err += item.StartDate.Day.ToString() + "/" + item.StartDate.Month.ToString() + "/" + item.StartDate.Year.ToString() + " - " + item.EndDate.Day.ToString() + "/" + item.EndDate.Month.ToString() + "/" + item.EndDate.Year.ToString() + "   ";

                            }
                        }
                    }
                    if (!checkDate)
                    {
                        ModelState.AddModelError("CustomError", err);
                        return View(model);
                    }
                }

                String priceString = (model.OilPrice).Replace('.', ',');
                double price = Double.Parse(priceString);

                oilPrice.Price = price;
                oilPrice.ID = model.ID;
                oilPrice.ModifiedBy = WebHelpers.UserInfoHelper.GetUserData().UserId;
                oilPrice.ModifiedDate = DateTime.Now;
                using (DbContextTransaction transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.OilPrices.Attach(oilPrice);
                        db.Entry(oilPrice).Property(a => a.StartDate).IsModified = true;
                        db.Entry(oilPrice).Property(a => a.EndDate).IsModified = true;
                        db.Entry(oilPrice).Property(a => a.Price).IsModified = true;
                        db.Entry(oilPrice).Property(a => a.ModifiedBy).IsModified = true;
                        db.Entry(oilPrice).Property(a => a.ModifiedDate).IsModified = true;
                        db.SaveChanges();

                        var listManageOil = db.ManageOils.Where(x=> x.OilDate <= oilPrice.EndDate && x.OilDate >= oilPrice.StartDate).ToList();

                        try
                        {
                            var oilPriceParam = new SqlParameter("@OilPrice", oilPrice.Price);
                            var startDateParam = new SqlParameter("@StartDate", oilPrice.StartDate);
                            var endDateParam = new SqlParameter("@EndDate", oilPrice.EndDate);
                            var modifiedByParam = new SqlParameter("@ModifiedBy", oilPrice.ModifiedBy);
                            var modifiedDateParam = new SqlParameter("@ModifiedDate", oilPrice.ModifiedDate);

                            var oldPriceParam = new SqlParameter("@OldPrice", oldPrice.Price);
                            var oldStartDateParam = new SqlParameter("@OldStartDate", oldPrice.StartDate);
                            var oldEndDateParam = new SqlParameter("@OldEndDate", oldPrice.EndDate);

                            var result = db.Database.ExecuteSqlCommand("ChangeOilPrice @OilPrice,@StartDate,@EndDate,@ModifiedBy,@ModifiedDate,@OldPrice,@OldStartDate,@OldEndDate", oilPriceParam, startDateParam, endDateParam, modifiedByParam, modifiedDateParam, oldPriceParam, oldStartDateParam, oldEndDateParam);

                            if (result != listManageOil.Count + 1)
                            {
                                throw new Exception();
                            }
                            ViewBag.StartupScript = "edit_success();";
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            ModelState.AddModelError("", "Không thể thay đổi giá! Vui lòng liên hệ quản trị viên!");
                            return View(model);
                        }
                        transaction.Commit();
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
            else
            {
                return View(model);
            }
        }
       
        [HttpPost]
        public ActionResult ExportExcel(string dataString)
        {
            List<OilPrice> listData = new List<OilPrice>();
            var dataListJson = dataString.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                OilPrice dataObj = null;
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
                dataObj = JsonConvert.DeserializeObject<OilPrice>(dataObjString);
                dataObj.StartDate = dataObj.StartDate.ToLocalTime();
                dataObj.EndDate = dataObj.EndDate.ToLocalTime();
                listData.Add(dataObj);
            }
            var result = DownloadTransportActual(listData);
            var fileStream = new MemoryStream(result);
            return File(fileStream, "application/ms-excel", "QL_Gia_Dau.xlsx");
        }
        public byte[] DownloadTransportActual(List<OilPrice> models)
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLGiaDau.xlsx", HostingEnvironment.MapPath("/Uploads")));

            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var productWorksheet = p.Workbook.Worksheets[0];
                    productWorksheet.Select();
                    int i = 0;
                    foreach (var item in models)
                    {
                        productWorksheet.Cells[i + 2, 1].Value = item.StartDate;
                        productWorksheet.Cells[i + 2, 2].Value = item.EndDate;
                        productWorksheet.Cells[i + 2, 3].Value = item.Price;
                        productWorksheet.Cells[i + 2, 3].Style.Numberformat.Format = "#,##0";
                        i++;
                    }
                    return p.GetAsByteArray();
                }
            }
            else
            {
                return null;
            }
        }

    }
}