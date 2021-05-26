using System.Web.Mvc;

namespace WEB.Areas.ContentType
{
    public class ContentTypeAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "ContentType";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            
            context.MapRoute(
              "ContentType_default",
              "ContentType/{controller}/{action}/{id}",
              new { controller = "Home", action = "Index", id = UrlParameter.Optional },
              namespaces: new[] { "WEB.Areas.ContentType.Controllers" }
          );
        }
    }
}
