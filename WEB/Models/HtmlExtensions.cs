using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using WebModels;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
namespace WEB.Models
{
    public static class HtmlExtensions
    {
        public static IHtmlString RawSubString(this HtmlHelper helper, string text, int length)
        {
            text = text.Trim();

            if (text.Length > length)
            {
                text = text.Substring(0, length) + "...";
            }

            return helper.Raw(text);
        }

        
    }
}