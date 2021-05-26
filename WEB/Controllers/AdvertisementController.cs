using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WEB.Models;
using WebModels;

namespace WEB.Controllers
{
    public class AdvertisementController : Controller
    {
        WebContext db = new WebContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        [ChildActionOnly]
        [HttpGet]
        // [OutputCache(Duration = 60 )]
        public ActionResult Index(string position)
        {
            var adv = db.Advertisements.Where(
                x => x.AdvertisementPosition.UID.ToLower().Equals(position) &&

                         ((x.Culture == null ||
                              (!string.IsNullOrEmpty(x.Culture) && x.Culture.Equals(ApplicationService.Culture)))
                              || (ApplicationService.Culture == null))

                );
            ViewBag.Position = position;
            return PartialView(adv);
        }
        [ChildActionOnly]
        [HttpGet]
        // [OutputCache(Duration = 60 )]
        public ActionResult _Adv(string position)
        {
            var adv = db.Advertisements.Where(
                x =>
                x.Status == (int)StatusEnum.Public
                && x.AdvertisementPosition.UID.ToLower().Equals(position));
            ViewBag.Position = position;
            int coutn = adv.Count();
            return PartialView(adv);
        }
        [ChildActionOnly]
        // [OutputCache(Duration = 60 )]
        public ActionResult _AdvHoSoTuLieu(string position)
        {
            var adv = db.Advertisements.Where(
                x =>
                x.Status == (int)StatusEnum.Public
                && x.AdvertisementPosition.UID.ToLower().Equals(position));
            ViewBag.Position = position;
            int coutn = adv.Count();
            return PartialView(adv);
        }
        
        [ChildActionOnly]
        [HttpGet]
        // [OutputCache(Duration = 60 )]
        public ActionResult _AdvBanner(string position)
        {
            var adv = db.Advertisements.Where(
                x => x.Status == (int)StatusEnum.Public
                &&
                x.AdvertisementPosition.UID.ToLower().Equals(position));
            ViewBag.Position = position;
            int coutn = adv.Count();
            return PartialView(adv);
        }

             [ChildActionOnly]
        [HttpGet]
        // [OutputCache(Duration = 60 )]
        public ActionResult _AdvCenterHome(string position)
        {
            var adv = db.Advertisements.Where(
                x => x.Status == (int) StatusEnum.Public
                && x.AdvertisementPosition.UID.ToLower().Equals(position));
            ViewBag.Position = position;
            int coutn = adv.Count();
            return PartialView(adv);
        }
        
        [ChildActionOnly]
        [HttpGet]
        // [OutputCache(Duration = 60 )]
        public ActionResult _AdvFooter(string position)
        {
            var adv = db.Advertisements.Where(
                x => x.AdvertisementPosition.UID.ToLower().Equals(position) &&
                x.Status == (int)StatusEnum.Public
                &&
                         ((x.Culture == null ||
                              (!string.IsNullOrEmpty(x.Culture) && x.Culture.Equals(ApplicationService.Culture)))
                              || (ApplicationService.Culture == null))

                );
            ViewBag.Position = position;
            return PartialView(adv);
        }
        [HttpGet]
        public JsonResult JIndex(string position)
        {
            var adv = db.Advertisements.Where(x =>
                x.AdvertisementPosition.UID.ToLower().Equals(position) &&

                         ((x.Culture == null ||
                              (!string.IsNullOrEmpty(x.Culture) && x.Culture.Equals(ApplicationService.Culture)))
                              || (ApplicationService.Culture == null))


                ).Select(x => new { x.ID, x.Title, x.Description, x.Link, x.Media, x.Target });
            return Json(adv, JsonRequestBehavior.AllowGet);
        }
    }
}