using System.Web;
using System.Web.Mvc;
using WEB.Filters;

namespace WEB
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            
        }
    }
}