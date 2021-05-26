using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WEB
{
    /// <summary>
    /// Summary description for KeepAlive
    /// </summary>
    public class KeepAlive : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
      {
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}