using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class PricingTableViewModel : PricingTable
    {
        [Display(ResourceType = typeof(AccountResources), Name = "StartLocation")]
        public string StartLocationName { get; set; }

        [Display(ResourceType = typeof(AccountResources), Name = "EndLocation")]
        public string EndLocationName { get; set; }

        [Display(ResourceType = typeof(AccountResources), Name = "SourcePartner")]
        public string SourcePartnerName { get; set; }

        [Display(ResourceType = typeof(AccountResources), Name = "DestinationPartner")]
        public string DestinationPartnerName { get; set; }
        [Display(ResourceType = typeof(AccountResources), Name = "RouteCode")]
        public string RouteCode { get; set; }
        [Display(ResourceType = typeof(AccountResources), Name = "WeightName")]
        public string WeightName { get; set; }
        public string PartnerCode { get; set; }

    }
}
