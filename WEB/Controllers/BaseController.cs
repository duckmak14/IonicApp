using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.SessionState;
using WEB.Models;
using WebMatrix.WebData;
using WebModels;

namespace WEB.Controllers
{
    
    public class BaseController : Controller
    {

        
        #region OnActionExecuting
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            
        }

        
        public List<Language> Language {

            get {
                if (HttpContext.Cache["Languages"] == null) { 
                    using (var db = new WebContext())
                    {
                        var langs = from x in db.Languages where x.Published != null && x.Published.Value orderby x.Order select x;
                        if (langs != null)
                        {
                            HttpContext.Cache["Languages"] = langs.ToList();
                            
                        }
                        else
                            return null;
                    }
                } 
                    return (List<Language>)HttpContext.Cache["Languages"];
                 
            }

        }
        #endregion
       
        public string Culture {
            get {
                return this.Session["culture"].ToString();
            }
        }
        protected override void ExecuteCore()
        {
            if (Language.Count >= 2)
            {
                string culture = "";
                if (RouteData.Values["lang"] != null
                    && !string.IsNullOrWhiteSpace(RouteData.Values["lang"].ToString()))
                {
                    try
                    {
                        var temp = RouteData.Values["lang"].ToString().ToLower();
                        if (temp.Length == 2)
                        {
                            culture = ApplicationService.CultureByIsoLanguage(temp);

                        }
                        //else if (temp.Length == 5)
                        //{
                        //    culture = temp;
                        //}
                    }
                    catch (Exception)
                    { }

                }

                if (culture == null || culture == "")
                {
                    if (this.Session["culture"] != null)
                    {
                        culture = this.Session["culture"].ToString();
                    }
                    else
                    {
                        try
                        {
                            if (this.Language != null)
                            {
                                culture = this.Language.First().ID;
                            }
                            //else
                            //    culture = CultureInfo.InvariantCulture.Name;

                        }
                        catch (Exception)
                        {
                            culture = CultureInfo.InvariantCulture.Name;
                        }

                    }
                }
                this.Session["culture"] = culture;
                if (culture != "")
                try
                {

                    Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(culture);  
                    Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
                }
                catch (Exception)
                { } 
            }
            base.ExecuteCore();
        }

        protected override bool DisableAsyncSupport
        {
            get { return true; }
        }
        public ActionResult ChangeLanguage(string clang, string returnUrl)
        {
             
            if (Url.IsLocalUrl(returnUrl))
            { 
                return Redirect(returnUrl);
            }
            else
            {
                var twolang = ApplicationService.TwoLetterISOLanguageName(clang).ToLower(); 
                return Redirect("/"+twolang);
            }
        }
    }
}
