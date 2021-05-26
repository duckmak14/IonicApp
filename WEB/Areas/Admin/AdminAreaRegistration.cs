using System.Web.Mvc;

namespace WEB.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {

            context.MapRoute(
               "Admin_webcontent",
               "Admin/webcontent/{id}",
               new { controller = "WebContent", action = "Index", id = UrlParameter.Optional },
               namespaces: new[] { "WEB.Areas.Admin.Controllers" }
           );
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "WEB.Areas.Admin.Controllers" }
            );
            
        }
    }
}
