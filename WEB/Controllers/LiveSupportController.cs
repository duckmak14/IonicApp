using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WEB.Controllers
{
    public class LiveSupportController : Controller
    {
        //
        // GET: /LiveSupport/
        public ActionResult _LeftBox()
        {
            return PartialView();
        }
        public ActionResult _DetailBox()
        {
            return PartialView();
        }
	}
}