using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using WEB.Models;
using WebMatrix.WebData;
using WebModels;

namespace WEB
{
    public class RouteConfig
    {
        
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               name: "Base_ChangeLanguage",
               url: "base/changelanguage/{clang}",
               defaults: new { controller = "Base", action = "ChangeLanguage", clang = UrlParameter.Optional },
                 namespaces: new[] { "WEB.Controllers" }
           );


            routes.MapRoute(
               "Local_Home_Index",
               "{lang}/{metatitle}-{id}",
               new { controller = "Home", action = "Index", metatitle = UrlParameter.Optional, lang = UrlParameter.Optional },
               new { lang = @"\w{2,3}(-\w{2,3})?", id = @"\d+" },
                namespaces: new[] { "WEB.Controllers" }
          );

            routes.MapRoute(
                "Local_Home_Index_Detail",
                "{lang}/{m_metatitle}-{m_id}/{metatitle}-{id}",
                new
                {
                    controller = "Home",
                    action = "Detail",
                    metatitle = UrlParameter.Optional,
                    id = UrlParameter.Optional,
                    m_metatitle = UrlParameter.Optional,
                    lang = UrlParameter.Optional
                },
                 new { lang = @"\w{2,3}(-\w{2,3})?", id = @"\d+", m_id = @"\d+" },
                 namespaces: new[] { "WEB.Controllers" }
           );
   
            
           routes.MapRoute(
                            "Local_Default",
                            "{lang}/{controller}/{action}",
                            new { controller = "Home", action = "Index", },
                            new { lang = @"\w{2,3}(-\w{2,3})?" },
                             namespaces: new[] { "WEB.Controllers" }
            );
            



            //-------------------------------------------------------------


            routes.MapRoute(
             name: "ProductCategories",
             url: "ds-{metatitle}-{id}",
             defaults: new { controller = "ProductCategory", action = "Index", metatitle = UrlParameter.Optional, id = UrlParameter.Optional },
             constraints: new { id = @"\d+", },
             namespaces: new[] { "WEB.Controllers" }
         );

            routes.MapRoute(
               name: "Home_Index",
               url: "{metatitle}-{id}",
               defaults: new { controller = "Home", action = "Index", metatitle = UrlParameter.Optional, id = UrlParameter.Optional },
               constraints: new { id = @"\d+", },
                 namespaces: new[] { "WEB.Controllers" }
           );

            routes.MapRoute(
               name: "Home_Index_Detail",
               url: "{m_metatitle}-{m_id}/{metatitle}-{id}",
               defaults: new { controller = "Home", action = "Detail", metatitle = UrlParameter.Optional, id = UrlParameter.Optional, m_metatitle = UrlParameter.Optional, m_id = UrlParameter.Optional },
               constraints: new { id = @"\d+", m_id = @"\d+", },
                 namespaces: new[] { "WEB.Controllers" }
           );

          
            routes.MapRoute(
                name: "Search",
                url: "tim-kiem",
                defaults: new { controller = "Home", action = "Search" },
                  namespaces: new[] { "WEB.Controllers" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                  namespaces: new[] { "WEB.Controllers" }
            );
        }
    }
}