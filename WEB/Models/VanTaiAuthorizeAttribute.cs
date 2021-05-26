using System.Linq;
using System.Web;
using System.Web.Mvc;
using WEB.WebHelpers;
using WebModels;

namespace WEB.Models
{
    public class VanTaiAuthorizeAttribute : AuthorizeAttribute
    {
        private WebContext db = new WebContext();

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.IsChildAction)
            {
                return;
            }

            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                var userData = UserInfoHelper.GetUserData();
                var url = string.Format("/admin/{0}", filterContext.RouteData.Values["controller"]);
                var currentWebModuleId = db.WebModules.FirstOrDefault(x => x.URL.ToLower() == url.ToLower()).ID;

                var userWebModules = from wm in db.WebModules
                                     join awmr in db.AccessWebModuleRoles on wm.ID equals awmr.WebModuleID
                                     join uir in db.UserInRoles on awmr.RoleId equals uir.RoleId
                                     where uir.UserId == userData.UserId && (awmr.View.HasValue && awmr.View.Value)
                                     select wm;
                if (!userWebModules.Select(x => x.ID).Contains(currentWebModuleId))
                {
                    filterContext.Result = new RedirectResult("/admin");
                }
            }
            else
            {
                filterContext.Result = new RedirectResult("/admin/account/login");
            }
        }
    }
}