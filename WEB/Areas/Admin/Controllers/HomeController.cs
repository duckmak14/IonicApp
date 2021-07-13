using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WEB.Models;

namespace WEB.Areas.Admin.Controllers
{


    public class HomeController : Controller
    {
        WebModels.WebContext db = new WebModels.WebContext();
        //
        // GET: /Admin/Home/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Loading()
        {
            return View();
        }

        public ActionResult _ContentLeft()
        {
            var userId = WebHelpers.UserInfoHelper.GetUserData().UserId;
            // join webmodule
            var result = from wm in db.WebModules
                         join awmr in db.AccessWebModuleRoles on wm.ID equals awmr.WebModuleID
                         join uir in db.UserInRoles on awmr.RoleId equals uir.RoleId
                         where uir.UserId == userId && ((awmr.Add.HasValue && awmr.Add.Value) || (awmr.Edit.HasValue && awmr.Edit.Value)
                         || (awmr.Delete.HasValue && awmr.Delete.Value) || (awmr.View.HasValue && awmr.View.Value))
                         select wm;

            var test = result.Where(x => x.Status == 1).ToList();
                        
            return PartialView(test);
        }
    }
}
