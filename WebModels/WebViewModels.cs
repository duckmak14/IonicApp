using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using System.Web;
using System.Configuration;
using Kendo.Mvc.UI;
using System.Web.Mvc;
namespace WebModels
{
    public class WebModuleTree
     {
         public int ID { get; set; }
         public string Title { get; set; }
         public string MetaTitle { get; set; }
         public string Description { get; set; }
         public int? Order { get; set; }
         public int? ParentID { get; set; }
         public string ContentTypeID { get; set; } 
     }
    public class WebCategoryNode
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string MetaTitle { get; set; }
        public string Description { get; set; }
        public int? Order { get; set; }
        public int? ParentID { get; set; }
    }


}
