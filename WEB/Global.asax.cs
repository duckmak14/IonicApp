using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.WebPages;
using WebMatrix.WebData;

namespace WEB
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public override string GetVaryByCustomString(HttpContext context, string arg)
        {
            //context.Session["culture"] = "vi-VN";
            if (arg.ToLower() == "culture")
            {
                var culture = context.Session["culture"];
                if (culture != null)
                    return culture.ToString();
            }
            return base.GetVaryByCustomString(context, arg);
        }
        private static SimpleMembershipInitializer _initializer;
        private static object _initializerLock = new object();
        private static bool _isInitialized;
        protected void Application_Start()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");

            AreaRegistration.RegisterAllAreas();
            //WebApiConfig.Register(GlobalConfiguration.Configuration);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

            LazyInitializer.EnsureInitialized(ref _initializer, ref _isInitialized, ref _initializerLock);
            //DisplayModeProvider.Instance.Modes.Insert(0,
            //new DefaultDisplayMode("Mobile")
            //{
            //    ContextCondition = (ctx => ctx.Request.UserAgent != null
            //                               && (ctx.Request.UserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0
            //                                   || ctx.Request.UserAgent.IndexOf("Mobile", StringComparison.OrdinalIgnoreCase) >= 0
            //                                   || ctx.Request.UserAgent.IndexOf("Opera Mobi", StringComparison.OrdinalIgnoreCase) >= 0
            //                                   || ctx.Request.UserAgent.IndexOf("Opera Mini", StringComparison.OrdinalIgnoreCase) >= 0))
            //});              
        }
        private class SimpleMembershipInitializer
        {
            public SimpleMembershipInitializer()
            {
                try
                {

                    if (!WebMatrix.WebData.WebSecurity.Initialized)
                    {
                        WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", autoCreateTables: true);
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("The ASP.NET Simple Membership database could not be initialized. For more information, please see http://go.microsoft.com/fwlink/?LinkId=256588", ex);
                }
            }
        }
    }


}