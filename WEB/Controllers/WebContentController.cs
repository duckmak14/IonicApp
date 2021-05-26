using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WEB.Controllers
{
    public class WebContentController : BaseController
    {
        //
        // GET: /WebContent/

        public ActionResult Index()
        {
            return View();
        }

    }
}
