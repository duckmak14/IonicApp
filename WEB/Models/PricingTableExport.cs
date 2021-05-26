using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class PricingTableExport : PricingTable
    {
        public String RouteCode { get; set; }
        public String PartnerCode { get; set; }
        public String StartLocationName { get; set; }
        public String EndLocationName { get; set; }
        public String WeightName { get; set; }
        public String SourcePartnerName { get; set; }
        public String DestinationPartnerName { get; set; }
   
    }
}