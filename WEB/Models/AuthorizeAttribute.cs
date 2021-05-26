using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using WebModels;

namespace System.Web.Mvc
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        private string accessKey = string.Empty;

        public string AccessKey
        {
            get { return accessKey; }
            set { accessKey = value; }
        }



        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
                return false;
            //var checkrole = System.Web.Security.Roles.GetRolesForUser(httpContext.User.Identity.Name).Contains("Administrators");

            //if (checkrole)
            //{
            //    return true;
            //}
            //else
            //{
            //    // FormsAuthentication.SignOut();
            //    return false;
            //}
            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
            else
            {
                var returnUrl = filterContext.RequestContext.HttpContext.Request.RawUrl;
                filterContext.Result = new RedirectToRouteResult(new
                RouteValueDictionary(new { controller = "Account", action = "Denied", Area = "Admin", returnUrl = returnUrl.ToString() }));
            }
        }
    }

    public class ClientAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                base.OnAuthorization(filterContext);
            }
            else
            {
                var returnUrl = filterContext.RequestContext.HttpContext.Request.RawUrl;
                filterContext.Result = new RedirectToRouteResult(new
                RouteValueDictionary(new { controller = "Account", action = "Login", Area = "", returnUrl = returnUrl.ToString() }));

            }
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {

            return httpContext.User.Identity.IsAuthenticated;
        }
    }
}
