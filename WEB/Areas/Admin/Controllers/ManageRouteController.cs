using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
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
    public class ManageRouteController : Controller
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
        public ActionResult Routes_Read([DataSourceRequest] DataSourceRequest request)
        {
            List<Route> Routes = new List<Route>();

            var routeList = from a in db.Route
                            join b in db.Location on a.StartLocationID equals b.ID
                            join c in db.Location on a.EndLocationID equals c.ID
                            select new
                            {
                                a.ID,
                                a.RouteCode,
                                a.Distance,
                                StartLocation = b.LocationName,
                                EndLocation = c.LocationName
                            };

            foreach (var item in routeList)
            {
                var route = new Route();
                route.ID = item.ID;
                route.RouteCode = item.RouteCode;
                route.StartLocation = new Location()
                {
                    LocationName = item.StartLocation
                };
                route.EndLocation = new Location()
                {
                    LocationName = item.EndLocation
                };
                if (item.Distance != null)
                {
                    var temp = Double.Parse(item.Distance.Replace(".", ","));
                    route.Distance = temp.ToString("N", new CultureInfo("en-US")) + " Km";
                }

                Routes.Add(route);
            }
            return Json(Routes.OrderByDescending(x => x.ID).ToDataSourceResult(request));

        }

        public ActionResult Add()
        {
            var model = new RouteViewModel();
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Add([Bind(Exclude = "")] RouteViewModel model)
        {
            var route = new Route();
            if (ModelState.IsValid)
            {
                var temp = (from p in db.Set<Route>().AsNoTracking()
                            where p.RouteCode.Equals(model.RouteCode, StringComparison.OrdinalIgnoreCase)
                            select p).FirstOrDefault();
                if (temp != null)
                {
                    ModelState.AddModelError("", AccountResources.RouteCodeExists);
                    return View(model);
                }
                else
                {
                    try
                    {
                        var distance = model.Distance;
                        if (distance != null)
                        {
                            route.Distance = distance;
                        }
                        route.RouteCode = model.RouteCode;
                        route.CreatedDate = DateTime.Now;
                        var user = db.Set<UserProfile>().Find(WebSecurity.GetUserId(User.Identity.Name));
                        route.CreatedBy = user.UserId;
                        route.ModifiedDate = null;
                        route.StartLocationID = model.StartLocationID;
                        route.EndLocationID = model.EndLocationID;
                        db.Set<Route>().Add(route);
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
            var model = db.Set<Route>().Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            var routeViewModel = new RouteViewModel();
            routeViewModel.ID = model.ID;
            routeViewModel.RouteCode = model.RouteCode;
            routeViewModel.StartLocationID = model.StartLocationID;
            routeViewModel.EndLocationID = model.EndLocationID;
            if (model.Distance != null)
            {
                routeViewModel.Distance = model.Distance;
            }
            return View("Edit", routeViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "")] RouteViewModel model)
        {
            var route = new Route();
            if (ModelState.IsValid)
            {
                var list = db.Route.Where(x => x.ID != model.ID).ToList();
                var temp = (from p in list
                            where p.RouteCode.Equals(model.RouteCode, StringComparison.OrdinalIgnoreCase)
                            select p).FirstOrDefault();
                if (temp != null)
                {
                    ModelState.AddModelError("", AccountResources.RouteCodeExists);
                    return View(model);
                }
                else
                {
                    try
                    {
                        var distance = model.Distance;
                        if (distance != null)
                        {
                            route.Distance = distance;
                        }
                        route.StartLocationID = model.StartLocationID;
                        route.EndLocationID = model.EndLocationID;
                        route.RouteCode = model.RouteCode;
                        route.ID = model.ID;
                        var user = db.Set<UserProfile>().Find(WebSecurity.GetUserId(User.Identity.Name));
                        route.ModifiedDate = DateTime.Now;
                        route.ModifiedBy = user.UserId;
                        db.Route.Attach(route);
                        db.Entry(route).Property(a => a.RouteCode).IsModified = true;
                        db.Entry(route).Property(a => a.StartLocationID).IsModified = true;
                        db.Entry(route).Property(a => a.EndLocationID).IsModified = true;
                        db.Entry(route).Property(a => a.Distance).IsModified = true;
                        db.Entry(route).Property(a => a.ModifiedBy).IsModified = true;
                        db.Entry(route).Property(a => a.ModifiedDate).IsModified = true;
                        db.SaveChanges();
                        ViewBag.StartupScript = "edit_success();";
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
                return View(route);
            }
        }


        [HttpPost]
        public ActionResult ExportExcel(string dataString)
        {
            List<RouteExport> listData = new List<RouteExport>();
            var dataListJson = dataString.Replace('?', '"');
            var dataObjSplit0 = dataListJson.Split('[');
            var dataObjSplit1 = dataObjSplit0[1].Split('}');
            for (var i = 0; i < (dataObjSplit1.Count() - 1); i++)
            {
                RouteExport dataObj = null;
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
                dataObj = JsonConvert.DeserializeObject<RouteExport>(dataObjString);
              
                listData.Add(dataObj);
            }
            var result = DownloadRoute(listData);
            var fileStream = new MemoryStream(result);
            return File(fileStream, "application/ms-excel", "QLLoTrinh.xlsx");
        }

        public byte[] DownloadRoute(List<RouteExport> models)
        {
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLLoTrinh.xlsx", HostingEnvironment.MapPath("/Uploads")));

            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var productWorksheet = p.Workbook.Worksheets[0];
                    productWorksheet.Select();
                    int i = 0;
                    foreach (var item in models)
                    {
                        productWorksheet.Cells[i + 2, 1].Value = item.RouteCode;
                        productWorksheet.Cells[i + 2, 2].Value = item.StartLocationName;
                        productWorksheet.Cells[i + 2, 3].Value = item.EndLocationName;
                        productWorksheet.Cells[i + 2, 4].Value = item.Distance;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExportExcelOld(List<Route> model)
        {
            var selectedIds = model.Select(x => x.ID);

            var route = from a in db.Route.Where(x => selectedIds.Contains(x.ID))
                        join b in db.Location on a.StartLocationID equals b.ID
                        join c in db.Location on a.EndLocationID equals c.ID
                        select new
                        {
                            a.RouteCode,
                            StartLocation = b.LocationName,
                            EndLocation = c.LocationName,
                            a.Distance
                        };


            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            var fileinfo = new FileInfo(string.Format(@"{0}\QLLoTrinh.xlsx", HostingEnvironment.MapPath("/Uploads")));
            if (fileinfo.Exists)
            {
                using (var p = new ExcelPackage(fileinfo))
                {
                    var productWorksheet = p.Workbook.Worksheets[0];
                    productWorksheet.Select();
                    int i = 0;
                    foreach (var item in route)
                    {
                        productWorksheet.Cells[i + 2, 1].Value = item.RouteCode;
                        productWorksheet.Cells[i + 2, 2].Value = item.StartLocation;
                        productWorksheet.Cells[i + 2, 3].Value = item.EndLocation;
                        productWorksheet.Cells[i + 2, 4].Value = item.Distance;
                        i++;
                    }
                    using (var memoryStream = new MemoryStream())
                    {
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        //here i have set filname as Students.xlsx
                        Response.AddHeader("content-disposition", "attachment;  filename=QLLoTrinh.xlsx");
                        p.SaveAs(memoryStream);
                        memoryStream.WriteTo(Response.OutputStream);
                        Response.Flush();
                        Response.End();
                    }
                }
            }
            ViewBag.StartupScript = "deletes_success();";
            return View("Index");
        }

        public ActionResult UploadExcel()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetUploadProgress()
        {
            var result = Session["UploadRouteProgress"] == null ? 0 : (int)Session["UploadRouteProgress"];

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
                var fileTemp = new FileInfo(string.Format(@"{0}\QLLoTrinh.xlsx", HostingEnvironment.MapPath("/Uploads")));
                var check = ExcelHelper.CheckHeaderFile(fileTemp, file, 1); ;
                if (check == 1)
                {
                    var result = new UploadRouteFromExcel().UploadProducts(file, Session["UploadRouteProgress"]);
                    if (result == null)
                    {
                        ViewBag.check = "Upload lộ trình thành công!";
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
                var result = new UploadRouteFromExcel().DownloadTemplate();
                var fileStream = new MemoryStream(result);
                return File(fileStream, "application/ms-excel", "QLLoTrinh_Template.xlsx");
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

            return File(dateGet, "application/ms-excel", "QLLoTrinh_UploadResult.xlsx");

        }
    }
}