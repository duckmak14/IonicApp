using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using WEB.Models;
using WebModels;

namespace WEB.Areas.ContentType.Controllers
{
    public class BaseController : Controller
    {
        
        
        public List<Language> Language
        {
            get
            {
              return  ApplicationService.Languages;

            }
        }

        protected override void ExecuteCore()
        {

            string culture = "";
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
                this.Session["culture"] = culture;
            }
            if (culture != "")
            try
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
            }
            catch (Exception)
            { }
            base.ExecuteCore();
        }

        protected override bool DisableAsyncSupport
        {
            get { return true; }
        }
    }
}
